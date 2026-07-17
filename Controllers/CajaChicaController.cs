using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovitecContabilidad.DTOs;
using NovitecContabilidad.Services;

namespace NovitecContabilidad.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CajaChicaController : ControllerBase
    {
        private readonly ICajaChicaService _cajaService;
        private readonly ExcelExportService _excelService;

        public CajaChicaController(ICajaChicaService cajaService, ExcelExportService excelService)
        {
            _cajaService = cajaService;
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

        private int GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : 0;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var sucursalId = GetUserSucursalId();
            var userId = GetUserId();
            var isSa = IsSuperAdmin();
            var list = await _cajaService.GetCajaChicasBySucursalAsync(sucursalId, userId, isSa);
            return Ok(new { ok = true, data = list });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var dto = await _cajaService.GetCajaChicaByIdAsync(id);
            if (dto == null)
            {
                return NotFound(new { ok = false, error = "Caja Chica no encontrada." });
            }

            if (!IsSuperAdmin() && dto.SucursalId != GetUserSucursalId())
            {
                return Forbid();
            }

            return Ok(new { ok = true, data = dto });
        }

        [HttpPost]
        public async Task<IActionResult> Open([FromBody] OpenCajaRequest req)
        {
            if (!IsSuperAdmin() && req.SucursalId != GetUserSucursalId())
            {
                return Forbid();
            }

            try
            {
                int defaultUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                string defaultUserName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Custodio";

                var result = await _cajaService.OpenCajaChicaAsync(req, defaultUserId, defaultUserName);
                return Ok(new { ok = true, message = "Caja Chica abierta correctamente.", data = result });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { ok = false, error = ex.Message });
            }
        }

        [HttpPost("{id}/items")]
        public async Task<IActionResult> AddItem(long id, [FromBody] AddItemRequest req)
        {
            try
            {
                var dto = await _cajaService.GetCajaChicaByIdAsync(id);
                if (dto == null)
                {
                    return NotFound(new { ok = false, error = "Caja Chica no encontrada." });
                }

                if (!IsSuperAdmin() && dto.SucursalId != GetUserSucursalId())
                {
                    return Forbid();
                }

                var item = await _cajaService.AddItemAsync(id, req);
                return Ok(new { ok = true, message = "Ítem registrado con éxito.", data = item });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { ok = false, error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { ok = false, error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { ok = false, error = ex.Message });
            }
        }

        [HttpPut("items/{itemId}")]
        public async Task<IActionResult> UpdateItem(long itemId, [FromBody] AddItemRequest req)
        {
            try
            {
                var itemDto = await _cajaService.UpdateItemAsync(itemId, req);
                return Ok(new { ok = true, message = "Ítem actualizado con éxito.", data = itemDto });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { ok = false, error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { ok = false, error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { ok = false, error = ex.Message });
            }
        }

        [HttpPatch("items/{itemId}/comprobante")]
        public async Task<IActionResult> UpdateComprobante(long itemId, [FromBody] UpdateComprobanteRequest req)
        {
            try
            {
                var dto = await _cajaService.UpdateComprobanteUrlAsync(itemId, req.ComprobanteUrl);
                return Ok(new { ok = true, message = "Comprobante digital actualizado con éxito.", data = dto });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { ok = false, error = ex.Message });
            }
        }

        [HttpDelete("items/{itemId}")]
        public async Task<IActionResult> DeleteItem(long itemId)
        {
            try
            {
                await _cajaService.DeleteItemAsync(itemId);
                return Ok(new { ok = true, message = "Ítem eliminado correctamente." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { ok = false, error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { ok = false, error = ex.Message });
            }
        }

        [HttpPost("{id}/close")]
        public async Task<IActionResult> Close(long id)
        {
            try
            {
                var dto = await _cajaService.GetCajaChicaByIdAsync(id);
                if (dto == null)
                {
                    return NotFound(new { ok = false, error = "Caja Chica no encontrada." });
                }

                if (!IsSuperAdmin() && dto.SucursalId != GetUserSucursalId())
                {
                    return Forbid();
                }

                var result = await _cajaService.CloseCajaChicaAsync(id);
                return Ok(new { ok = true, message = "Caja Chica cerrada correctamente.", data = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { ok = false, error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { ok = false, error = ex.Message });
            }
        }

        [HttpPost("{id}/reimburse")]
        public async Task<IActionResult> Reimburse(long id, [FromBody] ReimburseRequest req)
        {
            if (!IsSuperAdmin())
            {
                return Forbid();
            }

            try
            {
                var (cajaReembolsada, nuevaCajaAbierta) = await _cajaService.ReimburseCajaChicaAsync(id, req);
                return Ok(new { 
                    ok = true, 
                    message = "Caja Chica reembolsada y nuevo periodo abierto con éxito.", 
                    cajaReembolsada, 
                    nuevaCajaAbierta 
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { ok = false, error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { ok = false, error = ex.Message });
            }
        }

        [HttpGet("{id}/export")]
        public async Task<IActionResult> ExportExcel(long id)
        {
            var dto = await _cajaService.GetCajaChicaByIdAsync(id);
            if (dto == null)
            {
                return NotFound(new { ok = false, error = "Caja Chica no encontrada." });
            }

            if (!IsSuperAdmin() && dto.SucursalId != GetUserSucursalId())
            {
                return Forbid();
            }

            try
            {
                var rawCabecera = await _cajaService.GetRawModelForExportAsync(id);
                if (rawCabecera == null)
                {
                    return NotFound(new { ok = false, error = "Caja Chica no encontrada para exportar." });
                }

                string nombreSucursal = await _cajaService.GetSucursalNameAsync(rawCabecera.SucursalId);

                var fileBytes = _excelService.ExportCajaChica(rawCabecera, nombreSucursal);
                var fileName = $"Informe_Caja_Chica_{rawCabecera.NroCajaChica}.xlsx";

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { ok = false, error = $"Error al generar Excel: {ex.Message}" });
            }
        }
    }
}
