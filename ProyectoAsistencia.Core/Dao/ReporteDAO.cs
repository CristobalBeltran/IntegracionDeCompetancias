using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using ProyectoAsistencia.Core.Modelo;
using ProyectoAsistencia.Core.Util;

namespace ProyectoAsistencia.Core.Dao
{
    // RE-01, RE-02, RE-03: los 3 reportes que pide la pauta de la semana 5.
    // Las condiciones de cada consulta son las mismas reglas que ya están
    // codificadas en Modelo.Asistencia (EsAtraso, EsSalidaAnticipada,
    // EsInasistencia), solo que aquí se filtran directo en SQL para no
    // tener que traer toda la tabla asistencia a memoria.
    public class ReporteDAO
    {
        // RE-01: entrada registrada después de las 09:30
        public List<ReporteAsistenciaDTO> ListarAtrasos()
        {
            const string sql = @"
                SELECT a.id_asistencia, u.id_usuario, u.nombre, u.apellido, u.email,
                       a.fecha, a.hora_entrada, a.hora_salida
                FROM asistencia a
                JOIN usuarios u ON u.id_usuario = a.id_usuario
                WHERE a.hora_entrada IS NOT NULL AND a.hora_entrada > '09:30:00'
                ORDER BY a.fecha DESC, u.nombre ASC, u.apellido ASC";

            return Ejecutar(sql, "ATRASO");
        }

        // RE-02: salida registrada antes de las 17:30
        public List<ReporteAsistenciaDTO> ListarSalidasAnticipadas()
        {
            const string sql = @"
                SELECT a.id_asistencia, u.id_usuario, u.nombre, u.apellido, u.email,
                       a.fecha, a.hora_entrada, a.hora_salida
                FROM asistencia a
                JOIN usuarios u ON u.id_usuario = a.id_usuario
                WHERE a.hora_salida IS NOT NULL AND a.hora_salida < '17:30:00'
                ORDER BY a.fecha DESC, u.nombre ASC, u.apellido ASC";

            return Ejecutar(sql, "SALIDA_ANTICIPADA");
        }

        // RE-03: día con registro en la tabla pero sin entrada ni salida
        public List<ReporteAsistenciaDTO> ListarInasistencias()
        {
            const string sql = @"
                SELECT a.id_asistencia, u.id_usuario, u.nombre, u.apellido, u.email,
                       a.fecha, a.hora_entrada, a.hora_salida
                FROM asistencia a
                JOIN usuarios u ON u.id_usuario = a.id_usuario
                WHERE a.hora_entrada IS NULL AND a.hora_salida IS NULL
                ORDER BY a.fecha DESC, u.nombre ASC, u.apellido ASC";

            return Ejecutar(sql, "INASISTENCIA");
        }

        // Historial completo: todas las marcas de entrada/salida de todos los
        // empleados, sin filtrar por atraso/salida anticipada/inasistencia.
        public List<ReporteAsistenciaDTO> ListarHistorial()
        {
            const string sql = @"
                SELECT a.id_asistencia, u.id_usuario, u.nombre, u.apellido, u.email,
                       a.fecha, a.hora_entrada, a.hora_salida
                FROM asistencia a
                JOIN usuarios u ON u.id_usuario = a.id_usuario
                ORDER BY a.fecha DESC, u.nombre ASC, u.apellido ASC";

            return Ejecutar(sql, "HISTORIAL");
        }

        private List<ReporteAsistenciaDTO> Ejecutar(string sql, string tipo)
        {
            var resultado = new List<ReporteAsistenciaDTO>();
            try
            {
                using var con = ConexionBD.ObtenerConexion();
                using var cmd = new SqlCommand(sql, con);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    int ordEntrada = reader.GetOrdinal("hora_entrada");
                    int ordSalida = reader.GetOrdinal("hora_salida");

                    resultado.Add(new ReporteAsistenciaDTO
                    {
                        IdAsistencia = reader.GetInt32(reader.GetOrdinal("id_asistencia")),
                        IdUsuario = reader.GetInt32(reader.GetOrdinal("id_usuario")),
                        NombreCompleto = reader.GetString(reader.GetOrdinal("nombre")) + " " +
                                         reader.GetString(reader.GetOrdinal("apellido")),
                        Email = reader.GetString(reader.GetOrdinal("email")),
                        Fecha = reader.GetDateTime(reader.GetOrdinal("fecha")),
                        HoraEntrada = reader.IsDBNull(ordEntrada) ? null : reader.GetTimeSpan(ordEntrada),
                        HoraSalida = reader.IsDBNull(ordSalida) ? null : reader.GetTimeSpan(ordSalida),
                        Tipo = tipo
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al listar reporte de {tipo}: " + ex.Message);
            }
            return resultado;
        }
    }
}
