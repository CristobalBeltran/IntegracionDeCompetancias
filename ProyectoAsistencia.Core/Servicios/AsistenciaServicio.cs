using System;
using ProyectoAsistencia.Core.Dao;
using ProyectoAsistencia.Core.Modelo;

namespace ProyectoAsistencia.Core.Servicios
{
    // CA-01: control de asistencia (marcar entrada y salida).
    public class AsistenciaServicio
    {
        private readonly AsistenciaDAO _asistenciaDAO;

        public AsistenciaServicio() : this(new AsistenciaDAO())
        {
        }

        public AsistenciaServicio(AsistenciaDAO asistenciaDAO)
        {
            _asistenciaDAO = asistenciaDAO;
        }

        public ResultadoOperacion MarcarEntrada(int idUsuario)
        {
            // Guarda del lado del servidor: si ya se marcó entrada hoy, no se
            // vuelve a registrar (evita doble asistencia aunque el botón de
            // la interfaz no se haya deshabilitado a tiempo).
            if (_asistenciaDAO.ObtenerEstadoDeHoy(idUsuario).EntradaMarcada)
            {
                return ResultadoOperacion.Error("Ya registraste tu entrada de hoy.");
            }

            bool ok = _asistenciaDAO.RegistrarEntrada(idUsuario);
            return ok
                ? ResultadoOperacion.Exito("Entrada registrada.")
                : ResultadoOperacion.Error("No se pudo registrar la entrada.");
        }

        public ResultadoOperacion MarcarSalida(int idUsuario)
        {
            if (_asistenciaDAO.ObtenerEstadoDeHoy(idUsuario).SalidaMarcada)
            {
                return ResultadoOperacion.Error("Ya registraste tu salida de hoy.");
            }

            bool ok = _asistenciaDAO.RegistrarSalida(idUsuario);
            return ok
                ? ResultadoOperacion.Exito("Salida registrada.")
                : ResultadoOperacion.Error("No se pudo registrar la salida.");
        }

        // Estado de asistencia de hoy, usado por la interfaz para decidir qué
        // botones mostrar habilitados/deshabilitados (CA-01).
        public EstadoAsistenciaDTO ObtenerEstadoHoy(int idUsuario)
        {
            return _asistenciaDAO.ObtenerEstadoDeHoy(idUsuario);
        }

        // Permite al administrador corregir un registro de asistencia
        // existente (por ejemplo, una hora mal marcada) desde Reportes.
        public ResultadoOperacion ModificarRegistro(int idAsistencia, TimeSpan? horaEntrada, TimeSpan? horaSalida)
        {
            if (idAsistencia <= 0)
            {
                return ResultadoOperacion.Error("Registro de asistencia inválido.");
            }

            if (!horaEntrada.HasValue && !horaSalida.HasValue)
            {
                return ResultadoOperacion.Error("Debes indicar al menos una hora (entrada o salida).");
            }

            if (horaEntrada.HasValue && horaSalida.HasValue && horaEntrada.Value >= horaSalida.Value)
            {
                return ResultadoOperacion.Error("La hora de entrada debe ser anterior a la hora de salida.");
            }

            bool ok = _asistenciaDAO.ModificarRegistro(idAsistencia, horaEntrada, horaSalida);
            return ok
                ? ResultadoOperacion.Exito("Registro modificado con éxito.")
                : ResultadoOperacion.Error("No se pudo modificar el registro.");
        }
    }
}
