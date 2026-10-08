using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using BelisaDatos;

namespace BelisaNegocio
{
    public class ArchivoRespaldo
    {
        public string Nombre { get; set; }
        public DateTime Fecha { get; set; }
        public double TamanoMb { get; set; }
    }

    public class RespaldoN
    {
        public const string CarpetaPorDefecto = @"C:\RespaldosBelisa";

        private const string Prefijo = "Belisa_";
        private const int Conservar = 10;   // cuántos respaldos se guardan; los más viejos se borran

        private readonly RespaldoD objD = new RespaldoD();

        // ---------- Preferencias (en %AppData%\Belisa\respaldo.cfg, no en la base de datos) ----------

        private class Config
        {
            public string Carpeta = CarpetaPorDefecto;
            public DateTime UltimoAutomatico = DateTime.MinValue;
        }

        private static string RutaConfig
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Belisa", "respaldo.cfg");
            }
        }

        private static Config LeerConfig()
        {
            var c = new Config();
            try
            {
                if (!File.Exists(RutaConfig)) return c;

                foreach (string linea in File.ReadAllLines(RutaConfig))
                {
                    int i = linea.IndexOf('=');
                    if (i <= 0) continue;

                    string clave = linea.Substring(0, i).Trim();
                    string valor = linea.Substring(i + 1).Trim();

                    if (clave == "Carpeta" && valor.Length > 0)
                    {
                        c.Carpeta = valor;
                    }
                    else if (clave == "UltimoAutomatico")
                    {
                        DateTime fecha;
                        if (DateTime.TryParseExact(valor, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                                   DateTimeStyles.None, out fecha))
                            c.UltimoAutomatico = fecha;
                    }
                }
            }
            catch (IOException) { }                    // archivo ilegible: se usan los valores de fábrica
            catch (UnauthorizedAccessException) { }
            return c;
        }

        private static void GuardarConfig(Config c)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(RutaConfig));
            File.WriteAllLines(RutaConfig, new[]
            {
                "Carpeta=" + c.Carpeta.Replace("\r", " ").Replace("\n", " "),
                "UltimoAutomatico=" + c.UltimoAutomatico.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            });
        }

        // ---------- Uso desde la pantalla de Respaldos (solo con permiso) ----------

        public string Carpeta
        {
            get { return LeerConfig().Carpeta; }
        }

        public void CambiarCarpeta(string carpeta)
        {
            Sesion.ExigirPermiso("menurespaldo");

            carpeta = (carpeta ?? "").Trim();
            if (carpeta.Length == 0 || !Path.IsPathRooted(carpeta))
                throw new ArgumentException("Elige una carpeta completa, por ejemplo C:\\RespaldosBelisa.");

            var c = LeerConfig();
            c.Carpeta = carpeta;
            GuardarConfig(c);
        }

        public bool Respaldar(out string archivo, out string mensaje)
        {
            Sesion.ExigirPermiso("menurespaldo");
            return Ejecutar(LeerConfig().Carpeta, out archivo, out mensaje);
        }

        public List<ArchivoRespaldo> Listar()
        {
            Sesion.ExigirPermiso("menurespaldo");

            string carpeta = LeerConfig().Carpeta;
            if (!Directory.Exists(carpeta)) return new List<ArchivoRespaldo>();

            return new DirectoryInfo(carpeta).GetFiles(Prefijo + "*.bak")
                .OrderByDescending(f => f.LastWriteTime)
                .Select(f => new ArchivoRespaldo
                {
                    Nombre = f.Name,
                    Fecha = f.LastWriteTime,
                    TamanoMb = f.Length / 1048576.0
                })
                .ToList();
        }

        // ---------- Respaldo diario (lo dispara el programa, sin sesión ni permisos) ----------

        // Devuelve true si no tocaba respaldar o si salió bien; false (con el motivo) si falló
        public bool RespaldoAutomaticoSiToca(out string mensaje)
        {
            mensaje = "";

            var cfg = LeerConfig();
            if (cfg.UltimoAutomatico.Date == DateTime.Today) return true;   // ya se hizo hoy

            string archivo, motivo;
            if (!Ejecutar(cfg.Carpeta, out archivo, out motivo))
            {
                mensaje = "No se pudo hacer el respaldo automático de hoy.\n\n" + motivo
                        + "\n\nAvisa a la administradora.";
                return false;
            }

            cfg.UltimoAutomatico = DateTime.Today;
            try
            {
                GuardarConfig(cfg);
            }
            catch (IOException) { }                    // si no se puede anotar, mañana se repite: no es grave
            catch (UnauthorizedAccessException) { }

            return true;
        }

        // ---------- Comunes ----------

        private bool Ejecutar(string carpeta, out string archivo, out string mensaje)
        {
            archivo = "";
            mensaje = "";

            try
            {
                if (string.IsNullOrWhiteSpace(carpeta) || !Path.IsPathRooted(carpeta))
                {
                    mensaje = "La carpeta de respaldos no es válida. Elige una, por ejemplo C:\\RespaldosBelisa.";
                    return false;
                }

                Directory.CreateDirectory(carpeta);

                string nombre = Prefijo + DateTime.Now.ToString("yyyy-MM-dd_HHmmss", CultureInfo.InvariantCulture) + ".bak";
                string ruta = Path.Combine(carpeta, nombre);

                if (ruta.Length > 250)
                {
                    mensaje = "La ruta de la carpeta es demasiado larga. Elige una más corta.";
                    return false;
                }

                objD.Respaldar(ruta);
                archivo = ruta;
            }
            catch (InvalidOperationException ex)
            {
                mensaje = ex.Message;
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                mensaje = "No tienes permiso para crear la carpeta de respaldos. Elige otra.";
                return false;
            }
            catch (Exception ex)
            {
                mensaje = "No se pudo respaldar: " + ex.Message;
                return false;
            }

            LimpiarViejos(carpeta);
            return true;
        }

        // Solo toca archivos con el nombre de nuestros respaldos; nunca otros que haya en la carpeta
        private static void LimpiarViejos(string carpeta)
        {
            try
            {
                var viejos = new DirectoryInfo(carpeta).GetFiles(Prefijo + "*.bak")
                    .OrderByDescending(f => f.LastWriteTime)
                    .Skip(Conservar);

                foreach (var archivo in viejos) archivo.Delete();
            }
            catch (IOException) { }                    // si uno no se puede borrar, se intenta en el próximo respaldo
            catch (UnauthorizedAccessException) { }
        }
    }
}