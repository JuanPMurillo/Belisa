using System;
using System.Drawing;
using System.Windows.Forms;
using BelisaEntidad;
using BelisaNegocio;

namespace Belisa
{
    public class Login : Form
    {
        private readonly UsuarioN negocio = new UsuarioN();
        private TextBox txtuser, txtpass;
        private Label lblerror;
        private Button btningresar, btncancelar;

        public Login()
        {
            Estilo.ConfigurarVentana(this, 720, 480, "Belisa - Iniciar sesión");
            BackColor = Color.White;

            // Lado izquierdo: logo (la imagen cuadrada tiene fondo blanco, como la ventana)
            var logo = Estilo.Imagen(Properties.Resources.BelisaLogoCuadrado, 20, 70, 310, 330);

            // Línea divisoria
            var linea = new Panel { Location = new Point(350, 90), Size = new Size(1, 290), BackColor = Estilo.LilaClaro };

            // Lado derecho: formulario
            var titulo = new Label
            {
                Text = "Iniciar sesión",
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = Estilo.Texto,
                AutoSize = true,
                Location = new Point(385, 85)
            };
            var subtitulo = new Label
            {
                Text = "Punto de venta",
                Font = Estilo.Normal,
                ForeColor = Estilo.Acento,
                AutoSize = true,
                Location = new Point(388, 123)
            };

            txtuser = Estilo.Campo(385, 185, 300);
            txtpass = Estilo.Campo(385, 255, 300, true);

            lblerror = new Label
            {
                Location = new Point(385, 290),
                Size = new Size(300, 40),
                ForeColor = Estilo.Error,
                Font = Estilo.Normal
            };

            btningresar = Estilo.Boton("Ingresar", 385, 335, 300, 42, true);
            btncancelar = Estilo.Boton("Cancelar", 385, 387, 300, 36, false);

            var enlaceConexion = new LinkLabel
            {
                Text = "Configurar conexión",
                Location = new Point(385, 434),
                AutoSize = true,
                Font = new Font("Segoe UI", 9f),
                LinkColor = Estilo.Acento,
                ActiveLinkColor = Estilo.AcentoOscuro,
                LinkBehavior = LinkBehavior.HoverUnderline
            };

            btningresar.Click += (s, e) => Ingresar();
            btncancelar.Click += (s, e) => Close();
            enlaceConexion.LinkClicked += (s, e) =>
            {
                using (var f = new FrmConexion("Cambia aquí el servidor o la base de datos. Los cambios se aplican de inmediato."))
                    f.ShowDialog(this);
            };

            Controls.AddRange(new Control[]
            {
                logo, linea, titulo, subtitulo,
                Estilo.Etiqueta("Usuario", 385, 158), txtuser,
                Estilo.Etiqueta("Contraseña", 385, 228), txtpass,
                lblerror, btningresar, btncancelar, enlaceConexion
            });

            Estilo.AgregarFranja(this);

            AcceptButton = btningresar;   // Enter = Ingresar
            CancelButton = btncancelar;   // Esc = Cancelar
        }

        private void Ingresar()
        {
            lblerror.Text = "";
            string mensaje;
            Usuario usuario;

            try
            {
                usuario = negocio.Login(txtuser.Text, txtpass.Text, out mensaje);
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo conectar con la base de datos.\n\n" + ex.Message
                                + "\n\nSi cambió el servidor, usa «Configurar conexión».",
                    "Error de conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (usuario == null)
            {
                lblerror.Text = mensaje;
                txtpass.Clear();
                txtpass.Focus();
                return;
            }

            txtpass.Clear();
            var inicio = new Inicio(usuario);
            inicio.FormClosed += (s, e) =>
            {
                txtuser.Clear();
                txtuser.Focus();
                Show();
            };
            inicio.Show();
            Hide();
        }
    }
}