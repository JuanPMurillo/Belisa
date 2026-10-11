using System.Collections.Generic;
using BelisaDatos;
using BelisaEntidad;

namespace BelisaNegocio
{
    public class ClienteN
    {
        private readonly ClienteD objD = new ClienteD();

        public List<Cliente> Listar()
        {
            Sesion.ExigirPermiso("menuclientes");
            return objD.Listar();
        }

        public bool Guardar(Cliente c, out string mensaje)
        {
            Sesion.ExigirPermiso("menuclientes");
            mensaje = "";

            c.Documento = (c.Documento ?? "").Trim();
            c.NombreCompleto = (c.NombreCompleto ?? "").Trim();
            c.Correo = (c.Correo ?? "").Trim();
            c.Telefono = (c.Telefono ?? "").Trim();

            if (c.NombreCompleto.Length == 0) { mensaje = "El nombre es obligatorio."; return false; }
            if (c.NombreCompleto.Length > 100) { mensaje = "El nombre no puede pasar de 100 caracteres."; return false; }
            if (!Validaciones.DatosContacto(c.Documento, c.Correo, c.Telefono, out mensaje)) return false;

            // El cumpleaños es opcional, pero el día y el mes van juntos y la fecha debe existir
            if (c.CumpleDia.HasValue != c.CumpleMes.HasValue)
            {
                mensaje = "Para guardar el cumpleaños elige el día y el mes.";
                return false;
            }
            if (!CumpleanosN.EsValido(c.CumpleDia, c.CumpleMes))
            {
                mensaje = "La fecha de cumpleaños no es válida (por ejemplo, el 31 de febrero no existe).";
                return false;
            }

            int id = objD.Guardar(c, out mensaje);
            if (id > 0) c.IdCliente = id;
            return id > 0;
        }
    }
}