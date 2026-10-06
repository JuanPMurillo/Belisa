using System.Drawing;
using System.Windows.Forms;

namespace Belisa
{
    // Pantalla temporal: se reemplaza por la real en cada paso
    public class FrmPendiente : Form
    {
        public FrmPendiente(string titulo)
        {
            BackColor = Estilo.Fondo;
            Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 16f),
                ForeColor = Estilo.Texto,
                Text = titulo + "\n\n(Pantalla en construcción)"
            });
        }
    }
}