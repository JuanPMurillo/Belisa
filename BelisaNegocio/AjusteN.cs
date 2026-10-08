using System;
using System.Collections.Generic;
using System.Linq;
using BelisaDatos;
using BelisaEntidad;

namespace BelisaNegocio
{
    public class AjusteN
    {
        public static readonly string[] Motivos =
        {
            "Conteo físico", "Producto dañado", "Producto caducado",
            "Regalo o muestra", "Error de captura", "Otro"
        };

        // Con estos motivos tiene sentido que el stock solo baje
        private static readonly string[] SoloBajan = { "Producto dañado", "Producto caducado", "Regalo o muestra" };

        private const int StockMaximo = 1000000;

        private readonly AjusteD objD = new AjusteD();
        private readonly ProductoD productoD = new ProductoD();

        // Todos los productos, también los inactivos: pueden tener existencias que corregir
        public List<Producto> ListarProductos()
        {
            Sesion.ExigirPermiso("menugestion");
            return productoD.Listar().Where(p => !p.EsServicio).ToList();
        }

        // stockEsperado = el stock que se vio en pantalla al contar; stockNuevo = el conteo real
        public bool Registrar(int idProducto, int stockEsperado, int stockNuevo,
                              string motivo, string nota, out string mensaje)
        {
            Sesion.ExigirPermiso("menugestion");
            mensaje = "";

            motivo = (motivo ?? "").Trim();
            nota = (nota ?? "").Trim();

            if (idProducto <= 0) { mensaje = "Selecciona un producto."; return false; }
            if (!Motivos.Contains(motivo)) { mensaje = "Selecciona el motivo del ajuste."; return false; }
            if (stockNuevo < 0 || stockNuevo > StockMaximo) { mensaje = "El conteo no es válido."; return false; }

            if (stockNuevo == stockEsperado)
            {
                mensaje = "El conteo coincide con el sistema: no hay nada que ajustar.";
                return false;
            }
            if (SoloBajan.Contains(motivo) && stockNuevo > stockEsperado)
            {
                mensaje = "Con el motivo «" + motivo + "» el stock solo puede bajar.";
                return false;
            }
            if (motivo == "Otro" && nota.Length < 5)
            {
                mensaje = "Con el motivo «Otro» escribe una nota que lo explique (mínimo 5 letras).";
                return false;
            }
            if (nota.Length > 200)
            {
                mensaje = "La nota no puede pasar de 200 caracteres.";
                return false;
            }

            // El usuario sale de la sesión, nunca de la pantalla
            return objD.Registrar(idProducto, Sesion.UsuarioActual.IdUsuario, stockEsperado, stockNuevo,
                                  motivo, nota, out mensaje);
        }

        public List<Ajuste> Listar(DateTime desde, DateTime hasta)
        {
            Sesion.ExigirPermiso("menugestion");
            if (desde.Date > hasta.Date)
            {
                DateTime temporal = desde;
                desde = hasta;
                hasta = temporal;
            }
            return objD.Listar(desde, hasta);
        }
    }
}