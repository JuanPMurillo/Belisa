using System;

namespace BelisaDatos
{
    internal static class Ayuda
    {
        // Texto vacío => NULL en la base de datos
        public static object ANullable(string texto)
        {
            return string.IsNullOrWhiteSpace(texto) ? (object)DBNull.Value : texto;
        }

        // NULL de la base de datos => texto vacío
        public static string Texto(object valor)
        {
            return valor == DBNull.Value ? "" : Convert.ToString(valor);
        }
    }
}