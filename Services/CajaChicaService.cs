using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NovitecContabilidad.DTOs;
using NovitecContabilidad.Models;
using NovitecContabilidad.Repositories;

namespace NovitecContabilidad.Services
{
    public class CajaChicaService : ICajaChicaService
    {
        private readonly ICajaChicaRepository _repo;

        public CajaChicaService(ICajaChicaRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<CajaChicaCabeceraDto>> GetCajaChicasBySucursalAsync(int sucursalId, bool isSuperAdmin)
        {
            var list = await _repo.GetBySucursalAsync(sucursalId, isSuperAdmin);
            return list.Select(MapToCabeceraDto).ToList();
        }

        public async Task<CajaChicaCabeceraDto?> GetCajaChicaByIdAsync(long id)
        {
            var cabecera = await _repo.GetByIdWithDetallesAsync(id);
            return cabecera != null ? MapToCabeceraDto(cabecera) : null;
        }

        public async Task<string> GetSucursalNameAsync(int sucursalId)
        {
            try
            {
                using (var conn = _repo.GetDbConnection())
                {
                    await conn.OpenAsync();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT ciudad FROM sucursales WHERE id = @id";
                        var param = cmd.CreateParameter();
                        param.ParameterName = "@id";
                        param.Value = sucursalId;
                        cmd.Parameters.Add(param);

                        var result = await cmd.ExecuteScalarAsync();
                        return result?.ToString() ?? "Novitec Quito";
                    }
                }
            }
            catch
            {
                // Fallbacks
                if (sucursalId == 2) return "Novitec Guayaquil";
                if (sucursalId == 3) return "Novitec Manta";
                return "Novitec Quito";
            }
        }

        public async Task<CajaChicaCabeceraDto> OpenCajaChicaAsync(OpenCajaRequest req, int defaultUserId, string defaultUserName)
        {
            var existeAbierta = await _repo.AnyCajaChicaAbiertaAsync(req.SucursalId);
            if (existeAbierta)
            {
                throw new InvalidOperationException("Ya existe una Caja Chica abierta para esta sucursal.");
            }

            DateTime fechaCreacion = DateTime.Today;
            if (!string.IsNullOrEmpty(req.FechaCreacion) && DateTime.TryParse(req.FechaCreacion, out var parsedDate))
            {
                fechaCreacion = parsedDate;
            }

            var dateStr = fechaCreacion.ToString("dd-MMMM-yyyy").ToUpper();
            var nroCajaChica = $"{req.CodigoSucursal}-{dateStr}";

            int counter = 1;
            while (await _repo.AnyCajaChicaByNroAsync(nroCajaChica))
            {
                nroCajaChica = $"{req.CodigoSucursal}-{dateStr}-{counter}";
                counter++;
            }

            var cabecera = new CajaChicaCabecera
            {
                SucursalId = req.SucursalId,
                CodigoSucursal = req.CodigoSucursal,
                NroCajaChica = nroCajaChica,
                CustodioUsuarioId = req.CustodioUsuarioId > 0 ? req.CustodioUsuarioId : defaultUserId,
                CustodioNombre = !string.IsNullOrEmpty(req.CustodioNombre) ? req.CustodioNombre : defaultUserName,
                FechaCreacion = fechaCreacion,
                Estado = "Abierta",
                FondoInicial = req.FondoInicial,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(cabecera);
            await _repo.SaveAsync();

            return MapToCabeceraDto(cabecera);
        }

        public async Task<CajaChicaDetalleDto> AddItemAsync(long id, AddItemRequest req)
        {
            var cabecera = await _repo.GetByIdAsync(id);
            if (cabecera == null)
            {
                throw new KeyNotFoundException("Caja Chica no encontrada.");
            }

            if (cabecera.Estado != "Abierta")
            {
                throw new InvalidOperationException("No se pueden agregar ítems a una Caja Chica cerrada.");
            }

            if (!DateTime.TryParse(req.FechaComprobante, out var fechaComp))
            {
                throw new ArgumentException("Fecha de comprobante inválida.");
            }

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
                ComprobanteUrl = req.ComprobanteUrl,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _repo.AddDetalle(item);
            cabecera.UpdatedAt = DateTime.UtcNow;
            await _repo.SaveAsync();

            return MapToDetalleDto(item);
        }

        public async Task<CajaChicaDetalleDto> UpdateItemAsync(long itemId, AddItemRequest req)
        {
            var item = await _repo.GetDetalleByIdWithCabeceraAsync(itemId);
            if (item == null || item.CajaChica == null)
            {
                throw new KeyNotFoundException("Ítem no encontrado.");
            }

            if (item.CajaChica.Estado != "Abierta")
            {
                throw new InvalidOperationException("La Caja Chica está cerrada.");
            }

            if (item.EstadoVuelto == "Devuelto")
            {
                throw new InvalidOperationException("No se puede editar un ítem cuyo vuelto ya fue devuelto y conciliado.");
            }

            if (!DateTime.TryParse(req.FechaComprobante, out var fechaComp))
            {
                throw new ArgumentException("Fecha de comprobante inválida.");
            }

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
            item.ComprobanteUrl = req.ComprobanteUrl;
            item.UpdatedAt = DateTime.UtcNow;

            item.CajaChica.UpdatedAt = DateTime.UtcNow;
            await _repo.SaveAsync();

            return MapToDetalleDto(item);
        }

        public async Task DeleteItemAsync(long itemId)
        {
            var item = await _repo.GetDetalleByIdWithCabeceraAsync(itemId);
            if (item == null || item.CajaChica == null)
            {
                throw new KeyNotFoundException("Ítem no encontrado.");
            }

            if (item.CajaChica.Estado != "Abierta")
            {
                throw new InvalidOperationException("La Caja Chica está cerrada.");
            }

            if (item.EstadoVuelto == "Devuelto")
            {
                throw new InvalidOperationException("No se puede eliminar un ítem cuyo vuelto ya fue devuelto y conciliado.");
            }

            _repo.RemoveDetalle(item);
            item.CajaChica.UpdatedAt = DateTime.UtcNow;
            await _repo.SaveAsync();
        }

        public async Task<CajaChicaCabeceraDto> CloseCajaChicaAsync(long id)
        {
            var cabecera = await _repo.GetByIdAsync(id);
            if (cabecera == null)
            {
                throw new KeyNotFoundException("Caja Chica no encontrada.");
            }

            if (cabecera.Estado != "Abierta")
            {
                throw new InvalidOperationException("La Caja Chica ya está cerrada.");
            }

            cabecera.Estado = "Cerrada";
            cabecera.FechaCierre = DateTime.Today;
            cabecera.UpdatedAt = DateTime.UtcNow;

            await _repo.SaveAsync();

            return MapToCabeceraDto(cabecera);
        }

        public async Task<(CajaChicaCabeceraDto cajaReembolsada, CajaChicaCabeceraDto nuevaCajaAbierta)> ReimburseCajaChicaAsync(long id, ReimburseRequest req)
        {
            var cabecera = await _repo.GetByIdWithDetallesAsync(id);
            if (cabecera == null)
            {
                throw new KeyNotFoundException("Caja Chica no encontrada.");
            }

            if (cabecera.Estado != "Cerrada")
            {
                throw new InvalidOperationException("Solo se pueden reembolsar cajas chicas en estado 'Cerrada'.");
            }

            var totalGastado = cabecera.Detalles.Sum(d => d.Total);
            var saldoRestante = cabecera.FondoInicial - totalGastado;

            var montoEsperado = totalGastado;
            var montoRecarga = req.MontoRecarga <= 0 ? montoEsperado : req.MontoRecarga;

            cabecera.Estado = "Reembolsada";
            cabecera.UpdatedAt = DateTime.UtcNow;

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
            while (await _repo.AnyCajaChicaByNroAsync(nuevoNro))
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
                FondoInicial = saldoRestante + montoRecarga,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(nuevaCaja);
            await _repo.SaveAsync();

            return (MapToCabeceraDto(cabecera), MapToCabeceraDto(nuevaCaja));
        }

        public async Task<CajaChicaCabecera?> GetRawModelForExportAsync(long id)
        {
            return await _repo.GetByIdWithDetallesAsync(id);
        }

        private CajaChicaCabeceraDto MapToCabeceraDto(CajaChicaCabecera c)
        {
            return new CajaChicaCabeceraDto
            {
                Id = c.Id,
                SucursalId = c.SucursalId,
                CodigoSucursal = c.CodigoSucursal,
                NroCajaChica = c.NroCajaChica,
                CustodioUsuarioId = c.CustodioUsuarioId,
                CustodioNombre = c.CustodioNombre,
                FechaCreacion = c.FechaCreacion,
                FechaCierre = c.FechaCierre,
                Estado = c.Estado,
                FondoInicial = c.FondoInicial,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                Detalles = c.Detalles.Select(MapToDetalleDto).ToList()
            };
        }

        private CajaChicaDetalleDto MapToDetalleDto(CajaChicaDetalle d)
        {
            return new CajaChicaDetalleDto
            {
                Id = d.Id,
                CajaChicaId = d.CajaChicaId,
                FechaComprobante = d.FechaComprobante,
                NroComprobante = d.NroComprobante,
                Proveedor = d.Proveedor,
                Descripcion = d.Descripcion,
                TipoGasto = d.TipoGasto,
                SubtotalSinIva = d.SubtotalSinIva,
                SubtotalConIva = d.SubtotalConIva,
                Iva = d.Iva,
                Total = d.Total,
                ValorEntregado = d.ValorEntregado,
                UsuarioBeneficiado = d.UsuarioBeneficiado,
                VueltoEsperado = d.VueltoEsperado,
                EstadoVuelto = d.EstadoVuelto,
                ComprobanteUrl = d.ComprobanteUrl,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            };
        }
    }
}
