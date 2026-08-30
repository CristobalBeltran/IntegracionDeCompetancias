using System;
using Microsoft.Data.SqlClient;
using ProyectoAsistencia.Core.Util;

namespace ProyectoAsistencia.Core.Dao
{
    // CA-01: Control de asistencia (marcar entrada y salida)
    public class AsistenciaDAO
    {
        // Se llama cuando el usuario presiona el botón "Entrada".
        // Si ya existe un registro para hoy, actualiza la hora de entrada.
        // (SQL Server no tiene "ON DUPLICATE KEY UPDATE" como MySQL, por eso
        // se resuelve con un IF EXISTS ... UPDATE ... ELSE ... INSERT).
        public bool RegistrarEntrada(int idUsuario)
        {
            const string sql = @"
                IF EXISTS (SELECT 1 FROM asistencia WHERE id_usuario = @id AND fecha = @fecha)
                    UPDATE asistencia SET hora_entrada = @hora WHERE id_usuario = @id AND fecha = @fecha
                ELSE
                    INSERT INTO asistencia (id_usuario, fecha, hora_entrada) VALUES (@id, @fecha, @hora)";
            try
            {
                using var con = ConexionBD.ObtenerConexion();
                using var cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@id", idUsuario);
                cmd.Parameters.AddWithValue("@fecha", DateTime.Now.Date);
                cmd.Parameters.AddWithValue("@hora", DateTime.Now.TimeOfDay);

                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al registrar entrada: " + ex.Message);
                return false;
            }
        }

        // Se llama cuando el usuario presiona el botón "Salida".
        public bool RegistrarSalida(int idUsuario)
        {
            const string sql = @"
                IF EXISTS (SELECT 1 FROM asistencia WHERE id_usuario = @id AND fecha = @fecha)
                    UPDATE asistencia SET hora_salida = @hora WHERE id_usuario = @id AND fecha = @fecha
                ELSE
                    INSERT INTO asistencia (id_usuario, fecha, hora_salida) VALUES (@id, @fecha, @hora)";
            try
            {
                using var con = ConexionBD.ObtenerConexion();
                using var cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@id", idUsuario);
                cmd.Parameters.AddWithValue("@fecha", DateTime.Now.Date);
                cmd.Parameters.AddWithValue("@hora", DateTime.Now.TimeOfDay);

                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al registrar salida: " + ex.Message);
                return false;
            }
        }
    }
}
