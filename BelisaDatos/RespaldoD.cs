using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace BelisaDatos
{
    public class RespaldoD
    {
        public void Respaldar(string rutaArchivo)
        {
            try
            {
                using (var cn = new SqlConnection(Conexion.Cadena))
                {
                    cn.Open();
                    string baseDatos = cn.Database.Replace("]", "]]");   // sale de la cadena de conexión, no de un usuario

                    using (var cmd = new SqlCommand(
                        "BACKUP DATABASE [" + baseDatos + "] TO DISK = @ruta WITH CHECKSUM, INIT", cn))
                    {
                        cmd.CommandTimeout = 300;
                        cmd.Parameters.Add("@ruta", SqlDbType.NVarChar, 260).Value = rutaArchivo;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException(Traducir(ex), ex);
            }

            // SQL Server escribe el archivo con su propia cuenta: confirmamos que existe y no está vacío
            var archivo = new FileInfo(rutaArchivo);
            if (!archivo.Exists || archivo.Length == 0)
                throw new InvalidOperationException("SQL Server terminó, pero no se encontró el archivo de respaldo.");
        }

        private static string Traducir(SqlException ex)
        {
            foreach (SqlError error in ex.Errors)
            {
                if (error.Number == 3201)
                    return "SQL Server no pudo escribir en la carpeta de respaldos. Revisa que exista y que SQL Server "
                         + "tenga permiso para guardar ahí (por ejemplo, usa C:\\RespaldosBelisa).";

                if (error.Number == 262 || error.Number == 15247)
                    return "Tu usuario de Windows no tiene permiso para respaldar la base de datos.";
            }
            return "No se pudo respaldar la base de datos: " + ex.Message;
        }
    }
}