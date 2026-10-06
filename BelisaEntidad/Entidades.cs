using System;
using System.Collections.Generic;

namespace BelisaEntidad
{
    public class Rol
    {
        public int IdRol { get; set; }
        public string Descripcion { get; set; }
    }

    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string NameUser { get; set; }
        public string NombreCompleto { get; set; }
        public string Correo { get; set; }
        public int IdRol { get; set; }
        public string RolDescripcion { get; set; }   // solo para mostrar en pantalla
        public bool Estado { get; set; }
        public DateTime FechaRegistro { get; set; }
        // La clave NUNCA vive en esta clase: solo existe como hash en la BD
    }

    public class Categoria
    {
        public int IdCategoria { get; set; }
        public string Descripcion { get; set; }
        public bool Estado { get; set; }
    }

    public class Producto
    {
        public int IdProducto { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public int IdCategoria { get; set; }
        public string CategoriaDescripcion { get; set; }
        public int Stock { get; set; }
        public decimal PrecioCompra { get; set; }
        public decimal PrecioVenta { get; set; }
        public string Descripcion { get; set; }
        public bool Estado { get; set; }
    }

    public class Cliente
    {
        public int IdCliente { get; set; }
        public string Documento { get; set; }
        public string NombreCompleto { get; set; }
        public string Correo { get; set; }
        public string Telefono { get; set; }
        public bool Estado { get; set; }
    }

    public class Proveedor
    {
        public int IdProveedor { get; set; }
        public string Documento { get; set; }
        public string RazonSocial { get; set; }
        public string Correo { get; set; }
        public string Telefono { get; set; }
        public bool Estado { get; set; }
    }

    public class Compra
    {
        public int IdCompra { get; set; }
        public int IdUsuario { get; set; }
        public int IdProveedor { get; set; }
        public string ProveedorNombre { get; set; }
        public string TipoDocumento { get; set; }
        public string NumeroDocumento { get; set; }
        public decimal MontoTotal { get; set; }
        public DateTime FechaRegistro { get; set; }
        public List<DetalleCompra> Detalle { get; set; } = new List<DetalleCompra>();
    }

    public class DetalleCompra
    {
        public int IdProducto { get; set; }
        public string ProductoCodigo { get; set; }
        public string ProductoNombre { get; set; }
        public decimal PrecioCompra { get; set; }
        public decimal PrecioVenta { get; set; }
        public int Cantidad { get; set; }
        public decimal MontoTotal { get; set; }
    }

    public class Venta
    {
        public int IdVenta { get; set; }
        public int IdUsuario { get; set; }
        public int? IdCliente { get; set; }          // null = venta de mostrador
        public string ClienteNombre { get; set; }
        public string TipoDocumento { get; set; }
        public string NumeroDocumento { get; set; }
        public decimal MontoPago { get; set; }
        public decimal MontoCambio { get; set; }
        public decimal MontoTotal { get; set; }
        public DateTime FechaRegistro { get; set; }
        public List<DetalleVenta> Detalle { get; set; } = new List<DetalleVenta>();
    }

    public class DetalleVenta
    {
        public int IdProducto { get; set; }
        public string ProductoCodigo { get; set; }
        public string ProductoNombre { get; set; }
        public decimal PrecioVenta { get; set; }
        public int Cantidad { get; set; }
        public decimal SubTotal { get; set; }
    }
}