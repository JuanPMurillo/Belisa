using System;
using System.Collections.Generic;
using System.Linq;
using BelisaDatos;
using BelisaEntidad;

namespace BelisaNegocio
{
    public class LineaReporte
    {
        public string Etiqueta { get; set; }
        public int Cantidad { get; set; }
        public decimal Total { get; set; }
    }

    public class ResumenReporte
    {
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }

        public int NumVentas { get; set; }
        public decimal TotalVentas { get; set; }
        public decimal TotalDescuentos { get; set; }
        public decimal TicketPromedio { get; set; }

        public int NumCompras { get; set; }
        public decimal TotalCompras { get; set; }

        public int ProductosActivos { get; set; }
        public decimal InventarioCosto { get; set; }
        public decimal InventarioVenta { get; set; }

        public List<LineaReporte> PorMetodo { get; set; } = new List<LineaReporte>();
        public List<LineaReporte> PorVendedor { get; set; } = new List<LineaReporte>();
        public List<Producto> StockBajo { get; set; } = new List<Producto>();
    }

    public class ReporteN
    {
        private readonly VentaD ventaD = new VentaD();
        private readonly CompraD compraD = new CompraD();
        private readonly ProductoD productoD = new ProductoD();

        public ResumenReporte Generar(DateTime desde, DateTime hasta)
        {
            Sesion.ExigirPermiso("menureportes");

            if (desde.Date > hasta.Date)
            {
                DateTime temporal = desde;
                desde = hasta;
                hasta = temporal;
            }

            var ventas = ventaD.Listar(desde, hasta, null);   // null = las ventas de todos
            var compras = compraD.Listar(desde, hasta);
            var activos = productoD.Listar().Where(p => p.Estado).ToList();

            var r = new ResumenReporte { Desde = desde.Date, Hasta = hasta.Date };

            r.NumVentas = ventas.Count;
            r.TotalVentas = ventas.Sum(v => v.MontoTotal);
            r.TotalDescuentos = ventas.Sum(v => v.MontoDescuento);
            r.TicketPromedio = r.NumVentas == 0 ? 0m : Math.Round(r.TotalVentas / r.NumVentas, 2);

            r.NumCompras = compras.Count;
            r.TotalCompras = compras.Sum(c => c.MontoTotal);

            r.ProductosActivos = activos.Count;
            r.InventarioCosto = activos.Sum(p => p.Stock * p.PrecioCompra);
            r.InventarioVenta = activos.Sum(p => p.Stock * p.PrecioVenta);

            // Siempre salen los tres métodos, aunque alguno no tenga ventas
            r.PorMetodo = VentaN.MetodosPago.Select(m => new LineaReporte
            {
                Etiqueta = m,
                Cantidad = ventas.Count(v => v.MetodoPago == m),
                Total = ventas.Where(v => v.MetodoPago == m).Sum(v => v.MontoTotal)
            }).ToList();

            r.PorVendedor = ventas.GroupBy(v => v.UsuarioNombre).Select(g => new LineaReporte
            {
                Etiqueta = g.Key,
                Cantidad = g.Count(),
                Total = g.Sum(v => v.MontoTotal)
            }).OrderByDescending(l => l.Total).ToList();

            r.StockBajo = activos.Where(p => p.Stock <= p.StockMinimo)
                                 .OrderBy(p => p.Stock).ThenBy(p => p.Nombre).ToList();

            return r;
        }
    }
}