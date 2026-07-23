using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NovitecContabilidad.Models
{
    [Table("recuento_b2b_lote")]
    public class RecuentoB2BLote
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("nro_lote")]
        public string NroLote { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        [Column("empresa_nombre")]
        public string EmpresaNombre { get; set; } = string.Empty;

        [Column("total_ordenes")]
        public int TotalOrdenes { get; set; }

        [Column("subtotal", TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [Column("monto_neto_banco", TypeName = "decimal(18,2)")]
        public decimal MontoNetoBanco { get; set; }

        [Column("monto_retencion_renta", TypeName = "decimal(18,2)")]
        public decimal MontoRetencionRenta { get; set; }

        [Column("monto_retencion_iva", TypeName = "decimal(18,2)")]
        public decimal MontoRetencionIva { get; set; }

        [MaxLength(100)]
        [Column("nro_retencion")]
        public string? NroRetencion { get; set; }

        [MaxLength(100)]
        [Column("nro_comprobante_pago")]
        public string? NroComprobantePago { get; set; }

        [MaxLength(150)]
        [Column("banco_destino")]
        public string? BancoDestino { get; set; }

        [Required]
        [MaxLength(30)]
        [Column("estado")]
        public string Estado { get; set; } = "Cobrado";

        [Column("usuario_id")]
        public int UsuarioId { get; set; }

        [Required]
        [MaxLength(150)]
        [Column("usuario_nombre")]
        public string UsuarioNombre { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<RecuentoB2BItem> Items { get; set; } = new List<RecuentoB2BItem>();
    }
}
