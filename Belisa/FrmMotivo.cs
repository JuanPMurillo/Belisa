using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Belisa
{
    [DesignerCategory("Code")]
    public class FrmMotivo : Form
    {
        private readonly TextBox txtMotivo;
        private readonly Label lblError;

        public string Motivo { get; private set; }

        public FrmMotivo(string titulo, string resumen)
        {
            Estilo.ConfigurarVentana(this, 480, 340, titulo);
            BackColor = Color.White;

            var lblResumen = new Label
            {
                Location = new Point(24, 20),
                Size = new Size(430, 120),
                Font = Estilo.Normal,
                ForeColor = Estilo.Texto,
                Text = resumen
            };

            txtMotivo = Estilo.Campo(24, 174, 430);
            txtMotivo.MaxLength = 200;

            lblError = new Label
            {
                Location = new Point(24, 204),
                Size = new Size(430, 24),
                Font = Estilo.Normal,
                ForeColor = Estilo.Error
            };

            var btnAnular = Estilo.Boton("Anular", 24, 244, 205, 44, true);
            btnAnular.BackColor = Estilo.Error;
            btnAnular.FlatAppearance.MouseOverBackColor = Color.FromArgb(150, 40, 30);

            var btnCancelar = Estilo.Boton("Cancelar", 249, 244, 205, 44, false);

            btnAnular.Click += (s, e) => Confirmar();
            btnCancelar.Click += (s, e) => DialogResult = DialogResult.Cancel;

            Controls.AddRange(new Control[]
            {
                lblResumen,
                Estilo.Etiqueta("Motivo de la anulación (obligatorio)", 24, 150), txtMotivo,
                lblError, btnAnular, btnCancelar
            });

            AcceptButton = btnAnular;     // Enter = anular
            CancelButton = btnCancelar;   // Esc = cancelar
        }

        private void Confirmar()
        {
            string texto = txtMotivo.Text.Trim();
            if (texto.Length < 5)
            {
                lblError.Text = "Escribe el motivo (mínimo 5 letras).";
                txtMotivo.Focus();
                return;
            }

            Motivo = texto;
            DialogResult = DialogResult.OK;
        }
    }
}