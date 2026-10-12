using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BelisaEntidad;
using BelisaNegocio;

namespace Belisa
{
    [DesignerCategory("Code")]
    public class Inicio : Form
    {
        private const int AnchoMenu = 240;

        private readonly Usuario usuario;
        private HashSet<string> permisos;

        private Panel contenedor, bienvenida;
        private FlowLayoutPanel menu;
        private Label lblSeccion, lblUsuario;
        private BotonMenu botonInicio, botonActivo;
        private Form formularioActivo;

        public Inicio(Usuario usuario)
        {
            // 1. Evitar parpadeos y asegurar rendimiento visual fluido al redimensionar
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();

            // 2. Asegurar el escalado correcto de fuentes en distintas resoluciones
            AutoScaleMode = AutoScaleMode.Font;

            this.usuario = usuario;

            Text = "Belisa - Punto de venta";
            Icon = Estilo.IconoApp;
            MinimumSize = new Size(1200, 680);
            WindowState = FormWindowState.Maximized;
            BackColor = Estilo.Fondo;

            try
            {
                permisos = new HashSet<string>(
                    new UsuarioN().ListarPermisos(usuario.IdRol),
                    StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudieron cargar los permisos.\n\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                permisos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            Sesion.Iniciar(usuario, permisos);
            FormClosed += (s, e) => Sesion.Cerrar();
            FormClosed += (s, e) => RespaldoAutomatico();

            // El orden importa: lo que se agrega primero ocupa el centro,
            // lo que se agrega al final queda en los bordes
            ConstruirContenedor();
            ConstruirBarraSuperior();
            ConstruirMenuLateral();
            Estilo.AgregarFranja(this);

            MostrarBienvenida();
        }

        // Una vez al día, al cerrar sesión, se guarda un respaldo de la base de datos
        private void RespaldoAutomatico()
        {
            string mensaje;
            if (!new RespaldoN().RespaldoAutomaticoSiToca(out mensaje))
            {
                MessageBox.Show(mensaje, "Respaldo automático", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // ---------- Construcción de la pantalla ----------

        private void ConstruirContenedor()
        {
            contenedor = new Panel { Dock = DockStyle.Fill, BackColor = Estilo.Fondo };

            bienvenida = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(247, 247, 247) };
            bienvenida.Controls.Add(new PictureBox
            {
                Dock = DockStyle.Fill,
                Image = Properties.Resources.BelisaLogoCompleto,
                SizeMode = PictureBoxSizeMode.Zoom
            });
            bienvenida.Controls.Add(new Label
            {
                Dock = DockStyle.Bottom,
                Height = 70,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 14f),
                ForeColor = Estilo.Texto,
                Text = "Hola, " + usuario.NombreCompleto
            });

            contenedor.Controls.Add(bienvenida);
            Controls.Add(contenedor);
        }

        private void ConstruirBarraSuperior()
        {
            var barra = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White };
            barra.Controls.Add(new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = Estilo.LilaClaro });

            lblSeccion = new Label
            {
                AutoSize = false,
                Location = new Point(24, 0),
                Size = new Size(400, 59),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 15f, FontStyle.Bold),
                ForeColor = Estilo.Texto
            };

            lblUsuario = new Label
            {
                AutoSize = false,
                Size = new Size(320, 59),
                TextAlign = ContentAlignment.MiddleRight,
                Font = Estilo.Normal,
                ForeColor = Estilo.Texto,
                Text = usuario.NombreCompleto + "  ·  " + usuario.RolDescripcion
            };

            var btnClave = Estilo.Boton("Mi contraseña", 0, 12, 130, 36, false);
            btnClave.Click += (s, e) =>
            {
                using (var f = new FrmCambiarClave()) f.ShowDialog(this);
            };

            var btnSalir = Estilo.Boton("Cerrar sesión", 0, 12, 130, 36, false);
            btnSalir.Click += (s, e) => Close();   // el Login vuelve a mostrarse solo

            barra.Controls.AddRange(new Control[] { lblSeccion, lblUsuario, btnClave, btnSalir });
            barra.Resize += (s, e) =>
            {
                btnSalir.Left = barra.Width - btnSalir.Width - 24;
                btnClave.Left = btnSalir.Left - btnClave.Width - 10;
                lblUsuario.Left = btnClave.Left - lblUsuario.Width - 12;
            };

            Controls.Add(barra);
        }

        private void ConstruirMenuLateral()
        {
            var lateral = new Panel { Dock = DockStyle.Left, Width = AnchoMenu, BackColor = Color.White };

            // Cabecera con la marca
            var cabecera = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = Color.White };
            cabecera.Controls.Add(Estilo.Imagen(Properties.Resources.BelisaLogo, 18, 18, 62, 62));
            cabecera.Controls.Add(new Label
            {
                Text = "Belisa",
                Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                ForeColor = Estilo.Lila,
                AutoSize = true,
                Location = new Point(86, 20)
            });
            cabecera.Controls.Add(new Label
            {
                Text = "Centro de depilación",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Estilo.Texto,
                AutoSize = true,
                Location = new Point(89, 60)
            });
            cabecera.Controls.Add(new Panel { Dock = DockStyle.Bottom, Height = 3, BackColor = Estilo.Dorado });

            menu = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.White,
                Padding = new Padding(0, 8, 0, 0),
                AutoScroll = true
            };

            // ---- Opciones del menú (el nombre debe coincidir con la tabla PERMISO) ----
            botonInicio = NuevoBoton("Inicio", 0);
            botonInicio.Click += (s, e) => MostrarBienvenida();
            menu.Controls.Add(botonInicio);

            AgregarItem("menuventas", "Ventas", new[] { "Registrar venta", "Ver detalle" },
                new Func<Form>[] { () => new FrmVenta(), () => new FrmDetalleVenta() });

            AgregarItem("menucompras", "Compras", new[] { "Registrar compra", "Ver detalle" },
                new Func<Form>[] { () => new FrmCompra(), () => new FrmDetalleCompra() });

            AgregarItem("menugestion", "Gestión", new[] { "Categorías", "Productos y servicios", "Ajuste de inventario" },
                new Func<Form>[] { () => new FrmCategoria(), () => new FrmProducto(), () => new FrmAjuste() });

            AgregarItem("menuclientes", "Clientes", () => new FrmCliente());
            AgregarItem("menuproveedor", "Proveedores", () => new FrmProveedor());
            AgregarItem("menuusuarios", "Usuarios", () => new FrmUsuario());
            AgregarItem("menurespaldo", "Respaldos", () => new FrmRespaldo());
            AgregarItem("menureportes", "Reportes", () => new FrmReportes());
            AgregarItem("menuacercade", "Acerca de", () => new FrmAcercaDe());

            lateral.Controls.Add(menu);
            lateral.Controls.Add(cabecera);
            lateral.Controls.Add(new Panel { Dock = DockStyle.Right, Width = 1, BackColor = Estilo.LilaClaro });
            Controls.Add(lateral);
        }

        private BotonMenu NuevoBoton(string texto, int sangria)
        {
            // Ajustamos el ancho restando 16 píxeles para evitar que cause scroll horizontal al aparecer la barra vertical
            return new BotonMenu(texto, sangria, AnchoMenu - 17);
        }

        // Opción simple (sin submenú)
        private void AgregarItem(string permiso, string texto, Func<Form> crear)
        {
            var b = NuevoBoton(texto, 0);
            b.Visible = permisos.Contains(permiso);
            b.Click += (s, e) => Abrir(b, crear());
            menu.Controls.Add(b);
        }

        // Opción con submenú desplegable
        private void AgregarItem(string permiso, string texto, string[] subtextos, Func<Form>[] fabricas)
        {
            bool visible = permisos.Contains(permiso);

            // Grupo que contiene la cabecera y el panel de subitems
            var grupo = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                Width = AnchoMenu - 17
            };

            var cabecera = NuevoBoton(texto + "  ▾", 0);
            cabecera.Visible = visible;
            cabecera.Width = AnchoMenu - 17;
            grupo.Controls.Add(cabecera);

            var panelSubs = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                Visible = false,
                Width = AnchoMenu - 20,
                Padding = new Padding(0, 0, 0, 0)
            };

            for (int i = 0; i < subtextos.Length; i++)
            {
                int idx = i;
                var sub = NuevoBoton(subtextos[i], 22);
                sub.Click += (s, e) => Abrir(sub, fabricas[idx]());
                sub.Width = AnchoMenu - 39;
                panelSubs.Controls.Add(sub);
            }

            grupo.Controls.Add(panelSubs);
            menu.Controls.Add(grupo);

            bool abierto = false;
            cabecera.Click += (s, e) =>
            {
                abierto = !abierto;
                cabecera.Text = texto + (abierto ? "  ▴" : "  ▾");
                panelSubs.Visible = abierto;
                // Asegurar visibilidad del grupo cuando se expande
                if (abierto) menu.ScrollControlIntoView(grupo);
            };
        }

        // ---------- Navegación ----------

        private void CerrarFormularioActivo()
        {
            if (formularioActivo != null)
            {
                formularioActivo.Close();
                formularioActivo.Dispose();
                formularioActivo = null;
            }
        }

        private void MarcarActivo(BotonMenu boton)
        {
            if (botonActivo != null) botonActivo.Activo = false;
            botonActivo = boton;
            boton.Activo = true;
            lblSeccion.Text = boton.Text;
        }

        private void MostrarBienvenida()
        {
            CerrarFormularioActivo();
            MarcarActivo(botonInicio);
            bienvenida.Visible = true;
        }

        private void Abrir(BotonMenu boton, Form formulario)
        {
            CerrarFormularioActivo();
            MarcarActivo(boton);
            bienvenida.Visible = false;

            formularioActivo = formulario;
            formulario.TopLevel = false;
            formulario.FormBorderStyle = FormBorderStyle.None;
            formulario.Dock = DockStyle.Fill;
            formulario.BackColor = Estilo.Fondo;
            contenedor.Controls.Add(formulario);
            formulario.Show();
        }
    }

    // Botón del menú lateral: resalta el elemento activo con una barra lila
    [DesignerCategory("Code")]
    public class BotonMenu : Button
    {
        private bool activo;

        public bool Activo
        {
            get { return activo; }
            set
            {
                activo = value;
                BackColor = value ? Estilo.LilaSuave : Color.White;
                ForeColor = value ? Estilo.Acento : Estilo.Texto;
                Font = value ? Estilo.Negrita : Estilo.Normal;
                Invalidate();
            }
        }

        public BotonMenu(string texto, int sangria, int ancho)
        {
            // Evitar parpadeos y asegurar rendimiento visual fluido
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();

            Text = texto;
            Size = new Size(ancho, 44);
            Margin = new Padding(0);
            Padding = new Padding(22 + sangria, 0, 0, 0);
            TextAlign = ContentAlignment.MiddleLeft;
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            FlatAppearance.MouseOverBackColor = Estilo.LilaSuave;
            FlatAppearance.MouseDownBackColor = Estilo.LilaClaro;
            BackColor = Color.White;
            ForeColor = Estilo.Texto;
            Font = Estilo.Normal;
            Cursor = Cursors.Hand;
            UseVisualStyleBackColor = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (activo)
            {
                using (var pincel = new SolidBrush(Estilo.Lila))
                    e.Graphics.FillRectangle(pincel, 0, 0, 5, Height);
            }
        }
    }
}