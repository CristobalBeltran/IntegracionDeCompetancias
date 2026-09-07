using System.Collections.Generic;
using ProyectoAsistencia.Core.Dao;
using ProyectoAsistencia.Core.Modelo;

namespace ProyectoAsistencia.Core.Servicios
{
    // Junta los 3 reportes (RE-01, RE-02, RE-03) en una sola respuesta,
    // lista para mandarle al HTML de una vez.
    public class ReporteServicio
    {
        private readonly ReporteDAO _reporteDAO;

        public ReporteServicio() : this(new ReporteDAO())
        {
        }

        public ReporteServicio(ReporteDAO reporteDAO)
        {
            _reporteDAO = reporteDAO;
        }

        public ResumenReportes ObtenerResumen()
        {
            return new ResumenReportes
            {
                Atrasos = _reporteDAO.ListarAtrasos(),
                SalidasAnticipadas = _reporteDAO.ListarSalidasAnticipadas(),
                Inasistencias = _reporteDAO.ListarInasistencias()
            };
        }

        // Historial de entradas/salidas de todos los empleados, para la
        // pestaña de Historial del panel de administrador.
        public List<ReporteAsistenciaDTO> ObtenerHistorial()
        {
            return _reporteDAO.ListarHistorial();
        }
    }

    public class ResumenReportes
    {
        public List<ReporteAsistenciaDTO> Atrasos { get; set; } = new();
        public List<ReporteAsistenciaDTO> SalidasAnticipadas { get; set; } = new();
        public List<ReporteAsistenciaDTO> Inasistencias { get; set; } = new();
    }
}
