using System.Collections.Generic;
using System.Threading.Tasks;
using NovitecContabilidad.DTOs;
using NovitecContabilidad.Models;

namespace NovitecContabilidad.Services
{
    public interface IRecuentoB2BService
    {
        Task<IEnumerable<RecuentoB2BLote>> GetLotesAsync(string? empresaNombre = null);
        Task<RecuentoB2BLote> ProcesarRecuentoAsync(ProcesarRecuentoB2BRequest req, int usuarioId, string usuarioNombre);
    }
}
