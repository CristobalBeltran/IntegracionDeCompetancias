using System;

namespace ProyectoAsistencia.Core.Modelo
{
    // Una fila de cualquiera de los 3 reportes (RE-01, RE-02, RE-03).
    // "Tipo" indica de cuál de los tres se trata, para que el HTML
    // pueda reusar la misma tabla/plantilla si quiere.
    public class ReporteAsistenciaDTO
    {
        public int IdAsistencia { get; set; }
        public int IdUsuario { get; set; }
        public string NombreCompleto { get; set; }
        public string Email { get; set; }
        public DateTime Fecha { get; set; }
        public TimeSpan? HoraEntrada { get; set; }
        public TimeSpan? HoraSalida { get; set; }
        public string Tipo { get; set; } // "ATRASO" | "SALIDA_ANTICIPADA" | "INASISTENCIA"
    }
}
