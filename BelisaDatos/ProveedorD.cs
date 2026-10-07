using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BelisaEntidad;

namespace BelisaDatos
{
    public class ProveedorD
    {
        public List<Proveedor> Listar()
        {
            var lista = new List<Proveedor>();
            using (var cn = new SqlConnection(Conexion.Cadena))
            using (var cmd = new SqlCommand("SP_LISTAR_PROVEEDORES", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cn.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        lista.Add(new Proveedor
                        {
                            IdProveedor = Convert.ToInt32(rdr["IdProveedor"]),
                            Documento = Ayuda.Texto(rdr["Documento"]),
                            RazonSocial = Convert.ToString(rdr["RazonSocial"]),
                            Correo = Ayuda.Texto(rdr["Correo"]),
                            Telefono = Ayuda.Texto(rdr["Telefono"]),
                            Estado = Convert.ToBoolean(rdr["Estado"])
                        });
                    }
                }
            }
            return lista;
        }

        public int Guardar(Proveedor p, out string mensaje)
        {
            using (var cn = new SqlConnection(Conexion.Cadena))
            using (var cmd = new SqlCommand("SP_GUARDAR_PROVEEDOR", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IdProveedor", SqlDbType.Int).Value = p.IdProveedor;
                cmd.Parameters.Add("@RazonSocial", SqlDbType.VarChar, 100).Value = p.RazonSocial;
                cmd.Parameters.Add("@Correo", SqlDbType.VarChar, 100).Value = Ayuda.ANullable(p.Correo);
                cmd.Parameters.Add("@Telefono", SqlDbType.VarChar, 50).Value = Ayuda.ANullable(p.Telefono);
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
    }
}