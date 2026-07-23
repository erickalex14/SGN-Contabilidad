using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NovitecContabilidad.Data;
using NovitecContabilidad.Models;

namespace NovitecContabilidad.Repositories
{
    public class CajaGeneralRepository : ICajaGeneralRepository
    {
        private readonly AccountingDbContext _context;

        public CajaGeneralRepository(AccountingDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CajaGeneralArqueo>> GetAllAsync(int? sucursalId = null)
        {
            var query = _context.ArqueosCajaGeneral.AsNoTracking();
            if (sucursalId.HasValue && sucursalId.Value > 0)
            {
                query = query.Where(a => a.SucursalId == sucursalId.Value);
            }
            return await query.OrderByDescending(a => a.Fecha).ThenByDescending(a => a.Id).ToListAsync();
        }

        public async Task<CajaGeneralArqueo?> GetByIdAsync(long id)
        {
            return await _context.ArqueosCajaGeneral.FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task AddAsync(CajaGeneralArqueo arqueo)
        {
            await _context.ArqueosCajaGeneral.AddAsync(arqueo);
        }

        public async Task<IEnumerable<CajaGeneralCobro>> GetCobrosAsync(int? sucursalId = null)
        {
            var query = _context.CobrosCajaGeneral.AsNoTracking();
            if (sucursalId.HasValue && sucursalId.Value > 0)
            {
                query = query.Where(c => c.SucursalId == sucursalId.Value);
            }
            return await query.OrderByDescending(c => c.FechaCobro).ThenByDescending(c => c.Id).ToListAsync();
        }

        public async Task AddCobroAsync(CajaGeneralCobro cobro)
        {
            await _context.CobrosCajaGeneral.AddAsync(cobro);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
