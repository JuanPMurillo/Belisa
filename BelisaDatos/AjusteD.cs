using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BelisaEntidad;

namespace BelisaDatos
{
    public class AjusteD
    {
        public bool Registrar(int idProducto, int idUsuario, int stockEsperado, int stockNuevo,
                              string motivo, string nota, out string mensaje)
        {
            using (var cn = new SqlConnection(Conexion.Cadena))
            using (var cmd = new SqlCommand("SP_AJUSTAR_INVENTARIO", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IdProducto", SqlDbType.Int).Value = idProducto;
                cmd.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = idUsuario;
                cmd.Parameters.Add("@StockEsperado", SqlDbType.Int).Value = stockEsperado;
                cmd.Parameters.Add("@StockNuevo", SqlDbType.Int).Value = stockNuevo;
                cmd.Parameters.Add("@Motivo", SqlDbType.VarChar, 30).Value = motivo;
                cmd.Parameters.Add("@Nota", SqlDbType.VarChar, 200).Value = Ayuda.ANullable(nota);

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

        public List<Ajuste> Listar(DateTime desde, DateTime hasta)
        {
            var lista = new List<Ajuste>();
            using (var cn = new SqlConnection(Conexion.Cadena))
            using (var cmd = new SqlCommand("SP_LISTAR_AJUSTES", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Desde", SqlDbType.Date).Value = desde.Date;
                cmd.Parameters.Add("@Hasta", SqlDbType.Date).Value = hasta.Date;
                cn.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        lista.Add(new Ajuste
                        {
                            IdAjuste = Convert.ToInt32(rdr["IdAjuste"]),
                            FechaRegistro = Convert.ToDateTime(rdr["FechaRegistro"]),
                            ProductoCodigo = Convert.ToString(rdr["Codigo"]),
                            ProductoNombre = Convert.ToString(rdr["Nombre"]),
                            StockAnterior = Convert.ToInt32(rdr["StockAnterior"]),
                            StockNuevo = Convert.ToInt32(rdr["StockNuevo"]),
                            Diferencia = Convert.ToInt32(rdr["Diferencia"]),
                            Motivo = Convert.ToString(rdr["Motivo"]),
                            Nota = Ayuda.Texto(rdr["Nota"]),
                            UsuarioNombre = Convert.ToString(rdr["UsuarioNombre"])
                        });
                    }
                }
            }
            return lista;
        }
    }
}