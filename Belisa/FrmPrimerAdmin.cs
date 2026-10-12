using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BelisaNegocio;

namespace Belisa
{
    [DesignerCategory("Code")]
    public class FrmPrimerAdmin : Form
    {
        private readonly UsuarioN negocio = new UsuarioN();
        private TextBox txtuser, txtnombre, txtcorreo, txtclave, txtconfirmar;
        private Label lblerror;
        private Button btncrear;

        public FrmPrimerAdmin()
        {
            // 1. Evitar parpadeos y asegurar rendimiento visual fluido al redimensionar
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();

            // 2. Asegurar el escalado correcto de fuentes en distintas resoluciones
            AutoScaleMode = AutoScaleMode.Font;

            Estilo.ConfigurarVentana(this, 400, 560, "Belisa - Configuración inicial");

            var titulo = new Label
            {
                Text = "Bienvenido",
                Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                ForeColor = Estilo.Acento,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 20),
                Size = new Size(400, 45)
            };
            var aviso = new Label
            {
                Text = "Crea el usuario administrador del sistema.\nGuarda bien tu contraseña.",
                Font = Estilo.Normal,
                ForeColor = Estilo.Texto,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 65),
                Size = new Size(400, 45)
            };

            txtuser = Estilo.Campo(40, 145, 320);
            txtnombre = Estilo.Campo(40, 210, 320);
            txtcorreo = Estilo.Campo(40, 275, 320);
            txtclave = Estilo.Campo(40, 340, 320, true);
            txtconfirmar = Estilo.Campo(40, 405, 320, true);

            lblerror = new Label
            {
                Location = new Point(40, 440),
                Size = new Size(320, 40),
                ForeColor = Estilo.Error,
                Font = Estilo.Normal
            };

            btncrear = Estilo.Boton("Crear administrador", 40, 490, 320, 42, true);
            btncrear.Click += (s, e) => Crear();

            Controls.AddRange(new Control[]
            {
                titulo, aviso,
                Estilo.Etiqueta("Usuario", 40, 118), txtuser,
                Estilo.Etiqueta("Nombre completo", 40, 183), txtnombre,
                Estilo.Etiqueta("Correo (opcional)", 40, 248), txtcorreo,
                Estilo.Etiqueta("Contraseña (mín. 8, letras y números)", 40, 313), txtclave,
                Estilo.Etiqueta("Confirmar contraseña", 40, 378), txtconfirmar,
                lblerror, btncrear
            });

            AcceptButton = btncrear;
        }

        private void Crear()
        {
            lblerror.Text = "";

            if (txtclave.Text != txtconfirmar.Text)
            {
                lblerror.Text = "Las contraseñas no coinciden.";
                return;
            }

            string mensaje;
            bool ok;
            try
            {
                ok = negocio.CrearPrimerAdmin(txtuser.Text, txtnombre.Text, txtcorreo.Text,
                                              txtclave.Text, out mensaje);
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo guardar en la base de datos.\n\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!ok)
            {
                lblerror.Text = mensaje;
                return;
            }

            MessageBox.Show("Administrador creado. Ahora inicia sesión.", "Listo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
        }
    }
}