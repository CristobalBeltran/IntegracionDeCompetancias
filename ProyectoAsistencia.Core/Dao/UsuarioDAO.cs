using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using ProyectoAsistencia.Core.Modelo;
using ProyectoAsistencia.Core.Util;

namespace ProyectoAsistencia.Core.Dao
{
    public class UsuarioDAO
    {
        // GU-01: Crear usuarios
        public bool Crear(Usuario usuario)
        {
            const string sql = @"INSERT INTO usuarios (nombre, apellido, email, password, id_rol)
                                  VALUES (@nombre, @apellido, @email, @password,
                                         (SELECT id_rol FROM roles WHERE nombre_rol = @rol))";
            try
            {
                using var con = ConexionBD.ObtenerConexion();
                using var cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@nombre", usuario.Nombre);
                cmd.Parameters.AddWithValue("@apellido", usuario.Apellido);
                cmd.Parameters.AddWithValue("@email", usuario.Email);
                cmd.Parameters.AddWithValue("@password", usuario.PasswordHash);
                cmd.Parameters.AddWithValue("@rol", usuario.Rol.ToString());

                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al crear usuario: " + ex.Message);
                return false;
            }
        }

        // GU-02: Modificar usuarios
        public bool Modificar(Usuario usuario)
        {
            const string sql = @"UPDATE usuarios SET nombre = @nombre, apellido = @apellido,
                                  email = @email, id_rol = (SELECT id_rol FROM roles WHERE nombre_rol = @rol)
                                  WHERE id_usuario = @id";
            try
            {
                using var con = ConexionBD.ObtenerConexion();
                using var cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@nombre", usuario.Nombre);
                cmd.Parameters.AddWithValue("@apellido", usuario.Apellido);
                cmd.Parameters.AddWithValue("@email", usuario.Email);
                cmd.Parameters.AddWithValue("@rol", usuario.Rol.ToString());
                cmd.Parameters.AddWithValue("@id", usuario.IdUsuario);

                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al modificar usuario: " + ex.Message);
                return false;
            }
        }

        // GU-03: Eliminar usuarios
        public bool Eliminar(int idUsuario)
        {
            const string sql = "DELETE FROM usuarios WHERE id_usuario = @id";
            try
            {
                using var con = ConexionBD.ObtenerConexion();
                using var cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@id", idUsuario);

                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al eliminar usuario: " + ex.Message);
                return false;
            }
        }

        // Usado en el login (CA-01): busca un usuario por su correo
        public Usuario BuscarPorEmail(string email)
        {
            const string sql = @"SELECT u.id_usuario, u.nombre, u.apellido, u.email, u.password,
                                  r.nombre_rol, u.activo
                                  FROM usuarios u JOIN roles r ON r.id_rol = u.id_rol
                                  WHERE u.email = @email";
            try
            {
                using var con = ConexionBD.ObtenerConexion();
                using var cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@email", email);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return LeerUsuario(reader);
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al buscar usuario: " + ex.Message);
                return null;
            }
        }

        public List<Usuario> ListarTodos()
        {
            var usuarios = new List<Usuario>();
            const string sql = @"SELECT u.id_usuario, u.nombre, u.apellido, u.email, u.password,
                                  r.nombre_rol, u.activo
                                  FROM usuarios u JOIN roles r ON r.id_rol = u.id_rol";
            try
            {
                using var con = ConexionBD.ObtenerConexion();
                using var cmd = new SqlCommand(sql, con);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    usuarios.Add(LeerUsuario(reader));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al listar usuarios: " + ex.Message);
            }
            return usuarios;
        }

        // SqlDataReader (Microsoft.Data.SqlClient) no trae los métodos
        // GetInt32("columna") / GetString("columna") por nombre que sí
        // tenía MySqlDataReader; hay que resolver el ordinal primero.
        private static Usuario LeerUsuario(SqlDataReader reader)
        {
            return new Usuario
            {
                IdUsuario = reader.GetInt32(reader.GetOrdinal("id_usuario")),
                Nombre = reader.GetString(reader.GetOrdinal("nombre")),
                Apellido = reader.GetString(reader.GetOrdinal("apellido")),
                Email = reader.GetString(reader.GetOrdinal("email")),
                PasswordHash = reader.GetString(reader.GetOrdinal("password")),
                Rol = (Rol)Enum.Parse(typeof(Rol), reader.GetString(reader.GetOrdinal("nombre_rol"))),
                Activo = reader.GetBoolean(reader.GetOrdinal("activo"))
            };
        }
    }
}
