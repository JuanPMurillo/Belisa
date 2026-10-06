using System.Collections.Generic;
using BelisaDatos;
using BelisaEntidad;

namespace BelisaNegocio
{
    public class CategoriaN
    {
        private readonly CategoriaD objD = new CategoriaD();

        public List<Categoria> Listar()
        {
            Sesion.ExigirPermiso("menugestion");
            return objD.Listar();
        }

        public bool Guardar(Categoria c, out string mensaje)
        {
            Sesion.ExigirPermiso("menugestion");
            mensaje = "";

            c.Descripcion = (c.Descripcion ?? "").Trim();
            if (c.Descripcion.Length == 0)
            {
                mensaje = "La descripción es obligatoria.";
                return false;
            }
            if (c.Descripcion.Length > 100)
            {
                mensaje = "La descripción no puede pasar de 100 caracteres.";
                return false;
            }

            int id = objD.Guardar(c, out mensaje);
            if (id > 0) c.IdCategoria = id;
            return id > 0;
        }
    }
}