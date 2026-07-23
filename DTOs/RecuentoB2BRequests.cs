using System.Collections.Generic;

namespace NovitecContabilidad.DTOs
{
    public class ItemRecuentoRequest
    {
        public long OrdenId { get; set; }
        public string TipoOrden { get; set; } = "empresa"; // "empresa" or "personal"
        public string NroOrden { get; set; } = string.Empty;
        public string? Subtipo { get; set; }
        public string? TecnicoNombre { get; set; }
        public int CantidadTecnicos { get; set; } = 1;
        public decimal HorasTrabajadas { get; set; }
        public decimal TarifaAplicada { get; set; }
        public decimal ValorTotal { get; set; }
    }

    public class ProcesarRecuentoB2BRequest
    {
        public string EmpresaNombre { get; set; } = string.Empty;
        public decimal MontoNetoBanco { get; set; }
        public decimal MontoRetencionRenta { get; set; }
        public decimal MontoRetencionIva { get; set; }
        public string? NroRetencion { get; set; }
        public string? NroComprobantePago { get; set; }
        public string? BancoDestino { get; set; }
        public List<ItemRecuentoRequest> Items { get; set; } = new List<ItemRecuentoRequest>();
    }
}
