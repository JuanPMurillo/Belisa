using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BelisaEntidad;

namespace BelisaDatos
{
    public class ClienteD
    {
        public List<Cliente> Listar()
        {
            var lista = new List<Cliente>();
            using (var cn = new SqlConnection(Conexion.Cadena))
            using (var cmd = new SqlCommand("SP_LISTAR_CLIENTES", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cn.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        lista.Add(new Cliente
                        {
                            IdCliente = Convert.ToInt32(rdr["IdCliente"]),
                            Documento = Ayuda.Texto(rdr["Documento"]),
                            NombreCompleto = Convert.ToString(rdr["NombreCompleto"]),
                            Correo = Ayuda.Texto(rdr["Correo"]),
                            Telefono = Ayuda.Texto(rdr["Telefono"]),
                            Estado = Convert.ToBoolean(rdr["Estado"])
                        });
                    }
                }
            }
            return lista;
        }

        // Devuelve el Id guardado, o 0 si no se pudo (con el motivo en 'mensaje')
        public int Guardar(Cliente c, out string mensaje)
        {
            using (var cn = new SqlConnection(Conexion.Cadena))
            using (var cmd = new SqlCommand("SP_GUARDAR_CLIENTE", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IdCliente", SqlDbType.Int).Value = c.IdCliente;
                cmd.Parameters.Add("@NombreCompleto", SqlDbType.VarChar, 100).Value = c.NombreCompleto;
                cmd.Parameters.Add("@Correo", SqlDbType.VarChar, 100).Value = Ayuda.ANullable(c.Correo);
                cmd.Parameters.Add("@Telefono", SqlDbType.VarChar, 50).Value = Ayuda.ANullable(c.Telefono);
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
                    // Dos personas guardaron el mismo documento al mismo tiempo
                    mensaje = "No se pudo asignar el código. Intenta de nuevo.";
                    return 0;
                }

                mensaje = Convert.ToString(pMsg.Value);
                return Convert.ToInt32(pId.Value);
            }
        }
    }
}