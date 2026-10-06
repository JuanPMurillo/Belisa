using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BelisaEntidad;

namespace BelisaDatos
{
    public class UsuarioD
    {
        public CredencialUsuario ObtenerCredencial(string nameUser)
        {
            using (var cn = new SqlConnection(Conexion.Cadena))
            using (var cmd = new SqlCommand("SP_OBTENER_LOGIN", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@NameUser", SqlDbType.VarChar, 50).Value = nameUser;
                cn.Open();

                using (var rdr = cmd.ExecuteReader())
                {
                    if (!rdr.Read()) return null;

                    return new CredencialUsuario
                    {
                        ClaveHash = Convert.ToString(rdr["ClaveHash"]),
                        Bloqueado = Convert.ToBoolean(rdr["Bloqueado"]),
                        Usuario = new Usuario
                        {
                            IdUsuario = Convert.ToInt32(rdr["IdUsuario"]),
                            NameUser = Convert.ToString(rdr["NameUser"]),
                            NombreCompleto = Convert.ToString(rdr["NombreCompleto"]),
                            Correo = rdr["Correo"] == DBNull.Value ? "" : Convert.ToString(rdr["Correo"]),
                            IdRol = Convert.ToInt32(rdr["IdRol"]),
                            RolDescripcion = Convert.ToString(rdr["RolDescripcion"]),
                            Estado = Convert.ToBoolean(rdr["Estado"]),
                            FechaRegistro = Convert.ToDateTime(rdr["FechaRegistro"])
                        }
                    };
                }
            }
        }

        public void RegistrarIntentoFallido(int idUsuario) => EjecutarPorId("SP_REGISTRAR_INTENTO_FALLIDO", idUsuario);

        public void RegistrarLoginExitoso(int idUsuario) => EjecutarPorId("SP_LOGIN_EXITOSO", idUsuario);

        private void EjecutarPorId(string sp, int idUsuario)
        {
            using (var cn = new SqlConnection(Conexion.Cadena))
            using (var cmd = new SqlCommand(sp, cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = idUsuario;
                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public bool ExisteUsuarios()
        {
            using (var cn = new SqlConnection(Conexion.Cadena))
            using (var cmd = new SqlCommand("SP_EXISTE_USUARIOS", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        public List<Rol> ListarRoles()
        {
            var lista = new List<Rol>();
            using (var cn = new SqlConnection(Conexion.Cadena))
            using (var cmd = new SqlCommand("SP_LISTAR_ROLES", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cn.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        lista.Add(new Rol
                        {
                            IdRol = Convert.ToInt32(rdr["IdRol"]),
                            Descripcion = Convert.ToString(rdr["Descripcion"])
                        });
                    }
                }
            }
            return lista;
        }

        public List<string> ListarPermisos(int idRol)
        {
            var lista = new List<string>();
            using (var cn = new SqlConnection(Conexion.Cadena))
            using (var cmd = new SqlCommand("SP_LISTAR_PERMISOS", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IdRol", SqlDbType.Int).Value = idRol;
                cn.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                        lista.Add(Convert.ToString(rdr["NombreMenu"]));
                }
            }
            return lista;
        }

        public int CrearUsuario(Usuario u, string claveHash, out string mensaje)
        {
            using (var cn = new SqlConnection(Conexion.Cadena))
            using (var cmd = new SqlCommand("SP_CREAR_USUARIO", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@NameUser", SqlDbType.VarChar, 50).Value = u.NameUser;
                cmd.Parameters.Add("@NombreCompleto", SqlDbType.VarChar, 100).Value = u.NombreCompleto;
                cmd.Parameters.Add("@Correo", SqlDbType.VarChar, 100).Value =
                    string.IsNullOrWhiteSpace(u.Correo) ? (object)DBNull.Value : u.Correo;
                cmd.Parameters.Add("@ClaveHash", SqlDbType.VarChar, 200).Value = claveHash;
                cmd.Parameters.Add("@IdRol", SqlDbType.Int).Value = u.IdRol;

                var pId = cmd.Parameters.Add("@IdGenerado", SqlDbType.Int);
                pId.Direction = ParameterDirection.Output;
                var pMsg = cmd.Parameters.Add("@Mensaje", SqlDbType.VarChar, 200);
                pMsg.Direction = ParameterDirection.Output;

                cn.Open();
                cmd.ExecuteNonQuery();

                mensaje = Convert.ToString(pMsg.Value);
                return Convert.ToInt32(pId.Value);
            }
        }
    }
}