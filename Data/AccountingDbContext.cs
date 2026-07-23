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
        public DbSet<CajaGeneralArqueo> ArqueosCajaGeneral { get; set; } = null!;
        public DbSet<CajaGeneralCobro> CobrosCajaGeneral { get; set; } = null!;
        public DbSet<RecuentoB2BLote> RecuentosLotes { get; set; } = null!;
        public DbSet<RecuentoB2BItem> RecuentosItems { get; set; } = null!;

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

            modelBuilder.Entity<CajaGeneralArqueo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.SucursalId);
                entity.HasIndex(e => e.Fecha);
            });

            modelBuilder.Entity<CajaGeneralCobro>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.SucursalId);
                entity.HasIndex(e => e.NroOrden);
                entity.HasIndex(e => e.FechaCobro);
            });

            modelBuilder.Entity<RecuentoB2BLote>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.NroLote).IsUnique();
                entity.HasIndex(e => e.EmpresaNombre);

                entity.HasMany(d => d.Items)
                      .WithOne(p => p.Lote)
                      .HasForeignKey(d => d.LoteId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<RecuentoB2BItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.LoteId);
                entity.HasIndex(e => e.OrdenId);
            });
        }
    }
}
