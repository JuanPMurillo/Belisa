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
        private readonly CompraN negocio = new CompraN();
        private List<Compra> compras = new List<Compra>();
        private bool cargando;

        private DateTimePicker dtDesde, dtHasta;
        private TextBox txtFiltro;
        private Label lblResumen, lblTituloDetalle;
        private DataGridView gridCompras, gridDetalle;

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
            Tabla.ColumnaFija(gridCompras, "Tipo", "Documento", 140, null, false);
            Tabla.Columna(gridCompras, "Usuario", "Registró", 110);
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
            var abajo = new Panel { Dock = DockStyle.Bottom, Height = 280, Padding = new Padding(0, 12, 0, 0) };
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
                Dock = DockStyle.Top,
                Height = 32,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = Estilo.Negrita,
                ForeColor = Estilo.Texto,
                Text = "Selecciona una compra para ver su detalle"
            };

            tarjeta.Controls.Add(gridDetalle);        // Fill primero
            tarjeta.Controls.Add(lblTituloDetalle);   // luego arriba
            abajo.Controls.Add(tarjeta);
            padre.Controls.Add(abajo);
        }

        private void ConstruirBarra(Panel padre)
        {
            var barra = new Panel { Dock = DockStyle.Top, Height = 52 };

            lblResumen = new Label
            {
                Dock = DockStyle.Right,
                Width = 250,
                TextAlign = ContentAlignment.MiddleRight,
                Font = Estilo.Normal,
                ForeColor = Estilo.Texto
            };
            barra.Controls.Add(lblResumen);

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
            try
            {
                compras = negocio.Listar(dtDesde.Value, dtHasta.Value);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Compras", MessageBoxButtons.OK, MessageBoxIcon.Error);
                compras = new List<Compra>();
            }
            Refrescar();
        }

        private void Refrescar()
        {
            cargando = true;
            string filtro = txtFiltro.Text.Trim().ToLower();

            var vista = compras.Where(c => filtro.Length == 0
                || (c.NumeroDocumento ?? "").ToLower().Contains(filtro)
                || (c.ProveedorNombre ?? "").ToLower().Contains(filtro)
                || (c.UsuarioNombre ?? "").ToLower().Contains(filtro)
                || (c.TipoDocumento ?? "").ToLower().Contains(filtro)).ToList();

            lblResumen.Text = vista.Count + " compras  ·  " + vista.Sum(c => c.MontoTotal).ToString("C2");

            gridCompras.Rows.Clear();
            foreach (var c in vista)
            {
                int i = gridCompras.Rows.Add(c.FechaRegistro.ToString("dd/MM/yyyy HH:mm"),
                                             c.NumeroDocumento, c.ProveedorNombre, c.TipoDocumento,
                                             c.UsuarioNombre, c.MontoTotal);
                gridCompras.Rows[i].Tag = c;
            }

            gridCompras.ClearSelection();
            gridDetalle.Rows.Clear();
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

            lblTituloDetalle.Text = "Detalle de " + c.NumeroDocumento + "  ·  " + c.ProveedorNombre;

            gridDetalle.Rows.Clear();
            foreach (var d in detalle)
            {
                gridDetalle.Rows.Add(d.ProductoCodigo, d.ProductoNombre, d.Cantidad,
                                     d.PrecioCompra, d.PrecioVenta, d.MontoTotal);
            }
            gridDetalle.ClearSelection();
        }
    }
}