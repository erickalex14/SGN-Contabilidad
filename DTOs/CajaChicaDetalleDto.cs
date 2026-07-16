using System;

namespace NovitecContabilidad.DTOs
{
    public class CajaChicaDetalleDto
    {
        public long Id { get; set; }
        public long CajaChicaId { get; set; }
        public DateTime FechaComprobante { get; set; }
        public string NroComprobante { get; set; } = string.Empty;
        public string? Proveedor { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string TipoGasto { get; set; } = string.Empty;
        public decimal SubtotalSinIva { get; set; }
        public decimal SubtotalConIva { get; set; }
        public decimal Iva { get; set; }
        public decimal Total { get; set; }
        public decimal ValorEntregado { get; set; }
        public string? UsuarioBeneficiado { get; set; }
        public decimal VueltoEsperado { get; set; }
        public string EstadoVuelto { get; set; } = "No Aplica";
        public string? ComprobanteUrl { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
