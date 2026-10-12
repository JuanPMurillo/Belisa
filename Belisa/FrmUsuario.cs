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
    public class FrmUsuario : Form
    {
        private static readonly Color Verde = Color.FromArgb(39, 130, 80);

        private readonly UsuarioN negocio = new UsuarioN();
        private List<Usuario> usuarios = new List<Usuario>();
        private List<Rol> roles = new List<Rol>();
        private Usuario seleccion;     // null = se está creando uno nuevo
        private bool cargando;

        private DataGridView grid;
        private TextBox txtBuscar, txtUsuario, txtNombre, txtCorreo, txtClave, txtConfirmar;
        private ComboBox cmbRol;
        private CheckBox chkActivo;
        private Label lblTitulo, lblClave, lblMensaje, lblResumen;
        private Button btnGuardar, btnNuevo, btnRestablecer, btnDesbloquear;

        public FrmUsuario()
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

            grid = Tabla.Crear();
            grid.Columns.Add("Id", "Id");
            grid.Columns["Id"].Visible = false;
            Tabla.Columna(grid, "Usuario", "Usuario", 80);
            Tabla.Columna(grid, "Nombre", "Nombre completo", 140);
            Tabla.Columna(grid, "Rol", "Rol", 90);
            Tabla.Columna(grid, "Correo", "Correo", 120);
            Tabla.ColumnaFija(grid, "Estado", "Estado", 110, null, false);

            grid.SelectionChanged += (s, e) =>
            {
                if (cargando || grid.CurrentRow == null) return;
                var u = grid.CurrentRow.Tag as Usuario;
                if (u != null) Mostrar(u);
            };

            var barra = new Panel { Dock = DockStyle.Top, Height = 52 };
            lblResumen = new Label
            {
                Dock = DockStyle.Right,
                Width = 260,
                TextAlign = ContentAlignment.MiddleRight,
                Font = Estilo.Normal,
                ForeColor = Estilo.Texto
            };

            var btnExcel = Exportar.BotonExcel(grid, "Usuarios");
            barra.Controls.Add(lblResumen);
            barra.Controls.Add(btnExcel);

            barra.Controls.Add(Estilo.Etiqueta("Buscar", 0, 16));

            txtBuscar = Estilo.Campo(52, 12, 180);
            txtBuscar.TextChanged += (s, e) => Refrescar();
            barra.Controls.Add(txtBuscar);

            panel.Controls.Add(grid);
            panel.Controls.Add(barra);
            Controls.Add(panel);
        }

        private void ConstruirFormulario()
        {
            var panel = new Panel { Dock = DockStyle.Right, Width = 400, BackColor = Color.White, AutoScroll = true };

            lblTitulo = new Label
            {
                Location = new Point(28, 16),
                AutoSize = true,
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Estilo.Texto
            };

            txtUsuario = Estilo.Campo(28, 78, 344);
            txtUsuario.MaxLength = 50;
            txtNombre = Estilo.Campo(28, 140, 344);
            txtNombre.MaxLength = 100;
            txtCorreo = Estilo.Campo(28, 202, 344);
            txtCorreo.MaxLength = 100;

            cmbRol = new ComboBox
            {
                Location = new Point(28, 264),
                Width = 344,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = Estilo.Normal,
                DisplayMember = "Descripcion",
                ValueMember = "IdRol"
            };

            chkActivo = new CheckBox
            {
                Text = "Usuario activo",
                Location = new Point(28, 304),
                AutoSize = true,
                Font = Estilo.Normal,
                ForeColor = Estilo.Texto
            };

            lblClave = Estilo.Etiqueta("Contraseña", 28, 340);
            txtClave = Estilo.Campo(28, 362, 344, true);
            txtConfirmar = Estilo.Campo(28, 424, 344, true);

            lblMensaje = new Label
            {
                Location = new Point(28, 458),
                Size = new Size(344, 54),
                Font = Estilo.Normal
            };

            btnGuardar = Estilo.Boton("Guardar", 28, 516, 210, 42, true);
            btnNuevo = Estilo.Boton("Nuevo", 250, 516, 122, 42, false);
            btnRestablecer = Estilo.Boton("Restablecer contraseña", 28, 568, 344, 36, false);
            btnDesbloquear = Estilo.Boton("Desbloquear cuenta", 28, 612, 344, 36, false);

            btnGuardar.Click += (s, e) => Guardar();
            btnNuevo.Click += (s, e) => Nuevo();
            btnRestablecer.Click += (s, e) => Restablecer();
            btnDesbloquear.Click += (s, e) => Desbloquear();

            // Enter pasa al siguiente campo; en el último, guarda o restablece
            EnterPasaA(txtUsuario, txtNombre);
            EnterPasaA(txtNombre, txtCorreo);
            EnterPasaA(txtCorreo, txtClave);
            EnterPasaA(txtClave, txtConfirmar);
            txtConfirmar.KeyDown += (s, e) =>
            {
                if (e.KeyCode != Keys.Enter) return;
                e.SuppressKeyPress = true;
                if (seleccion == null) Guardar(); else Restablecer();
            };

            panel.Controls.AddRange(new Control[]
            {
                lblTitulo,
                Estilo.Etiqueta("Usuario (para iniciar sesión)", 28, 54), txtUsuario,
                Estilo.Etiqueta("Nombre completo", 28, 116), txtNombre,
                Estilo.Etiqueta("Correo (opcional)", 28, 178), txtCorreo,
                Estilo.Etiqueta("Rol", 28, 240), cmbRol,
                chkActivo,
                lblClave, txtClave,
                Estilo.Etiqueta("Confirmar contraseña", 28, 402), txtConfirmar,
                lblMensaje, btnGuardar, btnNuevo, btnRestablecer, btnDesbloquear,
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
                usuarios = negocio.Listar();
                if (roles.Count == 0)
                {
                    roles = negocio.ListarRoles();
                    cmbRol.DataSource = roles;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Usuarios", MessageBoxButtons.OK, MessageBoxIcon.Error);
                usuarios = new List<Usuario>();
            }
            Refrescar();
        }

        private static string TextoEstado(Usuario u)
        {
            if (!u.Estado) return "Inactivo";
            return u.Bloqueado ? "Bloqueado" : "Activo";
        }

        // Redibuja la tabla aplicando el filtro de búsqueda
        private void Refrescar()
        {
            cargando = true;
            string filtro = txtBuscar.Text.Trim().ToLower();
            int? idActual = seleccion == null ? (int?)null : seleccion.IdUsuario;

            var vista = usuarios.Where(x => filtro.Length == 0
                || (x.NameUser ?? "").ToLower().Contains(filtro)
                || (x.NombreCompleto ?? "").ToLower().Contains(filtro)
                || (x.RolDescripcion ?? "").ToLower().Contains(filtro)
                || (x.Correo ?? "").ToLower().Contains(filtro)).ToList();

            lblResumen.Text = vista.Count + " de " + usuarios.Count + " usuarios  ·  "
                            + usuarios.Count(x => x.Estado) + " activos";

            grid.Rows.Clear();
            foreach (var u in vista)
            {
                int i = grid.Rows.Add(u.IdUsuario, u.NameUser, u.NombreCompleto,
                                   u.RolDescripcion, u.Correo, TextoEstado(u));
                var fila = grid.Rows[i];
                fila.Tag = u;

                if (!u.Estado)
                {
                    fila.DefaultCellStyle.ForeColor = Color.Gray;
                }
                else if (u.Bloqueado)
                {
                    fila.Cells["Estado"].Style.ForeColor = Estilo.Error;
                    fila.Cells["Estado"].Style.Font = Estilo.Negrita;
                }
            }

            grid.ClearSelection();
            if (idActual.HasValue)
            {
                foreach (DataGridViewRow fila in grid.Rows)
                {
                    if (((Usuario)fila.Tag).IdUsuario == idActual.Value)
                    {
                        fila.Selected = true;
                        grid.CurrentCell = fila.Cells["Usuario"];
                        break;
                    }
                }
            }
            cargando = false;
        }

        // ---------- Formulario ----------

        private void Mensaje(string texto, Color color)
        {
            lblMensaje.ForeColor = color;
            lblMensaje.Text = texto;
        }

        private void Mostrar(Usuario u)
        {
            seleccion = u;
            bool esYo = Sesion.UsuarioActual != null && u.IdUsuario == Sesion.UsuarioActual.IdUsuario;

            lblTitulo.Text = "Editar usuario";
            txtUsuario.Text = u.NameUser;
            txtNombre.Text = u.NombreCompleto;
            txtCorreo.Text = u.Correo;
            cmbRol.SelectedValue = u.IdRol;
            chkActivo.Checked = u.Estado;

            // Nadie puede desactivarse ni cambiarse el rol a sí mismo
            chkActivo.Enabled = !esYo;
            cmbRol.Enabled = !esYo;

            lblClave.Text = "Nueva contraseña (solo para restablecerla)";
            txtClave.Clear();
            txtConfirmar.Clear();
            btnRestablecer.Visible = true;
            btnRestablecer.Enabled = !esYo;
            btnDesbloquear.Visible = u.Bloqueado;

            if (esYo)
                Mensaje("Es tu propio usuario: no puedes desactivarte ni cambiar tu rol. Tu contraseña se cambia con «Mi contraseña», arriba a la derecha.", Color.Gray);
            else
                lblMensaje.Text = "";
        }

        private void Nuevo()
        {
            seleccion = null;
            lblTitulo.Text = "Nuevo usuario";
            txtUsuario.Clear();
            txtNombre.Clear();
            txtCorreo.Clear();

            // Por defecto, el rol con menos permisos
            var empleado = roles.FirstOrDefault(r => r.Descripcion == "EMPLEADO");
            cmbRol.SelectedIndex = empleado == null ? -1 : roles.IndexOf(empleado);
            cmbRol.Enabled = true;

            chkActivo.Checked = true;
            chkActivo.Enabled = true;

            lblClave.Text = "Contraseña inicial (mín. 8, letras y números)";
            txtClave.Clear();
            txtConfirmar.Clear();
            btnRestablecer.Visible = false;
            btnDesbloquear.Visible = false;
            lblMensaje.Text = "";

            cargando = true;                // evita que el evento de la tabla vuelva a cargar la fila
            grid.CurrentCell = null;
            grid.ClearSelection();
            cargando = false;

            txtUsuario.Focus();
        }

        private Usuario Leer()
        {
            return new Usuario
            {
                IdUsuario = seleccion == null ? 0 : seleccion.IdUsuario,
                NameUser = txtUsuario.Text,
                NombreCompleto = txtNombre.Text,
                Correo = txtCorreo.Text,
                IdRol = cmbRol.SelectedValue == null ? 0 : (int)cmbRol.SelectedValue,
                Estado = chkActivo.Checked
            };
        }

        // Recarga la tabla y deja marcado el usuario con ese Id
        private void Recargar(int idUsuario)
        {
            seleccion = new Usuario { IdUsuario = idUsuario };
            Cargar();

            var u = usuarios.FirstOrDefault(x => x.IdUsuario == idUsuario);
            if (u != null) Mostrar(u); else Nuevo();
        }

        private void Guardar()
        {
            lblMensaje.Text = "";

            var u = Leer();
            bool esNuevo = seleccion == null;
            bool ok;
            string mensaje;

            try
            {
                if (esNuevo)
                {
                    if (txtClave.Text != txtConfirmar.Text)
                    {
                        Mensaje("Las contraseñas no coinciden.", Estilo.Error);
                        return;
                    }
                    ok = negocio.Crear(u, txtClave.Text, out mensaje);
                }
                else
                {
                    ok = negocio.Editar(u, out mensaje);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Usuarios", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!ok)
            {
                Mensaje(mensaje, Estilo.Error);
                return;
            }

            bool teniaClaveEscrita = !esNuevo && txtClave.Text.Length > 0;
            Recargar(u.IdUsuario);

            string texto = esNuevo ? "Usuario creado. Pídele que cambie su contraseña con «Mi contraseña»." : "Guardado correctamente.";
            if (teniaClaveEscrita)
                texto += " La contraseña no se cambió: usa «Restablecer contraseña».";
            Mensaje(texto, Verde);
        }

        private void Restablecer()
        {
            lblMensaje.Text = "";
            if (seleccion == null) return;

            if (txtClave.Text != txtConfirmar.Text)
            {
                Mensaje("Las contraseñas no coinciden.", Estilo.Error);
                return;
            }

            if (MessageBox.Show(
                    "¿Cambiar la contraseña de " + seleccion.NombreCompleto + "?\n\n" +
                    "Si su cuenta estaba bloqueada por intentos fallidos, se desbloquea.",
                    "Restablecer contraseña", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            int id = seleccion.IdUsuario;
            string mensaje;
            try
            {
                if (!negocio.RestablecerClave(id, txtClave.Text, out mensaje))
                {
                    Mensaje(mensaje, Estilo.Error);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Usuarios", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Recargar(id);
            Mensaje("Contraseña actualizada.", Verde);
        }

        private void Desbloquear()
        {
            if (seleccion == null) return;

            int id = seleccion.IdUsuario;
            try
            {
                negocio.Desbloquear(id);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Usuarios", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Recargar(id);
            Mensaje("Cuenta desbloqueada.", Verde);
        }
    }
}