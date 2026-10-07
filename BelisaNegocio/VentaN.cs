using System;
using System.Collections.Generic;
using System.Linq;
using BelisaDatos;
using BelisaEntidad;

namespace BelisaNegocio
{
    public class VentaN
    {
        public static readonly string[] MetodosPago = { "Efectivo", "Tarjeta", "Transferencia" };

        private const decimal ImporteMaximo = 99999999.99m;
        private const int CantidadMaxima = 100000;

        private readonly VentaD objD = new VentaD();
        private readonly ClienteD clienteD = new ClienteD();
        private readonly ProductoD productoD = new ProductoD();

        // Quien tiene el permiso de reportes ve las ventas de todos; los demás, solo las suyas
        public bool VeTodas
        {
            get { return Sesion.TienePermiso("menureportes"); }
        }

        private int? FiltroUsuario()
        {
            return VeTodas ? (int?)null : Sesion.UsuarioActual.IdUsuario;
        }

        public List<Cliente> ListarClientesActivos()
        {
            Sesion.ExigirPermiso("menuventas");
            return clienteD.Listar().Where(c => c.Estado).ToList();
        }

        public List<Producto> ListarProductosActivos()
        {
            Sesion.ExigirPermiso("menuventas");
            return productoD.Listar().Where(p => p.Estado).ToList();
        }

        public bool Registrar(Venta v, out string mensaje)
        {
            Sesion.ExigirPermiso("menuventas");
            mensaje = "";
            v.NumeroDocumento = "";

            if (v.Detalle == null || v.Detalle.Count == 0)
            { mensaje = "Agrega al menos un producto."; return false; }

            if (v.Detalle.GroupBy(d => d.IdProducto).Any(g => g.Count() > 1))
            { mensaje = "Hay productos repetidos en la venta."; return false; }

            if (!MetodosPago.Contains(v.MetodoPago))
            { mensaje = "Selecciona el método de pago."; return false; }

            if (v.DescuentoPorcentaje < 0 || v.DescuentoPorcentaje > 100)
            { mensaje = "El descuento debe estar entre 0 y 100%."; return false; }
            v.DescuentoPorcentaje = Math.Round(v.DescuentoPorcentaje, 2);

            v.ReferenciaPago = (v.ReferenciaPago ?? "").Trim();
            if (v.ReferenciaPago.Length > 50)
            { mensaje = "La referencia no puede pasar de 50 caracteres."; return false; }

            if (v.IdCliente.HasValue && v.IdCliente.Value <= 0) v.IdCliente = null;

            decimal subtotal = 0;
            foreach (var d in v.Detalle)
            {
                d.PrecioVenta = Math.Round(d.PrecioVenta, 2);

                if (d.Cantidad <= 0 || d.Cantidad > CantidadMaxima)
                { mensaje = "Hay cantidades no válidas."; return false; }
                if (d.PrecioVenta <= 0 || d.PrecioVenta > ImporteMaximo)
                { mensaje = "Hay precios no válidos."; return false; }

                d.SubTotal = d.Cantidad * d.PrecioVenta;
                subtotal += d.SubTotal;
            }

            if (subtotal > ImporteMaximo)
            { mensaje = "El total de la venta es demasiado grande."; return false; }

            decimal descuento = Math.Round(subtotal * v.DescuentoPorcentaje / 100m, 2, MidpointRounding.AwayFromZero);
            decimal total = subtotal - descuento;

            if (v.MetodoPago == "Efectivo")
            {
                v.MontoPago = Math.Round(v.MontoPago, 2);
                if (v.MontoPago < total) { mensaje = "El pago es menor al total."; return false; }
                if (v.MontoPago > ImporteMaximo) { mensaje = "El pago es demasiado grande."; return false; }
            }
            else
            {
                v.MontoPago = total;   // con tarjeta o transferencia se cobra exacto
            }

            // El usuario sale de la sesión, nunca de la pantalla
            v.IdUsuario = Sesion.UsuarioActual.IdUsuario;

            string numero;
            decimal montoTotal, montoCambio;
            int id = objD.Registrar(v, out numero, out montoTotal, out montoCambio, out mensaje);
            if (id > 0)
            {
                v.IdVenta = id;
                v.NumeroDocumento = numero;
                v.MontoTotal = montoTotal;      // lo que calculó el servidor
                v.MontoCambio = montoCambio;
            }
            return id > 0;
        }

        public List<Venta> Listar(DateTime desde, DateTime hasta)
        {
            Sesion.ExigirPermiso("menuventas");
            if (desde.Date > hasta.Date)
            {
                DateTime temporal = desde;
                desde = hasta;
                hasta = temporal;
            }
            return objD.Listar(desde, hasta, FiltroUsuario());
        }

        public List<DetalleVenta> ObtenerDetalle(int idVenta)
        {
            Sesion.ExigirPermiso("menuventas");
            return objD.ObtenerDetalle(idVenta, FiltroUsuario());
        }
    }
}