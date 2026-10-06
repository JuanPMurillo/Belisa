using System.Configuration;

namespace BelisaDatos
{
    public static class Conexion
    {
        public static string Cadena
        {
            get
            {
                var cs = ConfigurationManager.ConnectionStrings["cadena_conexion"];
                if (cs == null)
                    throw new ConfigurationErrorsException(
                        "Falta la cadena de conexión 'cadena_conexion' en el App.config.");
                return cs.ConnectionString;
            }
        }
    }
}