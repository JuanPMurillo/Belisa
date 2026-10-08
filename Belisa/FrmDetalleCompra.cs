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
    public class FrmDetalleCompra : Form
    {
        private static readonly Font Tachado = new Font(Estilo.Normal, FontStyle.Strikeout);

        private readonly CompraN negocio = new CompraN();
        private readonly AnulacionN anulacion = new AnulacionN();
        private List<Compra> compras = new List<Compra>();
        private Compra compraActual;
        private List<DetalleCompra> detalleActual = new List<DetalleCompra>();
        private DateTime desdeUsado = DateTime.Today, hastaUsado = DateTime.Today;
        private bool cargando;

        private DateTimePicker dtDesde, dtHasta;
        private TextBox txtFiltro;
        private Label lblResumen, lblTituloDetalle;
        private DataGridView gridCompras, gridDetalle;
        private Button btnComprobante, btnAnular;

        public FrmDetalleCompra()
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
            gridCompras = Tabla.Crear();
            Tabla.ColumnaFija(gridCompras, "Fecha", "Fecha", 150, null, false);
            Tabla.ColumnaFija(gridCompras, "Numero", "Número", 110, null, false);
            Tabla.Columna(gridCompras, "Proveedor", "Proveedor", 160);
            Tabla.ColumnaFija(gridCompras, "Tipo", "Documento", 130, null, false);
            Tabla.ColumnaFija(gridCompras, "Pago", "Pago", 110, null, false);
            Tabla.Columna(gridCompras, "Usuario", "Registró", 110);
            Tabla.ColumnaFija(gridCompras, "Estado", "Estado", 100, null, false);
            Tabla.ColumnaFija(gridCompras, "Total", "Total", 120, "C2", true);

            gridCompras.SelectionChanged += (s, e) =>
            {
                if (cargando || gridCompras.CurrentRow == null) return;
                var c = gridCompras.CurrentRow.Tag as Compra;
                if (c != null) MostrarDetalle(c);
            };

            padre.Controls.Add(gridCompras);
        }

        private void ConstruirDetalle(Panel padre)
        {
            var abajo = new Panel { Dock = DockStyle.Bottom, Height = 290, Padding = new Padding(0, 12, 0, 0) };
            var tarjeta = new Panel { Dock = DockStyle.Fill };

            gridDetalle = Tabla.Crear();
            Tabla.Columna(gridDetalle, "Codigo", "Código", 70);
            Tabla.Columna(gridDetalle, "Producto", "Producto", 180);
            Tabla.ColumnaFija(gridDetalle, "Cantidad", "Cantidad", 90, null, true);
            Tabla.ColumnaFija(gridDetalle, "Compra", "P. compra", 110, "C2", true);
            Tabla.ColumnaFija(gridDetalle, "Venta", "P. venta", 110, "C2", true);
            Tabla.ColumnaFija(gridDetalle, "Subtotal", "Subtotal", 120, "C2", true);

            lblTituloDetalle = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true,
                Font = Estilo.Negrita,
                ForeColor = Estilo.Texto,
                Text = "Selecciona una compra para ver su detalle"
            };

            btnAnular = Estilo.Boton("Anular compra", 0, 2, 150, 34, false);
            btnAnular.ForeColor = Estilo.Error;
            btnAnular.FlatAppearance.BorderColor = Estilo.Error;
            btnAnular.Visible = Sesion.TienePermiso("anular");   // solo el administrador
            btnAnular.Enabled = false;
            btnAnular.Click += (s, e) => Anular();

            btnComprobante = Estilo.Boton("Comprobante PDF", 160, 2, 170, 34, false);
            btnComprobante.Enabled = false;
            btnComprobante.Click += (s, e) => Comprobante();

            var panelBotones = new Panel { Dock = DockStyle.Right, Width = 338 };
            panelBotones.Controls.Add(btnAnular);
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
                Width = 400,
                TextAlign = ContentAlignment.MiddleRight,
                Font = Estilo.Normal,
                ForeColor = Estilo.Texto
            };
            barra.Controls.Add(lblResumen);
            barra.Controls.Add(Exportar.BotonExcel(gridCompras, "Compras"));
            barra.Controls.Add(Exportar.BotonPdf(gridCompras, "Reporte de compras", SubtituloReporte));

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
                compras = negocio.Listar(desde, hasta);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Compras", MessageBoxButtons.OK, MessageBoxIcon.Error);
                compras = new List<Compra>();
            }
            Refrescar();
        }

        private string SubtituloReporte()
        {
            DateTime a = desdeUsado <= hastaUsado ? desdeUsado : hastaUsado;
            DateTime b = desdeUsado <= hastaUsado ? hastaUsado : desdeUsado;
            string texto = "Del " + a.ToString("dd/MM/yyyy") + " al " + b.ToString("dd/MM/yyyy");

            string filtro = txtFiltro.Text.Trim();
            if (filtro.Length > 0) texto += "  ·  Filtro: " + filtro;
            return texto;
        }

        private void Refrescar()
        {
            cargando = true;
            string filtro = txtFiltro.Text.Trim().ToLower();

            var vista = compras.Where(c => filtro.Length == 0
                || (c.NumeroDocumento ?? "").ToLower().Contains(filtro)
                || (c.ProveedorNombre ?? "").ToLower().Contains(filtro)
                || (c.UsuarioNombre ?? "").ToLower().Contains(filtro)
                || (c.TipoDocumento ?? "").ToLower().Contains(filtro)
                || (c.MetodoPago ?? "").ToLower().Contains(filtro)
                || (c.ReferenciaPago ?? "").ToLower().Contains(filtro)
                || (c.Anulada ? "anulada" : "vigente").Contains(filtro)).ToList();

            // Las anuladas se ven en la lista pero no suman
            var vigentes = vista.Where(c => !c.Anulada).ToList();
            int anuladas = vista.Count - vigentes.Count;

            lblResumen.Text = vigentes.Count + " compras  ·  " + vigentes.Sum(c => c.MontoTotal).ToString("C2")
                              + (anuladas > 0 ? "  ·  " + anuladas + (anuladas == 1 ? " anulada" : " anuladas") : "");

            gridCompras.Rows.Clear();
            foreach (var c in vista)
            {
                // En las anuladas el dinero se muestra en cero: así los totales de Excel y PDF salen bien
                int i = gridCompras.Rows.Add(c.FechaRegistro.ToString("dd/MM/yyyy HH:mm"),
                                             c.NumeroDocumento, c.ProveedorNombre, c.TipoDocumento,
                                             c.MetodoPago, c.UsuarioNombre,
                                             c.Anulada ? "ANULADA" : "Vigente",
                                             c.Anulada ? 0m : c.MontoTotal);
                var fila = gridCompras.Rows[i];
                fila.Tag = c;

                if (c.Anulada)
                {
                    fila.DefaultCellStyle.ForeColor = Color.Gray;
                    fila.DefaultCellStyle.Font = Tachado;
                }
            }

            gridCompras.ClearSelection();
            gridDetalle.Rows.Clear();
            compraActual = null;
            detalleActual = new List<DetalleCompra>();
            btnComprobante.Enabled = false;
            btnAnular.Enabled = false;
            lblTituloDetalle.Text = "Selecciona una compra para ver su detalle";
            cargando = false;
        }

        private void MostrarDetalle(Compra c)
        {
            List<DetalleCompra> detalle;
            try
            {
                detalle = negocio.ObtenerDetalle(c.IdCompra);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Compras", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            compraActual = c;
            detalleActual = detalle;

            bool vigente = !c.Anulada;
            btnComprobante.Enabled = vigente;
            btnAnular.Enabled = vigente;

            if (c.Anulada)
            {
                lblTituloDetalle.Text = "ANULADA el " + (c.FechaAnulacion.HasValue ? c.FechaAnulacion.Value.ToString("dd/MM/yyyy HH:mm") : "")
                                        + " por " + c.AnuladaPor + "  ·  Motivo: " + c.MotivoAnulacion
                                        + "  ·  " + c.NumeroDocumento + " por " + c.MontoTotal.ToString("C2");
            }
            else
            {
                string titulo = "Detalle de " + c.NumeroDocumento + "  ·  " + c.ProveedorNombre + "  ·  " + c.MetodoPago;
                if (!string.IsNullOrEmpty(c.ReferenciaPago)) titulo += " (ref. " + c.ReferenciaPago + ")";
                lblTituloDetalle.Text = titulo;
            }

            gridDetalle.Rows.Clear();
            foreach (var d in detalle)
            {
                gridDetalle.Rows.Add(d.ProductoCodigo, d.ProductoNombre, d.Cantidad,
                                     d.PrecioCompra, d.PrecioVenta, d.MontoTotal);
            }
            gridDetalle.ClearSelection();
        }

        // ---------- Acciones ----------

        private void Comprobante()
        {
            if (compraActual == null) return;

            var compra = compraActual;
            var detalle = detalleActual;
            Exportar.GuardarPdf("Compra_" + compra.NumeroDocumento,
                ruta => ReportePdf.ComprobanteCompra(compra, detalle, ruta));
        }

        private void Anular()
        {
            if (compraActual == null || compraActual.Anulada) return;

            var compra = compraActual;
            string resumen = "Vas a anular la compra " + compra.NumeroDocumento + " a " + compra.ProveedorNombre
                           + " por " + compra.MontoTotal.ToString("C2") + ".\n\n"
                           + "Los productos de esta compra salen del inventario (si ya vendiste parte de ellos, "
                           + "el sistema no te deja). Los precios de los productos NO regresan a como estaban. "
                           + "Esto no se puede deshacer.";

            string motivo;
            using (var f = new FrmMotivo("Anular compra", resumen))
            {
                if (f.ShowDialog(this) != DialogResult.OK) return;
                motivo = f.Motivo;
            }

            bool ok;
            string mensaje;
            try
            {
                ok = anulacion.AnularCompra(compra.IdCompra, motivo, out mensaje);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Compras", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!ok)
            {
                MessageBox.Show(mensaje, "No se pudo anular", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Cargar(desdeUsado, hastaUsado);   // por si alguien más ya la había anulado
                return;
            }

            MessageBox.Show("Compra " + compra.NumeroDocumento + " anulada.\nLos productos ya salieron del inventario.",
                "Listo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Cargar(desdeUsado, hastaUsado);
        }
    }
}