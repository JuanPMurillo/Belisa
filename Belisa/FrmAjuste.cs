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
    public class FrmAjuste : Form
    {
        private const int Maximo = 1000000;
        private static readonly Color Verde = Color.FromArgb(39, 130, 80);

        private readonly AjusteN negocio = new AjusteN();
        private List<Producto> productos = new List<Producto>();
        private List<Ajuste> ajustes = new List<Ajuste>();
        private Producto productoActual;
        private DateTime desdeUsado = DateTime.Today, hastaUsado = DateTime.Today;

        private BuscadorProducto buscador;
        private NumericUpDown numConteo;
        private ComboBox cmbMotivo;
        private TextBox txtNota, txtFiltro;
        private Label lblInfo, lblDiferencia, lblMensaje, lblResumen;
        private DateTimePicker dtDesde, dtHasta;
        private DataGridView grid;
        private Button btnRegistrar;

        public FrmAjuste()
        {
            BackColor = Estilo.Fondo;

            var contenido = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24) };
            ConstruirTabla(contenido);        // el centro (Fill) primero...
            ConstruirBarra(contenido);        // ...luego la barra del historial...
            ConstruirFormulario(contenido);   // ...y arriba del todo el formulario
            Controls.Add(contenido);

            Load += (s, e) =>
            {
                CargarProductos();
                Buscar();
                BeginInvoke(new Action(() => buscador.Focus()));
            };
        }

        // ---------- Construcción ----------

        private void ConstruirTabla(Panel padre)
        {
            grid = Tabla.Crear();
            Tabla.ColumnaFija(grid, "Fecha", "Fecha", 150, null, false);
            Tabla.Columna(grid, "Codigo", "Código", 70);
            Tabla.Columna(grid, "Producto", "Producto", 170);
            Tabla.ColumnaFija(grid, "Antes", "Antes", 80, null, true);
            Tabla.ColumnaFija(grid, "Despues", "Después", 90, null, true);
            Tabla.ColumnaFija(grid, "Diferencia", "Diferencia", 100, "+#,##0;-#,##0;0", true);
            Tabla.Columna(grid, "Motivo", "Motivo", 110);
            Tabla.Columna(grid, "Nota", "Nota", 150);
            Tabla.Columna(grid, "Usuario", "Registró", 100);
            padre.Controls.Add(grid);
        }

        private void ConstruirBarra(Panel padre)
        {
            var barra = new Panel { Dock = DockStyle.Top, Height = 52 };

            lblResumen = new Label
            {
                Dock = DockStyle.Right,
                Width = 330,
                TextAlign = ContentAlignment.MiddleRight,
                Font = Estilo.Normal,
                ForeColor = Estilo.Texto
            };
            barra.Controls.Add(lblResumen);
            barra.Controls.Add(Exportar.BotonExcel(grid, "Ajustes de inventario"));
            barra.Controls.Add(Exportar.BotonPdf(grid, "Reporte de ajustes de inventario", SubtituloReporte));

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

        private void ConstruirFormulario(Panel padre)
        {
            var arriba = new Panel { Dock = DockStyle.Top, Height = 226, Padding = new Padding(0, 0, 0, 12) };
            var tarjeta = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };

            buscador = new BuscadorProducto { Location = new Point(20, 38), Width = 380 };

            numConteo = new NumericUpDown
            {
                Location = new Point(420, 38),
                Width = 110,
                Font = Estilo.Normal,
                Minimum = 0,
                Maximum = Maximo,
                ThousandsSeparator = true,
                TextAlign = HorizontalAlignment.Right
            };

            cmbMotivo = new ComboBox
            {
                Location = new Point(550, 38),
                Width = 230,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = Estilo.Normal
            };
            cmbMotivo.Items.AddRange(AjusteN.Motivos);
            cmbMotivo.SelectedIndex = 0;

            lblInfo = new Label
            {
                Location = new Point(20, 76),
                Size = new Size(760, 26),
                AutoEllipsis = true,
                Font = Estilo.Normal,
                ForeColor = Estilo.Texto
            };

            txtNota = Estilo.Campo(20, 130, 760);
            txtNota.MaxLength = 200;

            lblDiferencia = new Label
            {
                Location = new Point(20, 168),
                Size = new Size(300, 34),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 13f, FontStyle.Bold)
            };

            lblMensaje = new Label
            {
                Location = new Point(330, 162),
                Size = new Size(280, 46),
                Font = Estilo.Normal
            };

            btnRegistrar = Estilo.Boton("Registrar ajuste", 620, 166, 160, 40, true);
            btnRegistrar.Click += (s, e) => Registrar();

            buscador.TextChanged += (s, e) => ProductoElegido();
            numConteo.ValueChanged += (s, e) => Diferencia();
            numConteo.KeyUp += (s, e) => Diferencia();
            numConteo.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; Registrar(); }
            };
            txtNota.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; Registrar(); }
            };
            buscador.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; numConteo.Focus(); }
            };

            tarjeta.Controls.AddRange(new Control[]
            {
                Estilo.Etiqueta("Producto (código o nombre)", 20, 14), buscador,
                Estilo.Etiqueta("Conteo real", 420, 14), numConteo,
                Estilo.Etiqueta("Motivo", 550, 14), cmbMotivo,
                lblInfo,
                Estilo.Etiqueta("Nota (obligatoria si el motivo es «Otro»)", 20, 104), txtNota,
                lblDiferencia, lblMensaje, btnRegistrar
            });

            arriba.Controls.Add(tarjeta);
            padre.Controls.Add(arriba);
        }

        // ---------- Producto elegido ----------

        private void CargarProductos()
        {
            try
            {
                productos = negocio.ListarProductos();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Inventario", MessageBoxButtons.OK, MessageBoxIcon.Error);
                productos = new List<Producto>();
            }
            buscador.Cargar(productos);
        }

        // Al elegir un producto, el conteo arranca igual al stock del sistema
        private void ProductoElegido()
        {
            var p = buscador.Seleccionado();
            bool cambio = p == null || productoActual == null || p.IdProducto != productoActual.IdProducto;
            productoActual = p;

            if (p != null && cambio) numConteo.Value = Math.Min(p.Stock, Maximo);

            ActualizarInfo();
            Diferencia();
        }

        // Tras recargar la lista: muestra el stock actual sin tocar lo que la persona escribió
        private void ActualizarProducto()
        {
            productoActual = buscador.Seleccionado();
            ActualizarInfo();
            Diferencia();
        }

        private void ActualizarInfo()
        {
            var p = productoActual;
            if (p == null) { lblInfo.Text = ""; return; }

            lblInfo.Text = "Stock en el sistema: " + p.Stock
                         + "     ·     Mínimo: " + p.StockMinimo
                         + "     ·     " + p.CategoriaDescripcion
                         + (p.Estado ? "" : "     ·     PRODUCTO INACTIVO");
        }

        private void Diferencia()
        {
            var p = productoActual;
            if (p == null) { lblDiferencia.Text = ""; return; }

            int dif = (int)numConteo.Value - p.Stock;
            if (dif == 0)
            {
                lblDiferencia.ForeColor = Estilo.Texto;
                lblDiferencia.Text = "Sin diferencia";
            }
            else if (dif > 0)
            {
                lblDiferencia.ForeColor = Verde;
                lblDiferencia.Text = "Sobran " + dif + "  (+" + dif + ")";
            }
            else
            {
                lblDiferencia.ForeColor = Estilo.Error;
                lblDiferencia.Text = "Faltan " + (-dif) + "  (" + dif + ")";
            }
        }

        private void Mensaje(string texto, Color color)
        {
            lblMensaje.ForeColor = color;
            lblMensaje.Text = texto;
        }

        // ---------- Registrar ----------

        private void Registrar()
        {
            lblMensaje.Text = "";

            var p = buscador.Seleccionado();
            if (p == null)
            {
                Mensaje("Elige un producto de la lista (escribe su código o nombre).", Estilo.Error);
                buscador.Focus();
                return;
            }

            int conteo = (int)numConteo.Value;
            int dif = conteo - p.Stock;
            string motivo = Convert.ToString(cmbMotivo.SelectedItem);

            if (dif == 0)
            {
                Mensaje("El conteo coincide con el sistema: no hay nada que ajustar.", Estilo.Error);
                return;
            }

            string pregunta = p.Codigo + " · " + p.Nombre + "\n\n"
                            + "Stock en el sistema: " + p.Stock + "\n"
                            + "Conteo real: " + conteo + "\n"
                            + "Diferencia: " + dif.ToString("+0;-0") + "\n"
                            + "Motivo: " + motivo + "\n\n"
                            + "El ajuste queda en el historial. Si te equivocas, se corrige con otro ajuste.\n"
                            + "¿Registrarlo?";

            if (MessageBox.Show(pregunta, "Confirmar ajuste",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            btnRegistrar.Enabled = false;   // evita doble clic
            try
            {
                string mensaje;
                if (!negocio.Registrar(p.IdProducto, p.Stock, conteo, motivo, txtNota.Text, out mensaje))
                {
                    Mensaje(mensaje, Estilo.Error);
                    CargarProductos();       // trae el stock actual por si cambió mientras tanto
                    ActualizarProducto();
                    return;
                }

                string resumen = p.Codigo + ": de " + p.Stock + " a " + conteo;

                buscador.Clear();
                txtNota.Clear();
                cmbMotivo.SelectedIndex = 0;
                numConteo.Value = 0;

                CargarProductos();
                if (dtHasta.Value.Date < DateTime.Today) dtHasta.Value = DateTime.Today;   // que se vea el ajuste recién hecho
                Buscar();

                Mensaje("Ajuste registrado. " + resumen, Verde);
                buscador.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Inventario", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnRegistrar.Enabled = true;
            }
        }

        // ---------- Historial ----------

        private void Buscar()
        {
            // Se recuerda el rango realmente buscado para ponerlo en el reporte
            desdeUsado = dtDesde.Value.Date;
            hastaUsado = dtHasta.Value.Date;

            try
            {
                ajustes = negocio.Listar(dtDesde.Value, dtHasta.Value);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Inventario", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ajustes = new List<Ajuste>();
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
            string filtro = txtFiltro.Text.Trim().ToLower();

            var vista = ajustes.Where(a => filtro.Length == 0
                || (a.ProductoCodigo ?? "").ToLower().Contains(filtro)
                || (a.ProductoNombre ?? "").ToLower().Contains(filtro)
                || (a.Motivo ?? "").ToLower().Contains(filtro)
                || (a.Nota ?? "").ToLower().Contains(filtro)
                || (a.UsuarioNombre ?? "").ToLower().Contains(filtro)).ToList();

            int sobrantes = vista.Where(a => a.Diferencia > 0).Sum(a => a.Diferencia);
            int faltantes = vista.Where(a => a.Diferencia < 0).Sum(a => -a.Diferencia);

            lblResumen.Text = vista.Count + (vista.Count == 1 ? " ajuste" : " ajustes")
                              + "  ·  +" + sobrantes + " / -" + faltantes + " piezas";

            grid.Rows.Clear();
            foreach (var a in vista)
            {
                int i = grid.Rows.Add(a.FechaRegistro.ToString("dd/MM/yyyy HH:mm"),
                                      a.ProductoCodigo, a.ProductoNombre,
                                      a.StockAnterior, a.StockNuevo, a.Diferencia,
                                      a.Motivo, a.Nota, a.UsuarioNombre);

                var celda = grid.Rows[i].Cells["Diferencia"];
                celda.Style.Font = Estilo.Negrita;
                celda.Style.ForeColor = a.Diferencia < 0 ? Estilo.Error : Verde;
            }
            grid.ClearSelection();
        }
    }
}