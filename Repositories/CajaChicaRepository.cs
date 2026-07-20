using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NovitecContabilidad.Data;
using NovitecContabilidad.Models;

namespace NovitecContabilidad.Repositories
{
    public class CajaChicaRepository : ICajaChicaRepository
    {
        private readonly AccountingDbContext _context;

        public CajaChicaRepository(AccountingDbContext context)
        {
            _context = context;
        }

        public async Task<CajaChicaCabecera?> GetByIdAsync(long id)
        {
            return await _context.CajasChicas.FindAsync(id);
        }

        public async Task<CajaChicaCabecera?> GetByIdWithDetallesAsync(long id)
        {
            return await _context.CajasChicas
                .Include(c => c.Detalles)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<CajaChicaCabecera>> GetBySucursalAsync(int sucursalId, int userId, bool isSuperAdmin)
        {
            var query = _context.CajasChicas.AsQueryable();

            if (!isSuperAdmin)
            {
                query = query.Where(c => c.CustodioUsuarioId == userId);
            }

            return await query
                .Include(c => c.Detalles)
                .OrderByDescending(c => c.FechaCreacion)
                .ThenByDescending(c => c.Id)
                .ToListAsync();
        }

        public async Task<bool> AnyCajaChicaAbiertaAsync(int sucursalId)
        {
            return await _context.CajasChicas
                .AnyAsync(c => c.SucursalId == sucursalId && c.Estado == "Abierta");
        }

        public async Task<bool> AnyCajaChicaByNroAsync(string nroCajaChica)
        {
            return await _context.CajasChicas.AnyAsync(c => c.NroCajaChica == nroCajaChica);
        }

        public async Task AddAsync(CajaChicaCabecera cabecera)
        {
            await _context.CajasChicas.AddAsync(cabecera);
        }

        public async Task<CajaChicaDetalle?> GetDetalleByIdAsync(long itemId)
        {
            return await _context.Detalles.FindAsync(itemId);
        }

        public async Task<CajaChicaDetalle?> GetDetalleByIdWithCabeceraAsync(long itemId)
        {
            return await _context.Detalles
                .Include(i => i.CajaChica)
                .FirstOrDefaultAsync(i => i.Id == itemId);
        }

        public void AddDetalle(CajaChicaDetalle detalle)
        {
            _context.Detalles.Add(detalle);
        }

        public void RemoveDetalle(CajaChicaDetalle detalle)
        {
            _context.Detalles.Remove(detalle);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public DbConnection GetDbConnection()
        {
            return _context.Database.GetDbConnection();
        }
    }
}
