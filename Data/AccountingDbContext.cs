using Microsoft.EntityFrameworkCore;
using NovitecContabilidad.Models;

namespace NovitecContabilidad.Data
{
    public class AccountingDbContext : DbContext
    {
        public AccountingDbContext(DbContextOptions<AccountingDbContext> options)
            : base(options)
        {
        }

        public DbSet<CajaChicaCabecera> CajasChicas { get; set; } = null!;
        public DbSet<CajaChicaDetalle> Detalles { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CajaChicaCabecera>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.NroCajaChica).IsUnique();
                entity.HasIndex(e => e.SucursalId);
                entity.HasIndex(e => e.Estado);

                entity.HasMany(d => d.Detalles)
                      .WithOne(p => p.CajaChica)
                      .HasForeignKey(d => d.CajaChicaId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CajaChicaDetalle>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.CajaChicaId);
            });
        }
    }
}
