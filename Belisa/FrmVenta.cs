using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using BelisaEntidad;
using BelisaNegocio;

namespace Belisa
{
    [DesignerCategory("Code")]
    public class FrmVenta : Form
    {
        private const int CantidadMaxima = 100000;
        private static readonly Color Verde = Color.FromArgb(39, 130, 80);

        private readonly VentaN negocio = new VentaN();
        private List<Cliente> clientes = new List<Cliente>();
        private List<Producto> productos = new List<Producto>();
        private readonly List<DetalleVenta> carrito = new List<DetalleVenta>();
        private decimal totalActual;

        private BuscadorProducto buscador;
        private NumericUpDown numCantidad, numDescuento;
        private ComboBox cmbCliente, cmbMetodo;
        private TextBox txtReferencia, txtPago;
        private Label lblInfo, lblMensaje, lblSubtotal, lblDescuento, lblTotal, lblCambioTitulo, lblCambio;
        private DataGridView grid;
        private Button btnAgregar, btnExacto, btnCobrar, btnLimpiar;

        public FrmVenta()
        {
            BackColor = Estilo.Fondo;
            ConstruirCarrito();   // el centro (Fill) primero...
            ConstruirCobro();     // ...y luego el panel lateral (Right)

            Load += (s, e) =>
            {
                CargarDatos();
                Recalcular();
                BeginInvoke(new Action(() => buscador.Focus()));
            };
        }

        // ---------- Construcción ----------

        private void ConstruirCarrito()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24) };

            grid = Tabla.Crear();
            Tabla.Columna(grid, "Codigo", "Código", 70);
            Tabla.Columna(grid, "Producto", "Producto o servicio", 180);
            Tabla.ColumnaFija(grid, "Cantidad", "Cantidad", 90, null, true);
            Tabla.ColumnaFija(grid, "Precio", "Precio", 100, "C2", true);
            Tabla.ColumnaFija(grid, "Subtotal", "Subtotal", 110, "C2", true);
            grid.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "Quitar",
                HeaderText = "",
                Text = "Quitar",
                UseColumnTextForButtonValue = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                Width = 80,
                FlatStyle = FlatStyle.Flat
            });

            grid.CellContentClick += (s, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex != grid.Columns["Quitar"].Index) return;
                var d = grid.Rows[e.RowIndex].Tag as DetalleVenta;
                if (d == null) return;
                carrito.Remove(d);
                RedibujarCarrito();
                buscador.Focus();
            };

            var arriba = new Panel { Dock = DockStyle.Top, Height = 142, Padding = new Padding(0, 0, 0, 12) };
            var tarjeta = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };

            buscador = new BuscadorProducto { Location = new Point(20, 36), Width = 270 };
            numCantidad = Num(305, 36, 80, 0, CantidadMaxima);
            numCantidad.Minimum = 1;
            numCantidad.Value = 1;
            btnAgregar = Estilo.Boton("Agregar", 400, 32, 110, 34, true);

            lblInfo = new Label
            {
                Location = new Point(20, 80),
                Size = new Size(490, 44),
                Font = Estilo.Normal,
                ForeColor = Estilo.Texto
            };

            buscador.TextChanged += (s, e) => ProductoElegido();
            buscador.KeyDown += (s, e) =>
            {
                // Enter agrega el producto (así funciona también con un lector de código de barras)
                if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; Agregar(); }
            };
            numCantidad.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; Agregar(); }
            };
            btnAgregar.Click += (s, e) => Agregar();

            tarjeta.Controls.AddRange(new Control[]
            {
                Estilo.Etiqueta("Producto o servicio (código o nombre)", 20, 12), buscador,
                Estilo.Etiqueta("Cantidad", 305, 12), numCantidad,
                btnAgregar, lblInfo
            });

            arriba.Controls.Add(tarjeta);
            panel.Controls.Add(grid);
            panel.Controls.Add(arriba);
            Controls.Add(panel);
        }

        private void ConstruirCobro()
        {
            var panel = new Panel { Dock = DockStyle.Right, Width = 380, BackColor = Color.White, AutoScroll = true };

            var titulo = new Label
            {
                Text = "Cobro",
                Location = new Point(28, 14),
                AutoSize = true,
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Estilo.Texto
            };

            var btnConfigTicket = Estilo.Boton("Config. ticket", 228, 10, 124, 30, false);
            btnConfigTicket.Click += (s, e) =>
            {
                using (var f = new FrmTicketConfig()) f.ShowDialog(this);
            };

            cmbCliente = new ComboBox
            {
                Location = new Point(28, 74),
                Width = 324,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = Estilo.Normal,
                DisplayMember = "NombreCompleto",
                ValueMember = "IdCliente"
            };

            cmbMetodo = new ComboBox
            {
                Location = new Point(28, 134),
                Width = 324,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = Estilo.Normal
            };
            cmbMetodo.Items.AddRange(VentaN.MetodosPago);
            cmbMetodo.SelectedIndex = 0;

            txtReferencia = Estilo.Campo(28, 194, 324);
            txtReferencia.MaxLength = 50;

            numDescuento = Num(28, 254, 110, 2, 100);

            lblSubtotal = Valor(192, 296, 160, 24);
            lblDescuento = Valor(192, 320, 160, 24);

            lblTotal = new Label
            {
                Location = new Point(28, 366),
                Size = new Size(324, 44),
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 24f, FontStyle.Bold),
                ForeColor = Estilo.Acento
            };

            txtPago = Estilo.Campo(28, 438, 150);
            txtPago.TextAlign = HorizontalAlignment.Right;
            btnExacto = Estilo.Boton("Exacto", 190, 436, 162, 32, false);

            lblCambioTitulo = Estilo.Etiqueta("Cambio", 28, 484);
            lblCambio = Valor(192, 478, 160, 30);
            lblCambio.Font = new Font("Segoe UI", 13f, FontStyle.Bold);

            lblMensaje = new Label
            {
                Location = new Point(28, 514),
                Size = new Size(324, 40),
                Font = Estilo.Normal
            };

            btnCobrar = Estilo.Boton("Cobrar", 28, 558, 324, 46, true);
            btnLimpiar = Estilo.Boton("Limpiar venta", 28, 612, 324, 34, false);

            panel.Controls.AddRange(new Control[]
            {
                titulo, btnConfigTicket,
                Estilo.Etiqueta("Cliente", 28, 52), cmbCliente,
                Estilo.Etiqueta("Método de pago", 28, 112), cmbMetodo,
                Estilo.Etiqueta("Referencia de pago (opcional)", 28, 172), txtReferencia,
                Estilo.Etiqueta("Descuento (%)", 28, 232), numDescuento,
                Estilo.Etiqueta("Subtotal", 28, 298), lblSubtotal,
                Estilo.Etiqueta("Descuento", 28, 322), lblDescuento,
                Estilo.Etiqueta("Total", 28, 346), lblTotal,
                Estilo.Etiqueta("Paga con", 28, 416), txtPago, btnExacto,
                lblCambioTitulo, lblCambio,
                lblMensaje, btnCobrar, btnLimpiar,
                new Panel { Dock = DockStyle.Left, Width = 1, BackColor = Estilo.LilaClaro }
            });

            Controls.Add(panel);

            // Los eventos se conectan al final, cuando ya existen todos los controles
            numDescuento.ValueChanged += (s, e) => Recalcular();
            cmbMetodo.SelectedIndexChanged += (s, e) => Recalcular();
            txtPago.KeyUp += (s, e) => Recalcular();
            txtPago.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; Cobrar(); }
            };
            btnExacto.Click += (s, e) =>
            {
                txtPago.Text = totalActual.ToString("0.00", CultureInfo.CurrentCulture);
                Recalcular();
            };
            btnCobrar.Click += (s, e) => Cobrar();
            btnLimpiar.Click += (s, e) =>
            {
                if (carrito.Count == 0 ||
                    MessageBox.Show("¿Descartar la venta en curso?", "Limpiar",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    Reiniciar();
            };
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

        private static Label Valor(int x, int y, int ancho, int alto)
        {
            return new Label
            {
                Location = new Point(x, y),
                Size = new Size(ancho, alto),
                TextAlign = ContentAlignment.MiddleRight,
                Font = Estilo.Normal,
                ForeColor = Estilo.Texto
            };
        }

        // ---------- Datos ----------

        private void CargarDatos()
        {
            try
            {
                clientes = negocio.ListarClientesActivos();
                productos = negocio.ListarProductosActivos();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ventas", MessageBoxButtons.OK, MessageBoxIcon.Error);
                clientes = new List<Cliente>();
                productos = new List<Producto>();
            }

            var lista = new List<Cliente> { new Cliente { IdCliente = 0, NombreCompleto = "Público en general" } };
            lista.AddRange(clientes);
            cmbCliente.DataSource = lista;
            cmbCliente.SelectedIndex = 0;

            buscador.Cargar(productos);
            if (productos.Count == 0) Aviso("Primero registra productos o servicios activos.");
        }

        // Tras un rechazo del servidor: trae existencias y precios vigentes y ajusta el carrito
        private void RecargarProductos()
        {
            try
            {
                productos = negocio.ListarProductosActivos();
            }
            catch (Exception)
            {
                return;   // el error principal ya se mostró; se conserva la lista anterior
            }

            buscador.Cargar(productos);
            foreach (var d in carrito)
            {
                var p = productos.FirstOrDefault(x => x.IdProducto == d.IdProducto);
                if (p != null) d.PrecioVenta = p.PrecioVenta;
            }
            RedibujarCarrito();
        }

        private void Aviso(string texto)
        {
            lblMensaje.ForeColor = Estilo.Error;
            lblMensaje.Text = texto;
        }

        // ---------- Carrito ----------

        private void ProductoElegido()
        {
            var p = buscador.Seleccionado();
            if (p == null) { lblInfo.Text = ""; return; }

            // Un servicio no tiene inventario: no se muestra stock ni aviso de agotado
            if (p.EsServicio)
            {
                lblInfo.ForeColor = Estilo.Texto;
                lblInfo.Text = "Servicio   ·   Precio: " + p.PrecioVenta.ToString("C2");
                return;
            }

            if (p.Stock <= 0)
            {
                lblInfo.ForeColor = Estilo.Error;
                lblInfo.Text = "SIN STOCK   ·   Precio: " + p.PrecioVenta.ToString("C2");
            }
            else
            {
                lblInfo.ForeColor = Estilo.Texto;
                lblInfo.Text = "Stock: " + p.Stock + "   ·   Precio: " + p.PrecioVenta.ToString("C2");
            }
        }

        private void Agregar()
        {
            lblMensaje.Text = "";

            var p = buscador.Seleccionado();
            if (p == null)
            {
                Aviso("Elige un producto o servicio de la lista (escribe su código o nombre).");
                buscador.Focus();
                return;
            }

            int cantidad = (int)numCantidad.Value;
            var existente = carrito.FirstOrDefault(d => d.IdProducto == p.IdProducto);
            int enCarrito = existente == null ? 0 : existente.Cantidad;

            if (p.EsServicio)
            {
                // Los servicios no se agotan: solo se cuida un tope razonable
                if (enCarrito + cantidad > CantidadMaxima)
                {
                    Aviso("La cantidad máxima es " + CantidadMaxima.ToString("N0") + ".");
                    return;
                }
            }
            else
            {
                if (p.Stock <= 0)
                {
                    Aviso(p.Nombre + " no tiene stock.");
                    return;
                }
                if (enCarrito + cantidad > p.Stock)
                {
                    Aviso("Solo hay " + p.Stock + " en existencia"
                          + (enCarrito > 0 ? " (ya tienes " + enCarrito + " en la venta)." : "."));
                    return;
                }
            }

            if (existente == null)
            {
                carrito.Add(new DetalleVenta
                {
                    IdProducto = p.IdProducto,
                    ProductoCodigo = p.Codigo,
                    ProductoNombre = p.Nombre,
                    PrecioVenta = p.PrecioVenta,
                    Cantidad = cantidad
                });
            }
            else
            {
                existente.Cantidad += cantidad;
                existente.PrecioVenta = p.PrecioVenta;
            }

            buscador.Clear();
            numCantidad.Value = 1;
            lblInfo.Text = "";
            RedibujarCarrito();
            buscador.Focus();
        }

        private void RedibujarCarrito()
        {
            grid.Rows.Clear();
            foreach (var d in carrito)
            {
                d.SubTotal = d.Cantidad * d.PrecioVenta;
                int i = grid.Rows.Add(d.ProductoCodigo, d.ProductoNombre, d.Cantidad, d.PrecioVenta, d.SubTotal);
                grid.Rows[i].Tag = d;
            }
            grid.ClearSelection();
            Recalcular();
        }

        // ---------- Totales ----------

        private bool EsEfectivo()
        {
            return Convert.ToString(cmbMetodo.SelectedItem) == "Efectivo";
        }

        // Lo que el cliente paga, leyendo lo que está escrito (sin esperar a que se salga del campo)
        private decimal Pago()
        {
            decimal valor;
            if (decimal.TryParse(txtPago.Text.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out valor) && valor >= 0)
                return Math.Round(valor, 2);
            return 0m;
        }

        private void Recalcular()
        {
            decimal subtotal = carrito.Sum(d => d.Cantidad * d.PrecioVenta);
            decimal descuento = Math.Round(subtotal * numDescuento.Value / 100m, 2, MidpointRounding.AwayFromZero);
            totalActual = subtotal - descuento;

            lblSubtotal.Text = subtotal.ToString("C2");
            lblDescuento.Text = descuento > 0 ? "-" + descuento.ToString("C2") : descuento.ToString("C2");
            lblTotal.Text = totalActual.ToString("C2");

            bool efectivo = EsEfectivo();
            txtPago.Enabled = efectivo;
            btnExacto.Enabled = efectivo;
            txtReferencia.Enabled = !efectivo;

            if (!efectivo)
            {
                // Tarjeta o transferencia: se cobra el total exacto
                txtPago.Text = totalActual.ToString("0.00", CultureInfo.CurrentCulture);
                lblCambioTitulo.Text = "Cambio";
                lblCambio.ForeColor = Estilo.Texto;
                lblCambio.Text = 0m.ToString("C2");
                return;
            }

            if (txtReferencia.Text.Length > 0) txtReferencia.Clear();

            decimal pago = Pago();
            if (pago >= totalActual)
            {
                lblCambioTitulo.Text = "Cambio";
                lblCambio.ForeColor = Verde;
                lblCambio.Text = (pago - totalActual).ToString("C2");
            }
            else
            {
                lblCambioTitulo.Text = "Falta";
                lblCambio.ForeColor = Estilo.Error;
                lblCambio.Text = (totalActual - pago).ToString("C2");
            }
        }

        // ---------- Cobrar ----------

        private void Cobrar()
        {
            lblMensaje.Text = "";

            if (carrito.Count == 0)
            {
                Aviso("Agrega al menos un producto o servicio.");
                buscador.Focus();
                return;
            }

            Recalcular();   // toma el descuento tal como quedó escrito

            int idCliente = cmbCliente.SelectedValue == null ? 0 : (int)cmbCliente.SelectedValue;

            var venta = new Venta
            {
                IdCliente = idCliente > 0 ? (int?)idCliente : null,
                MetodoPago = Convert.ToString(cmbMetodo.SelectedItem),
                ReferenciaPago = txtReferencia.Text,
                DescuentoPorcentaje = numDescuento.Value,
                MontoPago = Pago(),
                Detalle = carrito.Select(d => new DetalleVenta
                {
                    IdProducto = d.IdProducto,
                    PrecioVenta = d.PrecioVenta,
                    Cantidad = d.Cantidad
                }).ToList()
            };

            btnCobrar.Enabled = false;   // evita doble clic
            try
            {
                string mensaje;
                if (!negocio.Registrar(venta, out mensaje))
                {
                    Aviso(mensaje);
                    RecargarProductos();
                    return;
                }

                string resumen = "Venta " + venta.NumeroDocumento + " registrada.\n\n"
                               + "Total: " + venta.MontoTotal.ToString("C2");
                if (venta.MetodoPago == "Efectivo")
                    resumen += "\nCambio: " + venta.MontoCambio.ToString("C2");

                // Ticket: según la configuración, se pregunta, se imprime solo o no se imprime
                string modo = TicketConfig.Cargar().AlCobrar;
                if (modo == "Preguntar")
                {
                    if (MessageBox.Show(resumen + "\n\n¿Imprimir el ticket?", "Venta registrada",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                        ImprimirTicket(venta.IdVenta);
                }
                else
                {
                    if (modo == "Siempre") ImprimirTicket(venta.IdVenta);
                    MessageBox.Show(resumen, "Venta registrada", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                Reiniciar();
                CargarDatos();   // trae el stock ya descontado
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ventas", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnCobrar.Enabled = true;
            }
        }

        // Se vuelve a leer la venta de la base de datos: el ticket muestra lo que quedó guardado
        private void ImprimirTicket(int idVenta)
        {
            try
            {
                var venta = negocio.Listar(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1))
                                   .FirstOrDefault(v => v.IdVenta == idVenta);
                if (venta == null)
                    throw new InvalidOperationException("No se encontró la venta registrada.");

                Ticket.Imprimir(venta, negocio.ObtenerDetalle(idVenta));
            }
            catch (Exception ex)
            {
                MessageBox.Show("La venta se guardó, pero no se pudo imprimir el ticket.\n\n" + ex.Message
                                + "\n\nPuedes reimprimirlo en Ventas → Ver detalle.",
                    "Ticket", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Reiniciar()
        {
            carrito.Clear();
            buscador.Clear();
            numCantidad.Value = 1;
            numDescuento.Value = 0;
            txtPago.Clear();
            txtReferencia.Clear();
            if (cmbCliente.Items.Count > 0) cmbCliente.SelectedIndex = 0;
            cmbMetodo.SelectedIndex = 0;
            lblInfo.Text = "";
            lblMensaje.Text = "";
            RedibujarCarrito();
            buscador.Focus();
        }
    }
}