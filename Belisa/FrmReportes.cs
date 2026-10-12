using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BelisaNegocio;

namespace Belisa
{
    [DesignerCategory("Code")]
    public class FrmReportes : Form
    {
        private readonly ReporteN negocio = new ReporteN();

        private DateTimePicker dtDesde, dtHasta;
        private Label lblPeriodo, lblTituloStock;
        private Label vVentas, dVentas, vDescuentos, dDescuentos, vCompras, dCompras, vInventario, dInventario;
        private DataGridView gridMetodo, gridVendedor, gridStock;

        public FrmReportes()
        {
            // 1. Evitar parpadeos y asegurar rendimiento visual fluido al redimensionar
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();

            // 2. Asegurar el escalado correcto de fuentes en distintas resoluciones
            AutoScaleMode = AutoScaleMode.Font;

            BackColor = Estilo.Fondo;

            var contenido = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24), AutoScroll = true };
            ConstruirTablas(contenido);     // el centro (Fill) primero...
            ConstruirTarjetas(contenido);   // ...luego las tarjetas...
            ConstruirBarra(contenido);      // ...y arriba del todo la barra
            Controls.Add(contenido);

            Load += (s, e) => Buscar();
        }

        // ---------- Construcción ----------

        private void ConstruirBarra(Panel padre)
        {
            var barra = new Panel { Dock = DockStyle.Top, Height = 52 };

            lblPeriodo = new Label
            {
                Dock = DockStyle.Right,
                Width = 250,
                TextAlign = ContentAlignment.MiddleRight,
                Font = Estilo.Negrita,
                ForeColor = Estilo.Texto
            };
            barra.Controls.Add(lblPeriodo);

            dtDesde = new DateTimePicker
            {
                Location = new Point(55, 12),
                Width = 120,
                Format = DateTimePickerFormat.Short,
                Font = Estilo.Normal,
                Value = DateTime.Today
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
            var btnHoy = Estilo.Boton("Hoy", 470, 10, 70, 32, false);
            var btnMes = Estilo.Boton("Este mes", 548, 10, 100, 32, false);

            btnBuscar.Click += (s, e) => Buscar();
            btnHoy.Click += (s, e) =>
            {
                dtDesde.Value = DateTime.Today;
                dtHasta.Value = DateTime.Today;
                Buscar();
            };
            btnMes.Click += (s, e) =>
            {
                DateTime hoy = DateTime.Today;
                dtDesde.Value = new DateTime(hoy.Year, hoy.Month, 1);
                dtHasta.Value = hoy;
                Buscar();
            };

            barra.Controls.AddRange(new Control[]
            {
                Estilo.Etiqueta("Desde", 0, 16), dtDesde,
                Estilo.Etiqueta("Hasta", 185, 16), dtHasta,
                btnBuscar, btnHoy, btnMes
            });

            padre.Controls.Add(barra);
        }

        private void ConstruirTarjetas(Panel padre)
        {
            var fila = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 112,
                ColumnCount = 4,
                RowCount = 1
            };
            for (int i = 0; i < 4; i++) fila.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            fila.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            fila.Controls.Add(Tarjeta("Ventas", out vVentas, out dVentas), 0, 0);
            fila.Controls.Add(Tarjeta("Descuentos otorgados", out vDescuentos, out dDescuentos), 1, 0);
            fila.Controls.Add(Tarjeta("Compras", out vCompras, out dCompras), 2, 0);
            fila.Controls.Add(Tarjeta("Inventario actual (a costo)", out vInventario, out dInventario), 3, 0);

            padre.Controls.Add(fila);
        }

        private static Panel Tarjeta(string titulo, out Label valor, out Label detalle)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Size = new Size(220, 112),
                BackColor = Color.White,
                Margin = new Padding(0, 0, 12, 0)
            };

            var etiqueta = new Label
            {
                Text = titulo,
                Location = new Point(16, 12),
                AutoSize = true,
                Font = Estilo.Normal,
                ForeColor = Estilo.Texto
            };

            valor = new Label
            {
                Location = new Point(14, 36),
                Size = new Size(192, 40),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                AutoEllipsis = true,
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = Estilo.Acento
            };

            detalle = new Label
            {
                Location = new Point(16, 80),
                Size = new Size(190, 24),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                AutoEllipsis = true,
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.Gray
            };

            panel.Controls.AddRange(new Control[] { etiqueta, valor, detalle });
            return panel;
        }

        private void ConstruirTablas(Panel padre)
        {
            gridMetodo = Tabla.Crear();
            Tabla.Columna(gridMetodo, "Metodo", "Método de pago", 100);
            Tabla.ColumnaFija(gridMetodo, "Ventas", "Cobros", 80, null, true);
            Tabla.ColumnaFija(gridMetodo, "Total", "Total", 130, "C2", true);
            Compactar(gridMetodo);

            gridVendedor = Tabla.Crear();
            Tabla.Columna(gridVendedor, "Vendedor", "Vendedor", 100);
            Tabla.ColumnaFija(gridVendedor, "Ventas", "Ventas", 80, null, true);
            Tabla.ColumnaFija(gridVendedor, "Total", "Total", 130, "C2", true);
            Compactar(gridVendedor);

            gridStock = Tabla.Crear();
            Tabla.Columna(gridStock, "Codigo", "Código", 70);
            Tabla.Columna(gridStock, "Producto", "Producto", 160);
            Tabla.ColumnaFija(gridStock, "Stock", "Stock", 70, null, true);
            Tabla.ColumnaFija(gridStock, "Minimo", "Mínimo", 80, null, true);
            Compactar(gridStock);

            var tabla = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(0, 12, 0, 0)
            };
            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            tabla.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            tabla.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));

            Label sinUso;
            tabla.Controls.Add(Bloque("Ventas por método de pago", gridMetodo, out sinUso), 0, 0);
            tabla.Controls.Add(Bloque("Ventas por vendedor", gridVendedor, out sinUso), 0, 1);

            var stock = Bloque("Productos con stock bajo", gridStock, out lblTituloStock);
            tabla.Controls.Add(stock, 1, 0);
            tabla.SetRowSpan(stock, 2);

            padre.Controls.Add(tabla);
        }

        // Filas más bajitas para que quepan varias en cada recuadro
        private static void Compactar(DataGridView g)
        {
            g.RowTemplate.Height = 30;
            g.ColumnHeadersHeight = 34;
        }

        private static Panel Bloque(string titulo, DataGridView grid, out Label etiqueta)
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 0, 12, 12) };

            etiqueta = new Label
            {
                Dock = DockStyle.Top,
                Height = 28,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = Estilo.Negrita,
                ForeColor = Estilo.Texto,
                Text = titulo
            };

            panel.Controls.Add(grid);        // Fill primero
            panel.Controls.Add(etiqueta);    // luego el título arriba
            return panel;
        }

        // ---------- Datos ----------

        private void Buscar()
        {
            ResumenReporte r;
            try
            {
                r = negocio.Generar(dtDesde.Value, dtHasta.Value);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Reportes", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Mostrar(r);
        }

        private void Mostrar(ResumenReporte r)
        {
            lblPeriodo.Text = (r.Desde == DateTime.Today && r.Hasta == DateTime.Today)
                ? "Hoy · " + r.Desde.ToString("dd/MM/yyyy")
                : "Del " + r.Desde.ToString("dd/MM/yyyy") + " al " + r.Hasta.ToString("dd/MM/yyyy");

            vVentas.Text = r.TotalVentas.ToString("C2");
            dVentas.Text = r.NumVentas + (r.NumVentas == 1 ? " venta" : " ventas")
                           + "  ·  prom. " + r.TicketPromedio.ToString("C2");

            decimal bruto = r.TotalVentas + r.TotalDescuentos;
            vDescuentos.Text = r.TotalDescuentos.ToString("C2");
            dDescuentos.Text = bruto > 0
                ? (r.TotalDescuentos / bruto * 100m).ToString("0.#") + "% de lo vendido"
                : "Sin ventas en el periodo";

            vCompras.Text = r.TotalCompras.ToString("C2");
            dCompras.Text = r.NumCompras + (r.NumCompras == 1 ? " compra" : " compras");

            vInventario.Text = r.InventarioCosto.ToString("C2");
            dInventario.Text = "A venta: " + r.InventarioVenta.ToString("C2");

            gridMetodo.Rows.Clear();
            foreach (var l in r.PorMetodo) gridMetodo.Rows.Add(l.Etiqueta, l.Cantidad, l.Total);
            gridMetodo.ClearSelection();

            gridVendedor.Rows.Clear();
            foreach (var l in r.PorVendedor) gridVendedor.Rows.Add(l.Etiqueta, l.Cantidad, l.Total);
            gridVendedor.ClearSelection();

            lblTituloStock.Text = "Productos con stock bajo (" + r.StockBajo.Count + ")";
            gridStock.Rows.Clear();
            foreach (var p in r.StockBajo)
            {
                int i = gridStock.Rows.Add(p.Codigo, p.Nombre, p.Stock, p.StockMinimo);
                var celda = gridStock.Rows[i].Cells["Stock"];
                celda.Style.Font = Estilo.Negrita;
                celda.Style.ForeColor = p.Stock <= 0 ? Estilo.Error : Color.FromArgb(176, 110, 10);
            }
            gridStock.ClearSelection();
        }
    }
}