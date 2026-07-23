using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NovitecContabilidad.DTOs;
using NovitecContabilidad.Models;
using NovitecContabilidad.Repositories;

namespace NovitecContabilidad.Services
{
    public class CajaGeneralService : ICajaGeneralService
    {
        private readonly ICajaGeneralRepository _repo;

        public CajaGeneralService(ICajaGeneralRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<CajaGeneralArqueo>> GetArqueosAsync(int? sucursalId = null)
        {
            return await _repo.GetAllAsync(sucursalId);
        }

        public async Task<CajaGeneralArqueo> RegistrarArqueoAsync(RegistrarArqueoRequest req, int usuarioId, string usuarioNombre)
        {
            decimal diferencia = req.MontoFisico - req.MontoSistema;
            string tipoDiferencia = "Cuadre Exacto";
            if (diferencia < 0) tipoDiferencia = "Faltante";
            else if (diferencia > 0) tipoDiferencia = "Sobrante";

            var arqueo = new CajaGeneralArqueo
            {
                SucursalId = req.SucursalId,
                CodigoSucursal = req.CodigoSucursal,
                Fecha = DateTime.Today,
                MontoSistema = req.MontoSistema,
                MontoFisico = req.MontoFisico,
                Diferencia = diferencia,
                TipoDiferencia = tipoDiferencia,
                Observaciones = req.Observaciones,
                UsuarioId = usuarioId,
                UsuarioNombre = usuarioNombre,
                Estado = "Pendiente Deposito",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(arqueo);
            await _repo.SaveAsync();

            return arqueo;
        }

        public async Task<CajaGeneralArqueo> RegistrarDepositoAsync(RegistrarDepositoRequest req)
        {
            var arqueo = await _repo.GetByIdAsync(req.ArqueoId);
            if (arqueo == null)
            {
                throw new InvalidOperationException("El registro de arqueo especificado no existe.");
            }

            arqueo.ComprobanteDepositoUrl = req.ComprobanteDepositoUrl;
            arqueo.NroComprobanteDeposito = req.NroComprobanteDeposito;
            arqueo.Estado = "Depositado";
            arqueo.UpdatedAt = DateTime.UtcNow;

            await _repo.SaveAsync();

            return arqueo;
        }

        public async Task<IEnumerable<CajaGeneralCobro>> GetCobrosAsync(int? sucursalId = null)
        {
            return await _repo.GetCobrosAsync(sucursalId);
        }

        public async Task<CajaGeneralCobro> RegistrarCobroAsync(RegistrarCobroRequest req, int usuarioId, string usuarioNombre)
        {
            string destinoCuenta = (req.MetodoPago == "Efectivo") ? "Caja General" : "Bancos";

            var cobro = new CajaGeneralCobro
            {
                OrdenId = req.OrdenId,
                NroOrden = req.NroOrden,
                ClienteNombre = req.ClienteNombre,
                EquipoInfo = req.EquipoInfo,
                MontoCobrado = req.MontoCobrado,
                MetodoPago = req.MetodoPago,
                DestinoCuenta = destinoCuenta,
                SucursalId = req.SucursalId,
                UsuarioId = usuarioId,
                UsuarioNombre = usuarioNombre,
                Observaciones = req.Observaciones,
                FechaCobro = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _repo.AddCobroAsync(cobro);
            await _repo.SaveAsync();

            return cobro;
        }
    }
}
