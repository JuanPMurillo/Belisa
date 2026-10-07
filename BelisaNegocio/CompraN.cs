using System;
using System.Collections.Generic;
using System.Linq;
using BelisaDatos;
using BelisaEntidad;

namespace BelisaNegocio
{
    public class CompraN
    {
        private const decimal ImporteMaximo = 99999999.99m;
        private const int CantidadMaxima = 1000000;

        private readonly CompraD objD = new CompraD();
        private readonly ProveedorD proveedorD = new ProveedorD();
        private readonly ProductoD productoD = new ProductoD();

        public List<Proveedor> ListarProveedoresActivos()
        {
            Sesion.ExigirPermiso("menucompras");
            return proveedorD.Listar().Where(p => p.Estado).ToList();
        }

        public List<Producto> ListarProductosActivos()
        {
            Sesion.ExigirPermiso("menucompras");
            return productoD.Listar().Where(p => p.Estado).ToList();
        }

        public bool Registrar(Compra c, out string mensaje)
        {
            Sesion.ExigirPermiso("menucompras");
            mensaje = "";
            c.NumeroDocumento = "";

            if (c.IdProveedor <= 0) { mensaje = "Selecciona un proveedor."; return false; }

            c.TipoDocumento = (c.TipoDocumento ?? "").Trim();
            if (c.TipoDocumento.Length == 0 || c.TipoDocumento.Length > 50)
            {
                mensaje = "Selecciona el tipo de documento.";
                return false;
            }

            if (c.Detalle == null || c.Detalle.Count == 0)
            {
                mensaje = "Agrega al menos un producto.";
                return false;
            }

            if (c.Detalle.GroupBy(d => d.IdProducto).Any(g => g.Count() > 1))
            {
                mensaje = "Hay productos repetidos en la compra.";
                return false;
            }

            decimal total = 0;
            foreach (var d in c.Detalle)
            {
                d.PrecioCompra = Math.Round(d.PrecioCompra, 2);
                d.PrecioVenta = Math.Round(d.PrecioVenta, 2);

                if (d.Cantidad <= 0 || d.Cantidad > CantidadMaxima)
                { mensaje = "Hay cantidades no válidas."; return false; }
                if (d.PrecioCompra < 0 || d.PrecioCompra > ImporteMaximo)
                { mensaje = "Hay precios de compra no válidos."; return false; }
                if (d.PrecioVenta <= 0 || d.PrecioVenta > ImporteMaximo)
                { mensaje = "El precio de venta debe ser mayor a 0."; return false; }

                d.MontoTotal = d.Cantidad * d.PrecioCompra;
                total += d.MontoTotal;
            }

            if (total > ImporteMaximo)
            {
                mensaje = "El total de la compra es demasiado grande.";
                return false;
            }

            // El usuario sale de la sesión, nunca de la pantalla
            c.IdUsuario = Sesion.UsuarioActual.IdUsuario;
            c.MontoTotal = total;

            string numero;
            int id = objD.Registrar(c, out numero, out mensaje);
            if (id > 0)
            {
                c.IdCompra = id;
                c.NumeroDocumento = numero;
            }
            return id > 0;
        }

        public List<Compra> Listar(DateTime desde, DateTime hasta)
        {
            Sesion.ExigirPermiso("menucompras");
            if (desde.Date > hasta.Date)
            {
                DateTime temporal = desde;
                desde = hasta;
                hasta = temporal;
            }
            return objD.Listar(desde, hasta);
        }

        public List<DetalleCompra> ObtenerDetalle(int idCompra)
        {
            Sesion.ExigirPermiso("menucompras");
            return objD.ObtenerDetalle(idCompra);
        }
    }
}