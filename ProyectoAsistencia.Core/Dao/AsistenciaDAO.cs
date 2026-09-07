using System;
using Microsoft.Data.SqlClient;
using ProyectoAsistencia.Core.Modelo;
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

        // Consulta si el usuario ya marcó entrada y/o salida hoy, para que la
        // interfaz pueda deshabilitar los botones ya usados (evita doble marcado).
        public EstadoAsistenciaDTO ObtenerEstadoDeHoy(int idUsuario)
        {
            const string sql = @"
                SELECT hora_entrada, hora_salida
                FROM asistencia
                WHERE id_usuario = @id AND fecha = @fecha";
            try
            {
                using var con = ConexionBD.ObtenerConexion();
                using var cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@id", idUsuario);
                cmd.Parameters.AddWithValue("@fecha", DateTime.Now.Date);

                using var reader = cmd.ExecuteReader();
                if (!reader.Read())
                {
                    return new EstadoAsistenciaDTO();
                }

                int ordEntrada = reader.GetOrdinal("hora_entrada");
                int ordSalida = reader.GetOrdinal("hora_salida");
                TimeSpan? horaEntrada = reader.IsDBNull(ordEntrada) ? null : reader.GetTimeSpan(ordEntrada);
                TimeSpan? horaSalida = reader.IsDBNull(ordSalida) ? null : reader.GetTimeSpan(ordSalida);

                return new EstadoAsistenciaDTO
                {
                    EntradaMarcada = horaEntrada.HasValue,
                    SalidaMarcada = horaSalida.HasValue,
                    HoraEntrada = horaEntrada,
                    HoraSalida = horaSalida
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al obtener el estado de asistencia de hoy: " + ex.Message);
                return new EstadoAsistenciaDTO();
            }
        }

        // Permite al administrador corregir un registro existente (por ejemplo,
        // una hora de entrada/salida mal marcada) desde el panel de reportes.
        public bool ModificarRegistro(int idAsistencia, TimeSpan? horaEntrada, TimeSpan? horaSalida)
        {
            const string sql = @"
                UPDATE asistencia
                SET hora_entrada = @horaEntrada, hora_salida = @horaSalida
                WHERE id_asistencia = @id";
            try
            {
                using var con = ConexionBD.ObtenerConexion();
                using var cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@id", idAsistencia);
                cmd.Parameters.AddWithValue("@horaEntrada", (object)horaEntrada ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@horaSalida", (object)horaSalida ?? DBNull.Value);

                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al modificar el registro de asistencia: " + ex.Message);
                return false;
            }
        }
    }
}
