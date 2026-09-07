using System;

namespace ProyectoAsistencia.Core.Modelo
{
    // Estado de la asistencia de HOY para un usuario. Se usa para que la
    // interfaz sepa si ya se marcó entrada y/o salida y así deshabilitar
    // los botones correspondientes (evita el doble registro, CA-01).
    public class EstadoAsistenciaDTO
    {
        public bool EntradaMarcada { get; set; }
        public bool SalidaMarcada { get; set; }
        public TimeSpan? HoraEntrada { get; set; }
        public TimeSpan? HoraSalida { get; set; }
    }
}
