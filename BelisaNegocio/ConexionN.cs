using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using BelisaDatos;

namespace BelisaNegocio
{
    public class ConexionN
    {
        public const string ServidorPorDefecto = Conexion.ServidorPorDefecto;
        public const string BaseDatosPorDefecto = Conexion.BaseDatosPorDefecto;

        public bool Configurada
        {
            get { return Conexion.Leer() != null; }
        }

        public string ServidorActual
        {
            get { var d = Conexion.Leer(); return d == null ? "" : d.Servidor; }
        }

        public string BaseDatosActual
        {
            get { var d = Conexion.Leer(); return d == null ? "" : d.BaseDatos; }
        }

        public List<string> InstanciasLocales()
        {
            return Conexion.InstanciasLocales();
        }

        public bool Validar(string servidor, string baseDatos, out string mensaje)
        {
            mensaje = "";
            servidor = (servidor ?? "").Trim();
            baseDatos = (baseDatos ?? "").Trim();

            if (servidor.Length == 0)
            {
                mensaje = "Escribe o elige el servidor de SQL Server.";
                return false;
            }
            // Letras, números, punto, barra invertida, coma (puerto), dos puntos, paréntesis y guiones
            if (!Regex.IsMatch(servidor, @"^[A-Za-z0-9_.\\,():\-]{1,128}$"))
            {
                mensaje = "El nombre del servidor tiene caracteres no permitidos. Ejemplo válido: .\\SQLEXPRESS";
                return false;
            }
            if (!Regex.IsMatch(baseDatos, @"^[A-Za-z0-9_]{1,128}$"))
            {
                mensaje = "El nombre de la base de datos solo puede tener letras, números y guion bajo.";
                return false;
            }
            return true;
        }

        public bool Probar(string servidor, string baseDatos, out string mensaje)
        {
            if (!Validar(servidor, baseDatos, out mensaje)) return false;
            return Conexion.Probar(servidor.Trim(), baseDatos.Trim(), out mensaje);
        }

        public bool Guardar(string servidor, string baseDatos, out string mensaje)
        {
            if (!Validar(servidor, baseDatos, out mensaje)) return false;

            try
            {
                Conexion.Guardar(servidor.Trim(), baseDatos.Trim());
                return true;
            }
            catch (IOException)
            {
                mensaje = "No se pudo guardar el archivo de configuración. Revisa que el disco no esté lleno ni protegido.";
            }
            catch (System.UnauthorizedAccessException)
            {
                mensaje = "No tienes permiso para guardar la configuración en tu carpeta de usuario.";
            }
            return false;
        }
    }
}