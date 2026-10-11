using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BelisaEntidad;
using BelisaNegocio;

namespace Belisa
{
    [DesignerCategory("Code")]
    public class FrmDetalleVenta : Form
    {
        private static readonly Font Tachado = new Font(Estilo.Normal, FontStyle.Strikeout);

        private readonly VentaN negocio = new VentaN();
        private readonly AnulacionN anulacion = new AnulacionN();
        private List<Venta> ventas = new List<Venta>();
        private Venta ventaActual;
        private List<DetalleVenta> detalleActual = new List<DetalleVenta>();
        private DateTime desdeUsado = DateTime.Today, hastaUsado = DateTime.Today;
        private bool cargando;

        private DateTimePicker dtDesde, dtHasta;
        private TextBox txtFiltro;
        private Label lblResumen, lblTituloDetalle;
        private DataGridView gridVentas, gridDetalle;
        private Button btnComprobante, btnTicket, btnAnular;

        public FrmDetalleVenta()
        {
            BackColor = Estilo.Fondo;

            var contenido = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24) };
            ConstruirLista(contenido);     // el centro (Fill) primero...
            ConstruirDetalle(contenido);   // ...luego abajo...
            ConstruirBarra(contenido);     // ...y arriba
            Controls.Add(contenido);

            Load += (s, e) => Buscar();
        }

        // ---------- Construcción ----------

        private void ConstruirLista(Panel padre)
        {
            gridVentas = Tabla.Crear();
            Tabla.ColumnaFija(gridVentas, "Fecha", "Fecha", 150, null, false);
            Tabla.ColumnaFija(gridVentas, "Numero", "Número", 110, null, false);
            Tabla.Columna(gridVentas, "Cliente", "Cliente", 150);
            Tabla.ColumnaFija(gridVentas, "Pago", "Pago", 120, null, false);
            Tabla.ColumnaFija(gridVentas, "Descuento", "Descuento", 100, "C2", true);
            Tabla.Columna(gridVentas, "Usuario", "Registró", 100);
            Tabla.ColumnaFija(gridVentas, "Estado", "Estado", 100, null, false);
            Tabla.ColumnaFija(gridVentas, "Total", "Total", 120, "C2", true);

            gridVentas.SelectionChanged += (s, e) =>
            {
                if (cargando || gridVentas.CurrentRow == null) return;
                var v = gridVentas.CurrentRow.Tag as Venta;
                if (v != null) MostrarDetalle(v);
            };

            padre.Controls.Add(gridVentas);
        }

        private void ConstruirDetalle(Panel padre)
        {
            var abajo = new Panel { Dock = DockStyle.Bottom, Height = 290, Padding = new Padding(0, 12, 0, 0) };
            var tarjeta = new Panel { Dock = DockStyle.Fill };

            gridDetalle = Tabla.Crear();
            Tabla.Columna(gridDetalle, "Codigo", "Código", 70);
            Tabla.Columna(gridDetalle, "Producto", "Producto", 180);
            Tabla.ColumnaFija(gridDetalle, "Cantidad", "Cantidad", 90, null, true);
            Tabla.ColumnaFija(gridDetalle, "Precio", "Precio", 110, "C2", true);
            Tabla.ColumnaFija(gridDetalle, "Subtotal", "Subtotal", 120, "C2", true);

            lblTituloDetalle = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true,
                Font = Estilo.Negrita,
                ForeColor = Estilo.Texto,
                Text = "Selecciona una venta para ver su detalle"
            };

            btnAnular = Estilo.Boton("Anular venta", 0, 2, 140, 34, false);
            btnAnular.ForeColor = Estilo.Error;
            btnAnular.FlatAppearance.BorderColor = Estilo.Error;
            btnAnular.Visible = Sesion.TienePermiso("anular");   // solo el administrador
            btnAnular.Enabled = false;
            btnAnular.Click += (s, e) => Anular();

            btnTicket = Estilo.Boton("Imprimir ticket", 150, 2, 170, 34, false);
            btnTicket.Enabled = false;
            btnTicket.Click += (s, e) => ImprimirTicket();

            btnComprobante = Estilo.Boton("Comprobante PDF", 330, 2, 170, 34, false);
            btnComprobante.Enabled = false;
            btnComprobante.Click += (s, e) => Comprobante();

            var panelBotones = new Panel { Dock = DockStyle.Right, Width = 508 };
            panelBotones.Controls.Add(btnAnular);
            panelBotones.Controls.Add(btnTicket);
            panelBotones.Controls.Add(btnComprobante);

            var cabecera = new Panel { Dock = DockStyle.Top, Height = 38 };
            cabecera.Controls.Add(lblTituloDetalle);   // Fill primero
            cabecera.Controls.Add(panelBotones);       // luego los botones a la derecha

            tarjeta.Controls.Add(gridDetalle);   // Fill primero
            tarjeta.Controls.Add(cabecera);      // luego arriba
            abajo.Controls.Add(tarjeta);
            padre.Controls.Add(abajo);
        }

        private void ConstruirBarra(Panel padre)
        {
            var barra = new Panel { Dock = DockStyle.Top, Height = 52 };

            lblResumen = new Label
            {
                Dock = DockStyle.Right,
                Width = 430,
                TextAlign = ContentAlignment.MiddleRight,
                Font = Estilo.Normal,
                ForeColor = Estilo.Texto
            };
            barra.Controls.Add(lblResumen);
            barra.Controls.Add(Exportar.BotonExcel(gridVentas, "Ventas"));
            barra.Controls.Add(Exportar.BotonPdf(gridVentas, "Reporte de ventas", SubtituloReporte));

            dtDesde = new DateTimePicker
            {
                Location = new Point(55, 12),
                Width = 120,
                Format = DateTimePickerFormat.Short,
                Font = Estilo.Normal,
                Value = DateTime.Today.AddDays(-30)
            };
            dtHasta = new DateTimePicker
            {
                Location = new Point(235, 12),
                Width = 120,
                Format = DateTimePickerFormat.Short,
                Font = Estilo.Normal,
                Value = DateTime.Today
            };

            var btnBuscar = Estilo.Boton("Buscar", 370, 10, 90, 32, true);
            btnBuscar.Click += (s, e) => Buscar();

            txtFiltro = Estilo.Campo(530, 12, 130);
            txtFiltro.TextChanged += (s, e) => Refrescar();

            barra.Controls.AddRange(new Control[]
            {
                Estilo.Etiqueta("Desde", 0, 16), dtDesde,
                Estilo.Etiqueta("Hasta", 185, 16), dtHasta,
                btnBuscar,
                Estilo.Etiqueta("Filtrar", 475, 16), txtFiltro
            });

            padre.Controls.Add(barra);
        }

        // ---------- Datos ----------

        private void Buscar()
        {
            Cargar(dtDesde.Value.Date, dtHasta.Value.Date);
        }

        private void Cargar(DateTime desde, DateTime hasta)
        {
            // Se recuerda el rango realmente buscado para ponerlo en el reporte
            desdeUsado = desde;
            hastaUsado = hasta;

            try
            {
                ventas = negocio.Listar(desde, hasta);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ventas", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ventas = new List<Venta>();
            }
            Refrescar();
        }

        private string SubtituloReporte()
        {
            DateTime a = desdeUsado <= hastaUsado ? desdeUsado : hastaUsado;
            DateTime b = desdeUsado <= hastaUsado ? hastaUsado : desdeUsado;
            string texto = "Del " + a.ToString("dd/MM/yyyy") + " al " + b.ToString("dd/MM/yyyy");
            if (!negocio.VeTodas) texto += "  ·  Solo mis ventas";

            string filtro = txtFiltro.Text.Trim();
            if (filtro.Length > 0) texto += "  ·  Filtro: " + filtro;
            return texto;
        }

        private void Refrescar()
        {
            cargando = true;
            string filtro = txtFiltro.Text.Trim().ToLower();

            var vista = ventas.Where(v => filtro.Length == 0
                || (v.NumeroDocumento ?? "").ToLower().Contains(filtro)
                || (v.ClienteNombre ?? "").ToLower().Contains(filtro)
                || (v.UsuarioNombre ?? "").ToLower().Contains(filtro)
                || (v.MetodoPago ?? "").ToLower().Contains(filtro)
                || (v.ReferenciaPago ?? "").ToLower().Contains(filtro)
                || (v.Anulada ? "anulada" : "vigente").Contains(filtro)).ToList();

            // Las anuladas se ven en la lista pero no suman
            var vigentes = vista.Where(v => !v.Anulada).ToList();
            int anuladas = vista.Count - vigentes.Count;

            lblResumen.Text = vigentes.Count + " ventas  ·  " + vigentes.Sum(v => v.MontoTotal).ToString("C2")
                              + (anuladas > 0 ? "  ·  " + anuladas + (anuladas == 1 ? " anulada" : " anuladas") : "")
                              + (negocio.VeTodas ? "" : "  (tus ventas)");

            gridVentas.Rows.Clear();
            foreach (var v in vista)
            {
                // En las anuladas el dinero se muestra en cero: así los totales de Excel y PDF salen bien
                int i = gridVentas.Rows.Add(v.FechaRegistro.ToString("dd/MM/yyyy HH:mm"),
                                            v.NumeroDocumento, v.ClienteNombre, v.MetodoPago,
                                            v.Anulada ? 0m : v.MontoDescuento, v.UsuarioNombre,
                                            v.Anulada ? "ANULADA" : "Vigente",
                                            v.Anulada ? 0m : v.MontoTotal);
                var fila = gridVentas.Rows[i];
                fila.Tag = v;

                if (v.Anulada)
                {
                    fila.DefaultCellStyle.ForeColor = Color.Gray;
                    fila.DefaultCellStyle.Font = Tachado;
                }
            }

            gridVentas.ClearSelection();
            gridDetalle.Rows.Clear();
            ventaActual = null;
            detalleActual = new List<DetalleVenta>();
            btnComprobante.Enabled = false;
            btnTicket.Enabled = false;
            btnAnular.Enabled = false;
            lblTituloDetalle.Text = "Selecciona una venta para ver su detalle";
            cargando = false;
        }

        private void MostrarDetalle(Venta v)
        {
            List<DetalleVenta> detalle;
            try
            {
                detalle = negocio.ObtenerDetalle(v.IdVenta);
                v.Pagos = negocio.ObtenerPagos(v.IdVenta);   // una forma de pago, o dos si fue mixta
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ventas", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ventaActual = v;
            detalleActual = detalle;

            // Una venta anulada ya no se puede volver a anular, reimprimir ni dar como comprobante
            bool vigente = !v.Anulada;
            btnComprobante.Enabled = vigente;
            btnTicket.Enabled = vigente;
            btnAnular.Enabled = vigente;

            if (v.Anulada)
            {
                lblTituloDetalle.Text = "ANULADA el " + (v.FechaAnulacion.HasValue ? v.FechaAnulacion.Value.ToString("dd/MM/yyyy HH:mm") : "")
                                        + " por " + v.AnuladaPor + "  ·  Motivo: " + v.MotivoAnulacion
                                        + "  ·  " + v.NumeroDocumento + " por " + v.MontoTotal.ToString("C2");
            }
            else
            {
                string titulo = "Detalle de " + v.NumeroDocumento + "  ·  " + v.ClienteNombre + "  ·  " + TextoPago(v);
                if (v.MontoDescuento > 0)
                    titulo += "  ·  Descuento " + v.DescuentoPorcentaje.ToString("0.##") + "% (-" + v.MontoDescuento.ToString("C2") + ")";
                lblTituloDetalle.Text = titulo;
            }

            gridDetalle.Rows.Clear();
            foreach (var d in detalle)
                gridDetalle.Rows.Add(d.ProductoCodigo, d.ProductoNombre, d.Cantidad, d.PrecioVenta, d.SubTotal);
            gridDetalle.ClearSelection();
        }

        // ---------- Acciones ----------
        // «Efectivo», «Tarjeta (ref. 123)» o, si fue mixto, «Transferencia $100.00 + Efectivo $250.00»
        private static string TextoPago(Venta v)
        {
            if (v.Pagos != null && v.Pagos.Count > 1)
                return string.Join(" + ", v.Pagos.Select(p =>
                    p.MetodoPago + " " + p.Monto.ToString("C2")
                    + (string.IsNullOrEmpty(p.ReferenciaPago) ? "" : " (ref. " + p.ReferenciaPago + ")")));

            string texto = v.MetodoPago;
            if (!string.IsNullOrEmpty(v.ReferenciaPago)) texto += " (ref. " + v.ReferenciaPago + ")";
            return texto;
        }
        private void Comprobante()
        {
            if (ventaActual == null) return;

            var venta = ventaActual;
            var detalle = detalleActual;
            Exportar.GuardarPdf("Venta_" + venta.NumeroDocumento,
                ruta => ReportePdf.ComprobanteVenta(venta, detalle, ruta));
        }

        private void ImprimirTicket()
        {
            if (ventaActual == null) return;

            try
            {
                Ticket.Imprimir(ventaActual, detalleActual, true);   // true = sale marcado como reimpresión
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo imprimir el ticket.\n\n" + ex.Message,
                    "Ticket", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Anular()
        {
            if (ventaActual == null || ventaActual.Anulada) return;

            var venta = ventaActual;
            string resumen = "Vas a anular la venta " + venta.NumeroDocumento + " por "
                           + venta.MontoTotal.ToString("C2") + " (" + venta.ClienteNombre + ").\n\n"
                           + "Los productos regresan al inventario. La venta queda en el historial "
                           + "marcada como anulada y esto no se puede deshacer.";

            string motivo;
            using (var f = new FrmMotivo("Anular venta", resumen))
            {
                if (f.ShowDialog(this) != DialogResult.OK) return;
                motivo = f.Motivo;
            }

            bool ok;
            string mensaje;
            try
            {
                ok = anulacion.AnularVenta(venta.IdVenta, motivo, out mensaje);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ventas", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!ok)
            {
                MessageBox.Show(mensaje, "No se pudo anular", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Cargar(desdeUsado, hastaUsado);   // por si alguien más ya la había anulado
                return;
            }

            MessageBox.Show("Venta " + venta.NumeroDocumento + " anulada.\nLos productos ya regresaron al inventario.",
                "Listo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Cargar(desdeUsado, hastaUsado);
        }
    }
}