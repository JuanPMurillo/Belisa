using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ClosedXML.Excel;

namespace Belisa
{
    public static class Exportar
    {
        // ---------- Botones para la barra superior ----------

        public static Panel BotonExcel(DataGridView grid, string titulo)
        {
            var panel = new Panel { Dock = DockStyle.Right, Width = 108 };
            var boton = Estilo.Boton("Excel", 8, 9, 96, 34, false);
            boton.Click += (s, e) => ExcelDesdeTabla(grid, titulo);
            panel.Controls.Add(boton);
            return panel;
        }

        // 'subtitulo' se calcula al hacer clic (por ejemplo, el rango de fechas buscado)
        public static Panel BotonPdf(DataGridView grid, string titulo, Func<string> subtitulo)
        {
            var panel = new Panel { Dock = DockStyle.Right, Width = 108 };
            var boton = Estilo.Boton("PDF", 8, 9, 96, 34, false);
            boton.Click += (s, e) => PdfDesdeTabla(grid, titulo, subtitulo == null ? "" : subtitulo());
            panel.Controls.Add(boton);
            return panel;
        }

        // ---------- Excel ----------

        public static void ExcelDesdeTabla(DataGridView grid, string titulo)
        {
            if (grid.Rows.Count == 0) { SinDatos(); return; }

            using (var dialogo = new SaveFileDialog())
            {
                dialogo.Title = "Guardar como Excel";
                dialogo.Filter = "Libro de Excel (*.xlsx)|*.xlsx";
                dialogo.FileName = NombreArchivo(titulo) + "_" + DateTime.Today.ToString("yyyy-MM-dd") + ".xlsx";
                dialogo.OverwritePrompt = true;

                if (dialogo.ShowDialog() != DialogResult.OK) return;
                if (!Ejecutar(() => GuardarExcel(grid, titulo, dialogo.FileName))) return;
                PreguntarAbrir(dialogo.FileName);
            }
        }

        private static void GuardarExcel(DataGridView grid, string titulo, string ruta)
        {
            var columnas = ColumnasVisibles(grid);
            int filas = grid.Rows.Count;

            using (var libro = new XLWorkbook())
            {
                var hoja = libro.Worksheets.Add(NombreHoja(titulo));

                for (int c = 0; c < columnas.Count; c++)
                    hoja.Cell(1, c + 1).Value = columnas[c].HeaderText;

                var encabezado = hoja.Range(1, 1, 1, columnas.Count);
                encabezado.Style.Font.Bold = true;
                encabezado.Style.Font.FontColor = XLColor.FromHtml("#4A4550");
                encabezado.Style.Fill.BackgroundColor = XLColor.FromHtml("#CCBBE8");

                for (int f = 0; f < filas; f++)
                {
                    var fila = grid.Rows[f];
                    for (int c = 0; c < columnas.Count; c++)
                        Poner(hoja.Cell(f + 2, c + 1), fila.Cells[columnas[c].Index].Value);
                }

                for (int c = 0; c < columnas.Count; c++)
                {
                    // Las columnas de dinero se guardan como número con formato de moneda
                    if (columnas[c].DefaultCellStyle.Format == "C2")
                        hoja.Range(2, c + 1, filas + 1, c + 1).Style.NumberFormat.Format = "$#,##0.00";

                    hoja.Column(c + 1).Width = Math.Min(60, Math.Max(10, columnas[c].Width / 7.0));
                }

                hoja.SheetView.FreezeRows(1);
                hoja.Range(1, 1, filas + 1, columnas.Count).SetAutoFilter();

                libro.SaveAs(ruta);
            }
        }

        private static void Poner(IXLCell celda, object valor)
        {
            if (valor == null || valor == DBNull.Value) return;

            if (valor is string texto) celda.Value = texto;
            else if (valor is decimal dinero) celda.Value = (double)dinero;
            else if (valor is int entero) celda.Value = entero;
            else if (valor is DateTime fecha) celda.Value = fecha;
            else if (valor is bool si) celda.Value = si;
            else celda.Value = Convert.ToString(valor);
        }

        // ---------- PDF ----------

        // Reporte con lo que se ve en la tabla (con búsqueda y filtros aplicados)
        public static void PdfDesdeTabla(DataGridView grid, string titulo, string subtitulo)
        {
            if (grid.Rows.Count == 0) { SinDatos(); return; }

            var columnas = ColumnasVisibles(grid);
            string[] encabezados = columnas.Select(c => c.HeaderText).ToArray();
            bool[] derecha = columnas.Select(c => c.DefaultCellStyle.Alignment == DataGridViewContentAlignment.MiddleRight).ToArray();
            double[] pesos = columnas.Select(c => (double)c.Width).ToArray();

            var filas = new List<string[]>();
            var sumas = new decimal[columnas.Count];

            foreach (DataGridViewRow fila in grid.Rows)
            {
                var valores = new string[columnas.Count];
                for (int c = 0; c < columnas.Count; c++)
                {
                    var celda = fila.Cells[columnas[c].Index];
                    valores[c] = Convert.ToString(celda.FormattedValue);
                    if (columnas[c].DefaultCellStyle.Format == "C2" && celda.Value is decimal dinero)
                        sumas[c] += dinero;
                }
                filas.Add(valores);
            }

            // Si hay columnas de dinero, se agrega una fila con sus totales
            string[] total = null;
            if (columnas.Any(c => c.DefaultCellStyle.Format == "C2"))
            {
                total = new string[columnas.Count];
                for (int c = 0; c < columnas.Count; c++)
                    total[c] = columnas[c].DefaultCellStyle.Format == "C2" ? sumas[c].ToString("C2") : "";
                if (total[0].Length == 0) total[0] = "TOTAL (" + filas.Count + ")";
            }

            string generado = "Generado el " + DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            string detalle = string.IsNullOrWhiteSpace(subtitulo) ? generado : subtitulo + "  ·  " + generado;

            GuardarPdf(titulo + "_" + DateTime.Today.ToString("yyyy-MM-dd"),
                ruta => ReportePdf.Listado(titulo, detalle, encabezados, derecha, pesos, filas, total, ruta));
        }

        // Pregunta dónde guardar, genera el PDF y ofrece abrirlo
        public static void GuardarPdf(string nombreSugerido, Action<string> generar)
        {
            using (var dialogo = new SaveFileDialog())
            {
                dialogo.Title = "Guardar como PDF";
                dialogo.Filter = "Documento PDF (*.pdf)|*.pdf";
                dialogo.FileName = NombreArchivo(nombreSugerido) + ".pdf";
                dialogo.OverwritePrompt = true;

                if (dialogo.ShowDialog() != DialogResult.OK) return;
                if (!Ejecutar(() => generar(dialogo.FileName))) return;
                PreguntarAbrir(dialogo.FileName);
            }
        }

        // ---------- Comunes ----------

        private static List<DataGridViewColumn> ColumnasVisibles(DataGridView grid)
        {
            // Solo las columnas que se ven (sin Id ni botones), en el orden de pantalla
            return grid.Columns.Cast<DataGridViewColumn>()
                .Where(c => c.Visible && !(c is DataGridViewButtonColumn))
                .OrderBy(c => c.DisplayIndex)
                .ToList();
        }

        private static void SinDatos()
        {
            MessageBox.Show("No hay datos para exportar.", "Exportar",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Ejecuta la acción de guardado y explica los fallos comunes en lugar de cerrar el programa
        private static bool Ejecutar(Action accion)
        {
            try
            {
                accion();
                return true;
            }
            catch (IOException)
            {
                MessageBox.Show("No se pudo guardar el archivo.\nSi lo tienes abierto en otro programa, ciérralo e intenta de nuevo.",
                    "Exportar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("No tienes permiso para guardar en esa carpeta.\nElige otra, por ejemplo Documentos.",
                    "Exportar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo crear el archivo.\n\n" + ex.Message,
                    "Exportar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }

        private static void PreguntarAbrir(string ruta)
        {
            if (MessageBox.Show("Archivo guardado.\n¿Quieres abrirlo ahora?", "Exportar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                Process.Start(new ProcessStartInfo(ruta) { UseShellExecute = true });
            }
            catch (Exception)
            {
                MessageBox.Show("No se pudo abrir el archivo. Búscalo en:\n" + ruta,
                    "Exportar", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private static string NombreArchivo(string titulo)
        {
            return string.Concat(titulo.Split(Path.GetInvalidFileNameChars())).Replace(' ', '_');
        }

        // Excel no permite estos caracteres en el nombre de la hoja, ni más de 31 letras
        private static string NombreHoja(string titulo)
        {
            string limpio = Regex.Replace(titulo, @"[\\/?*\[\]:]", "").Trim();
            if (limpio.Length == 0) limpio = "Datos";
            return limpio.Length > 31 ? limpio.Substring(0, 31) : limpio;
        }
    }
}