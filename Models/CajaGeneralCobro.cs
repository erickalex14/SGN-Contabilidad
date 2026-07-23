using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NovitecContabilidad.Models
{
    [Table("caja_general_cobros")]
    public class CajaGeneralCobro
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("orden_id")]
        public long? OrdenId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("nro_orden")]
        public string NroOrden { get; set; } = string.Empty;

        [MaxLength(255)]
        [Column("cliente_nombre")]
        public string? ClienteNombre { get; set; }

        [MaxLength(255)]
        [Column("equipo_info")]
        public string? EquipoInfo { get; set; }

        [Column("monto_cobrado", TypeName = "decimal(18,2)")]
        public decimal MontoCobrado { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("metodo_pago")]
        public string MetodoPago { get; set; } = "Efectivo";

        [Required]
        [MaxLength(50)]
        [Column("destino_cuenta")]
        public string DestinoCuenta { get; set; } = "Caja General";

        [Column("sucursal_id")]
        public int SucursalId { get; set; }

        [Column("usuario_id")]
        public int UsuarioId { get; set; }

        [Required]
        [MaxLength(150)]
        [Column("usuario_nombre")]
        public string UsuarioNombre { get; set; } = string.Empty;

        [Column("observaciones", TypeName = "text")]
        public string? Observaciones { get; set; }

        [Column("fecha_cobro")]
        public DateTime FechaCobro { get; set; } = DateTime.UtcNow;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
