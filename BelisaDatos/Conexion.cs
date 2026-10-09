using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security;
using Microsoft.Win32;

namespace BelisaDatos
{
    public class DatosConexion
    {
        public string Servidor { get; set; }
        public string BaseDatos { get; set; }
    }

    // La conexión vive en %AppData%\Belisa\conexion.cfg, fuera de la carpeta del programa:
    // reinstalar o actualizar el programa no la borra. Solo guarda servidor y nombre de la base.
    public static class Conexion
    {
        public const string ServidorPorDefecto = @".\SQLEXPRESS";
        public const string BaseDatosPorDefecto = "DBBELISA";

        private static string cadena;   // se arma una vez; Guardar() la reinicia

        private static string Ruta
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Belisa", "conexion.cfg");
            }
        }

        // Todas las clases de datos usan esta propiedad
        public static string Cadena
        {
            get
            {
                if (cadena == null)
                {
                    var datos = Leer();
                    if (datos == null)
                        throw new InvalidOperationException("La conexión a la base de datos todavía no está configurada.");

                    cadena = Construir(datos.Servidor, datos.BaseDatos, 15);
                }
                return cadena;
            }
        }

        // Devuelve null si el archivo no existe o está incompleto
        public static DatosConexion Leer()
        {
            try
            {
                if (!File.Exists(Ruta)) return null;

                string servidor = null, baseDatos = null;
                foreach (string linea in File.ReadAllLines(Ruta))
                {
                    int i = linea.IndexOf('=');
                    if (i <= 0) continue;

                    string clave = linea.Substring(0, i).Trim();
                    string valor = linea.Substring(i + 1).Trim();

                    if (clave == "Servidor") servidor = valor;
                    else if (clave == "BaseDatos") baseDatos = valor;
                }

                if (string.IsNullOrEmpty(servidor) || string.IsNullOrEmpty(baseDatos)) return null;
                return new DatosConexion { Servidor = servidor, BaseDatos = baseDatos };
            }
            catch (IOException) { return null; }                   // ilegible: se pide configurar de nuevo
            catch (UnauthorizedAccessException) { return null; }
        }

        public static void Guardar(string servidor, string baseDatos)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Ruta));
            File.WriteAllLines(Ruta, new[] { "Servidor=" + servidor, "BaseDatos=" + baseDatos });
            cadena = null;   // la próxima consulta ya usa los datos nuevos
        }

        // El constructor de cadenas se encarga de escapar los valores: nadie puede colar opciones extra
        private static string Construir(string servidor, string baseDatos, int segundosDeEspera)
        {
            var b = new SqlConnectionStringBuilder
            {
                DataSource = servidor,
                InitialCatalog = baseDatos,
                IntegratedSecurity = true,
                ConnectTimeout = segundosDeEspera,
                ApplicationName = "Belisa",
                TrustServerCertificate = true
            };
            return b.ConnectionString;
        }

        // Prueba la conexión (5 segundos de espera) y revisa que la base tenga lo que el programa necesita
        public static bool Probar(string servidor, string baseDatos, out string mensaje)
        {
            mensaje = "";
            try
            {
                using (var cn = new SqlConnection(Construir(servidor, baseDatos, 5)))
                {
                    cn.Open();

                    using (var cmd = new SqlCommand(
                        "SELECT CASE WHEN OBJECT_ID(N'dbo.USUARIO') IS NOT NULL " +
                        "AND OBJECT_ID(N'dbo.SP_OBTENER_LOGIN') IS NOT NULL THEN 1 ELSE 0 END", cn))
                    {
                        if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                        {
                            mensaje = "Se conectó y la base existe, pero está incompleta: faltan tablas o procedimientos. "
                                    + "Ejecuta el script de creación completo de la base de datos.";
                            return false;
                        }
                    }
                }
                return true;
            }
            catch (SqlException ex)
            {
                mensaje = Traducir(ex, servidor, baseDatos);
                return false;
            }
        }

        private static string Traducir(SqlException ex, string servidor, string baseDatos)
        {
            foreach (SqlError error in ex.Errors)
            {
                if (error.Number == 4060)
                    return "El servidor responde, pero no existe la base de datos «" + baseDatos + "». "
                         + "Ejecuta el script de creación de la base o revisa el nombre.";

                if (error.Number == 18456)
                    return "SQL Server rechazó tu usuario de Windows: no tiene acceso. "
                         + "Instala SQL Server con la misma cuenta de Windows con la que se usa el programa, "
                         + "o agrega ese usuario en SQL Server.";
            }

            return "No se pudo conectar con «" + servidor + "». Revisa el nombre del servidor y que el servicio "
                 + "de SQL Server esté en ejecución.\n\nDetalle: " + ex.Message;
        }

        // Instancias de SQL Server instaladas en esta computadora, listas para poner en el cuadro de servidor
        public static List<string> InstanciasLocales()
        {
            var lista = new List<string>();
            try
            {
                // El programa puede correr en 32 bits sobre un Windows de 64: se pide la vista correcta del registro
                var vista = Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32;

                using (var raiz = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, vista))
                using (var llave = raiz.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL"))
                {
                    if (llave == null) return lista;

                    foreach (string nombre in llave.GetValueNames())
                    {
                        // La instancia predeterminada se llama MSSQLSERVER y se escribe solo con un punto
                        lista.Add(string.Equals(nombre, "MSSQLSERVER", StringComparison.OrdinalIgnoreCase)
                                  ? "." : @".\" + nombre);
                    }
                }
            }
            catch (SecurityException) { }                   // sin permiso para leer el registro:
            catch (UnauthorizedAccessException) { }         // la persona escribe el servidor a mano

            return lista.OrderBy(x => x).ToList();
        }
    }
}