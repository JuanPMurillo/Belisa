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
    public class FrmCompra : Form
    {
        private const int CantidadMaxima = 1000000;

        private readonly CompraN negocio = new CompraN();
        private List<Proveedor> proveedores = new List<Proveedor>();
        private List<Producto> productos = new List<Producto>();
        private readonly List<DetalleCompra> carrito = new List<DetalleCompra>();

        private ComboBox cmbProveedor, cmbTipo;
        private BuscadorProducto buscador;
        private NumericUpDown numCantidad, numCompra, numVenta;
        private Label lblInfo, lblMensaje, lblTotal;
        private DataGridView grid;
        private Button btnAgregar, btnRegistrar, btnLimpiar;

        public FrmCompra()
        {
            BackColor = Estilo.Fondo;

            var contenido = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24) };
            ConstruirTabla(contenido);        // el centro (Fill) primero...
            ConstruirPie(contenido);          // ...luego abajo...
            ConstruirEncabezado(contenido);   // ...y arriba
            Controls.Add(contenido);

            Load += (s, e) => CargarDatos();
        }

        // ---------- Construcción ----------

        private void ConstruirTabla(Panel padre)
        {
            grid = Tabla.Crear();
            Tabla.Columna(grid, "Codigo", "Código", 70);
            Tabla.Columna(grid, "Producto", "Producto", 180);
            Tabla.ColumnaFija(grid, "Cantidad", "Cantidad", 90, null, true);
            Tabla.ColumnaFija(grid, "Compra", "P. compra", 110, "C2", true);
            Tabla.ColumnaFija(grid, "Venta", "P. venta", 110, "C2", true);
            Tabla.ColumnaFija(grid, "Subtotal", "Subtotal", 120, "C2", true);

            grid.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "Quitar",
                HeaderText = "",
                Text = "Quitar",
                UseColumnTextForButtonValue = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                Width = 90,
                FlatStyle = FlatStyle.Flat
            });

            grid.CellContentClick += (s, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex != grid.Columns["Quitar"].Index) return;
                var d = grid.Rows[e.RowIndex].Tag as DetalleCompra;
                if (d == null) return;
                carrito.Remove(d);
                RedibujarCarrito();
            };

            padre.Controls.Add(grid);
        }

        private void ConstruirPie(Panel padre)
        {
            var pie = new Panel { Dock = DockStyle.Bottom, Height = 104, Padding = new Padding(0, 12, 0, 0) };
            var tarjeta = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };

            lblTotal = new Label
            {
                Location = new Point(20, 10),
                Size = new Size(380, 40),
                Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                ForeColor = Estilo.Acento,
                TextAlign = ContentAlignment.MiddleLeft,
                Text = "Total: " + 0m.ToString("C2")
            };

            lblMensaje = new Label
            {
                Location = new Point(20, 54),
                Size = new Size(560, 32),
                Font = Estilo.Normal
            };

            btnRegistrar = Estilo.Boton("Registrar compra", 0, 24, 200, 44, true);
            btnLimpiar = Estilo.Boton("Limpiar", 0, 24, 120, 44, false);
            btnRegistrar.Click += (s, e) => Registrar();
            btnLimpiar.Click += (s, e) =>
            {
                if (carrito.Count == 0 ||
                    MessageBox.Show("¿Descartar la compra en curso?", "Limpiar",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    Reiniciar();
            };

            tarjeta.Controls.AddRange(new Control[] { lblTotal, lblMensaje, btnRegistrar, btnLimpiar });
            tarjeta.Resize += (s, e) =>
            {
                btnRegistrar.Left = tarjeta.Width - btnRegistrar.Width - 20;
                btnLimpiar.Left = btnRegistrar.Left - btnLimpiar.Width - 12;
            };

            pie.Controls.Add(tarjeta);
            padre.Controls.Add(pie);
        }

        private void ConstruirEncabezado(Panel padre)
        {
            var arriba = new Panel { Dock = DockStyle.Top, Height = 190, Padding = new Padding(0, 0, 0, 12) };
            var tarjeta = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };

            cmbProveedor = new ComboBox
            {
                Location = new Point(20, 38),
                Width = 380,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = Estilo.Normal,
                DisplayMember = "RazonSocial",
                ValueMember = "IdProveedor"
            };

            cmbTipo = new ComboBox
            {
                Location = new Point(420, 38),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = Estilo.Normal
            };
            cmbTipo.Items.AddRange(new object[] { "Ticket", "Nota de remisión", "Factura", "Otro" });
            cmbTipo.SelectedIndex = 0;

            buscador = new BuscadorProducto { Location = new Point(20, 108), Width = 380 };
            numCantidad = Num(420, 108, 90, 0, CantidadMaxima);
            numCantidad.Minimum = 1;
            numCantidad.Value = 1;
            numCompra = Num(525, 108, 110, 2, 99999999.99m);
            numVenta = Num(650, 108, 110, 2, 99999999.99m);

            btnAgregar = Estilo.Boton("Agregar", 780, 104, 120, 34, true);
            btnAgregar.Click += (s, e) => Agregar();

            lblInfo = new Label
            {
                Location = new Point(20, 148),
                Size = new Size(880, 28),
                Font = Estilo.Normal,
                ForeColor = Estilo.Texto
            };

            // Al elegir un producto se llenan sus precios actuales
            buscador.TextChanged += (s, e) => ProductoElegido();

            // Enter avanza al siguiente campo; en el último, agrega
            EnterPasaA(buscador, numCantidad);
            EnterPasaA(numCantidad, numCompra);
            EnterPasaA(numCompra, numVenta);
            numVenta.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; Agregar(); }
            };

            tarjeta.Controls.AddRange(new Control[]
            {
                Estilo.Etiqueta("Proveedor", 20, 14), cmbProveedor,
                Estilo.Etiqueta("Tipo de documento", 420, 14), cmbTipo,
                Estilo.Etiqueta("Producto (código o nombre)", 20, 84), buscador,
                Estilo.Etiqueta("Cantidad", 420, 84), numCantidad,
                Estilo.Etiqueta("P. compra", 525, 84), numCompra,
                Estilo.Etiqueta("P. venta", 650, 84), numVenta,
                btnAgregar, lblInfo
            });

            arriba.Controls.Add(tarjeta);
            padre.Controls.Add(arriba);
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

        private static void EnterPasaA(Control desde, Control hacia)
        {
            desde.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; hacia.Focus(); }
            };
        }

        // ---------- Datos ----------

        private void CargarDatos()
        {
            try
            {
                proveedores = negocio.ListarProveedoresActivos();
                productos = negocio.ListarProductosActivos();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Compras", MessageBoxButtons.OK, MessageBoxIcon.Error);
                proveedores = new List<Proveedor>();
                productos = new List<Producto>();
            }

            cmbProveedor.DataSource = proveedores;
            cmbProveedor.SelectedIndex = -1;
            buscador.Cargar(productos);

            if (proveedores.Count == 0) Aviso("Primero registra un proveedor activo.");
            else if (productos.Count == 0) Aviso("Primero registra productos activos.");
        }

        private void ProductoElegido()
        {
            var p = buscador.Seleccionado();
            if (p == null) { lblInfo.Text = ""; return; }

            numCompra.Value = p.PrecioCompra;
            numVenta.Value = p.PrecioVenta;
            lblInfo.Text = "Stock actual: " + p.Stock
                         + "     ·     Compra actual: " + p.PrecioCompra.ToString("C2")
                         + "     ·     Venta actual: " + p.PrecioVenta.ToString("C2");
        }

        private void Aviso(string texto)
        {
            lblMensaje.ForeColor = Estilo.Error;
            lblMensaje.Text = texto;
        }

        // ---------- Carrito ----------

        private void Agregar()
        {
            lblMensaje.Text = "";

            var p = buscador.Seleccionado();
            if (p == null)
            {
                Aviso("Elige un producto de la lista (escribe su código o nombre).");
                buscador.Focus();
                return;
            }

            int cantidad = (int)numCantidad.Value;
            decimal compra = numCompra.Value;
            decimal venta = numVenta.Value;

            if (venta <= 0)
            {
                Aviso("El precio de venta debe ser mayor a 0.");
                numVenta.Focus();
                return;
            }

            if (venta < compra &&
                MessageBox.Show("El precio de venta es menor al de compra.\n¿Agregar de todos modos?",
                    "Precio", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            var existente = carrito.FirstOrDefault(d => d.IdProducto == p.IdProducto);
            if (existente == null)
            {
                carrito.Add(new DetalleCompra
                {
                    IdProducto = p.IdProducto,
                    ProductoCodigo = p.Codigo,
                    ProductoNombre = p.Nombre,
                    Cantidad = cantidad,
                    PrecioCompra = compra,
                    PrecioVenta = venta
                });
            }
            else
            {
                // Mismo producto otra vez: se suma la cantidad y quedan los precios más recientes
                if (existente.Cantidad + cantidad > CantidadMaxima)
                {
                    Aviso("La cantidad máxima por producto es " + CantidadMaxima.ToString("N0") + ".");
                    return;
                }
                existente.Cantidad += cantidad;
                existente.PrecioCompra = compra;
                existente.PrecioVenta = venta;
            }

            buscador.Clear();
            numCantidad.Value = 1;
            numCompra.Value = 0;
            numVenta.Value = 0;
            lblInfo.Text = "";
            RedibujarCarrito();
            buscador.Focus();
        }

        private void RedibujarCarrito()
        {
            grid.Rows.Clear();
            foreach (var d in carrito)
            {
                d.MontoTotal = d.Cantidad * d.PrecioCompra;
                int i = grid.Rows.Add(d.ProductoCodigo, d.ProductoNombre, d.Cantidad,
                                      d.PrecioCompra, d.PrecioVenta, d.MontoTotal);
                grid.Rows[i].Tag = d;
            }
            grid.ClearSelection();

            decimal total = carrito.Sum(d => d.Cantidad * d.PrecioCompra);
            lblTotal.Text = "Total: " + total.ToString("C2");
        }

        private void Reiniciar()
        {
            carrito.Clear();
            RedibujarCarrito();
            buscador.Clear();
            numCantidad.Value = 1;
            numCompra.Value = 0;
            numVenta.Value = 0;
            lblInfo.Text = "";
            lblMensaje.Text = "";
            cmbProveedor.SelectedIndex = -1;
            cmbTipo.SelectedIndex = 0;
        }

        // ---------- Registrar ----------

        private void Registrar()
        {
            lblMensaje.Text = "";

            if (cmbProveedor.SelectedValue == null)
            {
                Aviso("Selecciona un proveedor.");
                cmbProveedor.Focus();
                return;
            }
            if (carrito.Count == 0)
            {
                Aviso("Agrega al menos un producto.");
                buscador.Focus();
                return;
            }

            decimal total = carrito.Sum(d => d.Cantidad * d.PrecioCompra);
            string proveedor = ((Proveedor)cmbProveedor.SelectedItem).RazonSocial;

            if (MessageBox.Show(
                    "¿Registrar la compra a " + proveedor + " por " + total.ToString("C2") + "?\n\n" +
                    "El stock y los precios de los productos se actualizarán.",
                    "Confirmar compra", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            var compra = new Compra
            {
                IdProveedor = (int)cmbProveedor.SelectedValue,
                TipoDocumento = Convert.ToString(cmbTipo.SelectedItem),
                Detalle = carrito.Select(d => new DetalleCompra
                {
                    IdProducto = d.IdProducto,
                    Cantidad = d.Cantidad,
                    PrecioCompra = d.PrecioCompra,
                    PrecioVenta = d.PrecioVenta
                }).ToList()
            };

            btnRegistrar.Enabled = false;   // evita doble clic
            try
            {
                string mensaje;
                if (!negocio.Registrar(compra, out mensaje))
                {
                    Aviso(mensaje);
                    return;
                }

                MessageBox.Show("Compra " + compra.NumeroDocumento + " registrada.\nEl inventario ya está actualizado.",
                    "Listo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Reiniciar();
                CargarDatos();   // trae el stock y los precios ya actualizados
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Compras", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnRegistrar.Enabled = true;
            }
        }
    }
}