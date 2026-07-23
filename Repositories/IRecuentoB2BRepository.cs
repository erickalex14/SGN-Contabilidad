using System.Collections.Generic;
using System.Threading.Tasks;
using NovitecContabilidad.Models;

namespace NovitecContabilidad.Repositories
{
    public interface IRecuentoB2BRepository
    {
        Task<IEnumerable<RecuentoB2BLote>> GetAllLotesAsync(string? empresaNombre = null);
        Task<RecuentoB2BLote?> GetLoteByIdAsync(long id);
        Task AddLoteAsync(RecuentoB2BLote lote);
        Task SaveAsync();
    }
}
