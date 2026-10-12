using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Belisa
{
    // Pantalla temporal: se reemplaza por la real en cada paso
    [DesignerCategory("Code")]
    public class FrmPendiente : Form
    {
        public FrmPendiente(string titulo)
        {
            // 1. Evitar parpadeos y asegurar rendimiento visual fluido al redimensionar
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();

            // 2. Asegurar el escalado correcto de fuentes en distintas resoluciones
            AutoScaleMode = AutoScaleMode.Font;

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