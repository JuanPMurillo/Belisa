using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BelisaEntidad;

namespace BelisaDatos
{
    public class VentaD
    {
        // Devuelve el Id de la venta, o 0 si no se pudo (con el motivo en 'mensaje')
        public int Registrar(Venta v, out string numeroDocumento, out decimal montoTotal,
                             out decimal montoCambio, out string mensaje)
        {
            var tabla = new DataTable();
            tabla.Columns.Add("IdProducto", typeof(int));
            tabla.Columns.Add("PrecioVenta", typeof(decimal));
            tabla.Columns.Add("Cantidad", typeof(int));
            foreach (var d in v.Detalle)
                tabla.Rows.Add(d.IdProducto, d.PrecioVenta, d.Cantidad);

            using (var cn = new SqlConnection(Conexion.Cadena))
            using (var cmd = new SqlCommand("SP_REGISTRAR_VENTA", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = v.IdUsuario;
                cmd.Parameters.Add("@IdCliente", SqlDbType.Int).Value =
                    v.IdCliente.HasValue ? (object)v.IdCliente.Value : DBNull.Value;
                cmd.Parameters.Add("@MetodoPago", SqlDbType.VarChar, 30).Value = v.MetodoPago;
                cmd.Parameters.Add("@ReferenciaPago", SqlDbType.VarChar, 50).Value = Ayuda.ANullable(v.ReferenciaPago);
                cmd.Parameters.Add(ParametroDecimal("@DescuentoPorcentaje", v.DescuentoPorcentaje, 5, 2));
                cmd.Parameters.Add(ParametroDecimal("@MontoPago", v.MontoPago, 10, 2));

                var pDetalle = cmd.Parameters.Add("@Detalle", SqlDbType.Structured);
                pDetalle.TypeName = "dbo.EDetalle_Venta";
                pDetalle.Value = tabla;

                var pId = cmd.Parameters.Add("@IdGenerado", SqlDbType.Int);
                pId.Direction = ParameterDirection.Output;
                var pNum = cmd.Parameters.Add("@NumeroDocumento", SqlDbType.VarChar, 50);
                pNum.Direction = ParameterDirection.Output;
                var pTotal = ParametroDecimal("@MontoTotal", 0m, 10, 2);
                pTotal.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(pTotal);
                var pCambio = ParametroDecimal("@MontoCambio", 0m, 10, 2);
                pCambio.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(pCambio);
                var pMsg = cmd.Parameters.Add("@Mensaje", SqlDbType.VarChar, 300);
                pMsg.Direction = ParameterDirection.Output;

                cn.Open();
                cmd.ExecuteNonQuery();

                numeroDocumento = Convert.ToString(pNum.Value);
                montoTotal = Convert.ToDecimal(pTotal.Value);
                montoCambio = Convert.ToDecimal(pCambio.Value);
                mensaje = Convert.ToString(pMsg.Value);
                return Convert.ToInt32(pId.Value);
            }
        }

        // idUsuario = null trae las ventas de todos; con un Id, solo las de esa persona
        public List<Venta> Listar(DateTime desde, DateTime hasta, int? idUsuario)
        {
            var lista = new List<Venta>();
            using (var cn = new SqlConnection(Conexion.Cadena))
            using (var cmd = new SqlCommand("SP_LISTAR_VENTAS", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Desde", SqlDbType.Date).Value = desde.Date;
                cmd.Parameters.Add("@Hasta", SqlDbType.Date).Value = hasta.Date;
                cmd.Parameters.Add("@IdUsuario", SqlDbType.Int).Value =
                    idUsuario.HasValue ? (object)idUsuario.Value : DBNull.Value;
                cn.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        lista.Add(new Venta
                        {
                            IdVenta = Convert.ToInt32(rdr["IdVenta"]),
                            NumeroDocumento = Convert.ToString(rdr["NumeroDocumento"]),
                            TipoDocumento = Convert.ToString(rdr["TipoDocumento"]),
                            MetodoPago = Convert.ToString(rdr["MetodoPago"]),
                            ReferenciaPago = Ayuda.Texto(rdr["ReferenciaPago"]),
                            Subtotal = Convert.ToDecimal(rdr["Subtotal"]),
                            DescuentoPorcentaje = Convert.ToDecimal(rdr["DescuentoPorcentaje"]),
                            MontoDescuento = Convert.ToDecimal(rdr["MontoDescuento"]),
                            MontoTotal = Convert.ToDecimal(rdr["MontoTotal"]),
                            MontoPago = Convert.ToDecimal(rdr["MontoPago"]),
                            MontoCambio = Convert.ToDecimal(rdr["MontoCambio"]),
                            FechaRegistro = Convert.ToDateTime(rdr["FechaRegistro"]),
                            IdCliente = rdr["IdCliente"] == DBNull.Value ? (int?)null : Convert.ToInt32(rdr["IdCliente"]),
                            ClienteNombre = Convert.ToString(rdr["ClienteNombre"]),
                            IdUsuario = Convert.ToInt32(rdr["IdUsuario"]),
                            UsuarioNombre = Convert.ToString(rdr["UsuarioNombre"]),
                            Anulada = Convert.ToBoolean(rdr["Anulada"]),
                            FechaAnulacion = rdr["FechaAnulacion"] == DBNull.Value
                                ? (DateTime?)null : Convert.ToDateTime(rdr["FechaAnulacion"]),
                            AnuladaPor = Ayuda.Texto(rdr["AnuladaPor"]),
                            MotivoAnulacion = Ayuda.Texto(rdr["MotivoAnulacion"])
                        });
                    }
                }
            }
            return lista;
        }

        public List<DetalleVenta> ObtenerDetalle(int idVenta, int? idUsuario)
        {
            var lista = new List<DetalleVenta>();
            using (var cn = new SqlConnection(Conexion.Cadena))
            using (var cmd = new SqlCommand("SP_DETALLE_VENTA", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IdVenta", SqlDbType.Int).Value = idVenta;
                cmd.Parameters.Add("@IdUsuario", SqlDbType.Int).Value =
                    idUsuario.HasValue ? (object)idUsuario.Value : DBNull.Value;
                cn.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        lista.Add(new DetalleVenta
                        {
                            IdProducto = Convert.ToInt32(rdr["IdProducto"]),
                            ProductoCodigo = Convert.ToString(rdr["ProductoCodigo"]),
                            ProductoNombre = Convert.ToString(rdr["ProductoNombre"]),
                            PrecioVenta = Convert.ToDecimal(rdr["PrecioVenta"]),
                            Cantidad = Convert.ToInt32(rdr["Cantidad"]),
                            SubTotal = Convert.ToDecimal(rdr["SubTotal"])
                        });
                    }
                }
            }
            return lista;
        }

        private static SqlParameter ParametroDecimal(string nombre, decimal valor, byte precision, byte escala)
        {
            return new SqlParameter(nombre, SqlDbType.Decimal)
            {
                Precision = precision,
                Scale = escala,
                Value = valor
            };
        }
    }
}