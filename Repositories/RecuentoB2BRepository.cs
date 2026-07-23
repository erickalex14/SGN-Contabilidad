using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NovitecContabilidad.Data;
using NovitecContabilidad.Models;

namespace NovitecContabilidad.Repositories
{
    public class RecuentoB2BRepository : IRecuentoB2BRepository
    {
        private readonly AccountingDbContext _context;

        public RecuentoB2BRepository(AccountingDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RecuentoB2BLote>> GetAllLotesAsync(string? empresaNombre = null)
        {
            var query = _context.RecuentosLotes.Include(l => l.Items).AsNoTracking();
            if (!string.IsNullOrEmpty(empresaNombre))
            {
                query = query.Where(l => l.EmpresaNombre.ToLower() == empresaNombre.ToLower());
            }
            return await query.OrderByDescending(l => l.CreatedAt).ToListAsync();
        }

        public async Task<RecuentoB2BLote?> GetLoteByIdAsync(long id)
        {
            return await _context.RecuentosLotes.Include(l => l.Items).FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task AddLoteAsync(RecuentoB2BLote lote)
        {
            await _context.RecuentosLotes.AddAsync(lote);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
