using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace NovitecContabilidad.Models
{
    [Table("caja_chica_detalle")]
    public class CajaChicaDetalle
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("caja_chica_id")]
        public long CajaChicaId { get; set; }

        [Column("fecha_comprobante")]
        public DateTime FechaComprobante { get; set; }

        [Required]
        [StringLength(50)]
        [Column("nro_comprobante")]
        public string NroComprobante { get; set; } = string.Empty;

        [StringLength(150)]
        [Column("proveedor")]
        public string? Proveedor { get; set; }

        [Required]
        [Column("descripcion")]
        public string Descripcion { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Column("tipo_gasto")]
        public string TipoGasto { get; set; } = string.Empty;

        [Column("subtotal_sin_iva", TypeName = "decimal(10,2)")]
        public decimal SubtotalSinIva { get; set; } = 0.00m;

        [Column("subtotal_con_iva", TypeName = "decimal(10,2)")]
        public decimal SubtotalConIva { get; set; } = 0.00m;

        [Column("iva", TypeName = "decimal(10,2)")]
        public decimal Iva { get; set; } = 0.00m;

        [Column("total", TypeName = "decimal(10,2)")]
        public decimal Total { get; set; } = 0.00m;

        [Column("valor_entregado", TypeName = "decimal(10,2)")]
        public decimal ValorEntregado { get; set; } = 0.00m;

        [StringLength(150)]
        [Column("usuario_beneficiado")]
        public string? UsuarioBeneficiado { get; set; }

        [Column("vuelto_esperado", TypeName = "decimal(10,2)")]
        public decimal VueltoEsperado { get; set; } = 0.00m;

        [Required]
        [StringLength(20)]
        [Column("estado_vuelto")]
        public string EstadoVuelto { get; set; } = "No Aplica"; // Pendiente, Devuelto, No Aplica

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [JsonIgnore]
        [ForeignKey("CajaChicaId")]
        public virtual CajaChicaCabecera? CajaChica { get; set; }
    }
}
