using System.Collections.Generic;
using BelisaDatos;
using BelisaEntidad;

namespace BelisaNegocio
{
    public class ProveedorN
    {
        private readonly ProveedorD objD = new ProveedorD();

        public List<Proveedor> Listar()
        {
            Sesion.ExigirPermiso("menuproveedor");
            return objD.Listar();
        }

        public bool Guardar(Proveedor p, out string mensaje)
        {
            Sesion.ExigirPermiso("menuproveedor");
            mensaje = "";

            p.Documento = (p.Documento ?? "").Trim();
            p.RazonSocial = (p.RazonSocial ?? "").Trim();
            p.Correo = (p.Correo ?? "").Trim();
            p.Telefono = (p.Telefono ?? "").Trim();

            if (!Validaciones.DatosContacto(p.Documento, p.Correo, p.Telefono, out mensaje)) return false;

            int id = objD.Guardar(p, out mensaje);
            if (id > 0) p.IdProveedor = id;
            return id > 0;
        }
    }
}