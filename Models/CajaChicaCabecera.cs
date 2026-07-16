using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NovitecContabilidad.Models
{
    [Table("caja_chica_cabecera")]
    public class CajaChicaCabecera
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("sucursal_id")]
        public int SucursalId { get; set; }

        [Required]
        [StringLength(20)]
        [Column("codigo_sucursal")]
        public string CodigoSucursal { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Column("nro_caja_chica")]
        public string NroCajaChica { get; set; } = string.Empty;

        [Column("custodio_usuario_id")]
        public int CustodioUsuarioId { get; set; }

        [Required]
        [StringLength(150)]
        [Column("custodio_nombre")]
        public string CustodioNombre { get; set; } = string.Empty;

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; }

        [Column("fecha_cierre")]
        public DateTime? FechaCierre { get; set; }

        [Required]
        [StringLength(20)]
        [Column("estado")]
        public string Estado { get; set; } = "Abierta"; // Abierta, Cerrada, Reembolsada

        [Column("fondo_inicial", TypeName = "decimal(10,2)")]
        public decimal FondoInicial { get; set; } = 1000.00m;

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<CajaChicaDetalle> Detalles { get; set; } = new List<CajaChicaDetalle>();
    }
}
