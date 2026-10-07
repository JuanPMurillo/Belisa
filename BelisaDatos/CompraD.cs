using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BelisaEntidad;

namespace BelisaDatos
{
    public class CompraD
    {
        // Devuelve el Id de la compra, o 0 si no se pudo (con el motivo en 'mensaje')
        public int Registrar(Compra c, out string numeroDocumento, out string mensaje)
        {
            var tabla = new DataTable();
            tabla.Columns.Add("IdProducto", typeof(int));
            tabla.Columns.Add("PrecioCompra", typeof(decimal));
            tabla.Columns.Add("PrecioVenta", typeof(decimal));
            tabla.Columns.Add("Cantidad", typeof(int));
            foreach (var d in c.Detalle)
                tabla.Rows.Add(d.IdProducto, d.PrecioCompra, d.PrecioVenta, d.Cantidad);

            using (var cn = new SqlConnection(Conexion.Cadena))
            using (var cmd = new SqlCommand("SP_REGISTRAR_COMPRA", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = c.IdUsuario;
                cmd.Parameters.Add("@IdProveedor", SqlDbType.Int).Value = c.IdProveedor;
                cmd.Parameters.Add("@TipoDocumento", SqlDbType.VarChar, 50).Value = c.TipoDocumento;

                var pDetalle = cmd.Parameters.Add("@Detalle", SqlDbType.Structured);
                pDetalle.TypeName = "dbo.EDetalle_Compra";
                pDetalle.Value = tabla;

                var pId = cmd.Parameters.Add("@IdGenerado", SqlDbType.Int);
                pId.Direction = ParameterDirection.Output;
                var pNum = cmd.Parameters.Add("@NumeroDocumento", SqlDbType.VarChar, 50);
                pNum.Direction = ParameterDirection.Output;
                var pMsg = cmd.Parameters.Add("@Mensaje", SqlDbType.VarChar, 300);
                pMsg.Direction = ParameterDirection.Output;

                cn.Open();
                cmd.ExecuteNonQuery();

                numeroDocumento = Convert.ToString(pNum.Value);
                mensaje = Convert.ToString(pMsg.Value);
                return Convert.ToInt32(pId.Value);
            }
        }

        public List<Compra> Listar(DateTime desde, DateTime hasta)
        {
            var lista = new List<Compra>();
            using (var cn = new SqlConnection(Conexion.Cadena))
            using (var cmd = new SqlCommand("SP_LISTAR_COMPRAS", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Desde", SqlDbType.Date).Value = desde.Date;
                cmd.Parameters.Add("@Hasta", SqlDbType.Date).Value = hasta.Date;
                cn.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        lista.Add(new Compra
                        {
                            IdCompra = Convert.ToInt32(rdr["IdCompra"]),
                            NumeroDocumento = Convert.ToString(rdr["NumeroDocumento"]),
                            TipoDocumento = Convert.ToString(rdr["TipoDocumento"]),
                            MontoTotal = Convert.ToDecimal(rdr["MontoTotal"]),
                            FechaRegistro = Convert.ToDateTime(rdr["FechaRegistro"]),
                            IdProveedor = Convert.ToInt32(rdr["IdProveedor"]),
                            ProveedorNombre = Convert.ToString(rdr["ProveedorNombre"]),
                            IdUsuario = Convert.ToInt32(rdr["IdUsuario"]),
                            UsuarioNombre = Convert.ToString(rdr["UsuarioNombre"])
                        });
                    }
                }
            }
            return lista;
        }

        public List<DetalleCompra> ObtenerDetalle(int idCompra)
        {
            var lista = new List<DetalleCompra>();
            using (var cn = new SqlConnection(Conexion.Cadena))
            using (var cmd = new SqlCommand("SP_DETALLE_COMPRA", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IdCompra", SqlDbType.Int).Value = idCompra;
                cn.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        lista.Add(new DetalleCompra
                        {
                            IdProducto = Convert.ToInt32(rdr["IdProducto"]),
                            ProductoCodigo = Convert.ToString(rdr["ProductoCodigo"]),
                            ProductoNombre = Convert.ToString(rdr["ProductoNombre"]),
                            PrecioCompra = Convert.ToDecimal(rdr["PrecioCompra"]),
                            PrecioVenta = Convert.ToDecimal(rdr["PrecioVenta"]),
                            Cantidad = Convert.ToInt32(rdr["Cantidad"]),
                            MontoTotal = Convert.ToDecimal(rdr["MontoTotal"])
                        });
                    }
                }
            }
            return lista;
        }
    }
}