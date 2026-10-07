using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BelisaDatos;
using BelisaEntidad;

namespace BelisaNegocio
{
    public class UsuarioN
    {
        private readonly UsuarioD objD = new UsuarioD();

        public const int LongitudMinimaClave = 8;
        private const string MensajeGenerico = "Usuario o contraseña incorrectos.";

        // ---------- Inicio de sesión ----------

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

        // Se usa al iniciar sesión, antes de que exista la sesión
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
            return CrearInterno(u, clave, out mensaje);
        }

        // ---------- Mi contraseña (cualquier usuario con sesión) ----------

        public bool CambiarMiClave(string actual, string nueva, out string mensaje)
        {
            mensaje = "";

            if (Sesion.UsuarioActual == null)
            {
                mensaje = "No hay una sesión iniciada.";
                return false;
            }
            if (string.IsNullOrEmpty(actual))
            {
                mensaje = "Escribe tu contraseña actual.";
                return false;
            }
            if (!ClaveEsValida(nueva, out mensaje)) return false;
            if (nueva == actual)
            {
                mensaje = "La nueva contraseña debe ser distinta a la actual.";
                return false;
            }

            int id = Sesion.UsuarioActual.IdUsuario;
            CredencialUsuario cred = objD.ObtenerCredencial(Sesion.UsuarioActual.NameUser);

            if (cred == null || cred.Usuario.IdUsuario != id || !SeguridadClave.Verificar(actual, cred.ClaveHash))
            {
                // Cuenta como intento fallido: así nadie puede adivinar la clave desde una sesión abierta
                objD.RegistrarIntentoFallido(id);
                mensaje = "La contraseña actual no es correcta.";
                return false;
            }

            if (!objD.CambiarClave(id, SeguridadClave.Hashear(nueva)))
            {
                mensaje = "No se pudo actualizar la contraseña.";
                return false;
            }
            return true;
        }

        // ---------- Administración de usuarios (solo con permiso) ----------

        public List<Usuario> Listar()
        {
            Sesion.ExigirPermiso("menuusuarios");
            return objD.Listar();
        }

        public List<Rol> ListarRoles()
        {
            Sesion.ExigirPermiso("menuusuarios");
            return objD.ListarRoles();
        }

        public bool Crear(Usuario u, string clave, out string mensaje)
        {
            Sesion.ExigirPermiso("menuusuarios");
            return CrearInterno(u, clave, out mensaje);
        }

        public bool Editar(Usuario u, out string mensaje)
        {
            Sesion.ExigirPermiso("menuusuarios");
            mensaje = "";

            if (u.IdUsuario <= 0)
            {
                mensaje = "Selecciona un usuario.";
                return false;
            }
            if (!ValidarDatos(u, out mensaje)) return false;

            // Nadie puede quitarse el acceso a sí mismo
            if (u.IdUsuario == Sesion.UsuarioActual.IdUsuario
                && (!u.Estado || u.IdRol != Sesion.UsuarioActual.IdRol))
            {
                mensaje = "No puedes desactivarte ni cambiar tu propio rol.";
                return false;
            }

            return objD.Editar(u, out mensaje);
        }

        public bool RestablecerClave(int idUsuario, string clave, out string mensaje)
        {
            Sesion.ExigirPermiso("menuusuarios");
            mensaje = "";

            if (idUsuario <= 0)
            {
                mensaje = "Selecciona un usuario.";
                return false;
            }
            if (idUsuario == Sesion.UsuarioActual.IdUsuario)
            {
                mensaje = "Para cambiar tu propia contraseña usa el botón \"Mi contraseña\".";
                return false;
            }
            if (!ClaveEsValida(clave, out mensaje)) return false;

            if (!objD.CambiarClave(idUsuario, SeguridadClave.Hashear(clave)))
            {
                mensaje = "El usuario no existe.";
                return false;
            }
            return true;
        }

        public void Desbloquear(int idUsuario)
        {
            Sesion.ExigirPermiso("menuusuarios");
            objD.Desbloquear(idUsuario);
        }

        // ---------- Reglas comunes ----------

        private bool CrearInterno(Usuario u, string clave, out string mensaje)
        {
            if (!ValidarDatos(u, out mensaje)) return false;
            if (!ClaveEsValida(clave, out mensaje)) return false;

            int id = objD.CrearUsuario(u, SeguridadClave.Hashear(clave), out mensaje);
            if (id > 0) u.IdUsuario = id;
            return id > 0;
        }

        private static bool ValidarDatos(Usuario u, out string mensaje)
        {
            mensaje = "";

            u.NameUser = (u.NameUser ?? "").Trim();
            u.NombreCompleto = (u.NombreCompleto ?? "").Trim();
            u.Correo = (u.Correo ?? "").Trim();

            if (!Regex.IsMatch(u.NameUser, @"^[A-Za-z0-9._-]{3,50}$"))
            {
                mensaje = "El usuario debe tener de 3 a 50 caracteres: letras, números, punto, guion o guion bajo (sin espacios).";
                return false;
            }
            if (u.NombreCompleto.Length == 0 || u.NombreCompleto.Length > 100)
            {
                mensaje = "El nombre completo es obligatorio (máximo 100 caracteres).";
                return false;
            }
            if (u.IdRol <= 0)
            {
                mensaje = "Selecciona un rol.";
                return false;
            }

            return Validaciones.DatosContacto("", u.Correo, "", out mensaje);
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