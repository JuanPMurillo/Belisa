using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BelisaEntidad;
using BelisaNegocio;

namespace Belisa
{
    public class FrmCategoria : Form
    {
        private readonly CategoriaN negocio = new CategoriaN();
        private List<Categoria> categorias = new List<Categoria>();
        private Categoria seleccion;     // null = se está creando una nueva
        private bool cargando;

        private DataGridView grid;
        private TextBox txtBuscar, txtDescripcion;
        private CheckBox chkActiva;
        private Label lblTitulo, lblMensaje;
        private Button btnGuardar, btnNuevo;

        public FrmCategoria()
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
            grid.Columns.Add("Descripcion", "Descripción");
            grid.Columns.Add("Estado", "Estado");
            grid.Columns["Estado"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            grid.Columns["Estado"].Width = 110;

            grid.SelectionChanged += (s, e) =>
            {
                if (cargando || grid.CurrentRow == null) return;
                var c = grid.CurrentRow.Tag as Categoria;
                if (c != null) Mostrar(c);
            };

            var barraBuscar = new Panel { Dock = DockStyle.Top, Height = 52 };
            barraBuscar.Controls.Add(Estilo.Etiqueta("Buscar", 0, 16));
            txtBuscar = Estilo.Campo(70, 12, 300);
            txtBuscar.TextChanged += (s, e) => Refrescar();
            barraBuscar.Controls.Add(txtBuscar);

            panel.Controls.Add(grid);
            panel.Controls.Add(barraBuscar);
            Controls.Add(panel);
        }

        private void ConstruirFormulario()
        {
            var panel = new Panel { Dock = DockStyle.Right, Width = 340, BackColor = Color.White };

            lblTitulo = new Label
            {
                Location = new Point(28, 28),
                AutoSize = true,
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Estilo.Texto
            };

            txtDescripcion = Estilo.Campo(28, 110, 284);
            txtDescripcion.MaxLength = 100;
            txtDescripcion.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; Guardar(); }
            };

            chkActiva = new CheckBox
            {
                Text = "Categoría activa",
                Location = new Point(28, 158),
                AutoSize = true,
                Font = Estilo.Normal,
                ForeColor = Estilo.Texto
            };

            lblMensaje = new Label
            {
                Location = new Point(28, 195),
                Size = new Size(284, 44),
                Font = Estilo.Normal
            };

            btnGuardar = Estilo.Boton("Guardar", 28, 248, 284, 42, true);
            btnGuardar.Click += (s, e) => Guardar();

            btnNuevo = Estilo.Boton("Nueva categoría", 28, 300, 284, 36, false);
            btnNuevo.Click += (s, e) => Nuevo();

            panel.Controls.AddRange(new Control[]
            {
                lblTitulo,
                Estilo.Etiqueta("Descripción", 28, 83), txtDescripcion,
                chkActiva, lblMensaje, btnGuardar, btnNuevo,
                new Panel { Dock = DockStyle.Left, Width = 1, BackColor = Estilo.LilaClaro }
            });

            Controls.Add(panel);
        }

        // ---------- Datos ----------

        private void Cargar()
        {
            try
            {
                categorias = negocio.Listar();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Categorías", MessageBoxButtons.OK, MessageBoxIcon.Error);
                categorias = new List<Categoria>();
            }
            Refrescar();
        }

        // Redibuja la tabla aplicando el filtro de búsqueda
        private void Refrescar()
        {
            cargando = true;
            string filtro = txtBuscar.Text.Trim().ToLower();
            int? idActual = seleccion == null ? (int?)null : seleccion.IdCategoria;

            grid.Rows.Clear();
            foreach (var c in categorias.Where(x => filtro.Length == 0 || x.Descripcion.ToLower().Contains(filtro)))
            {
                int i = grid.Rows.Add(c.IdCategoria, c.Descripcion, c.Estado ? "Activa" : "Inactiva");
                grid.Rows[i].Tag = c;
                if (!c.Estado) grid.Rows[i].DefaultCellStyle.ForeColor = Color.Gray;
            }

            grid.ClearSelection();
            if (idActual.HasValue)
            {
                foreach (DataGridViewRow fila in grid.Rows)
                {
                    if (((Categoria)fila.Tag).IdCategoria == idActual.Value)
                    {
                        fila.Selected = true;
                        grid.CurrentCell = fila.Cells["Descripcion"];
                        break;
                    }
                }
            }
            cargando = false;
        }

        // ---------- Formulario ----------

        private void Mostrar(Categoria c)
        {
            seleccion = c;
            lblTitulo.Text = "Editar categoría";
            txtDescripcion.Text = c.Descripcion;
            chkActiva.Checked = c.Estado;
            lblMensaje.Text = "";
        }

        private void Nuevo()
        {
            seleccion = null;
            lblTitulo.Text = "Nueva categoría";
            txtDescripcion.Clear();
            chkActiva.Checked = true;
            lblMensaje.Text = "";

            cargando = true;
            grid.CurrentCell = null;
            grid.ClearSelection();
            cargando = false;

            txtDescripcion.Focus();
        }

        private void Guardar()
        {
            lblMensaje.Text = "";

            var c = new Categoria
            {
                IdCategoria = seleccion == null ? 0 : seleccion.IdCategoria,
                Descripcion = txtDescripcion.Text,
                Estado = chkActiva.Checked
            };

            try
            {
                string mensaje;
                if (!negocio.Guardar(c, out mensaje))
                {
                    lblMensaje.ForeColor = Estilo.Error;
                    lblMensaje.Text = mensaje;
                    return;
                }

                seleccion = new Categoria { IdCategoria = c.IdCategoria };   // para que se quede marcada
                Cargar();
                Mostrar(categorias.First(x => x.IdCategoria == c.IdCategoria));
                lblMensaje.ForeColor = Color.FromArgb(39, 130, 80);
                lblMensaje.Text = "Guardado correctamente.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Categorías", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}