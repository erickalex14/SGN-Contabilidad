using System.Collections.Generic;
using System.Threading.Tasks;
using NovitecContabilidad.Models;

namespace NovitecContabilidad.Repositories
{
    public interface ICajaGeneralRepository
    {
        Task<IEnumerable<CajaGeneralArqueo>> GetAllAsync(int? sucursalId = null);
        Task<CajaGeneralArqueo?> GetByIdAsync(long id);
        Task AddAsync(CajaGeneralArqueo arqueo);
        Task<IEnumerable<CajaGeneralCobro>> GetCobrosAsync(int? sucursalId = null);
        Task AddCobroAsync(CajaGeneralCobro cobro);
        Task SaveAsync();
    }
}
