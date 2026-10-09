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

            bool hayUsuarios;
            if (!ConectarConLaBase(out hayUsuarios)) return;

            // Primer arranque del sistema: todavía no hay ningún usuario
            if (!hayUsuarios)
            {
                using (var f = new FrmPrimerAdmin())
                {
                    if (f.ShowDialog() != DialogResult.OK) return;
                }
            }

            Application.Run(new Login());
        }

        // Consulta la base; si no se puede, muestra "Configurar conexión" hasta que funcione o se cancele
        private static bool ConectarConLaBase(out bool hayUsuarios)
        {
            hayUsuarios = false;
            var conexion = new ConexionN();

            while (true)
            {
                string aviso;

                if (conexion.Configurada)
                {
                    try
                    {
                        hayUsuarios = new UsuarioN().ExisteUsuarios();
                        return true;
                    }
                    catch (Exception ex)   // cualquier fallo de la base lleva a la pantalla de conexión
                    {
                        aviso = "No se pudo usar la base de datos.\n\n" + ex.Message;
                    }
                }
                else
                {
                    aviso = "Antes de empezar, indica con qué base de datos va a trabajar el programa.";
                }

                using (var f = new FrmConexion(aviso))
                {
                    if (f.ShowDialog() != DialogResult.OK) return false;
                }
            }
        }
    }
}