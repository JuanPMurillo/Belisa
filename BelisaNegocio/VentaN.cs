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
        public const string MetodoMixto = "Mixto";   // lo que queda anotado en la venta cuando se pagó con dos métodos

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

        // La venta trae sus formas de pago en v.Pagos (una o dos) y, si alguna es efectivo,
        // en v.MontoPago lo que el cliente entrega en efectivo
        public bool Registrar(Venta v, out string mensaje)
        {
            Sesion.ExigirPermiso("menuventas");
            mensaje = "";
            v.NumeroDocumento = "";

            if (v.Detalle == null || v.Detalle.Count == 0)
            { mensaje = "Agrega al menos un producto."; return false; }

            if (v.Detalle.GroupBy(d => d.IdProducto).Any(g => g.Count() > 1))
            { mensaje = "Hay productos repetidos en la venta."; return false; }

            if (v.DescuentoPorcentaje < 0 || v.DescuentoPorcentaje > 100)
            { mensaje = "El descuento debe estar entre 0 y 100%."; return false; }
            v.DescuentoPorcentaje = Math.Round(v.DescuentoPorcentaje, 2);

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

            // ----- Formas de pago -----
            if (v.Pagos == null || v.Pagos.Count < 1 || v.Pagos.Count > 2)
            { mensaje = "La venta debe tener una o dos formas de pago."; return false; }

            foreach (var forma in v.Pagos)
            {
                if (!MetodosPago.Contains(forma.MetodoPago))
                { mensaje = "Selecciona el método de pago."; return false; }

                forma.Monto = Math.Round(forma.Monto, 2);
                forma.ReferenciaPago = (forma.ReferenciaPago ?? "").Trim();
                if (forma.ReferenciaPago.Length > 50)
                { mensaje = "La referencia no puede pasar de 50 caracteres."; return false; }
            }

            if (v.Pagos.Count == 1)
            {
                v.Pagos[0].Monto = total;   // un solo método cubre todo
            }
            else
            {
                if (v.Pagos[0].MetodoPago == v.Pagos[1].MetodoPago)
                { mensaje = "En un pago mixto los dos métodos deben ser distintos."; return false; }

                if (v.Pagos.Any(forma => forma.Monto <= 0))
                { mensaje = "En un pago mixto cada método debe cubrir una cantidad mayor a 0."; return false; }

                decimal suma = v.Pagos.Sum(forma => forma.Monto);
                if (suma != total)
                {
                    mensaje = "Los pagos suman " + suma.ToString("C2") + " y el total es " + total.ToString("C2") + ".";
                    return false;
                }
            }

            // El efectivo que entrega el cliente solo importa si alguna parte se paga en efectivo
            bool hayEfectivo = v.Pagos.Any(forma => forma.MetodoPago == "Efectivo");
            if (hayEfectivo)
            {
                decimal efectivo = v.Pagos.Where(forma => forma.MetodoPago == "Efectivo").Sum(forma => forma.Monto);
                v.MontoPago = Math.Round(v.MontoPago, 2);

                if (v.MontoPago < efectivo)
                { mensaje = "El efectivo recibido es menor a lo que se cobra en efectivo."; return false; }
                if (v.MontoPago > ImporteMaximo)
                { mensaje = "El efectivo recibido es demasiado grande."; return false; }
            }
            else
            {
                v.MontoPago = 0;
            }

            v.MetodoPago = v.Pagos.Count == 1 ? v.Pagos[0].MetodoPago : MetodoMixto;

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

        // Las formas de pago de una venta (una, o dos si fue mixta)
        public List<PagoVenta> ObtenerPagos(int idVenta)
        {
            Sesion.ExigirPermiso("menuventas");
            return objD.ObtenerPagos(idVenta, FiltroUsuario());
        }
    }
}