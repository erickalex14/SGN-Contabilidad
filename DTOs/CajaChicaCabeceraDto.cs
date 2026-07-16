using System;
using System.Collections.Generic;

namespace NovitecContabilidad.DTOs
{
    public class CajaChicaCabeceraDto
    {
        public long Id { get; set; }
        public int SucursalId { get; set; }
        public string CodigoSucursal { get; set; } = string.Empty;
        public string NroCajaChica { get; set; } = string.Empty;
        public int CustodioUsuarioId { get; set; }
        public string CustodioNombre { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaCierre { get; set; }
        public string Estado { get; set; } = "Abierta";
        public decimal FondoInicial { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<CajaChicaDetalleDto> Detalles { get; set; } = new List<CajaChicaDetalleDto>();
    }
}
