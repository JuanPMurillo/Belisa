using System;
using System.Windows.Forms;
using BelisaNegocio;

namespace Belisa
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                if (!new UsuarioN().ExisteUsuarios())
                {
                    using (var f = new FrmPrimerAdmin())
                    {
                        if (f.ShowDialog() != DialogResult.OK) return;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo conectar con la base de datos.\n\n" + ex.Message,
                    "Error de conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Application.Run(new Login());
        }
    }
}