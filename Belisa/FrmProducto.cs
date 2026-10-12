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
    public class FrmProducto : Form
    {
        private const int Maximo = 1000000;

        private readonly ProductoN negocio = new ProductoN();
        private readonly CategoriaN negocioCat = new CategoriaN();
        private readonly ToolTip ayuda = new ToolTip();

        private List<Producto> productos = new List<Producto>();
        private List<Categoria> categorias = new List<Categoria>();
        private Producto seleccion;     // null = se está creando uno nuevo
        private bool cargando;

        private DataGridView grid;
        private TextBox txtBuscar, txtNombre, txtDescripcion;
        private CheckBox chkStockBajo, chkActivo, chkServicio;
        private ComboBox cmbCategoria;
        private NumericUpDown numCompra, numVenta, numStock, numStockMin;
        private Label lblTitulo, lblMensaje, lblResumen, lblCompra, lblStock, lblStockMin;
        private Button btnGuardar, btnNuevo;

        public FrmProducto()
        {
            // 1. Evitar parpadeos y asegurar rendimiento visual fluido al redimensionar
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();

            // 2. Asegurar el escalado correcto de fuentes en distintas resoluciones
            AutoScaleMode = AutoScaleMode.Font;

            BackColor = Estilo.Fondo;
            ConstruirLista();        // primero el panel central (Fill)...
            ConstruirFormulario();   // ...y luego el lateral (Right)
            Load += (s, e) => { Cargar(); Nuevo(); };
        }

        // ---------- Construcción ----------

        private void ConstruirLista()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24), AutoScroll = true };

            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Estilo.LilaSuave,
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 40,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            grid.RowTemplate.Height = 36;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Estilo.LilaSuave;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Estilo.Texto;
            grid.ColumnHeadersDefaultCellStyle.Font = Estilo.Negrita;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Estilo.LilaSuave;
            grid.DefaultCellStyle.Font = Estilo.Normal;
            grid.DefaultCellStyle.ForeColor = Estilo.Texto;
            grid.DefaultCellStyle.SelectionBackColor = Estilo.LilaClaro;
            grid.DefaultCellStyle.SelectionForeColor = Estilo.Texto;

            grid.Columns.Add("Id", "Id");
            grid.Columns["Id"].Visible = false;
            grid.Columns.Add("Codigo", "Código");
            grid.Columns["Codigo"].FillWeight = 70;
            grid.Columns.Add("Nombre", "Nombre");
            grid.Columns["Nombre"].FillWeight = 160;
            grid.Columns.Add("Categoria", "Categoría");
            grid.Columns["Categoria"].FillWeight = 100;
            AgregarColumnaFija("Tipo", "Tipo", 90, null);
            AgregarColumnaFija("Stock", "Stock", 70, null);
            AgregarColumnaFija("Compra", "P. compra", 100, "C2");
            AgregarColumnaFija("Venta", "P. venta", 100, "C2");
            AgregarColumnaFija("Estado", "Estado", 90, null);

            grid.SelectionChanged += (s, e) =>
            {
                if (cargando || grid.CurrentRow == null) return;
                var p = grid.CurrentRow.Tag as Producto;
                if (p != null) Mostrar(p);
            };

            var barra = new Panel { Dock = DockStyle.Top, Height = 52 };
            lblResumen = new Label
            {
                Dock = DockStyle.Right,
                Width = 270,
                TextAlign = ContentAlignment.MiddleRight,
                Font = Estilo.Normal,
                ForeColor = Estilo.Texto
            };

            var btnExcel = Exportar.BotonExcel(grid, "Productos y servicios");
            barra.Controls.Add(lblResumen);
            barra.Controls.Add(btnExcel);

            // Controles optimizados y compactos para 1366x768
            // Controles organizados con espacio holgado
            barra.Controls.Add(Estilo.Etiqueta("Buscar", 0, 16));

            txtBuscar = Estilo.Campo(52, 12, 130);
            txtBuscar.TextChanged += (s, e) => Refrescar();
            barra.Controls.Add(txtBuscar);

            chkStockBajo = new CheckBox
            {
                Text = "Stock Bajo",
                Location = new Point(192, 15),
                AutoSize = true,
                Font = Estilo.Normal,
                ForeColor = Estilo.Texto
            };
            chkStockBajo.CheckedChanged += (s, e) => Refrescar();
            barra.Controls.Add(chkStockBajo);

            panel.Controls.Add(grid);
            panel.Controls.Add(barra);
            Controls.Add(panel);
        }

        private void AgregarColumnaFija(string nombre, string titulo, int ancho, string formato)
        {
            grid.Columns.Add(nombre, titulo);
            var col = grid.Columns[nombre];
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            col.Width = ancho;
            if (formato != null) col.DefaultCellStyle.Format = formato;
            if (nombre != "Estado" && nombre != "Tipo")
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }

        private static NumericUpDown Num(int x, int y, int ancho, int decimales, decimal max)
        {
            return new NumericUpDown
            {
                Location = new Point(x, y),
                Width = ancho,
                Font = Estilo.Normal,
                DecimalPlaces = decimales,
                Minimum = 0,
                Maximum = max,
                ThousandsSeparator = true,
                TextAlign = HorizontalAlignment.Right
            };
        }

        private void ConstruirFormulario()
        {
            var panel = new Panel { Dock = DockStyle.Right, Width = 400, BackColor = Color.White, AutoScroll = true };

            lblTitulo = new Label
            {
                Location = new Point(28, 14),
                AutoSize = true,
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Estilo.Texto
            };

            chkServicio = new CheckBox
            {
                Text = "Es un servicio (no maneja inventario)",
                Location = new Point(28, 54),
                AutoSize = true,
                Font = Estilo.Normal,
                ForeColor = Estilo.Texto
            };
            ayuda.SetToolTip(chkServicio,
                "Los servicios (por ejemplo, una depilación) no tienen stock, no se compran ni se ajustan.\n" +
                "El tipo se elige al crear y ya no se puede cambiar.");

            txtNombre = Estilo.Campo(28, 112, 344);
            txtNombre.MaxLength = 100;

            cmbCategoria = new ComboBox
            {
                Location = new Point(28, 178),
                Width = 344,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = Estilo.Normal,
                DisplayMember = "Descripcion",
                ValueMember = "IdCategoria"
            };

            numCompra = Num(28, 244, 164, 2, 99999999.99m);
            numVenta = Num(212, 244, 160, 2, 99999999.99m);
            numStock = Num(28, 310, 164, 0, Maximo);
            numStockMin = Num(212, 310, 160, 0, Maximo);

            lblCompra = Estilo.Etiqueta("Precio de compra", 28, 220);
            lblStock = Estilo.Etiqueta("Stock inicial", 28, 286);
            lblStockMin = Estilo.Etiqueta("Stock mínimo", 212, 286);
            ayuda.SetToolTip(lblStock, "El stock solo se captura al crear el producto.\nDespués cambia con las compras y las ventas.");
            ayuda.SetToolTip(lblStockMin, "Cuando el stock llegue a este número, el producto se marca como stock bajo.");
            ayuda.SetToolTip(numStockMin, "Cuando el stock llegue a este número, el producto se marca como stock bajo.");

            txtDescripcion = Estilo.Campo(28, 376, 344);
            txtDescripcion.MaxLength = 200;
            txtDescripcion.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; Guardar(); }
            };

            chkActivo = new CheckBox
            {
                Text = "Activo",
                Location = new Point(28, 418),
                AutoSize = true,
                Font = Estilo.Normal,
                ForeColor = Estilo.Texto
            };

            lblMensaje = new Label
            {
                Location = new Point(28, 448),
                Size = new Size(344, 40),
                Font = Estilo.Normal
            };

            btnGuardar = Estilo.Boton("Guardar", 28, 496, 210, 42, true);
            btnGuardar.Click += (s, e) => Guardar();

            btnNuevo = Estilo.Boton("Nuevo", 250, 496, 122, 42, false);
            btnNuevo.Click += (s, e) => Nuevo();

            panel.Controls.AddRange(new Control[]
            {
                lblTitulo, chkServicio,
                Estilo.Etiqueta("Nombre", 28, 88), txtNombre,
                Estilo.Etiqueta("Categoría", 28, 154), cmbCategoria,
                lblCompra, numCompra,
                Estilo.Etiqueta("Precio de venta", 212, 220), numVenta,
                lblStock, numStock, lblStockMin, numStockMin,
                Estilo.Etiqueta("Descripción (opcional)", 28, 352), txtDescripcion,
                chkActivo, lblMensaje, btnGuardar, btnNuevo,
                new Panel { Dock = DockStyle.Left, Width = 1, BackColor = Estilo.LilaClaro }
            });

            Controls.Add(panel);

            // Se conecta al final, cuando ya existen todos los controles
            chkServicio.CheckedChanged += (s, e) => ActualizarTipo();
        }

        // ---------- Datos ----------

        private void Cargar()
        {
            try
            {
                productos = negocio.Listar();
                categorias = negocioCat.Listar();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Productos", MessageBoxButtons.OK, MessageBoxIcon.Error);
                productos = new List<Producto>();
                categorias = new List<Categoria>();
            }
            Refrescar();
        }

        // Los servicios no manejan inventario: nunca cuentan como stock bajo
        private static bool StockBajo(Producto p)
        {
            return p.Estado && !p.EsServicio && p.Stock <= p.StockMinimo;
        }

        // Redibuja la tabla aplicando búsqueda y filtro de stock bajo
        private void Refrescar()
        {
            cargando = true;
            string filtro = txtBuscar.Text.Trim().ToLower();
            int? idActual = seleccion == null ? (int?)null : seleccion.IdProducto;

            var vista = productos.Where(x =>
                (filtro.Length == 0
                    || x.Codigo.ToLower().Contains(filtro)
                    || x.Nombre.ToLower().Contains(filtro)
                    || (x.CategoriaDescripcion ?? "").ToLower().Contains(filtro)
                    || (x.EsServicio ? "servicio" : "producto").Contains(filtro))
                && (!chkStockBajo.Checked || StockBajo(x))).ToList();

            lblResumen.Text = vista.Count + " de " + productos.Count + " registros  ·  "
                            + productos.Count(StockBajo) + " con stock bajo";

            grid.Rows.Clear();
            foreach (var p in vista)
            {
                int i = grid.Rows.Add(p.IdProducto, p.Codigo, p.Nombre, p.CategoriaDescripcion,
                                   p.EsServicio ? "Servicio" : "Producto",
                                   p.EsServicio ? (object)"—" : p.Stock,
                                   p.PrecioCompra, p.PrecioVenta,
                                   p.Estado ? "Activo" : "Inactivo");
                var fila = grid.Rows[i];
                fila.Tag = p;

                if (!p.Estado)
                {
                    fila.DefaultCellStyle.ForeColor = Color.Gray;
                }
                else if (!p.EsServicio && p.Stock <= 0)
                {
                    fila.Cells["Stock"].Style.ForeColor = Estilo.Error;
                    fila.Cells["Stock"].Style.Font = Estilo.Negrita;
                }
                else if (!p.EsServicio && p.Stock <= p.StockMinimo)
                {
                    fila.Cells["Stock"].Style.ForeColor = Color.FromArgb(176, 110, 10);
                    fila.Cells["Stock"].Style.Font = Estilo.Negrita;
                }
            }

            grid.ClearSelection();
            if (idActual.HasValue)
            {
                foreach (DataGridViewRow fila in grid.Rows)
                {
                    if (((Producto)fila.Tag).IdProducto == idActual.Value)
                    {
                        fila.Selected = true;
                        grid.CurrentCell = fila.Cells["Codigo"];
                        break;
                    }
                }
            }
            cargando = false;
        }

        // ---------- Formulario ----------

        // Solo categorías activas (más la del producto, si ya la tenía y se desactivó)
        private void LlenarCategorias(int? incluirId)
        {
            cmbCategoria.DataSource = categorias
                .Where(c => c.Estado || (incluirId.HasValue && c.IdCategoria == incluirId.Value))
                .ToList();
        }

        // Un servicio no tiene stock: se ocultan esos campos y el precio de compra pasa a ser un costo estimado
        private void ActualizarTipo()
        {
            bool servicio = chkServicio.Checked;

            lblStock.Visible = !servicio;
            numStock.Visible = !servicio;
            lblStockMin.Visible = !servicio;
            numStockMin.Visible = !servicio;

            lblCompra.Text = servicio ? "Costo estimado (opcional)" : "Precio de compra";

            if (seleccion == null)
                lblTitulo.Text = servicio ? "Nuevo servicio" : "Nuevo producto";
        }

        private void Mostrar(Producto p)
        {
            seleccion = p;

            chkServicio.Checked = p.EsServicio;
            chkServicio.Enabled = false;        // el tipo no se cambia después de crear
            lblTitulo.Text = p.EsServicio ? "Editar servicio" : "Editar producto";

            txtNombre.Text = p.Nombre;
            LlenarCategorias(p.IdCategoria);
            cmbCategoria.SelectedValue = p.IdCategoria;
            numCompra.Value = p.PrecioCompra;
            numVenta.Value = p.PrecioVenta;
            lblStock.Text = "Stock actual";
            numStock.Value = Math.Min(p.Stock, Maximo);
            numStock.Enabled = false;              // el stock no se edita a mano
            numStockMin.Value = Math.Min(p.StockMinimo, Maximo);
            txtDescripcion.Text = p.Descripcion;
            chkActivo.Checked = p.Estado;
            lblMensaje.Text = "";

            ActualizarTipo();
        }

        private void Nuevo()
        {
            seleccion = null;

            chkServicio.Checked = false;
            chkServicio.Enabled = true;

            txtNombre.Clear();
            LlenarCategorias(null);
            cmbCategoria.SelectedIndex = -1;
            numCompra.Value = 0;
            numVenta.Value = 0;
            lblStock.Text = "Stock inicial";
            numStock.Value = 0;
            numStock.Enabled = true;
            numStockMin.Value = 5;
            txtDescripcion.Clear();
            chkActivo.Checked = true;
            lblMensaje.Text = "";

            cargando = true;                    // evita que el evento de la tabla vuelva a cargar la fila
            grid.CurrentCell = null;
            grid.ClearSelection();
            cargando = false;

            ActualizarTipo();
            txtNombre.Focus();
        }

        private void Guardar()
        {
            lblMensaje.Text = "";
            bool esNuevo = seleccion == null;
            bool servicio = chkServicio.Checked;

            var p = new Producto
            {
                IdProducto = esNuevo ? 0 : seleccion.IdProducto,
                Nombre = txtNombre.Text,
                IdCategoria = cmbCategoria.SelectedValue == null ? 0 : (int)cmbCategoria.SelectedValue,
                EsServicio = servicio,
                Stock = servicio ? 0 : (int)numStock.Value,
                StockMinimo = servicio ? 0 : (int)numStockMin.Value,
                PrecioCompra = numCompra.Value,
                PrecioVenta = numVenta.Value,
                Descripcion = txtDescripcion.Text,
                Estado = chkActivo.Checked
            };

            if (p.PrecioVenta > 0 && p.PrecioVenta < p.PrecioCompra &&
                MessageBox.Show(servicio
                        ? "El precio de venta es menor al costo estimado.\n¿Guardar de todos modos?"
                        : "El precio de venta es menor al de compra.\n¿Guardar de todos modos?",
                    "Precio", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                string mensaje;
                if (!negocio.Guardar(p, out mensaje))
                {
                    lblMensaje.ForeColor = Estilo.Error;
                    lblMensaje.Text = mensaje;
                    return;
                }

                seleccion = new Producto { IdProducto = p.IdProducto, EsServicio = servicio };   // para que se quede marcado
                Cargar();

                var guardado = productos.FirstOrDefault(x => x.IdProducto == p.IdProducto);
                if (guardado != null) Mostrar(guardado); else Nuevo();

                lblMensaje.ForeColor = Color.FromArgb(39, 130, 80);
                lblMensaje.Text = "Guardado correctamente."
                                + (esNuevo && guardado != null ? " Código asignado: " + guardado.Codigo : "");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Productos", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}