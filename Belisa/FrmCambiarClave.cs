using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BelisaNegocio;

namespace Belisa
{
    [DesignerCategory("Code")]
    public class FrmCambiarClave : Form
    {
        private readonly UsuarioN negocio = new UsuarioN();
        private readonly TextBox txtActual, txtNueva, txtConfirmar;
        private readonly Label lblMensaje;

        public FrmCambiarClave()
        {
            Estilo.ConfigurarVentana(this, 420, 410, "Cambiar mi contraseña");
            BackColor = Color.White;

            txtActual = Estilo.Campo(30, 56, 360, true);
            txtNueva = Estilo.Campo(30, 126, 360, true);
            txtConfirmar = Estilo.Campo(30, 196, 360, true);

            lblMensaje = new Label
            {
                Location = new Point(30, 236),
                Size = new Size(360, 56),
                Font = Estilo.Normal,
                ForeColor = Estilo.Error
            };

            var btnCambiar = Estilo.Boton("Cambiar contraseña", 30, 296, 360, 42, true);
            var btnCancelar = Estilo.Boton("Cancelar", 30, 348, 360, 36, false);

            btnCambiar.Click += (s, e) => Cambiar();
            btnCancelar.Click += (s, e) => DialogResult = DialogResult.Cancel;

            Controls.AddRange(new Control[]
            {
                Estilo.Etiqueta("Contraseña actual", 30, 32), txtActual,
                Estilo.Etiqueta("Nueva contraseña (mín. 8, letras y números)", 30, 102), txtNueva,
                Estilo.Etiqueta("Confirmar nueva contraseña", 30, 172), txtConfirmar,
                lblMensaje, btnCambiar, btnCancelar
            });

            AcceptButton = btnCambiar;   // Enter = cambiar
            CancelButton = btnCancelar;  // Esc = cancelar
        }

        private void Cambiar()
        {
            lblMensaje.Text = "";

            if (txtNueva.Text != txtConfirmar.Text)
            {
                lblMensaje.Text = "Las contraseñas nuevas no coinciden.";
                return;
            }

            string mensaje;
            bool ok;
            try
            {
                ok = negocio.CambiarMiClave(txtActual.Text, txtNueva.Text, out mensaje);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Contraseña", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!ok)
            {
                lblMensaje.Text = mensaje;
                return;
            }

            MessageBox.Show("Tu contraseña se actualizó.", "Listo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
        }
    }
}