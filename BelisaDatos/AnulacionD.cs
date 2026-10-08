using System;
using System.Data;
using System.Data.SqlClient;

namespace BelisaDatos
{
    public class AnulacionD
    {
        public bool AnularVenta(int idVenta, int idUsuario, string motivo, out string mensaje)
        {
            return Ejecutar("SP_ANULAR_VENTA", "@IdVenta", idVenta, idUsuario, motivo, out mensaje);
        }

        public bool AnularCompra(int idCompra, int idUsuario, string motivo, out string mensaje)
        {
            return Ejecutar("SP_ANULAR_COMPRA", "@IdCompra", idCompra, idUsuario, motivo, out mensaje);
        }

        private bool Ejecutar(string procedimiento, string parametroId, int id, int idUsuario,
                              string motivo, out string mensaje)
        {
            using (var cn = new SqlConnection(Conexion.Cadena))
            using (var cmd = new SqlCommand(procedimiento, cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(parametroId, SqlDbType.Int).Value = id;
                cmd.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = idUsuario;
                cmd.Parameters.Add("@Motivo", SqlDbType.VarChar, 200).Value = motivo;

                var pRes = cmd.Parameters.Add("@Resultado", SqlDbType.Bit);
                pRes.Direction = ParameterDirection.Output;
                var pMsg = cmd.Parameters.Add("@Mensaje", SqlDbType.VarChar, 300);
                pMsg.Direction = ParameterDirection.Output;

                cn.Open();
                cmd.ExecuteNonQuery();

                mensaje = Convert.ToString(pMsg.Value);
                return Convert.ToBoolean(pRes.Value);
            }
        }
    }
}