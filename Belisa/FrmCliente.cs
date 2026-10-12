using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BelisaEntidad;
using BelisaNegocio;

namespace Belisa
{
    [DesignerCategory("Code")]
    public class FrmCliente : FrmContactoBase
    {
        private readonly ClienteN negocio = new ClienteN();

        // El tercer dato (true) activa el cumpleaños opcional: los proveedores no lo usan
        public FrmCliente() : base("cliente", "Nombre completo", true)
        {
            // 1. Evitar parpadeos y asegurar rendimiento visual fluido al redimensionar
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();

            // 2. Asegurar el escalado correcto de fuentes en distintas resoluciones
            AutoScaleMode = AutoScaleMode.Font;
        }

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