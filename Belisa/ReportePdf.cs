using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BelisaEntidad;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;

namespace Belisa
{
    // Genera los PDF (comprobantes y reportes). No muestra ventanas: solo escribe el archivo.
    public static class ReportePdf
    {
        private static readonly Color Acento = Color.FromRgb(142, 91, 201);
        private static readonly Color Lila = Color.FromRgb(204, 187, 232);
        private static readonly Color LilaSuave = Color.FromRgb(243, 238, 250);
        private static readonly Color Texto = Color.FromRgb(74, 69, 80);
        private static readonly Color Linea = Color.FromRgb(200, 192, 210);

        // Hoja carta con 1.8 cm de margen a cada lado
        private const double AnchoVertical = 17.9;
        private const double AnchoHorizontal = 24.3;

        // ---------- Comprobante de compra ----------

        public static void ComprobanteCompra(Compra c, List<DetalleCompra> detalle, string ruta)
        {
            var doc = NuevoDocumento(false, "Comprobante interno, sin validez fiscal");
            var s = doc.LastSection;

            Encabezado(s, "Comprobante de compra", c.NumeroDocumento, AnchoVertical);
            Dato(s, "Fecha", c.FechaRegistro.ToString("dd/MM/yyyy HH:mm"));
            Dato(s, "Proveedor", c.ProveedorNombre);
            Dato(s, "Documento", c.TipoDocumento);
            Dato(s, "Registró", c.UsuarioNombre);
            s.AddParagraph().Format.SpaceAfter = 4;

            var t = CrearTabla(s,
                new[] { "Código", "Producto", "Cantidad", "P. compra", "Subtotal" },
                new[] { 2.6, 7.2, 2.0, 3.0, 3.1 },
                new[] { false, false, true, true, true });

            foreach (var d in detalle)
            {
                AgregarFila(t, new[]
                {
                    d.ProductoCodigo, d.ProductoNombre, d.Cantidad.ToString("N0"),
                    d.PrecioCompra.ToString("C2"), d.MontoTotal.ToString("C2")
                }, false);
            }

            Totales(s, new[] { new[] { "Total", c.MontoTotal.ToString("C2") } });
            Guardar(doc, ruta);
        }

        // ---------- Comprobante de venta ----------

        public static void ComprobanteVenta(Venta v, List<DetalleVenta> detalle, string ruta)
        {
            var doc = NuevoDocumento(false, "Comprobante interno, sin validez fiscal");
            var s = doc.LastSection;

            Encabezado(s, "Comprobante de venta", v.NumeroDocumento, AnchoVertical);
            Dato(s, "Fecha", v.FechaRegistro.ToString("dd/MM/yyyy HH:mm"));
            Dato(s, "Cliente", v.ClienteNombre);
            Dato(s, "Atendió", v.UsuarioNombre);
            string pago = v.MetodoPago;
            if (!string.IsNullOrEmpty(v.ReferenciaPago)) pago += "  (ref. " + v.ReferenciaPago + ")";
            Dato(s, "Método de pago", pago);
            s.AddParagraph().Format.SpaceAfter = 4;

            var t = CrearTabla(s,
                new[] { "Código", "Producto", "Cantidad", "Precio", "Subtotal" },
                new[] { 2.6, 7.2, 2.0, 3.0, 3.1 },
                new[] { false, false, true, true, true });

            foreach (var d in detalle)
            {
                AgregarFila(t, new[]
                {
                    d.ProductoCodigo, d.ProductoNombre, d.Cantidad.ToString("N0"),
                    d.PrecioVenta.ToString("C2"), d.SubTotal.ToString("C2")
                }, false);
            }

            var lineas = new List<string[]>();
            lineas.Add(new[] { "Subtotal", v.Subtotal.ToString("C2") });
            if (v.MontoDescuento > 0)
                lineas.Add(new[] { "Descuento (" + v.DescuentoPorcentaje.ToString("0.##") + "%)", "-" + v.MontoDescuento.ToString("C2") });
            if (v.MetodoPago == "Efectivo")
            {
                lineas.Add(new[] { "Pagó con", v.MontoPago.ToString("C2") });
                lineas.Add(new[] { "Cambio", v.MontoCambio.ToString("C2") });
            }
            // El total va al final de la lista: es la línea que se resalta
            lineas.Add(new[] { "Total", v.MontoTotal.ToString("C2") });

            Totales(s, lineas.ToArray());
            Guardar(doc, ruta);
        }

        // ---------- Reporte de una tabla (por rango de fechas, etc.) ----------

        public static void Listado(string titulo, string subtitulo, string[] encabezados, bool[] derecha,
                                   double[] pesos, List<string[]> filas, string[] filaTotal, string ruta)
        {
            var doc = NuevoDocumento(true, "Reporte interno");
            var s = doc.LastSection;

            Encabezado(s, titulo, subtitulo, AnchoHorizontal);

            double suma = pesos.Sum();
            double[] anchos = pesos.Select(p => p / suma * AnchoHorizontal).ToArray();

            var t = CrearTabla(s, encabezados, anchos, derecha);
            foreach (var fila in filas) AgregarFila(t, fila, false);
            if (filaTotal != null) AgregarFila(t, filaTotal, true);

            Guardar(doc, ruta);
        }

        // ---------- Piezas comunes ----------

        private static Document NuevoDocumento(bool horizontal, string pie)
        {
            var doc = new Document();
            doc.Info.Title = "Belisa";

            var normal = doc.Styles[StyleNames.Normal];
            normal.Font.Name = "Arial";
            normal.Font.Size = 9.5;
            normal.Font.Color = Texto;

            var s = doc.AddSection();
            s.PageSetup.PageFormat = PageFormat.Letter;
            s.PageSetup.Orientation = horizontal ? Orientation.Landscape : Orientation.Portrait;
            s.PageSetup.LeftMargin = Unit.FromCentimeter(1.8);
            s.PageSetup.RightMargin = Unit.FromCentimeter(1.8);
            s.PageSetup.TopMargin = Unit.FromCentimeter(1.5);
            s.PageSetup.BottomMargin = Unit.FromCentimeter(1.8);

            var p = s.Footers.Primary.AddParagraph();
            p.Format.Font.Size = 8;
            p.Format.Alignment = ParagraphAlignment.Center;
            p.AddText(pie + "  ·  Página ");
            p.AddPageField();
            p.AddText(" de ");
            p.AddNumPagesField();

            return doc;
        }

        private static void Encabezado(Section s, string titulo, string subtitulo, double anchoCm)
        {
            var t = s.AddTable();
            t.LeftPadding = 0;
            t.AddColumn(Unit.FromCentimeter(3.4));
            t.AddColumn(Unit.FromCentimeter(anchoCm - 3.4));

            var fila = t.AddRow();

            string logo = RutaLogo();
            if (logo != null)
            {
                var imagen = fila.Cells[0].AddImage(logo);
                imagen.Width = Unit.FromCentimeter(3);
                imagen.LockAspectRatio = true;
            }

            var celda = fila.Cells[1];
            celda.VerticalAlignment = VerticalAlignment.Center;

            var p = celda.AddParagraph("Belisa");
            p.Format.Font.Size = 20;
            p.Format.Font.Bold = true;
            p.Format.Font.Color = Acento;

            p = celda.AddParagraph("Centro de depilación · Cera española");
            p.Format.Font.Size = 9;

            p = celda.AddParagraph(titulo);
            p.Format.Font.Size = 14;
            p.Format.Font.Bold = true;
            p.Format.SpaceBefore = 8;

            if (!string.IsNullOrEmpty(subtitulo))
            {
                p = celda.AddParagraph(subtitulo);
                p.Format.Font.Size = 10;
            }

            s.AddParagraph().Format.SpaceAfter = 6;
        }

        private static void Dato(Section s, string etiqueta, string valor)
        {
            var p = s.AddParagraph();
            p.Format.SpaceAfter = 2;
            p.AddFormattedText(etiqueta + ": ", TextFormat.Bold);
            p.AddText(valor ?? "");
        }

        private static Table CrearTabla(Section s, string[] encabezados, double[] anchosCm, bool[] derecha)
        {
            var t = s.AddTable();
            t.Borders.Color = Linea;
            t.Borders.Width = 0.25;
            t.TopPadding = 3;
            t.BottomPadding = 3;
            t.LeftPadding = 4;
            t.RightPadding = 4;

            for (int i = 0; i < encabezados.Length; i++)
            {
                var col = t.AddColumn(Unit.FromCentimeter(anchosCm[i]));
                col.Format.Alignment = derecha[i] ? ParagraphAlignment.Right : ParagraphAlignment.Left;
            }

            var cabecera = t.AddRow();
            cabecera.HeadingFormat = true;   // se repite al cambiar de página
            cabecera.Shading.Color = Lila;
            cabecera.Format.Font.Bold = true;
            for (int i = 0; i < encabezados.Length; i++)
                cabecera.Cells[i].AddParagraph(encabezados[i] ?? "");

            return t;
        }

        private static void AgregarFila(Table t, string[] valores, bool resaltar)
        {
            var fila = t.AddRow();
            if (resaltar)
            {
                fila.Format.Font.Bold = true;
                fila.Shading.Color = LilaSuave;
            }
            for (int i = 0; i < valores.Length; i++)
                fila.Cells[i].AddParagraph(valores[i] ?? "");
        }

        // Cuadro de totales alineado a la derecha; la última línea se resalta
        private static void Totales(Section s, string[][] lineas)
        {
            s.AddParagraph().Format.SpaceAfter = 4;

            var t = s.AddTable();
            t.Rows.Alignment = RowAlignment.Right;
            t.TopPadding = 2;
            t.BottomPadding = 2;
            t.AddColumn(Unit.FromCentimeter(4.8)).Format.Alignment = ParagraphAlignment.Left;
            t.AddColumn(Unit.FromCentimeter(3.6)).Format.Alignment = ParagraphAlignment.Right;

            foreach (var linea in lineas)
            {
                var fila = t.AddRow();
                fila.Cells[0].AddParagraph(linea[0]);
                fila.Cells[1].AddParagraph(linea[1]);
            }

            var ultima = t.Rows[t.Rows.Count - 1];
            ultima.Format.Font.Bold = true;
            ultima.Format.Font.Size = 11;
            ultima.Shading.Color = LilaSuave;
        }

        private static void Guardar(Document doc, string ruta)
        {
            var render = new PdfDocumentRenderer { Document = doc };
            render.RenderDocument();
            render.PdfDocument.Save(ruta);
        }

        // MigraDoc lee las imágenes desde un archivo: guardamos el logo una vez en la carpeta temporal
        private static string RutaLogo()
        {
            try
            {
                string ruta = Path.Combine(Path.GetTempPath(), "belisa_logo_pdf.png");
                if (!File.Exists(ruta))
                    Properties.Resources.BelisaLogoCuadrado.Save(ruta, System.Drawing.Imaging.ImageFormat.Png);
                return ruta;
            }
            catch (Exception)
            {
                return null;   // sin logo el PDF sale igual: es un adorno, no un dato
            }
        }
    }
}