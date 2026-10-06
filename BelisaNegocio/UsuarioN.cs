using System;
using System.Collections.Generic;
using System.Linq;
using BelisaDatos;
using BelisaEntidad;

namespace BelisaNegocio
{
    public class UsuarioN
    {
        private readonly UsuarioD objD = new UsuarioD();

        public const int LongitudMinimaClave = 8;
        private const string MensajeGenerico = "Usuario o contraseña incorrectos.";

        public Usuario Login(string nameUser, string clave, out string mensaje)
        {
            mensaje = "";

            if (string.IsNullOrWhiteSpace(nameUser) || string.IsNullOrEmpty(clave))
            {
                mensaje = "Ingresa usuario y contraseña.";
                return null;
            }

            CredencialUsuario cred = objD.ObtenerCredencial(nameUser.Trim());

            // Usuario inexistente: mismo mensaje y mismo tiempo que una clave incorrecta
            if (cred == null)
            {
                SeguridadClave.Verificar(clave, SeguridadClave.HashFalso);
                mensaje = MensajeGenerico;
                return null;
            }

            if (cred.Bloqueado)
            {
                mensaje = "Cuenta bloqueada temporalmente por demasiados intentos. Intenta de nuevo en unos minutos.";
                return null;
            }

            if (!SeguridadClave.Verificar(clave, cred.ClaveHash))
            {
                objD.RegistrarIntentoFallido(cred.Usuario.IdUsuario);
                mensaje = MensajeGenerico;
                return null;
            }

            // Solo quien conoce la clave correcta se entera de que la cuenta está desactivada
            if (!cred.Usuario.Estado)
            {
                mensaje = "Esta cuenta está desactivada. Contacta al administrador.";
                return null;
            }

            objD.RegistrarLoginExitoso(cred.Usuario.IdUsuario);
            return cred.Usuario;
        }

        public bool ExisteUsuarios() => objD.ExisteUsuarios();

        public List<Rol> ListarRoles() => objD.ListarRoles();

        public List<string> ListarPermisos(int idRol) => objD.ListarPermisos(idRol);

        // Solo funciona si la tabla USUARIO está vacía (primer arranque del sistema)
        public bool CrearPrimerAdmin(string nameUser, string nombreCompleto, string correo,
                                     string clave, out string mensaje)
        {
            if (objD.ExisteUsuarios())
            {
                mensaje = "Ya existe un usuario registrado.";
                return false;
            }

            Rol admin = objD.ListarRoles().FirstOrDefault(r => r.Descripcion == "ADMINISTRADOR");
            if (admin == null)
            {
                mensaje = "No se encontró el rol ADMINISTRADOR en la base de datos.";
                return false;
            }

            var u = new Usuario
            {
                NameUser = nameUser,
                NombreCompleto = nombreCompleto,
                Correo = correo,
                IdRol = admin.IdRol
            };
            return CrearUsuario(u, clave, out mensaje);
        }

        public bool CrearUsuario(Usuario u, string clave, out string mensaje)
        {
            mensaje = "";

            if (string.IsNullOrWhiteSpace(u.NameUser) || string.IsNullOrWhiteSpace(u.NombreCompleto))
            {
                mensaje = "El usuario y el nombre completo son obligatorios.";
                return false;
            }

            if (!ClaveEsValida(clave, out mensaje))
                return false;

            u.NameUser = u.NameUser.Trim();
            u.NombreCompleto = u.NombreCompleto.Trim();

            int id = objD.CrearUsuario(u, SeguridadClave.Hashear(clave), out mensaje);
            return id > 0;
        }

        public static bool ClaveEsValida(string clave, out string mensaje)
        {
            mensaje = "";

            if (string.IsNullOrEmpty(clave) || clave.Length < LongitudMinimaClave)
            {
                mensaje = "La contraseña debe tener al menos " + LongitudMinimaClave + " caracteres.";
                return false;
            }
            if (!clave.Any(char.IsLetter) || !clave.Any(char.IsDigit))
            {
                mensaje = "La contraseña debe incluir letras y números.";
                return false;
            }
            return true;
        }
    }
}