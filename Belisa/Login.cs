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
            Estilo.ConfigurarVentana(this, 720, 450, "Belisa - Iniciar sesión");
            BackColor = Color.White;

            // Lado izquierdo: logo (la imagen cuadrada tiene fondo blanco, como la ventana)
            var logo = Estilo.Imagen(Properties.Resources.BelisaLogoCuadrado, 20, 60, 310, 330);

            // Línea divisoria
            var linea = new Panel { Location = new Point(350, 80), Size = new Size(1, 290), BackColor = Estilo.LilaClaro };

            // Lado derecho: formulario
            var titulo = new Label
            {
                Text = "Iniciar sesión",
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = Estilo.Texto,
                AutoSize = true,
                Location = new Point(385, 75)
            };
            var subtitulo = new Label
            {
                Text = "Punto de venta",
                Font = Estilo.Normal,
                ForeColor = Estilo.Acento,
                AutoSize = true,
                Location = new Point(388, 113)
            };

            txtuser = Estilo.Campo(385, 175, 300);
            txtpass = Estilo.Campo(385, 245, 300, true);

            lblerror = new Label
            {
                Location = new Point(385, 280),
                Size = new Size(300, 40),
                ForeColor = Estilo.Error,
                Font = Estilo.Normal
            };

            btningresar = Estilo.Boton("Ingresar", 385, 325, 300, 42, true);
            btncancelar = Estilo.Boton("Cancelar", 385, 377, 300, 36, false);

            btningresar.Click += (s, e) => Ingresar();
            btncancelar.Click += (s, e) => Close();

            Controls.AddRange(new Control[]
            {
                logo, linea, titulo, subtitulo,
                Estilo.Etiqueta("Usuario", 385, 148), txtuser,
                Estilo.Etiqueta("Contraseña", 385, 218), txtpass,
                lblerror, btningresar, btncancelar
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
                MessageBox.Show("No se pudo conectar con la base de datos.\n\n" + ex.Message,
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