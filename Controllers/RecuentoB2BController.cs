using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovitecContabilidad.DTOs;
using NovitecContabilidad.Services;

namespace NovitecContabilidad.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RecuentoB2BController : ControllerBase
    {
        private readonly IRecuentoB2BService _service;

        public RecuentoB2BController(IRecuentoB2BService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetLotes([FromQuery] string? empresaNombre)
        {
            try
            {
                var lotes = await _service.GetLotesAsync(empresaNombre);
                return Ok(new { ok = true, data = lotes });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ok = false, error = ex.Message });
            }
        }

        [HttpPost("procesar")]
        public async Task<IActionResult> ProcesarRecuento([FromBody] ProcesarRecuentoB2BRequest request)
        {
            try
            {
                int usuarioId = 1;
                string usuarioNombre = User.Identity?.Name ?? "Usuario Sistema";
                var claimId = User.FindFirst("id")?.Value;
                if (!string.IsNullOrEmpty(claimId) && int.TryParse(claimId, out int parsedId))
                {
                    usuarioId = parsedId;
                }

                var lote = await _service.ProcesarRecuentoAsync(request, usuarioId, usuarioNombre);
                return Ok(new { ok = true, mensaje = "Lote de recuento y facturación B2B procesado con éxito.", data = lote });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ok = false, error = ex.Message });
            }
        }
    }
}
