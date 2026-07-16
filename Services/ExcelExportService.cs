using System;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using NovitecContabilidad.Models;

namespace NovitecContabilidad.Services
{
    public class ExcelExportService
    {
        private readonly string _templatePath;

        public ExcelExportService(string templatePath)
        {
            _templatePath = templatePath;
        }

        public byte[] ExportCajaChica(CajaChicaCabecera cabecera, string nombreSucursal)
        {
            if (!File.Exists(_templatePath))
            {
                throw new FileNotFoundException($"La plantilla de Excel no existe en {_templatePath}");
            }

            using (var workbook = new XLWorkbook(_templatePath))
            {
                var ws = workbook.Worksheet("INFORME_CAJA_CHICA");

                // Escribir cabecera
                ws.Cell("C3").Value = cabecera.CodigoSucursal;
                ws.Cell("E3").Value = cabecera.CustodioNombre;
                ws.Cell("I3").Value = cabecera.NroCajaChica;
                ws.Cell("C4").Value = cabecera.CodigoSucursal + " " + nombreSucursal;
                ws.Cell("C5").Value = nombreSucursal;
                ws.Cell("E5").Value = cabecera.FechaCierre?.ToString("dd/MM/yyyy") ?? DateTime.Now.ToString("dd/MM/yyyy");

                var items = cabecera.Detalles.OrderBy(d => d.FechaComprobante).ThenBy(d => d.Id).ToList();
                
                int startRow = 8;
                int maxRow = 38; 

                for (int i = 0; i < items.Count; i++)
                {
                    int currentRow = startRow + i;
                    var item = items[i];

                    // Si excede la fila 38, insertamos una nueva fila copiando el estilo de la anterior
                    if (currentRow > maxRow)
                    {
                        ws.Row(currentRow - 1).InsertRowsBelow(1);
                        maxRow++;
                    }

                    ws.Cell(currentRow, 1).Value = i + 1;
                    ws.Cell(currentRow, 2).Value = item.FechaComprobante.ToString("yyyy-MM-dd");
                    ws.Cell(currentRow, 3).Value = item.NroComprobante;
                    ws.Cell(currentRow, 4).Value = item.Proveedor;
                    ws.Cell(currentRow, 5).Value = item.Descripcion;
                    ws.Cell(currentRow, 6).Value = item.TipoGasto;
                    ws.Cell(currentRow, 7).Value = item.SubtotalSinIva;
                    ws.Cell(currentRow, 8).Value = item.SubtotalConIva;
                    
                    // Fórmulas
                    ws.Cell(currentRow, 9).FormulaA1 = $"IF(H{currentRow}>0,($I$7*H{currentRow}),0)";
                    ws.Cell(currentRow, 10).FormulaA1 = $"G{currentRow}+H{currentRow}+I{currentRow}";

                    // Nuevos campos
                    ws.Cell(currentRow, 11).Value = item.ValorEntregado;
                    ws.Cell(currentRow, 12).Value = item.UsuarioBeneficiado ?? "";
                    ws.Cell(currentRow, 13).FormulaA1 = $"IF(K{currentRow}>J{currentRow},K{currentRow}-J{currentRow},0)";
                    ws.Cell(currentRow, 14).Value = item.EstadoVuelto;
                }

                // Ajustar totales (Fila de total está después de maxRow)
                int totalRow = maxRow + 1;
                ws.Cell(totalRow, 7).FormulaA1 = $"SUM(G8:G{maxRow})";
                ws.Cell(totalRow, 8).FormulaA1 = $"SUM(H8:H{maxRow})";
                ws.Cell(totalRow, 9).FormulaA1 = $"SUM(I8:I{maxRow})";
                ws.Cell(totalRow, 10).FormulaA1 = $"SUM(J8:J{maxRow})";
                ws.Cell(totalRow, 11).FormulaA1 = $"SUM(K8:K{maxRow})";
                ws.Cell(totalRow, 13).FormulaA1 = $"SUM(M8:M{maxRow})";

                using (var ms = new MemoryStream())
                {
                    workbook.SaveAs(ms);
                    return ms.ToArray();
                }
            }
        }
    }
}
