using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NovitecContabilidad.Models
{
    [Table("caja_general_arqueo")]
    public class CajaGeneralArqueo
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("sucursal_id")]
        public int SucursalId { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("codigo_sucursal")]
        public string CodigoSucursal { get; set; } = string.Empty;

        [Column("fecha")]
        public DateTime Fecha { get; set; }

        [Column("monto_sistema", TypeName = "decimal(18,2)")]
        public decimal MontoSistema { get; set; }

        [Column("monto_fisico", TypeName = "decimal(18,2)")]
        public decimal MontoFisico { get; set; }

        [Column("diferencia", TypeName = "decimal(18,2)")]
        public decimal Diferencia { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("tipo_diferencia")]
        public string TipoDiferencia { get; set; } = "Cuadre Exacto";

        [Column("observaciones", TypeName = "text")]
        public string? Observaciones { get; set; }

        [MaxLength(500)]
        [Column("comprobante_deposito_url")]
        public string? ComprobanteDepositoUrl { get; set; }

        [MaxLength(100)]
        [Column("nro_comprobante_deposito")]
        public string? NroComprobanteDeposito { get; set; }

        [Column("usuario_id")]
        public int UsuarioId { get; set; }

        [Required]
        [MaxLength(150)]
        [Column("usuario_nombre")]
        public string UsuarioNombre { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        [Column("estado")]
        public string Estado { get; set; } = "Pendiente Deposito";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
