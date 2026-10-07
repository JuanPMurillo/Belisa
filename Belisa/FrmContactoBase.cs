using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Belisa
{
    // Datos mínimos que comparten clientes y proveedores
    public class Contacto
    {
        public int Id { get; set; }
        public string Documento { get; set; }
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string Telefono { get; set; }
        public bool Estado { get; set; }
    }

    [DesignerCategory("Code")]   // evita que Visual Studio intente abrirla en el diseñador
    public abstract class FrmContactoBase : Form
    {
        private readonly string singular;          // "cliente"
        private readonly string etiquetaNombre;    // "Nombre completo"

        private List<Contacto> contactos = new List<Contacto>();
        private Contacto seleccion;                // null = se está creando uno nuevo
        private bool cargando;

        private DataGridView grid;
        private TextBox txtBuscar, txtDocumento, txtNombre, txtTelefono, txtCorreo;
        private CheckBox chkActivo;
        private Label lblTitulo, lblMensaje, lblResumen;
        private Button btnGuardar, btnNuevo;

        // Cada pantalla concreta dice cómo listar y cómo guardar
        protected abstract List<Contacto> ListarContactos();
        protected abstract bool GuardarContacto(Contacto c, out string mensaje);

        protected FrmContactoBase(string singular, string etiquetaNombre)
        {
            this.singular = singular;
            this.etiquetaNombre = etiquetaNombre;

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
            grid.Columns.Add("Documento", "Código");
            grid.Columns["Documento"].FillWeight = 70;
            grid.Columns.Add("Nombre", etiquetaNombre);
            grid.Columns["Nombre"].FillWeight = 150;
            grid.Columns.Add("Telefono", "Teléfono");
            grid.Columns["Telefono"].FillWeight = 80;
            grid.Columns.Add("Correo", "Correo");
            grid.Columns["Correo"].FillWeight = 120;
            grid.Columns.Add("Estado", "Estado");
            grid.Columns["Estado"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            grid.Columns["Estado"].Width = 90;

            grid.SelectionChanged += (s, e) =>
            {
                if (cargando || grid.CurrentRow == null) return;
                var c = grid.CurrentRow.Tag as Contacto;
                if (c != null) Mostrar(c);
            };

            var barra = new Panel { Dock = DockStyle.Top, Height = 52 };
            lblResumen = new Label
            {
                Dock = DockStyle.Right,
                Width = 220,
                TextAlign = ContentAlignment.MiddleRight,
                Font = Estilo.Normal,
                ForeColor = Estilo.Texto
            };
            barra.Controls.Add(lblResumen);
            barra.Controls.Add(Exportar.BotonExcel(grid, singular == "cliente" ? "Clientes" : "Proveedores"));
            barra.Controls.Add(Estilo.Etiqueta("Buscar", 0, 16));

            txtBuscar = Estilo.Campo(70, 12, 300);
            txtBuscar.TextChanged += (s, e) => Refrescar();
            barra.Controls.Add(txtBuscar);

            panel.Controls.Add(grid);
            panel.Controls.Add(barra);
            Controls.Add(panel);
        }

        private void ConstruirFormulario()
        {
            var panel = new Panel { Dock = DockStyle.Right, Width = 360, BackColor = Color.White, AutoScroll = true };

            lblTitulo = new Label
            {
                Location = new Point(28, 24),
                AutoSize = true,
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Estilo.Texto
            };

            // El código lo asigna la base de datos: el campo solo se puede leer
            txtDocumento = Estilo.Campo(28, 96, 304);
            txtDocumento.MaxLength = 50;
            txtDocumento.ReadOnly = true;
            txtDocumento.TabStop = false;
            txtDocumento.BackColor = Estilo.LilaSuave;

            txtNombre = Estilo.Campo(28, 162, 304);
            txtNombre.MaxLength = 100;
            txtTelefono = Estilo.Campo(28, 228, 304);
            txtTelefono.MaxLength = 50;
            txtCorreo = Estilo.Campo(28, 294, 304);
            txtCorreo.MaxLength = 100;

            // Enter pasa al siguiente campo; en el último, guarda
            EnterPasaA(txtNombre, txtTelefono);
            EnterPasaA(txtTelefono, txtCorreo);
            txtCorreo.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; Guardar(); }
            };

            chkActivo = new CheckBox
            {
                Text = "Activo",
                Location = new Point(28, 340),
                AutoSize = true,
                Font = Estilo.Normal,
                ForeColor = Estilo.Texto
            };

            lblMensaje = new Label
            {
                Location = new Point(28, 372),
                Size = new Size(304, 44),
                Font = Estilo.Normal
            };

            btnGuardar = Estilo.Boton("Guardar", 28, 424, 304, 42, true);
            btnGuardar.Click += (s, e) => Guardar();

            btnNuevo = Estilo.Boton("Nuevo " + singular, 28, 476, 304, 36, false);
            btnNuevo.Click += (s, e) => Nuevo();

            panel.Controls.AddRange(new Control[]
            {
                lblTitulo,
                Estilo.Etiqueta("Código (automático)", 28, 70), txtDocumento,
                Estilo.Etiqueta(etiquetaNombre, 28, 136), txtNombre,
                Estilo.Etiqueta("Teléfono (opcional)", 28, 202), txtTelefono,
                Estilo.Etiqueta("Correo (opcional)", 28, 268), txtCorreo,
                chkActivo, lblMensaje, btnGuardar, btnNuevo,
                new Panel { Dock = DockStyle.Left, Width = 1, BackColor = Estilo.LilaClaro }
            });

            Controls.Add(panel);
        }

        private static void EnterPasaA(TextBox desde, Control hacia)
        {
            desde.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; hacia.Focus(); }
            };
        }

        // ---------- Datos ----------

        private void Cargar()
        {
            try
            {
                contactos = ListarContactos();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                contactos = new List<Contacto>();
            }
            Refrescar();
        }

        private static string Minusculas(string texto)
        {
            return (texto ?? "").ToLower();
        }

        // Redibuja la tabla aplicando el filtro de búsqueda
        private void Refrescar()
        {
            cargando = true;
            string filtro = txtBuscar.Text.Trim().ToLower();
            int? idActual = seleccion == null ? (int?)null : seleccion.Id;

            var vista = contactos.Where(x => filtro.Length == 0
                || Minusculas(x.Documento).Contains(filtro)
                || Minusculas(x.Nombre).Contains(filtro)
                || Minusculas(x.Telefono).Contains(filtro)
                || Minusculas(x.Correo).Contains(filtro)).ToList();

            lblResumen.Text = vista.Count + " de " + contactos.Count + " registrados";

            grid.Rows.Clear();
            foreach (var c in vista)
            {
                int i = grid.Rows.Add(c.Id, c.Documento, c.Nombre, c.Telefono, c.Correo,
                                      c.Estado ? "Activo" : "Inactivo");
                grid.Rows[i].Tag = c;
                if (!c.Estado) grid.Rows[i].DefaultCellStyle.ForeColor = Color.Gray;
            }

            grid.ClearSelection();
            if (idActual.HasValue)
            {
                foreach (DataGridViewRow fila in grid.Rows)
                {
                    if (((Contacto)fila.Tag).Id == idActual.Value)
                    {
                        fila.Selected = true;
                        grid.CurrentCell = fila.Cells["Nombre"];
                        break;
                    }
                }
            }
            cargando = false;
        }

        // ---------- Formulario ----------

        private void Mostrar(Contacto c)
        {
            seleccion = c;
            lblTitulo.Text = "Editar " + singular;
            txtDocumento.Text = c.Documento;
            txtNombre.Text = c.Nombre;
            txtTelefono.Text = c.Telefono;
            txtCorreo.Text = c.Correo;
            chkActivo.Checked = c.Estado;
            lblMensaje.Text = "";
        }

        private void Nuevo()
        {
            seleccion = null;
            lblTitulo.Text = "Nuevo " + singular;
            txtDocumento.Text = "Se asigna al guardar";
            txtNombre.Clear();
            txtTelefono.Clear();
            txtCorreo.Clear();
            chkActivo.Checked = true;
            lblMensaje.Text = "";

            cargando = true;               // evita que el evento de la tabla vuelva a cargar la fila
            grid.CurrentCell = null;
            grid.ClearSelection();
            cargando = false;

            txtNombre.Focus();
        }

        private void Guardar()
        {
            lblMensaje.Text = "";

            var c = new Contacto
            {
                Id = seleccion == null ? 0 : seleccion.Id,
                Documento = "",            // el código lo asigna la base de datos
                Nombre = txtNombre.Text,
                Telefono = txtTelefono.Text,
                Correo = txtCorreo.Text,
                Estado = chkActivo.Checked
            };

            try
            {
                string mensaje;
                if (!GuardarContacto(c, out mensaje))
                {
                    lblMensaje.ForeColor = Estilo.Error;
                    lblMensaje.Text = mensaje;
                    return;
                }

                seleccion = new Contacto { Id = c.Id };   // para que se quede marcado
                Cargar();

                var guardado = contactos.FirstOrDefault(x => x.Id == c.Id);
                if (guardado != null) Mostrar(guardado); else Nuevo();

                lblMensaje.ForeColor = Color.FromArgb(39, 130, 80);
                lblMensaje.Text = "Guardado correctamente.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}