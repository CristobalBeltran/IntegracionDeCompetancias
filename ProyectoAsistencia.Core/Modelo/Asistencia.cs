using System;

namespace ProyectoAsistencia.Core.Modelo
{
    // Representa el registro de entrada/salida de un usuario en un día.
    // Cubre CA-01 (control de asistencia) y la lógica de RE-01, RE-02 y RE-03.
    public class Asistencia
    {
        // Horarios límite definidos en el caso
        private static readonly TimeSpan HoraLimiteEntrada = new TimeSpan(9, 30, 0);
        private static readonly TimeSpan HoraLimiteSalida = new TimeSpan(17, 30, 0);

        public int IdAsistencia { get; set; }
        public int IdUsuario { get; set; }
        public DateTime Fecha { get; set; }
        public TimeSpan? HoraEntrada { get; set; }
        public TimeSpan? HoraSalida { get; set; }

        public Asistencia()
        {
        }

        public Asistencia(int idUsuario, DateTime fecha)
        {
            IdUsuario = idUsuario;
            Fecha = fecha;
        }

        // RE-01: entrada posterior a las 09:30
        public bool EsAtraso()
        {
            return HoraEntrada.HasValue && HoraEntrada.Value > HoraLimiteEntrada;
        }

        // RE-02: salida anterior a las 17:30
        public bool EsSalidaAnticipada()
        {
            return HoraSalida.HasValue && HoraSalida.Value < HoraLimiteSalida;
        }

        // RE-03: no se registró ni entrada ni salida ese día
        public bool EsInasistencia()
        {
            return !HoraEntrada.HasValue && !HoraSalida.HasValue;
        }

        public override string ToString()
        {
            return $"Asistencia{{usuario={IdUsuario}, fecha={Fecha:yyyy-MM-dd}, entrada={HoraEntrada}, salida={HoraSalida}}}";
        }
    }
}
