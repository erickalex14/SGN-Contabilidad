using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NovitecContabilidad.DTOs;
using NovitecContabilidad.Models;
using NovitecContabilidad.Repositories;

namespace NovitecContabilidad.Services
{
    public class RecuentoB2BService : IRecuentoB2BService
    {
        private readonly IRecuentoB2BRepository _repo;

        public RecuentoB2BService(IRecuentoB2BRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<RecuentoB2BLote>> GetLotesAsync(string? empresaNombre = null)
        {
            return await _repo.GetAllLotesAsync(empresaNombre);
        }

        public async Task<RecuentoB2BLote> ProcesarRecuentoAsync(ProcesarRecuentoB2BRequest req, int usuarioId, string usuarioNombre)
        {
            if (req.Items == null || !req.Items.Any())
            {
                throw new InvalidOperationException("Debe seleccionar al menos una orden para facturar.");
            }

            string nroLote = $"LOTE-B2B-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}";
            decimal subtotal = req.Items.Sum(i => i.ValorTotal);

            var lote = new RecuentoB2BLote
            {
                NroLote = nroLote,
                EmpresaNombre = req.EmpresaNombre,
                TotalOrdenes = req.Items.Count,
                Subtotal = subtotal,
                MontoNetoBanco = req.MontoNetoBanco,
                MontoRetencionRenta = req.MontoRetencionRenta,
                MontoRetencionIva = req.MontoRetencionIva,
                NroRetencion = req.NroRetencion,
                NroComprobantePago = req.NroComprobantePago,
                BancoDestino = req.BancoDestino,
                Estado = "Cobrado",
                UsuarioId = usuarioId,
                UsuarioNombre = usuarioNombre,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            foreach (var item in req.Items)
            {
                lote.Items.Add(new RecuentoB2BItem
                {
                    OrdenId = item.OrdenId,
                    TipoOrden = item.TipoOrden,
                    NroOrden = item.NroOrden,
                    Subtipo = item.Subtipo,
                    TecnicoNombre = item.TecnicoNombre,
                    CantidadTecnicos = item.CantidadTecnicos > 0 ? item.CantidadTecnicos : 1,
                    HorasTrabajadas = item.HorasTrabajadas,
                    TarifaAplicada = item.TarifaAplicada,
                    ValorTotal = item.ValorTotal,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _repo.AddLoteAsync(lote);
            await _repo.SaveAsync();

            return lote;
        }
    }
}
