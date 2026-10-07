using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BelisaEntidad;
using BelisaNegocio;

namespace Belisa
{
    public class FrmProducto : Form
    {
        private readonly ProductoN negocio = new ProductoN();
        private readonly CategoriaN negocioCat = new CategoriaN();
        private readonly ToolTip ayuda = new ToolTip();

        private List<Producto> productos = new List<Producto>();
        private List<Categoria> categorias = new List<Categoria>();
        private Producto seleccion;     // null = se está creando uno nuevo
        private bool cargando;

        private DataGridView grid;
        private TextBox txtBuscar, txtCodigo, txtNombre, txtDescripcion;
        private CheckBox chkStockBajo, chkActivo;
        private ComboBox cmbCategoria;
        private NumericUpDown numCompra, numVenta, numStock, numStockMin;
        private Label lblTitulo, lblMensaje, lblResumen, lblStock;
        private Button btnGuardar, btnNuevo;

        public FrmProducto()
        {
            BackColor = Estilo.Fondo;
            ConstruirLista();        // primero el panel central (Fill)...
            ConstruirFormulario();   // ...y luego el lateral (Right)
            Load += (s, e) => { Cargar(); Nuevo(); };
        }

        // ---------- Construcción ----------

        private void ConstruirLista()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24) };

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
                Width = 300,
                TextAlign = ContentAlignment.MiddleRight,
                Font = Estilo.Normal,
                ForeColor = Estilo.Texto
            };
            barra.Controls.Add(lblResumen);
            barra.Controls.Add(Exportar.BotonExcel(grid, "Productos"));
            barra.Controls.Add(Estilo.Etiqueta("Buscar", 0, 16));

            txtBuscar = Estilo.Campo(70, 12, 280);
            txtBuscar.TextChanged += (s, e) => Refrescar();
            barra.Controls.Add(txtBuscar);

            chkStockBajo = new CheckBox
            {
                Text = "Solo stock bajo",
                Location = new Point(368, 15),
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
            if (nombre != "Estado") col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
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
                Location = new Point(28, 18),
                AutoSize = true,
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Estilo.Texto
            };

            txtCodigo = Estilo.Campo(28, 86, 344);
            txtCodigo.MaxLength = 100;
            txtCodigo.KeyDown += (s, e) =>
            {
                // El lector de código de barras termina con Enter: pasamos al nombre
                if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; txtNombre.Focus(); }
            };

            txtNombre = Estilo.Campo(28, 152, 344);
            txtNombre.MaxLength = 100;

            cmbCategoria = new ComboBox
            {
                Location = new Point(28, 218),
                Width = 344,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = Estilo.Normal,
                DisplayMember = "Descripcion",
                ValueMember = "IdCategoria"
            };

            numCompra = Num(28, 284, 164, 2, 99999999.99m);
            numVenta = Num(212, 284, 160, 2, 99999999.99m);
            numStock = Num(28, 350, 164, 0, 1000000);
            numStockMin = Num(212, 350, 160, 0, 1000000);

            lblStock = Estilo.Etiqueta("Stock inicial", 28, 326);
            var lblStockMin = Estilo.Etiqueta("Stock mínimo", 212, 326);
            ayuda.SetToolTip(lblStock, "El stock solo se captura al crear el producto.\nDespués cambia con las compras y las ventas.");
            ayuda.SetToolTip(lblStockMin, "Cuando el stock llegue a este número, el producto se marca como stock bajo.");
            ayuda.SetToolTip(numStockMin, "Cuando el stock llegue a este número, el producto se marca como stock bajo.");

            txtDescripcion = Estilo.Campo(28, 416, 344);
            txtDescripcion.MaxLength = 200;
            txtDescripcion.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; Guardar(); }
            };

            chkActivo = new CheckBox
            {
                Text = "Producto activo",
                Location = new Point(28, 458),
                AutoSize = true,
                Font = Estilo.Normal,
                ForeColor = Estilo.Texto
            };

            lblMensaje = new Label
            {
                Location = new Point(28, 488),
                Size = new Size(344, 40),
                Font = Estilo.Normal
            };

            btnGuardar = Estilo.Boton("Guardar", 28, 536, 210, 42, true);
            btnGuardar.Click += (s, e) => Guardar();

            btnNuevo = Estilo.Boton("Nuevo", 250, 536, 122, 42, false);
            btnNuevo.Click += (s, e) => Nuevo();

            panel.Controls.AddRange(new Control[]
            {
                lblTitulo,
                Estilo.Etiqueta("Código", 28, 62), txtCodigo,
                Estilo.Etiqueta("Nombre", 28, 128), txtNombre,
                Estilo.Etiqueta("Categoría", 28, 194), cmbCategoria,
                Estilo.Etiqueta("Precio de compra", 28, 260), numCompra,
                Estilo.Etiqueta("Precio de venta", 212, 260), numVenta,
                lblStock, numStock, lblStockMin, numStockMin,
                Estilo.Etiqueta("Descripción (opcional)", 28, 392), txtDescripcion,
                chkActivo, lblMensaje, btnGuardar, btnNuevo,
                new Panel { Dock = DockStyle.Left, Width = 1, BackColor = Estilo.LilaClaro }
            });

            Controls.Add(panel);
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

        private static bool StockBajo(Producto p)
        {
            return p.Estado && p.Stock <= p.StockMinimo;
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
                    || (x.CategoriaDescripcion ?? "").ToLower().Contains(filtro))
                && (!chkStockBajo.Checked || StockBajo(x))).ToList();

            lblResumen.Text = vista.Count + " de " + productos.Count + " productos  ·  "
                              + productos.Count(StockBajo) + " con stock bajo";

            grid.Rows.Clear();
            foreach (var p in vista)
            {
                int i = grid.Rows.Add(p.IdProducto, p.Codigo, p.Nombre, p.CategoriaDescripcion,
                                      p.Stock, p.PrecioCompra, p.PrecioVenta,
                                      p.Estado ? "Activo" : "Inactivo");
                var fila = grid.Rows[i];
                fila.Tag = p;

                if (!p.Estado)
                {
                    fila.DefaultCellStyle.ForeColor = Color.Gray;
                }
                else if (p.Stock <= 0)
                {
                    fila.Cells["Stock"].Style.ForeColor = Estilo.Error;
                    fila.Cells["Stock"].Style.Font = Estilo.Negrita;
                }
                else if (p.Stock <= p.StockMinimo)
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

        private void Mostrar(Producto p)
        {
            seleccion = p;
            lblTitulo.Text = "Editar producto";
            txtCodigo.Text = p.Codigo;
            txtNombre.Text = p.Nombre;
            LlenarCategorias(p.IdCategoria);
            cmbCategoria.SelectedValue = p.IdCategoria;
            numCompra.Value = p.PrecioCompra;
            numVenta.Value = p.PrecioVenta;
            lblStock.Text = "Stock actual";
            numStock.Value = p.Stock;
            numStock.Enabled = false;          // el stock no se edita a mano
            numStockMin.Value = p.StockMinimo;
            txtDescripcion.Text = p.Descripcion;
            chkActivo.Checked = p.Estado;
            lblMensaje.Text = "";
        }

        private void Nuevo()
        {
            seleccion = null;
            lblTitulo.Text = "Nuevo producto";
            txtCodigo.Clear();
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

            cargando = true;               // evita que el evento de la tabla vuelva a cargar la fila
            grid.CurrentCell = null;
            grid.ClearSelection();
            cargando = false;

            txtCodigo.Focus();
        }

        private void Guardar()
        {
            lblMensaje.Text = "";

            var p = new Producto
            {
                IdProducto = seleccion == null ? 0 : seleccion.IdProducto,
                Codigo = txtCodigo.Text,
                Nombre = txtNombre.Text,
                IdCategoria = cmbCategoria.SelectedValue == null ? 0 : (int)cmbCategoria.SelectedValue,
                Stock = (int)numStock.Value,
                StockMinimo = (int)numStockMin.Value,
                PrecioCompra = numCompra.Value,
                PrecioVenta = numVenta.Value,
                Descripcion = txtDescripcion.Text,
                Estado = chkActivo.Checked
            };

            if (p.PrecioVenta > 0 && p.PrecioVenta < p.PrecioCompra &&
                MessageBox.Show("El precio de venta es menor al de compra.\n¿Guardar de todos modos?",
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

                seleccion = new Producto { IdProducto = p.IdProducto };   // para que se quede marcado
                Cargar();

                var guardado = productos.FirstOrDefault(x => x.IdProducto == p.IdProducto);
                if (guardado != null) Mostrar(guardado); else Nuevo();

                lblMensaje.ForeColor = Color.FromArgb(39, 130, 80);
                lblMensaje.Text = "Guardado correctamente.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Productos", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}