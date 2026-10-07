using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BelisaNegocio;

namespace Belisa
{
    [DesignerCategory("Code")]
    public class FrmAcercaDe : Form
    {
        private readonly Panel tarjeta;

        public FrmAcercaDe()
        {
            BackColor = Estilo.Fondo;

            tarjeta = new Panel { Size = new Size(520, 470), BackColor = Color.White };

            tarjeta.Controls.Add(Estilo.Imagen(Properties.Resources.BelisaLogoCuadrado, 160, 20, 200, 200));

            tarjeta.Controls.Add(Centrado("Belisa · Punto de venta", 228, 32, new Font("Segoe UI", 16f, FontStyle.Bold), Estilo.Acento));
            tarjeta.Controls.Add(Centrado("Versión " + Application.ProductVersion, 264, 26, Estilo.Normal, Estilo.Texto));

            string sesion = Sesion.UsuarioActual == null
                ? ""
                : "Sesión: " + Sesion.UsuarioActual.NombreCompleto + " (" + Sesion.UsuarioActual.RolDescripcion + ")";
            tarjeta.Controls.Add(Centrado(sesion, 294, 26, Estilo.Normal, Estilo.Texto));

            tarjeta.Controls.Add(Centrado(
                "Hecho a la medida de Belisa, Centro de depilación.\nCera española.",
                340, 48, Estilo.Normal, Estilo.Texto));

            tarjeta.Controls.Add(Centrado(
                "Componentes de código abierto: ClosedXML, PDFsharp y MigraDoc (licencia MIT).\nPrograma Creado Por: Juan Pablo Murillo Vazquez",
                410, 40, new Font("Segoe UI", 9f), Color.Gray));

            Controls.Add(tarjeta);

            Resize += (s, e) => Centrar();
            Load += (s, e) => Centrar();
        }

        private static Label Centrado(string texto, int y, int alto, Font fuente, Color color)
        {
            return new Label
            {
                Text = texto,
                Location = new Point(20, y),
                Size = new Size(480, alto),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = fuente,
                ForeColor = color
            };
        }

        private void Centrar()
        {
            tarjeta.Location = new Point(
                Math.Max(0, (ClientSize.Width - tarjeta.Width) / 2),
                Math.Max(0, (ClientSize.Height - tarjeta.Height) / 2));
        }
    }
}