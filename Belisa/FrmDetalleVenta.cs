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
        private readonly VentaN negocio = new VentaN();
        private List<Venta> ventas = new List<Venta>();
        private Venta ventaActual;
        private List<DetalleVenta> detalleActual = new List<DetalleVenta>();
        private DateTime desdeUsado = DateTime.Today, hastaUsado = DateTime.Today;
        private bool cargando;

        private DateTimePicker dtDesde, dtHasta;
        private TextBox txtFiltro;
        private Label lblResumen, lblTituloDetalle;
        private DataGridView gridVentas, gridDetalle;
        private Button btnComprobante, btnTicket;

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
                Font = Estilo.Negrita,
                ForeColor = Estilo.Texto,
                Text = "Selecciona una venta para ver su detalle"
            };

            btnTicket = Estilo.Boton("Imprimir ticket", 0, 2, 170, 34, false);
            btnTicket.Enabled = false;
            btnTicket.Click += (s, e) => ImprimirTicket();

            btnComprobante = Estilo.Boton("Comprobante PDF", 180, 2, 170, 34, false);
            btnComprobante.Enabled = false;
            btnComprobante.Click += (s, e) => Comprobante();

            var panelBotones = new Panel { Dock = DockStyle.Right, Width = 358 };
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
                Width = 320,
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
            // Se recuerda el rango realmente buscado para ponerlo en el reporte
            desdeUsado = dtDesde.Value.Date;
            hastaUsado = dtHasta.Value.Date;

            try
            {
                ventas = negocio.Listar(dtDesde.Value, dtHasta.Value);
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
                || (v.ReferenciaPago ?? "").ToLower().Contains(filtro)).ToList();

            lblResumen.Text = vista.Count + " ventas  ·  " + vista.Sum(v => v.MontoTotal).ToString("C2")
                              + (negocio.VeTodas ? "" : "  (tus ventas)");

            gridVentas.Rows.Clear();
            foreach (var v in vista)
            {
                int i = gridVentas.Rows.Add(v.FechaRegistro.ToString("dd/MM/yyyy HH:mm"),
                                            v.NumeroDocumento, v.ClienteNombre, v.MetodoPago,
                                            v.MontoDescuento, v.UsuarioNombre, v.MontoTotal);
                gridVentas.Rows[i].Tag = v;
            }

            gridVentas.ClearSelection();
            gridDetalle.Rows.Clear();
            ventaActual = null;
            detalleActual = new List<DetalleVenta>();
            btnComprobante.Enabled = false;
            btnTicket.Enabled = false;
            lblTituloDetalle.Text = "Selecciona una venta para ver su detalle";
            cargando = false;
        }

        private void MostrarDetalle(Venta v)
        {
            List<DetalleVenta> detalle;
            try
            {
                detalle = negocio.ObtenerDetalle(v.IdVenta);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ventas", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ventaActual = v;
            detalleActual = detalle;
            btnComprobante.Enabled = true;
            btnTicket.Enabled = true;

            string titulo = "Detalle de " + v.NumeroDocumento + "  ·  " + v.ClienteNombre + "  ·  " + v.MetodoPago;
            if (!string.IsNullOrEmpty(v.ReferenciaPago)) titulo += " (ref. " + v.ReferenciaPago + ")";
            if (v.MontoDescuento > 0)
                titulo += "  ·  Descuento " + v.DescuentoPorcentaje.ToString("0.##") + "% (-" + v.MontoDescuento.ToString("C2") + ")";
            lblTituloDetalle.Text = titulo;

            gridDetalle.Rows.Clear();
            foreach (var d in detalle)
                gridDetalle.Rows.Add(d.ProductoCodigo, d.ProductoNombre, d.Cantidad, d.PrecioVenta, d.SubTotal);
            gridDetalle.ClearSelection();
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
    }
}