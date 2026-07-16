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

                // 4. Insertar 4 columnas nuevas antes de la columna K (columna 11)
                // Esto desplaza el cuadro de resumen (columnas L a P) automáticamente a la derecha
                ws.Column(11).InsertColumnsBefore(4);

                // Escribir los encabezados en las nuevas columnas K, L, M, N (columnas 11, 12, 13, 14)
                ws.Cell(7, 11).Value = "VALOR ENTREGADO";
                ws.Cell(7, 12).Value = "VUELTO ESPERADO";
                ws.Cell(7, 13).Value = "ESTADO VUELTO";
                ws.Cell(7, 14).Value = "COMPROBANTE ADJUNTO";

                // Copiar estilo de cabecera de la columna J (columna 10)
                var headerStyle = ws.Cell(7, 10).Style;
                ws.Cell(7, 11).Style = headerStyle;
                ws.Cell(7, 12).Style = headerStyle;
                ws.Cell(7, 13).Style = headerStyle;
                ws.Cell(7, 14).Style = headerStyle;

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

                    // Escribir campos de vuelto y comprobante en las nuevas columnas al lado del total (K, L, M, N)
                    ws.Cell(currentRow, 11).Value = item.ValorEntregado;
                    ws.Cell(currentRow, 12).FormulaA1 = $"IF(K{currentRow}>J{currentRow},K{currentRow}-J{currentRow},0)";
                    ws.Cell(currentRow, 13).Value = item.EstadoVuelto;

                    if (!string.IsNullOrEmpty(item.ComprobanteUrl))
                    {
                        try
                        {
                            ws.Cell(currentRow, 14).Value = "Ver Comprobante";
                            ws.Cell(currentRow, 14).GetHyperlink().ExternalAddress = new Uri(item.ComprobanteUrl);
                        }
                        catch
                        {
                            ws.Cell(currentRow, 14).Value = item.ComprobanteUrl;
                        }
                    }

                    // Formatear las nuevas celdas copiando el estilo de la columna J
                    var dataStyle = ws.Cell(currentRow, 10).Style;
                    
                    ws.Cell(currentRow, 11).Style = dataStyle;
                    ws.Cell(currentRow, 11).Style.NumberFormat.Format = "$#,##0.00";
                    
                    ws.Cell(currentRow, 12).Style = dataStyle;
                    ws.Cell(currentRow, 12).Style.NumberFormat.Format = "$#,##0.00";

                    ws.Cell(currentRow, 13).Style = dataStyle;
                    ws.Cell(currentRow, 13).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Cell(currentRow, 14).Style = dataStyle;
                    ws.Cell(currentRow, 14).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(currentRow, 14).Style.Font.Underline = XLFontUnderlineValues.Single;
                    ws.Cell(currentRow, 14).Style.Font.FontColor = XLColor.FromHtml("#0f766e"); // Teal brand color
                }

                // Ajustar totales principales (Fila de total está después de maxRow)
                int totalRow = maxRow + 1;
                ws.Cell(totalRow, 7).FormulaA1 = $"SUM(G8:G{maxRow})";
                ws.Cell(totalRow, 8).FormulaA1 = $"SUM(H8:H{maxRow})";
                ws.Cell(totalRow, 9).FormulaA1 = $"SUM(I8:I{maxRow})";
                ws.Cell(totalRow, 10).FormulaA1 = $"SUM(J8:J{maxRow})";
                
                // Sumar totales de las nuevas columnas K y L
                ws.Cell(totalRow, 11).FormulaA1 = $"SUM(K8:K{maxRow})";
                ws.Cell(totalRow, 12).FormulaA1 = $"SUM(L8:L{maxRow})";

                var totalStyle = ws.Cell(totalRow, 10).Style;
                
                ws.Cell(totalRow, 11).Style = totalStyle;
                ws.Cell(totalRow, 11).Style.NumberFormat.Format = "$#,##0.00";

                ws.Cell(totalRow, 12).Style = totalStyle;
                ws.Cell(totalRow, 12).Style.NumberFormat.Format = "$#,##0.00";

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
