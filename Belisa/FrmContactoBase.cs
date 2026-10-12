using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BelisaNegocio;

namespace Belisa
{
    // Datos mínimos que comparten clientes y proveedores
    public class Contacto
    {
        public int Id { get; set; }
        public string Documento { get; set; }   // el código automático (CLI-0001, PRV-0001...)
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string Telefono { get; set; }
        public bool Estado { get; set; }
        public int? CumpleDia { get; set; }     // solo se usa en clientes
        public int? CumpleMes { get; set; }
    }

    [DesignerCategory("Code")]   // evita que Visual Studio intente abrirla en el diseñador
    public abstract class FrmContactoBase : Form
    {
        private readonly string singular;              // "cliente"
        private readonly string etiquetaNombre;    // "Nombre completo"
        private readonly bool conCumpleanos;       // solo los clientes guardan su cumpleaños

        private List<Contacto> contactos = new List<Contacto>();
        private Contacto seleccion;                 // null = se está creando uno nuevo
        private bool cargando;

        private DataGridView grid;
        private TextBox txtBuscar, txtNombre, txtTelefono, txtCorreo;
        private ComboBox cmbCumpleDia, cmbCumpleMes;
        private CheckBox chkActivo, chkCumpleMes;
        private Label lblTitulo, lblMensaje, lblResumen;
        private Button btnGuardar, btnNuevo;

        // Cada pantalla concreta dice cómo listar y cómo guardar
        protected abstract List<Contacto> ListarContactos();
        protected abstract bool GuardarContacto(Contacto c, out string mensaje);

        protected FrmContactoBase(string singular, string etiquetaNombre)
            : this(singular, etiquetaNombre, false)
        {
        }

        protected FrmContactoBase(string singular, string etiquetaNombre, bool conCumpleanos)
        {
            // 1. Evitar parpadeos y asegurar rendimiento visual fluido al redimensionar
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();

            // 2. Asegurar el escalado correcto de fuentes en distintas resoluciones
            AutoScaleMode = AutoScaleMode.Font;

            this.singular = singular;
            this.etiquetaNombre = etiquetaNombre;
            this.conCumpleanos = conCumpleanos;

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
            if (conCumpleanos)
            {
                grid.Columns.Add("Cumple", "Cumpleaños");
                grid.Columns["Cumple"].FillWeight = 80;
            }
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

            var btnExcel = Exportar.BotonExcel(grid, singular == "cliente" ? "Clientes" : "Proveedores");
            barra.Controls.Add(lblResumen);
            barra.Controls.Add(btnExcel);

            // Controles de búsqueda y filtros organizados de izquierda a derecha sin solaparse
            barra.Controls.Add(Estilo.Etiqueta("Buscar", 0, 16));

            txtBuscar = Estilo.Campo(52, 12, 160);
            txtBuscar.TextChanged += (s, e) => Refrescar();
            barra.Controls.Add(txtBuscar);

            if (conCumpleanos)
            {
                chkCumpleMes = new CheckBox
                {
                    Text = "Cumpleaños este mes",
                    Location = new Point(225, 15),
                    AutoSize = true,
                    Font = Estilo.Normal,
                    ForeColor = Estilo.Texto
                };
                chkCumpleMes.CheckedChanged += (s, e) => Refrescar();
                barra.Controls.Add(chkCumpleMes);
            }

            panel.Controls.Add(grid);
            panel.Controls.Add(barra);
            Controls.Add(panel);
        }

        private static ComboBox NuevoCombo(int x, int y, int ancho, string primero, IEnumerable<string> opciones)
        {
            var combo = new ComboBox
            {
                Location = new Point(x, y),
                Width = ancho,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = Estilo.Normal
            };
            combo.Items.Add(primero);   // la posición 0 significa «sin dato»
            foreach (string opcion in opciones) combo.Items.Add(opcion);
            combo.SelectedIndex = 0;
            return combo;
        }

        private void ConstruirFormulario()
        {
            // El cumpleaños ocupa una fila más: lo que va debajo se recorre hacia abajo
            int extra = conCumpleanos ? 66 : 0;

            var panel = new Panel { Dock = DockStyle.Right, Width = 360, BackColor = Color.White, AutoScroll = true };

            lblTitulo = new Label
            {
                Location = new Point(28, 24),
                AutoSize = true,
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Estilo.Texto
            };

            txtNombre = Estilo.Campo(28, 96, 304);
            txtNombre.MaxLength = 100;
            txtTelefono = Estilo.Campo(28, 162, 304);
            txtTelefono.MaxLength = 50;
            txtCorreo = Estilo.Campo(28, 228, 304);
            txtCorreo.MaxLength = 100;

            if (conCumpleanos)
            {
                cmbCumpleDia = NuevoCombo(28, 294, 90, "—", Enumerable.Range(1, 31).Select(n => n.ToString()));
                cmbCumpleMes = NuevoCombo(128, 294, 204, "—",
                    CumpleanosN.Meses.Select(m => char.ToUpper(m[0]).ToString() + m.Substring(1)));
            }

            // Enter pasa al siguiente campo; en el último, guarda
            EnterPasaA(txtNombre, txtTelefono);
            EnterPasaA(txtTelefono, txtCorreo);
            txtCorreo.KeyDown += (s, e) =>
            {
                if (e.KeyCode != Keys.Enter) return;
                e.SuppressKeyPress = true;
                if (conCumpleanos) cmbCumpleDia.Focus(); else Guardar();
            };

            chkActivo = new CheckBox
            {
                Text = "Activo",
                Location = new Point(28, 274 + extra),
                AutoSize = true,
                Font = Estilo.Normal,
                ForeColor = Estilo.Texto
            };

            lblMensaje = new Label
            {
                Location = new Point(28, 306 + extra),
                Size = new Size(304, 44),
                Font = Estilo.Normal
            };

            btnGuardar = Estilo.Boton("Guardar", 28, 358 + extra, 304, 42, true);
            btnGuardar.Click += (s, e) => Guardar();

            btnNuevo = Estilo.Boton("Nuevo " + singular, 28, 410 + extra, 304, 36, false);
            btnNuevo.Click += (s, e) => Nuevo();

            var controles = new List<Control>
            {
                lblTitulo,
                Estilo.Etiqueta(etiquetaNombre, 28, 70), txtNombre,
                Estilo.Etiqueta("Teléfono (opcional)", 28, 136), txtTelefono,
                Estilo.Etiqueta("Correo (opcional)", 28, 202), txtCorreo
            };
            if (conCumpleanos)
            {
                controles.Add(Estilo.Etiqueta("Cumpleaños (opcional)", 28, 268));
                controles.Add(cmbCumpleDia);
                controles.Add(cmbCumpleMes);
            }
            controles.Add(chkActivo);
            controles.Add(lblMensaje);
            controles.Add(btnGuardar);
            controles.Add(btnNuevo);
            controles.Add(new Panel { Dock = DockStyle.Left, Width = 1, BackColor = Estilo.LilaClaro });

            panel.Controls.AddRange(controles.ToArray());
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
            bool soloEsteMes = conCumpleanos && chkCumpleMes != null && chkCumpleMes.Checked;

            var vista = contactos.Where(x =>
                (filtro.Length == 0
                    || Minusculas(x.Documento).Contains(filtro)
                    || Minusculas(x.Nombre).Contains(filtro)
                    || Minusculas(x.Telefono).Contains(filtro)
                    || Minusculas(x.Correo).Contains(filtro)
                    || (conCumpleanos && Minusculas(CumpleanosN.Texto(x.CumpleDia, x.CumpleMes)).Contains(filtro)))
                && (!soloEsteMes || CumpleanosN.EsEsteMes(x.CumpleMes))).ToList();

            // Con el filtro del mes, las más próximas van primero
            if (soloEsteMes)
                vista = vista.OrderBy(x => x.CumpleDia ?? 99).ThenBy(x => x.Nombre).ToList();

            lblResumen.Text = vista.Count + " de " + contactos.Count + " registrados";

            grid.Rows.Clear();
            foreach (var c in vista)
            {
                object[] valores = conCumpleanos
                    ? new object[] { c.Id, c.Documento, c.Nombre, c.Telefono, c.Correo,
                                     CumpleanosN.Texto(c.CumpleDia, c.CumpleMes),
                                     c.Estado ? "Activo" : "Inactivo" }
                    : new object[] { c.Id, c.Documento, c.Nombre, c.Telefono, c.Correo,
                                     c.Estado ? "Activo" : "Inactivo" };

                int i = grid.Rows.Add(valores);
                var fila = grid.Rows[i];
                fila.Tag = c;

                if (!c.Estado) fila.DefaultCellStyle.ForeColor = Color.Gray;

                // Quien cumple años hoy se resalta para no dejar pasar la promoción
                if (conCumpleanos && CumpleanosN.EsHoy(c.CumpleDia, c.CumpleMes))
                {
                    fila.Cells["Cumple"].Style.Font = Estilo.Negrita;
                    fila.Cells["Cumple"].Style.ForeColor = Estilo.Acento;
                }
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
            txtNombre.Text = c.Nombre;
            txtTelefono.Text = c.Telefono;
            txtCorreo.Text = c.Correo;
            if (conCumpleanos)
            {
                cmbCumpleDia.SelectedIndex = c.CumpleDia ?? 0;
                cmbCumpleMes.SelectedIndex = c.CumpleMes ?? 0;
            }
            chkActivo.Checked = c.Estado;
            lblMensaje.Text = "";
        }

        private void Nuevo()
        {
            seleccion = null;
            lblTitulo.Text = "Nuevo " + singular;
            txtNombre.Clear();
            txtTelefono.Clear();
            txtCorreo.Clear();
            if (conCumpleanos)
            {
                cmbCumpleDia.SelectedIndex = 0;
                cmbCumpleMes.SelectedIndex = 0;
            }
            chkActivo.Checked = true;
            lblMensaje.Text = "";

            cargando = true;                // evita que el evento de la tabla vuelva a cargar la fila
            grid.CurrentCell = null;
            grid.ClearSelection();
            cargando = false;

            txtNombre.Focus();
        }

        private void Guardar()
        {
            lblMensaje.Text = "";
            bool esNuevo = seleccion == null;

            var c = new Contacto
            {
                Id = esNuevo ? 0 : seleccion.Id,
                Documento = "",             // el código lo asigna la base de datos
                Nombre = txtNombre.Text,
                Telefono = txtTelefono.Text,
                Correo = txtCorreo.Text,
                Estado = chkActivo.Checked,
                CumpleDia = conCumpleanos && cmbCumpleDia.SelectedIndex > 0 ? (int?)cmbCumpleDia.SelectedIndex : null,
                CumpleMes = conCumpleanos && cmbCumpleMes.SelectedIndex > 0 ? (int?)cmbCumpleMes.SelectedIndex : null
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
                lblMensaje.Text = "Guardado correctamente."
                                + (esNuevo && guardado != null ? " Código asignado: " + guardado.Documento : "");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}