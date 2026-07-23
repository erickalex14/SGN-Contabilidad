using System;

namespace NovitecContabilidad.DTOs
{
    public class OpenCajaRequest
    {
        public int SucursalId { get; set; }
        public string CodigoSucursal { get; set; } = string.Empty;
        public decimal FondoInicial { get; set; } = 1000.00m;
        public int CustodioUsuarioId { get; set; }
        public string CustodioNombre { get; set; } = string.Empty;
        public string? FechaCreacion { get; set; }
    }

    public class AddItemRequest
    {
        public string FechaComprobante { get; set; } = string.Empty;
        public string NroComprobante { get; set; } = string.Empty;
        public string? Proveedor { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string TipoGasto { get; set; } = string.Empty;
        public decimal SubtotalSinIva { get; set; }
        public decimal SubtotalConIva { get; set; }
        public decimal MontoRetencion { get; set; }
        public string? NroRetencion { get; set; }
        public decimal ValorEntregado { get; set; }
        public string? UsuarioBeneficiado { get; set; }
        public string EstadoVuelto { get; set; } = "No Aplica";
        public string? ComprobanteUrl { get; set; }
    }

    public class ReimburseRequest
    {
        public decimal MontoRecarga { get; set; }
    }

    public class UpdateComprobanteRequest
    {
        public string? ComprobanteUrl { get; set; }
    }
}
