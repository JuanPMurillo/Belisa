using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BelisaEntidad;

namespace BelisaDatos
{
    public class ProductoD
    {
        public List<Producto> Listar()
        {
            var lista = new List<Producto>();
            using (var cn = new SqlConnection(Conexion.Cadena))
            using (var cmd = new SqlCommand("SP_LISTAR_PRODUCTOS", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cn.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        lista.Add(new Producto
                        {
                            IdProducto = Convert.ToInt32(rdr["IdProducto"]),
                            Codigo = Convert.ToString(rdr["Codigo"]),
                            Nombre = Convert.ToString(rdr["Nombre"]),
                            IdCategoria = Convert.ToInt32(rdr["IdCategoria"]),
                            CategoriaDescripcion = Convert.ToString(rdr["CategoriaDescripcion"]),
                            Stock = Convert.ToInt32(rdr["Stock"]),
                            StockMinimo = Convert.ToInt32(rdr["StockMinimo"]),
                            PrecioCompra = Convert.ToDecimal(rdr["PrecioCompra"]),
                            PrecioVenta = Convert.ToDecimal(rdr["PrecioVenta"]),
                            Descripcion = rdr["Descripcion"] == DBNull.Value ? "" : Convert.ToString(rdr["Descripcion"]),
                            Estado = Convert.ToBoolean(rdr["Estado"])
                        });
                    }
                }
            }
            return lista;
        }

        // Devuelve el Id guardado, o 0 si no se pudo (con el motivo en 'mensaje')
        public int Guardar(Producto p, out string mensaje)
        {
            using (var cn = new SqlConnection(Conexion.Cadena))
            using (var cmd = new SqlCommand("SP_GUARDAR_PRODUCTO", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IdProducto", SqlDbType.Int).Value = p.IdProducto;
                cmd.Parameters.Add("@Nombre", SqlDbType.VarChar, 100).Value = p.Nombre;
                cmd.Parameters.Add("@IdCategoria", SqlDbType.Int).Value = p.IdCategoria;
                cmd.Parameters.Add("@Stock", SqlDbType.Int).Value = p.Stock;
                cmd.Parameters.Add("@StockMinimo", SqlDbType.Int).Value = p.StockMinimo;
                cmd.Parameters.Add(Dinero("@PrecioCompra", p.PrecioCompra));
                cmd.Parameters.Add(Dinero("@PrecioVenta", p.PrecioVenta));
                cmd.Parameters.Add("@Descripcion", SqlDbType.VarChar, 200).Value = Ayuda.ANullable(p.Descripcion);
                cmd.Parameters.Add("@Estado", SqlDbType.Bit).Value = p.Estado;

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
                    mensaje = "No se pudo asignar el código. Intenta de nuevo.";
                    return 0;
                }

                mensaje = Convert.ToString(pMsg.Value);
                return Convert.ToInt32(pId.Value);
            }
        }

        private static SqlParameter Dinero(string nombre, decimal valor)
        {
            return new SqlParameter(nombre, SqlDbType.Decimal)
            {
                Precision = 10,
                Scale = 2,
                Value = valor
            };
        }
    }
}