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

                // 1. Insertar logo de Novitec en A1 si existe
                string directory = Path.GetDirectoryName(_templatePath) ?? "";
                string logoPath = Path.Combine(directory, "logo_novitec.png");
                if (File.Exists(logoPath))
                {
                    try
                    {
                        ws.AddPicture(logoPath)
                          .MoveTo(ws.Cell("A1"))
                          .Scale(0.3);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al insertar logo en Excel: {ex.Message}");
                    }
                }

                // 2. Modificar encabezados de cabecera
                ws.Cell("C3").Value = cabecera.CodigoSucursal;
                ws.Cell("E3").Value = cabecera.CustodioNombre;
                ws.Cell("I3").Value = cabecera.NroCajaChica;
                ws.Cell("C4").Value = cabecera.CodigoSucursal + " " + nombreSucursal;
                ws.Cell("C5").Value = nombreSucursal;
                ws.Cell("E5").Value = cabecera.FechaCierre?.ToString("dd/MM/yyyy") ?? DateTime.Now.ToString("dd/MM/yyyy");

                // 3. Renombrar columna Proveedor a Beneficiario
                ws.Cell(7, 4).Value = "BENEFICIARIO / EMPLEADO";

                // 4. Crear encabezados para los campos adicionales en la columna R (columna 18) en adelante
                // Esto evita chocar con el cuadro de resumen (columnas L a P)
                ws.Cell(7, 18).Value = "VALOR ENTREGADO";
                ws.Cell(7, 19).Value = "VUELTO ESPERADO";
                ws.Cell(7, 20).Value = "ESTADO VUELTO";
                ws.Cell(7, 21).Value = "COMPROBANTE ADJUNTO";

                // Copiar estilo de cabecera de la columna J
                var headerStyle = ws.Cell(7, 10).Style;
                ws.Cell(7, 18).Style = headerStyle;
                ws.Cell(7, 19).Style = headerStyle;
                ws.Cell(7, 20).Style = headerStyle;
                ws.Cell(7, 21).Style = headerStyle;

                var items = cabecera.Detalles.OrderBy(d => d.FechaComprobante).ThenBy(d => d.Id).ToList();
                
                int startRow = 8;
                int maxRow = 38; 

                for (int i = 0; i < items.Count; i++)
                {
                    int currentRow = startRow + i;
                    var item = items[i];

                    // Si excede la fila 38, insertamos una nueva fila
                    if (currentRow > maxRow)
                    {
                        ws.Row(currentRow - 1).InsertRowsBelow(1);
                        maxRow++;
                    }

                    ws.Cell(currentRow, 1).Value = i + 1;
                    ws.Cell(currentRow, 2).Value = item.FechaComprobante.ToString("yyyy-MM-dd");
                    ws.Cell(currentRow, 3).Value = item.NroComprobante;
                    ws.Cell(currentRow, 4).Value = item.UsuarioBeneficiado ?? ""; // Beneficiario en vez de Proveedor
                    ws.Cell(currentRow, 5).Value = item.Descripcion;
                    ws.Cell(currentRow, 6).Value = item.TipoGasto;
                    ws.Cell(currentRow, 7).Value = item.SubtotalSinIva;
                    ws.Cell(currentRow, 8).Value = item.SubtotalConIva;
                    
                    // Fórmulas de impuestos y totales originales
                    ws.Cell(currentRow, 9).FormulaA1 = $"IF(H{currentRow}>0,($I$7*H{currentRow}),0)";
                    ws.Cell(currentRow, 10).FormulaA1 = $"G{currentRow}+H{currentRow}+I{currentRow}";

                    // Escribir campos de vuelto y comprobante en las columnas adicionales R, S, T, U
                    ws.Cell(currentRow, 18).Value = item.ValorEntregado;
                    ws.Cell(currentRow, 19).FormulaA1 = $"IF(R{currentRow}>J{currentRow},R{currentRow}-J{currentRow},0)";
                    ws.Cell(currentRow, 20).Value = item.EstadoVuelto;

                    if (!string.IsNullOrEmpty(item.ComprobanteUrl))
                    {
                        try
                        {
                            ws.Cell(currentRow, 21).Value = "Ver Comprobante";
                            ws.Cell(currentRow, 21).GetHyperlink().ExternalAddress = new Uri(item.ComprobanteUrl);
                        }
                        catch
                        {
                            ws.Cell(currentRow, 21).Value = item.ComprobanteUrl;
                        }
                    }

                    // Formatear las nuevas celdas
                    var dataStyle = ws.Cell(currentRow, 10).Style;
                    
                    ws.Cell(currentRow, 18).Style = dataStyle;
                    ws.Cell(currentRow, 18).Style.NumberFormat.Format = "$#,##0.00";
                    
                    ws.Cell(currentRow, 19).Style = dataStyle;
                    ws.Cell(currentRow, 19).Style.NumberFormat.Format = "$#,##0.00";

                    ws.Cell(currentRow, 20).Style = dataStyle;
                    ws.Cell(currentRow, 20).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Cell(currentRow, 21).Style = dataStyle;
                    ws.Cell(currentRow, 21).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(currentRow, 21).Style.Font.Underline = XLFontUnderlineValues.Single;
                    ws.Cell(currentRow, 21).Style.Font.FontColor = XLColor.FromHtml("#0f766e"); // Teal brand color
                }

                // Ajustar totales principales (Fila de total está después de maxRow)
                int totalRow = maxRow + 1;
                ws.Cell(totalRow, 7).FormulaA1 = $"SUM(G8:G{maxRow})";
                ws.Cell(totalRow, 8).FormulaA1 = $"SUM(H8:H{maxRow})";
                ws.Cell(totalRow, 9).FormulaA1 = $"SUM(I8:I{maxRow})";
                ws.Cell(totalRow, 10).FormulaA1 = $"SUM(J8:J{maxRow})";
                
                // Sumar totales de las nuevas columnas en R y S
                ws.Cell(totalRow, 18).FormulaA1 = $"SUM(R8:R{maxRow})";
                ws.Cell(totalRow, 19).FormulaA1 = $"SUM(S8:S{maxRow})";

                var totalStyle = ws.Cell(totalRow, 10).Style;
                
                ws.Cell(totalRow, 18).Style = totalStyle;
                ws.Cell(totalRow, 18).Style.NumberFormat.Format = "$#,##0.00";

                ws.Cell(totalRow, 19).Style = totalStyle;
                ws.Cell(totalRow, 19).Style.NumberFormat.Format = "$#,##0.00";

                // 5. Escribir el custodio de forma dinámica en el bloque de firmas (originalmente en D43)
                int signatureNameRow = 43 + (maxRow - 38);
                ws.Cell(signatureNameRow, 4).Value = cabecera.CustodioNombre;

                using (var ms = new MemoryStream())
                {
                    workbook.SaveAs(ms);
                    return ms.ToArray();
                }
            }
        }
    }
}
