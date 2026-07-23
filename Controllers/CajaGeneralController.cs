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
    public class CajaGeneralController : ControllerBase
    {
        private readonly ICajaGeneralService _service;

        public CajaGeneralController(ICajaGeneralService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetArqueos([FromQuery] int? sucursalId)
        {
            try
            {
                var list = await _service.GetArqueosAsync(sucursalId);
                return Ok(new { ok = true, data = list });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ok = false, error = ex.Message });
            }
        }

        [HttpPost("arqueo")]
        public async Task<IActionResult> RegistrarArqueo([FromBody] RegistrarArqueoRequest request)
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

                var arqueo = await _service.RegistrarArqueoAsync(request, usuarioId, usuarioNombre);
                return Ok(new { ok = true, mensaje = "Arqueo de caja general registrado correctamente.", data = arqueo });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ok = false, error = ex.Message });
            }
        }

        [HttpPost("deposito")]
        public async Task<IActionResult> RegistrarDeposito([FromBody] RegistrarDepositoRequest request)
        {
            try
            {
                var arqueo = await _service.RegistrarDepositoAsync(request);
                return Ok(new { ok = true, mensaje = "Comprobante de depósito bancario registrado con éxito.", data = arqueo });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ok = false, error = ex.Message });
            }
        }

        [HttpGet("cobros")]
        public async Task<IActionResult> GetCobros([FromQuery] int? sucursalId)
        {
            try
            {
                var list = await _service.GetCobrosAsync(sucursalId);
                return Ok(new { ok = true, data = list });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ok = false, error = ex.Message });
            }
        }

        [HttpPost("cobro")]
        public async Task<IActionResult> RegistrarCobro([FromBody] RegistrarCobroRequest request)
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

                var cobro = await _service.RegistrarCobroAsync(request, usuarioId, usuarioNombre);
                return Ok(new { ok = true, mensaje = "Cobro de orden registrado con éxito.", data = cobro });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ok = false, error = ex.Message });
            }
        }
    }
}
