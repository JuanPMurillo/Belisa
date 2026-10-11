using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using BelisaEntidad;
using BelisaNegocio;

namespace Belisa
{
    [DesignerCategory("Code")]
    public class FrmCliente : FrmContactoBase
    {
        private readonly ClienteN negocio = new ClienteN();

        // El tercer dato (true) activa el cumpleaños opcional: los proveedores no lo usan
        public FrmCliente() : base("cliente", "Nombre completo", true) { }

        protected override List<Contacto> ListarContactos()
        {
            return negocio.Listar().Select(c => new Contacto
            {
                Id = c.IdCliente,
                Documento = c.Documento,
                Nombre = c.NombreCompleto,
                Correo = c.Correo,
                Telefono = c.Telefono,
                Estado = c.Estado,
                CumpleDia = c.CumpleDia,
                CumpleMes = c.CumpleMes
            }).ToList();
        }

        protected override bool GuardarContacto(Contacto c, out string mensaje)
        {
            var cliente = new Cliente
            {
                IdCliente = c.Id,
                Documento = c.Documento,
                NombreCompleto = c.Nombre,
                Correo = c.Correo,
                Telefono = c.Telefono,
                Estado = c.Estado,
                CumpleDia = c.CumpleDia,
                CumpleMes = c.CumpleMes
            };

            bool ok = negocio.Guardar(cliente, out mensaje);
            if (ok) c.Id = cliente.IdCliente;
            return ok;
        }
    }
}