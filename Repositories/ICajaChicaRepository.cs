using System.Collections.Generic;
using System.Threading.Tasks;
using NovitecContabilidad.Models;

namespace NovitecContabilidad.Repositories
{
    public interface ICajaChicaRepository
    {
        Task<CajaChicaCabecera?> GetByIdAsync(long id);
        Task<CajaChicaCabecera?> GetByIdWithDetallesAsync(long id);
        Task<List<CajaChicaCabecera>> GetBySucursalAsync(int sucursalId, int userId, bool isSuperAdmin);
        Task<bool> AnyCajaChicaAbiertaAsync(int sucursalId);
        Task<bool> AnyCajaChicaByNroAsync(string nroCajaChica);
        Task<int> GetCountBySucursalAsync(int sucursalId);
        Task AddAsync(CajaChicaCabecera cabecera);
        Task<CajaChicaDetalle?> GetDetalleByIdAsync(long itemId);
        Task<CajaChicaDetalle?> GetDetalleByIdWithCabeceraAsync(long itemId);
        void AddDetalle(CajaChicaDetalle detalle);
        void RemoveDetalle(CajaChicaDetalle detalle);
        Task SaveAsync();
        System.Data.Common.DbConnection GetDbConnection();
    }
}
