using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using BelisaEntidad;
using BelisaNegocio;

namespace Belisa
{
    [DesignerCategory("Code")]
    public class FrmProveedor : FrmContactoBase
    {
        private readonly ProveedorN negocio = new ProveedorN();

        public FrmProveedor() : base("proveedor", "Razón social") { }

        protected override List<Contacto> ListarContactos()
        {
            return negocio.Listar().Select(p => new Contacto
            {
                Id = p.IdProveedor,
                Documento = p.Documento,
                Nombre = p.RazonSocial,
                Correo = p.Correo,
                Telefono = p.Telefono,
                Estado = p.Estado
            }).ToList();
        }

        protected override bool GuardarContacto(Contacto c, out string mensaje)
        {
            var proveedor = new Proveedor
            {
                IdProveedor = c.Id,
                Documento = c.Documento,
                RazonSocial = c.Nombre,
                Correo = c.Correo,
                Telefono = c.Telefono,
                Estado = c.Estado
            };

            bool ok = negocio.Guardar(proveedor, out mensaje);
            if (ok) c.Id = proveedor.IdProveedor;
            return ok;
        }
    }
}