using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using BelisaEntidad;

namespace Belisa
{
    // Preferencias del ticket. Se guardan en un archivo de texto, no en la base de datos
    public class TicketConfig
    {
        public static readonly string[] Modos = { "Preguntar", "Siempre", "Nunca" };

        public string Impresora = "";               // vacío = impresora predeterminada de Windows
        public int AnchoMm = 80;                    // 58 u 80
        public string AlCobrar = "Preguntar";       // Preguntar, Siempre o Nunca
        public string Contacto = "";
        public string Mensaje = "¡Gracias por tu visita!";

        private static string Ruta
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Belisa", "ticket.cfg");
            }
        }

        public static TicketConfig Cargar()
        {
            var c = new TicketConfig();
            try
            {
                if (!File.Exists(Ruta)) return c;

                foreach (string linea in File.ReadAllLines(Ruta))
                {
                    int i = linea.IndexOf('=');
                    if (i <= 0) continue;

                    string clave = linea.Substring(0, i).Trim();
                    string valor = linea.Substring(i + 1).Trim();

                    switch (clave)
                    {
                        case "Impresora": c.Impresora = valor; break;
                        case "AnchoMm": c.AnchoMm = valor == "58" ? 58 : 80; break;
                        case "AlCobrar": if (Modos.Contains(valor)) c.AlCobrar = valor; break;
                        case "Contacto": c.Contacto = valor; break;
                        case "Mensaje": c.Mensaje = valor; break;
                    }
                }
            }
            catch (IOException) { }                    // archivo ilegible: se usan los valores de fábrica
            catch (UnauthorizedAccessException) { }
            return c;
        }

        public void Guardar()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Ruta));
            File.WriteAllLines(Ruta, new[]
            {
                "Impresora=" + Limpio(Impresora),
                "AnchoMm=" + AnchoMm,
                "AlCobrar=" + AlCobrar,
                "Contacto=" + Limpio(Contacto),
                "Mensaje=" + Limpio(Mensaje)
            });
        }

        private static string Limpio(string texto)
        {
            return (texto ?? "").Replace("\r", " ").Replace("\n", " ").Trim();
        }
    }

    public static class Ticket
    {
        private enum Tipo { Texto, Par, Separador, Espacio }

        private class Linea
        {
            public Tipo Tipo;
            public string Izq, Der;
            public float Tam, Alto;
            public bool Negrita, Centro;
        }

        public static void Imprimir(Venta v, List<DetalleVenta> detalle, bool reimpresion = false)
        {
            Imprimir(v, detalle, TicketConfig.Cargar(), reimpresion);
        }

        public static void Imprimir(Venta v, List<DetalleVenta> detalle, TicketConfig cfg, bool reimpresion)
        {
            var lineas = Construir(v, detalle, cfg, reimpresion);
            float anchoUtil = Mm(cfg.AnchoMm == 58 ? 48 : 72);
            int indice = 0;

            using (var doc = new PrintDocument())
            {
                doc.DocumentName = "Ticket " + v.NumeroDocumento;
                doc.PrintController = new StandardPrintController();   // sin ventanita de progreso

                if (!string.IsNullOrEmpty(cfg.Impresora))
                    doc.PrinterSettings.PrinterName = cfg.Impresora;

                if (!doc.PrinterSettings.IsValid)
                {
                    throw new InvalidOperationException(string.IsNullOrEmpty(cfg.Impresora)
                        ? "No hay una impresora predeterminada en Windows. Elige una en \"Config. ticket\"."
                        : "La impresora \"" + cfg.Impresora + "\" no está disponible. Revisa \"Config. ticket\".");
                }

                doc.PrintPage += (s, e) =>
                {
                    RectangleF area = e.Graphics.VisibleClipBounds;
                    float ancho = Math.Min(anchoUtil, area.Width);
                    float x = area.Left;
                    float y = area.Top;

                    while (indice < lineas.Count)
                    {
                        float alto = Pintar(e.Graphics, lineas[indice], x, y, ancho, false);

                        // Si ya no cabe y algo se imprimió en esta hoja, el resto va en la siguiente
                        if (y + alto > area.Bottom && y > area.Top)
                        {
                            e.HasMorePages = true;
                            return;
                        }

                        Pintar(e.Graphics, lineas[indice], x, y, ancho, true);
                        y += alto;
                        indice++;
                    }
                    e.HasMorePages = false;
                };

                doc.Print();
            }
        }

        // ---------- Contenido del ticket ----------

        private static List<Linea> Construir(Venta v, List<DetalleVenta> detalle, TicketConfig cfg, bool reimpresion)
        {
            float t = cfg.AnchoMm == 58 ? 8f : 9f;
            float titulo = cfg.AnchoMm == 58 ? 14f : 17f;

            var L = new List<Linea>();

            L.Add(C("BELISA", titulo, true));
            L.Add(C("Centro de depilación", t, false));
            L.Add(C("Cera española", t, false));
            if (!string.IsNullOrWhiteSpace(cfg.Contacto)) L.Add(C(cfg.Contacto, t - 1, false));
            if (reimpresion) L.Add(C("*** REIMPRESIÓN ***", t, true));
            L.Add(S());

            L.Add(T("Ticket: " + v.NumeroDocumento, t, true));
            L.Add(T("Fecha: " + v.FechaRegistro.ToString("dd/MM/yyyy HH:mm"), t, false));
            L.Add(T("Cliente: " + v.ClienteNombre, t, false));
            L.Add(T("Atendió: " + v.UsuarioNombre, t, false));
            L.Add(S());

            foreach (var d in detalle)
            {
                L.Add(T(d.ProductoNombre, t, false));
                L.Add(P(d.Cantidad + " x " + d.PrecioVenta.ToString("C2"), d.SubTotal.ToString("C2"), t, false));
            }
            L.Add(S());

            if (v.MontoDescuento > 0)
            {
                L.Add(P("Subtotal", v.Subtotal.ToString("C2"), t, false));
                L.Add(P("Descuento (" + v.DescuentoPorcentaje.ToString("0.##") + "%)",
                        "-" + v.MontoDescuento.ToString("C2"), t, false));
            }
            L.Add(P("TOTAL", v.MontoTotal.ToString("C2"), t + 3, true));

            if (v.Pagos != null && v.Pagos.Count > 0)
            {
                // Una línea por forma de pago: una si fue normal, dos si fue mixta
                foreach (var pago in v.Pagos)
                {
                    L.Add(P("Pago " + pago.MetodoPago, pago.Monto.ToString("C2"), t, false));
                    if (!string.IsNullOrEmpty(pago.ReferenciaPago)) L.Add(T("Ref: " + pago.ReferenciaPago, t, false));
                }
            }
            else
            {
                L.Add(P("Pago", v.MetodoPago, t, false));
                if (!string.IsNullOrEmpty(v.ReferenciaPago)) L.Add(T("Ref: " + v.ReferenciaPago, t, false));
            }

            // Efectivo recibido = lo que se cobró en efectivo + el cambio
            bool hayEfectivo = v.MetodoPago == "Efectivo"
                               || (v.Pagos != null && v.Pagos.Any(x => x.MetodoPago == "Efectivo"));
            if (hayEfectivo)
            {
                decimal efectivoParte = (v.Pagos != null && v.Pagos.Count > 0)
                    ? v.Pagos.Where(x => x.MetodoPago == "Efectivo").Sum(x => x.Monto)
                    : v.MontoTotal;
                L.Add(P("Efectivo recibido", (efectivoParte + v.MontoCambio).ToString("C2"), t, false));
                L.Add(P("Cambio", v.MontoCambio.ToString("C2"), t, false));
            }
            L.Add(S());

            if (!string.IsNullOrWhiteSpace(cfg.Mensaje)) L.Add(C(cfg.Mensaje, t, false));
            L.Add(C("Ticket sin validez fiscal", t - 1, false));
            L.Add(E(Mm(12)));   // espacio para que la cuchilla no corte el texto

            return L;
        }

        private static Linea T(string texto, float tam, bool negrita)
        {
            return new Linea { Tipo = Tipo.Texto, Izq = texto, Tam = tam, Negrita = negrita };
        }

        private static Linea C(string texto, float tam, bool negrita)
        {
            return new Linea { Tipo = Tipo.Texto, Izq = texto, Tam = tam, Negrita = negrita, Centro = true };
        }

        private static Linea P(string izq, string der, float tam, bool negrita)
        {
            return new Linea { Tipo = Tipo.Par, Izq = izq, Der = der, Tam = tam, Negrita = negrita };
        }

        private static Linea S()
        {
            return new Linea { Tipo = Tipo.Separador };
        }

        private static Linea E(float alto)
        {
            return new Linea { Tipo = Tipo.Espacio, Alto = alto };
        }

        // ---------- Dibujo ----------

        // Mide (dibujar = false) o dibuja (dibujar = true) una línea y devuelve su alto
        private static float Pintar(Graphics g, Linea l, float x, float y, float ancho, bool dibujar)
        {
            if (l.Tipo == Tipo.Espacio) return l.Alto;

            if (l.Tipo == Tipo.Separador)
            {
                if (dibujar)
                {
                    using (var lapiz = new Pen(Color.Black, 1f) { DashStyle = DashStyle.Dash })
                        g.DrawLine(lapiz, x, y + 4, x + ancho, y + 4);
                }
                return 8;
            }

            string izq = l.Izq ?? "";
            string der = l.Der ?? "";

            using (var fuente = new Font("Arial", l.Tam, l.Negrita ? FontStyle.Bold : FontStyle.Regular, GraphicsUnit.Point))
            using (var izquierda = new StringFormat(StringFormat.GenericTypographic))
            using (var centro = new StringFormat(StringFormat.GenericTypographic) { Alignment = StringAlignment.Center })
            using (var derecha = new StringFormat(StringFormat.GenericTypographic) { Alignment = StringAlignment.Far })
            {
                if (l.Tipo == Tipo.Texto)
                {
                    StringFormat formato = l.Centro ? centro : izquierda;
                    float alto = g.MeasureString(izq, fuente, new SizeF(ancho, 10000f), formato).Height + 2;
                    if (dibujar)
                        g.DrawString(izq, fuente, Brushes.Black, new RectangleF(x, y, ancho, alto), formato);
                    return alto;
                }

                // Par: texto a la izquierda y valor pegado a la derecha en la misma línea
                SizeF medidaDer = g.MeasureString(der, fuente, 10000, derecha);
                float anchoIzq = Math.Max(ancho * 0.35f, ancho - medidaDer.Width - 6);
                float altoIzq = g.MeasureString(izq, fuente, new SizeF(anchoIzq, 10000f), izquierda).Height;
                float altoPar = Math.Max(altoIzq, medidaDer.Height) + 2;

                if (dibujar)
                {
                    g.DrawString(der, fuente, Brushes.Black, new RectangleF(x, y, ancho, altoPar), derecha);
                    g.DrawString(izq, fuente, Brushes.Black, new RectangleF(x, y, anchoIzq, altoPar), izquierda);
                }
                return altoPar;
            }
        }

        // Milímetros a centésimas de pulgada, la unidad de las impresoras en .NET
        private static float Mm(float mm)
        {
            return mm * 100f / 25.4f;
        }
    }
}