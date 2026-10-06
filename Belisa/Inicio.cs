using System;
using System.Drawing;
using System.Windows.Forms;
using BelisaEntidad;
using BelisaNegocio;

namespace Belisa
{
    public class Inicio : Form
    {
        public Inicio(Usuario usuario)
        {
            Text = "Belisa - Punto de venta";
            Icon = Estilo.IconoApp;
            WindowState = FormWindowState.Maximized;
            BackColor = Color.FromArgb(247, 247, 247);   // mismo gris del logo completo

            string permisos;
            try
            {
                permisos = string.Join(", ", new UsuarioN().ListarPermisos(usuario.IdRol));
            }
            catch (Exception ex)
            {
                permisos = "(error: " + ex.Message + ")";
            }

            var texto = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 130,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 13f),
                ForeColor = Estilo.Texto,
                Text = "Hola, " + usuario.NombreCompleto + " (" + usuario.RolDescripcion + ")\n" +
                       "Menús permitidos: " + permisos
            };

            var logo = new PictureBox
            {
                Dock = DockStyle.Fill,
                Image = Properties.Resources.BelisaLogoCompleto,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            };

            Controls.Add(logo);
            Controls.Add(texto);
            Estilo.AgregarFranja(this);
        }
    }
}
