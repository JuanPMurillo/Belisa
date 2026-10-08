using System.Collections.Generic;
using BelisaDatos;
using BelisaEntidad;

namespace BelisaNegocio
{
    public class ProductoN
    {
        private readonly ProductoD objD = new ProductoD();

        public List<Producto> Listar()
        {
            Sesion.ExigirPermiso("menugestion");
            return objD.Listar();
        }

        public bool Guardar(Producto p, out string mensaje)
        {
            Sesion.ExigirPermiso("menugestion");
            mensaje = "";

            // El código no se valida: lo asigna la base de datos al crear
            p.Nombre = (p.Nombre ?? "").Trim();
            p.Descripcion = (p.Descripcion ?? "").Trim();

            // Un servicio no maneja inventario
            if (p.EsServicio)
            {
                p.Stock = 0;
                p.StockMinimo = 0;
            }

            if (p.Nombre.Length == 0) { mensaje = "El nombre es obligatorio."; return false; }
            if (p.Nombre.Length > 100) { mensaje = "El nombre no puede pasar de 100 caracteres."; return false; }
            if (p.Descripcion.Length > 200) { mensaje = "La descripción no puede pasar de 200 caracteres."; return false; }
            if (p.IdCategoria <= 0) { mensaje = "Selecciona una categoría."; return false; }
            if (p.PrecioCompra < 0) { mensaje = "El costo no puede ser negativo."; return false; }
            if (p.PrecioVenta <= 0) { mensaje = "El precio de venta debe ser mayor a 0."; return false; }
            if (p.Stock < 0 || p.StockMinimo < 0) { mensaje = "El stock no puede ser negativo."; return false; }

            int id = objD.Guardar(p, out mensaje);
            if (id > 0) p.IdProducto = id;
            return id > 0;
        }
    }
}