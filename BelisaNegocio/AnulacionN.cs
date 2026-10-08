using BelisaDatos;

namespace BelisaNegocio
{
    public class AnulacionN
    {
        private readonly AnulacionD objD = new AnulacionD();

        public bool AnularVenta(int idVenta, string motivo, out string mensaje)
        {
            Sesion.ExigirPermiso("menuventas");
            Sesion.ExigirPermiso("anular");

            if (!ValidarMotivo(idVenta, ref motivo, out mensaje)) return false;
            return objD.AnularVenta(idVenta, Sesion.UsuarioActual.IdUsuario, motivo, out mensaje);
        }

        public bool AnularCompra(int idCompra, string motivo, out string mensaje)
        {
            Sesion.ExigirPermiso("menucompras");
            Sesion.ExigirPermiso("anular");

            if (!ValidarMotivo(idCompra, ref motivo, out mensaje)) return false;
            return objD.AnularCompra(idCompra, Sesion.UsuarioActual.IdUsuario, motivo, out mensaje);
        }

        private static bool ValidarMotivo(int id, ref string motivo, out string mensaje)
        {
            mensaje = "";
            motivo = (motivo ?? "").Trim();

            if (id <= 0)
            {
                mensaje = "Selecciona un registro.";
                return false;
            }
            if (motivo.Length < 5)
            {
                mensaje = "Escribe el motivo de la anulación (mínimo 5 letras).";
                return false;
            }
            if (motivo.Length > 200)
            {
                mensaje = "El motivo no puede pasar de 200 caracteres.";
                return false;
            }
            return true;
        }
    }
}