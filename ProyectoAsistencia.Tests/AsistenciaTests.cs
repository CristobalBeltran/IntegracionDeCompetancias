using System;
using ProyectoAsistencia.Core.Modelo;
using Xunit;

namespace ProyectoAsistencia.Tests
{
    // Pruebas unitarias para validar la lógica de RE-01, RE-02 y RE-03
    // sin necesidad de conectarse a la base de datos.
    public class AsistenciaTests
    {
        [Fact]
        public void EntradaDespuesDeLas0930EsAtraso()
        {
            var a = new Asistencia(1, DateTime.Now) { HoraEntrada = new TimeSpan(9, 45, 0) };
            Assert.True(a.EsAtraso(), "Una entrada a las 09:45 debería marcarse como atraso");
        }

        [Fact]
        public void EntradaAntesDeLas0930NoEsAtraso()
        {
            var a = new Asistencia(1, DateTime.Now) { HoraEntrada = new TimeSpan(9, 0, 0) };
            Assert.False(a.EsAtraso(), "Una entrada a las 09:00 no debería marcarse como atraso");
        }

        [Fact]
        public void SalidaAntesDeLas1730EsSalidaAnticipada()
        {
            var a = new Asistencia(1, DateTime.Now) { HoraSalida = new TimeSpan(17, 0, 0) };
            Assert.True(a.EsSalidaAnticipada(), "Una salida a las 17:00 debería marcarse como anticipada");
        }

        [Fact]
        public void SalidaDespuesDeLas1730NoEsSalidaAnticipada()
        {
            var a = new Asistencia(1, DateTime.Now) { HoraSalida = new TimeSpan(18, 0, 0) };
            Assert.False(a.EsSalidaAnticipada(), "Una salida a las 18:00 no debería marcarse como anticipada");
        }

        [Fact]
        public void SinEntradaNiSalidaEsInasistencia()
        {
            var a = new Asistencia(1, DateTime.Now);
            Assert.True(a.EsInasistencia(), "Sin entrada ni salida debería marcarse como inasistencia");
        }

        [Fact]
        public void ConEntradaRegistradaNoEsInasistencia()
        {
            var a = new Asistencia(1, DateTime.Now) { HoraEntrada = new TimeSpan(9, 0, 0) };
            Assert.False(a.EsInasistencia(), "Si hay entrada registrada, no debería ser inasistencia");
        }
    }
}
