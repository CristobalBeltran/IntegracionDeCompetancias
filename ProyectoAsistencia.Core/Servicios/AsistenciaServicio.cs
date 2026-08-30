using ProyectoAsistencia.Core.Dao;

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
            bool ok = _asistenciaDAO.RegistrarEntrada(idUsuario);
            return ok
                ? ResultadoOperacion.Exito("Entrada registrada.")
                : ResultadoOperacion.Error("No se pudo registrar la entrada.");
        }

        public ResultadoOperacion MarcarSalida(int idUsuario)
        {
            bool ok = _asistenciaDAO.RegistrarSalida(idUsuario);
            return ok
                ? ResultadoOperacion.Exito("Salida registrada.")
                : ResultadoOperacion.Error("No se pudo registrar la salida.");
        }
    }
}
