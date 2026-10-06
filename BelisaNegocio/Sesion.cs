using System;
using System.Collections.Generic;
using BelisaEntidad;

namespace BelisaNegocio
{
    public static class Sesion
    {
        private static HashSet<string> permisos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public static Usuario UsuarioActual { get; private set; }

        public static void Iniciar(Usuario usuario, IEnumerable<string> permisosUsuario)
        {
            UsuarioActual = usuario;
            permisos = new HashSet<string>(permisosUsuario, StringComparer.OrdinalIgnoreCase);
        }

        public static void Cerrar()
        {
            UsuarioActual = null;
            permisos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public static bool TienePermiso(string menu)
        {
            return UsuarioActual != null && permisos.Contains(menu);
        }

        public static void ExigirPermiso(string menu)
        {
            if (!TienePermiso(menu))
                throw new UnauthorizedAccessException("No tienes permiso para realizar esta acción.");
        }
    }
}