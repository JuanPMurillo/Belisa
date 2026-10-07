using System;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace BelisaNegocio
{
    internal static class Validaciones
    {
        // Documento, correo y teléfono son opcionales, pero si se escriben deben ser válidos
        public static bool DatosContacto(string documento, string correo, string telefono, out string mensaje)
        {
            mensaje = "";

            if (documento.Length > 50)
            {
                mensaje = "El documento no puede pasar de 50 caracteres.";
                return false;
            }

            if (correo.Length > 0 && (correo.Length > 100 || !CorreoValido(correo)))
            {
                mensaje = "El correo no es válido.";
                return false;
            }

            if (telefono.Length > 0 && !Regex.IsMatch(telefono, @"^[0-9+\-() ]{7,20}$"))
            {
                mensaje = "El teléfono no es válido (usa solo números, espacios, + - y paréntesis).";
                return false;
            }

            return true;
        }

        private static bool CorreoValido(string correo)
        {
            try
            {
                var m = new MailAddress(correo);
                int arroba = correo.IndexOf('@');
                return m.Address == correo && correo.IndexOf('.', arroba) > arroba;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}