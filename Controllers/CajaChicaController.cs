using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovitecContabilidad.Data;
using NovitecContabilidad.Models;
using NovitecContabilidad.Services;

namespace NovitecContabilidad.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CajaChicaController : ControllerBase
    {
        private readonly AccountingDbContext _context;
        private readonly ExcelExportService _excelService;

        public CajaChicaController(AccountingDbContext context, ExcelExportService excelService)
        {
            _context = context;
            _excelService = excelService;
        }

        private int GetUserSucursalId()
        {
            var claim = User.FindFirst("sucursal_id")?.Value;
            return int.TryParse(claim, out var id) ? id : 0;
        }

        private int GetUserRolId()
        {
            var claim = User.FindFirst("rol_id")?.Value;
            return int.TryParse(claim, out var id) ? id : 0;
        }

        private bool IsSuperAdmin()
        {
            var esSuperAdminClaim = User.FindFirst("es_superadmin")?.Value;
            if (bool.TryParse(esSuperAdminClaim, out var isSa) && isSa) return true;
            return GetUserRolId() == 3;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var sucursalId = GetUserSucursalId();
            var isSa = IsSuperAdmin();

            var query = _context.CajasChicas.AsQueryable();

            if (!isSa)
            {
                query = query.Where(c => c.SucursalId == sucursalId);
            }

            var list = await query
                .OrderByDescending(c => c.FechaCreacion)
                .ThenByDescending(c => c.Id)
                .ToListAsync();

            return Ok(new { ok = true, data = list });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var cabecera = await _context.CajasChicas
                .Include(c => c.Detalles)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cabecera == null)
            {
                return NotFound(new { ok = false, error = "Caja Chica no encontrada." });
            }

            // Validar restricción de sucursal
            if (!IsSuperAdmin() && cabecera.SucursalId != GetUserSucursalId())
            {
                return Forbid();
            }

            return Ok(new { ok = true, data = cabecera });
        }

        public class OpenCajaRequest
        {
            public int SucursalId { get; set; }
            public string CodigoSucursal { get; set; } = string.Empty;
            public decimal FondoInicial { get; set; } = 1000.00m;
            public int CustodioUsuarioId { get; set; }
            public string CustodioNombre { get; set; } = string.Empty;
            public string? FechaCreacion { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> Open([FromBody] OpenCajaRequest req)
        {
            // Validar restricción de sucursal
            if (!IsSuperAdmin() && req.SucursalId != GetUserSucursalId())
            {
                return Forbid();
            }

            // Verificar si ya existe una Caja Chica abierta para la sucursal
            var existeAbierta = await _context.CajasChicas
                .AnyAsync(c => c.SucursalId == req.SucursalId && c.Estado == "Abierta");

            if (existeAbierta)
            {
                return BadRequest(new { ok = false, error = "Ya existe una Caja Chica abierta para esta sucursal." });
            }

            DateTime fechaCreacion = DateTime.Today;
            if (!string.IsNullOrEmpty(req.FechaCreacion) && DateTime.TryParse(req.FechaCreacion, out var parsedDate))
            {
                fechaCreacion = parsedDate;
            }

            var dateStr = fechaCreacion.ToString("dd-MMMM-yyyy").ToUpper();
            var nroCajaChica = $"{req.CodigoSucursal}-{dateStr}";

            // Asegurar nro_caja_chica único
            int counter = 1;
            while (await _context.CajasChicas.AnyAsync(c => c.NroCajaChica == nroCajaChica))
            {
                nroCajaChica = $"{req.CodigoSucursal}-{dateStr}-{counter}";
                counter++;
            }

            var cabecera = new CajaChicaCabecera
            {
                SucursalId = req.SucursalId,
                CodigoSucursal = req.CodigoSucursal,
                NroCajaChica = nroCajaChica,
                CustodioUsuarioId = req.CustodioUsuarioId > 0 ? req.CustodioUsuarioId : int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"),
                CustodioNombre = !string.IsNullOrEmpty(req.CustodioNombre) ? req.CustodioNombre : (User.FindFirst(ClaimTypes.Name)?.Value ?? "Custodio"),
                FechaCreacion = fechaCreacion,
                Estado = "Abierta",
                FondoInicial = req.FondoInicial,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.CajasChicas.Add(cabecera);
            await _context.SaveChangesAsync();

            return Ok(new { ok = true, message = "Caja Chica abierta correctamente.", data = cabecera });
        }

        public class AddItemRequest
        {
            public string FechaComprobante { get; set; } = string.Empty;
            public string NroComprobante { get; set; } = string.Empty;
            public string Proveedor { get; set; } = string.Empty;
            public string Descripcion { get; set; } = string.Empty;
            public string TipoGasto { get; set; } = string.Empty;
            public decimal SubtotalSinIva { get; set; }
            public decimal SubtotalConIva { get; set; }
            public decimal ValorEntregado { get; set; }
            public string? UsuarioBeneficiado { get; set; }
            public string EstadoVuelto { get; set; } = "No Aplica";
        }

        [HttpPost("{id}/items")]
        public async Task<IActionResult> AddItem(long id, [FromBody] AddItemRequest req)
        {
            var cabecera = await _context.CajasChicas.FindAsync(id);
            if (cabecera == null)
            {
                return NotFound(new { ok = false, error = "Caja Chica no encontrada." });
            }

            if (cabecera.Estado != "Abierta")
            {
                return BadRequest(new { ok = false, error = "No se pueden agregar ítems a una Caja Chica cerrada." });
            }

            if (!IsSuperAdmin() && cabecera.SucursalId != GetUserSucursalId())
            {
                return Forbid();
            }

            if (!DateTime.TryParse(req.FechaComprobante, out var fechaComp))
            {
                return BadRequest(new { ok = false, error = "Fecha de comprobante inválida." });
            }

            // Calcular montos
            var iva = Math.Round(req.SubtotalConIva * 0.15m, 2);
            var total = req.SubtotalSinIva + req.SubtotalConIva + iva;

            // Calcular vuelto esperado
            var vueltoEsperado = 0.00m;
            var estadoVuelto = req.EstadoVuelto;

            if (req.ValorEntregado > total)
            {
                vueltoEsperado = req.ValorEntregado - total;
                // Forzar a "Pendiente" si hay vuelto por entregar y no se ha especificado "Devuelto"
                if (estadoVuelto == "No Aplica" || string.IsNullOrEmpty(estadoVuelto))
                {
                    estadoVuelto = "Pendiente";
                }
            }
            else
            {
                vueltoEsperado = 0.00m;
                estadoVuelto = "No Aplica";
            }

            var item = new CajaChicaDetalle
            {
                CajaChicaId = id,
                FechaComprobante = fechaComp,
                NroComprobante = req.NroComprobante,
                Proveedor = req.Proveedor,
                Descripcion = req.Descripcion,
                TipoGasto = req.TipoGasto,
                SubtotalSinIva = req.SubtotalSinIva,
                SubtotalConIva = req.SubtotalConIva,
                Iva = iva,
                Total = total,
                ValorEntregado = req.ValorEntregado,
                UsuarioBeneficiado = req.UsuarioBeneficiado,
                VueltoEsperado = vueltoEsperado,
                EstadoVuelto = estadoVuelto,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Detalles.Add(item);
            cabecera.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { ok = true, message = "Ítem registrado con éxito.", data = item });
        }

        [HttpPut("items/{itemId}")]
        public async Task<IActionResult> UpdateItem(long itemId, [FromBody] AddItemRequest req)
        {
            var item = await _context.Detalles
                .Include(i => i.CajaChica)
                .FirstOrDefaultAsync(i => i.Id == itemId);

            if (item == null || item.CajaChica == null)
            {
                return NotFound(new { ok = false, error = "Ítem no encontrado." });
            }

            if (item.CajaChica.Estado != "Abierta")
            {
                return BadRequest(new { ok = false, error = "La Caja Chica está cerrada." });
            }

            if (!IsSuperAdmin() && item.CajaChica.SucursalId != GetUserSucursalId())
            {
                return Forbid();
            }

            if (!DateTime.TryParse(req.FechaComprobante, out var fechaComp))
            {
                return BadRequest(new { ok = false, error = "Fecha de comprobante inválida." });
            }

            // Calcular montos
            var iva = Math.Round(req.SubtotalConIva * 0.15m, 2);
            var total = req.SubtotalSinIva + req.SubtotalConIva + iva;

            var vueltoEsperado = 0.00m;
            var estadoVuelto = req.EstadoVuelto;

            if (req.ValorEntregado > total)
            {
                vueltoEsperado = req.ValorEntregado - total;
                if (estadoVuelto == "No Aplica" || string.IsNullOrEmpty(estadoVuelto))
                {
                    estadoVuelto = "Pendiente";
                }
            }
            else
            {
                vueltoEsperado = 0.00m;
                estadoVuelto = "No Aplica";
            }

            item.FechaComprobante = fechaComp;
            item.NroComprobante = req.NroComprobante;
            item.Proveedor = req.Proveedor;
            item.Descripcion = req.Descripcion;
            item.TipoGasto = req.TipoGasto;
            item.SubtotalSinIva = req.SubtotalSinIva;
            item.SubtotalConIva = req.SubtotalConIva;
            item.Iva = iva;
            item.Total = total;
            item.ValorEntregado = req.ValorEntregado;
            item.UsuarioBeneficiado = req.UsuarioBeneficiado;
            item.VueltoEsperado = vueltoEsperado;
            item.EstadoVuelto = estadoVuelto;
            item.UpdatedAt = DateTime.UtcNow;

            item.CajaChica.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { ok = true, message = "Ítem actualizado con éxito.", data = item });
        }

        [HttpDelete("items/{itemId}")]
        public async Task<IActionResult> DeleteItem(long itemId)
        {
            var item = await _context.Detalles
                .Include(i => i.CajaChica)
                .FirstOrDefaultAsync(i => i.Id == itemId);

            if (item == null || item.CajaChica == null)
            {
                return NotFound(new { ok = false, error = "Ítem no encontrado." });
            }

            if (item.CajaChica.Estado != "Abierta")
            {
                return BadRequest(new { ok = false, error = "La Caja Chica está cerrada." });
            }

            if (!IsSuperAdmin() && item.CajaChica.SucursalId != GetUserSucursalId())
            {
                return Forbid();
            }

            _context.Detalles.Remove(item);
            item.CajaChica.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { ok = true, message = "Ítem eliminado correctamente." });
        }

        [HttpPost("{id}/close")]
        public async Task<IActionResult> Close(long id)
        {
            var cabecera = await _context.CajasChicas.FindAsync(id);
            if (cabecera == null)
            {
                return NotFound(new { ok = false, error = "Caja Chica no encontrada." });
            }

            if (!IsSuperAdmin() && cabecera.SucursalId != GetUserSucursalId())
            {
                return Forbid();
            }

            if (cabecera.Estado != "Abierta")
            {
                return BadRequest(new { ok = false, error = "La Caja Chica ya está cerrada." });
            }

            cabecera.Estado = "Cerrada";
            cabecera.FechaCierre = DateTime.Today;
            cabecera.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { ok = true, message = "Caja Chica cerrada correctamente.", data = cabecera });
        }

        public class ReimburseRequest
        {
            public decimal MontoRecarga { get; set; }
        }

        [HttpPost("{id}/reimburse")]
        public async Task<IActionResult> Reimburse(long id, [FromBody] ReimburseRequest req)
        {
            // Solo Superadministradores/Contadores autorizan reembolsos
            if (!IsSuperAdmin())
            {
                return Forbid();
            }

            var cabecera = await _context.CajasChicas
                .Include(c => c.Detalles)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cabecera == null)
            {
                return NotFound(new { ok = false, error = "Caja Chica no encontrada." });
            }

            if (cabecera.Estado != "Cerrada")
            {
                return BadRequest(new { ok = false, error = "Solo se pueden reembolsar cajas chicas en estado 'Cerrada'." });
            }

            // Validaciones matemáticas
            var totalGastado = cabecera.Detalles.Sum(d => d.Total);
            var saldoRestante = cabecera.FondoInicial - totalGastado;

            // Recarga sugerida o manual para completar los 1000.00
            var montoEsperado = totalGastado;
            if (req.MontoRecarga <= 0)
            {
                req.MontoRecarga = montoEsperado;
            }

            // Marcar la caja actual como Reembolsada
            cabecera.Estado = "Reembolsada";
            cabecera.UpdatedAt = DateTime.UtcNow;

            // Calcular automáticamente la fecha de inicio del siguiente período (23 del siguiente mes)
            var nuevaFechaCreacion = cabecera.FechaCreacion.AddMonths(1);
            if (nuevaFechaCreacion.Day != 23)
            {
                try
                {
                    nuevaFechaCreacion = new DateTime(nuevaFechaCreacion.Year, nuevaFechaCreacion.Month, 23);
                }
                catch { }
            }

            var dateStr = nuevaFechaCreacion.ToString("dd-MMMM-yyyy").ToUpper();
            var nuevoNro = $"{cabecera.CodigoSucursal}-{dateStr}";

            int counter = 1;
            while (await _context.CajasChicas.AnyAsync(c => c.NroCajaChica == nuevoNro))
            {
                nuevoNro = $"{cabecera.CodigoSucursal}-{dateStr}-{counter}";
                counter++;
            }

            var nuevaCaja = new CajaChicaCabecera
            {
                SucursalId = cabecera.SucursalId,
                CodigoSucursal = cabecera.CodigoSucursal,
                NroCajaChica = nuevoNro,
                CustodioUsuarioId = cabecera.CustodioUsuarioId,
                CustodioNombre = cabecera.CustodioNombre,
                FechaCreacion = nuevaFechaCreacion,
                Estado = "Abierta",
                FondoInicial = saldoRestante + req.MontoRecarga, // Restante + Recarga manual = Nuevo Fondo
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.CajasChicas.Add(nuevaCaja);
            await _context.SaveChangesAsync();

            return Ok(new { 
                ok = true, 
                message = "Caja Chica reembolsada y nuevo periodo abierto con éxito.", 
                cajaReembolsada = cabecera, 
                nuevaCajaAbierta = nuevaCaja 
            });
        }

        [HttpGet("{id}/export")]
        public async Task<IActionResult> ExportExcel(long id)
        {
            var cabecera = await _context.CajasChicas
                .Include(c => c.Detalles)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cabecera == null)
            {
                return NotFound(new { ok = false, error = "Caja Chica no encontrada." });
            }

            if (!IsSuperAdmin() && cabecera.SucursalId != GetUserSucursalId())
            {
                return Forbid();
            }

            try
            {
                // Nombre de la sucursal para rellenar en cabecera de la plantilla
                // Esto podría venir de la base de datos o por parámetro simple
                string nombreSucursal = "SUCURSAL NOVICOMPU QUITO";
                if (cabecera.SucursalId == 1739) nombreSucursal = "SUCURSAL NOVICOMPU MANTA";
                else if (cabecera.SucursalId == 1738) nombreSucursal = "SUCURSAL NOVICOMPU GUAYAQUIL";

                var fileBytes = _excelService.ExportCajaChica(cabecera, nombreSucursal);
                var fileName = $"Informe_Caja_Chica_{cabecera.NroCajaChica}.xlsx";

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { ok = false, error = $"Error al generar Excel: {ex.Message}" });
            }
        }
    }
}
