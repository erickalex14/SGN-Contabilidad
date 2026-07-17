using System.Collections.Generic;
using System.Threading.Tasks;
using NovitecContabilidad.DTOs;

namespace NovitecContabilidad.Services
{
    public interface ICajaChicaService
    {
        Task<List<CajaChicaCabeceraDto>> GetCajaChicasBySucursalAsync(int sucursalId, int userId, bool isSuperAdmin);
        Task<CajaChicaCabeceraDto?> GetCajaChicaByIdAsync(long id);
        Task<string> GetSucursalNameAsync(int sucursalId);
        Task<CajaChicaCabeceraDto> OpenCajaChicaAsync(OpenCajaRequest req, int defaultUserId, string defaultUserName);
        Task<CajaChicaDetalleDto> AddItemAsync(long id, AddItemRequest req);
        Task<CajaChicaDetalleDto> UpdateItemAsync(long itemId, AddItemRequest req);
        Task DeleteItemAsync(long itemId);
        Task<CajaChicaCabeceraDto> CloseCajaChicaAsync(long id);
        Task<(CajaChicaCabeceraDto cajaReembolsada, CajaChicaCabeceraDto nuevaCajaAbierta)> ReimburseCajaChicaAsync(long id, ReimburseRequest req);
        Task<Models.CajaChicaCabecera?> GetRawModelForExportAsync(long id);
        Task<CajaChicaDetalleDto> UpdateComprobanteUrlAsync(long itemId, string? comprobanteUrl);
    }
}
