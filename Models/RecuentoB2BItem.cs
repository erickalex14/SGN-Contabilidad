using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace NovitecContabilidad.Models
{
    [Table("recuento_b2b_item")]
    public class RecuentoB2BItem
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("lote_id")]
        public long LoteId { get; set; }

        [Column("orden_id")]
        public long OrdenId { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("tipo_orden")]
        public string TipoOrden { get; set; } = "empresa";

        [Required]
        [MaxLength(50)]
        [Column("nro_orden")]
        public string NroOrden { get; set; } = string.Empty;

        [MaxLength(50)]
        [Column("subtipo")]
        public string? Subtipo { get; set; }

        [MaxLength(150)]
        [Column("tecnico_nombre")]
        public string? TecnicoNombre { get; set; }

        [Column("cantidad_tecnicos")]
        public int CantidadTecnicos { get; set; } = 1;

        [Column("horas_trabajadas", TypeName = "decimal(18,2)")]
        public decimal HorasTrabajadas { get; set; }

        [Column("tarifa_aplicada", TypeName = "decimal(18,2)")]
        public decimal TarifaAplicada { get; set; }

        [Column("valor_total", TypeName = "decimal(18,2)")]
        public decimal ValorTotal { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        [ForeignKey("LoteId")]
        public RecuentoB2BLote? Lote { get; set; }
    }
}
