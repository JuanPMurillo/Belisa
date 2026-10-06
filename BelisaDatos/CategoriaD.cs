using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BelisaEntidad;

namespace BelisaDatos
{
    public class CategoriaD
    {
        public List<Categoria> Listar()
        {
            var lista = new List<Categoria>();
            using (var cn = new SqlConnection(Conexion.Cadena))
            using (var cmd = new SqlCommand("SP_LISTAR_CATEGORIAS", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cn.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        lista.Add(new Categoria
                        {
                            IdCategoria = Convert.ToInt32(rdr["IdCategoria"]),
                            Descripcion = Convert.ToString(rdr["Descripcion"]),
                            Estado = Convert.ToBoolean(rdr["Estado"])
                        });
                    }
                }
            }
            return lista;
        }

        // Devuelve el Id guardado, o 0 si no se pudo (con el motivo en 'mensaje')
        public int Guardar(Categoria c, out string mensaje)
        {
            using (var cn = new SqlConnection(Conexion.Cadena))
            using (var cmd = new SqlCommand("SP_GUARDAR_CATEGORIA", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IdCategoria", SqlDbType.Int).Value = c.IdCategoria;
                cmd.Parameters.Add("@Descripcion", SqlDbType.VarChar, 100).Value = c.Descripcion;
                cmd.Parameters.Add("@Estado", SqlDbType.Bit).Value = c.Estado;

                var pId = cmd.Parameters.Add("@IdGenerado", SqlDbType.Int);
                pId.Direction = ParameterDirection.Output;
                var pMsg = cmd.Parameters.Add("@Mensaje", SqlDbType.VarChar, 200);
                pMsg.Direction = ParameterDirection.Output;

                try
                {
                    cn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
                {
                    // Dos personas guardaron el mismo nombre al mismo tiempo
                    mensaje = "Ya existe una categoría con ese nombre.";
                    return 0;
                }

                mensaje = Convert.ToString(pMsg.Value);
                return Convert.ToInt32(pId.Value);
            }
        }
    }
}