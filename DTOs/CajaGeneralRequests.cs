using System;

namespace NovitecContabilidad.DTOs
{
    public class RegistrarArqueoRequest
    {
        public int SucursalId { get; set; }
        public string CodigoSucursal { get; set; } = string.Empty;
        public decimal MontoSistema { get; set; }
        public decimal MontoFisico { get; set; }
        public string? Observaciones { get; set; }
    }

    public class RegistrarDepositoRequest
    {
        public long ArqueoId { get; set; }
        public string ComprobanteDepositoUrl { get; set; } = string.Empty;
        public string NroComprobanteDeposito { get; set; } = string.Empty;
    }

    public class RegistrarCobroRequest
    {
        public long? OrdenId { get; set; }
        public string NroOrden { get; set; } = string.Empty;
        public string? ClienteNombre { get; set; }
        public string? EquipoInfo { get; set; }
        public decimal MontoCobrado { get; set; }
        public string MetodoPago { get; set; } = "Efectivo";
        public string DestinoCuenta { get; set; } = "Caja General";
        public int SucursalId { get; set; }
        public string? Observaciones { get; set; }
    }
}
