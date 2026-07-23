using System.Collections.Generic;
using System.Threading.Tasks;
using NovitecContabilidad.DTOs;
using NovitecContabilidad.Models;

namespace NovitecContabilidad.Services
{
    public interface ICajaGeneralService
    {
        Task<IEnumerable<CajaGeneralArqueo>> GetArqueosAsync(int? sucursalId = null);
        Task<CajaGeneralArqueo> RegistrarArqueoAsync(RegistrarArqueoRequest req, int usuarioId, string usuarioNombre);
        Task<CajaGeneralArqueo> RegistrarDepositoAsync(RegistrarDepositoRequest req);
        Task<IEnumerable<CajaGeneralCobro>> GetCobrosAsync(int? sucursalId = null);
        Task<CajaGeneralCobro> RegistrarCobroAsync(RegistrarCobroRequest req, int usuarioId, string usuarioNombre);
    }
}
