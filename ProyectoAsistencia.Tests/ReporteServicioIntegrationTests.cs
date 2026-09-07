using System.Linq;
using ProyectoAsistencia.Core.Servicios;
using Xunit;

namespace ProyectoAsistencia.Tests
{
    // Pruebas DE INTEGRACIÓN (a diferencia de AsistenciaTests y
    // ServiciosValidacionTests, que son puramente unitarias y no tocan
    // nada externo). Estas SÍ necesitan la base de datos real corriendo
    // (LocalDB, con el script script_bd_asistencia_sqlserver.sql ya
    // ejecutado — ver SETUP_SQLSERVER.md), porque validan que el
    // ReporteServicio + ReporteDAO + SQL Server trabajen juntos bien.
    //
    // Se apoyan en los datos de prueba que deja el script:
    //   Pedro Rojas  -> entrada 09:45 el 2026-08-19  => debería salir en Atrasos
    //   Maria Diaz   -> salida  17:15 el 2026-08-18  => debería salir en Salidas Anticipadas
    //   Maria Diaz   -> sin registro el 2026-08-19   => debería salir en Inasistencias
    [Trait("Categoria", "Integracion")]
    public class ReporteServicioIntegrationTests
    {
        private readonly ReporteServicio _servicio = new ReporteServicio();

        [Fact]
        public void ElReporteDeAtrasosIncluyeAPedroRojasEl19DeAgosto()
        {
            var resumen = _servicio.ObtenerResumen();

            bool encontrado = resumen.Atrasos.Any(r =>
                r.Email == "pedro.rojas@empresa.cl" &&
                r.Fecha.ToString("yyyy-MM-dd") == "2026-08-19");

            Assert.True(encontrado, "Se esperaba encontrar el atraso de Pedro Rojas del 2026-08-19");
        }

        [Fact]
        public void ElReporteDeSalidasAnticipadasIncluyeAMariaDiazEl18DeAgosto()
        {
            var resumen = _servicio.ObtenerResumen();

            bool encontrado = resumen.SalidasAnticipadas.Any(r =>
                r.Email == "maria.diaz@empresa.cl" &&
                r.Fecha.ToString("yyyy-MM-dd") == "2026-08-18");

            Assert.True(encontrado, "Se esperaba encontrar la salida anticipada de Maria Diaz del 2026-08-18");
        }

        [Fact]
        public void ElReporteDeInasistenciasIncluyeAMariaDiazEl19DeAgosto()
        {
            var resumen = _servicio.ObtenerResumen();

            bool encontrado = resumen.Inasistencias.Any(r =>
                r.Email == "maria.diaz@empresa.cl" &&
                r.Fecha.ToString("yyyy-MM-dd") == "2026-08-19");

            Assert.True(encontrado, "Se esperaba encontrar la inasistencia de Maria Diaz del 2026-08-19");
        }

        [Fact]
        public void NingunReporteIncluyeElDiaNormalDePedroDel18DeAgosto()
        {
            // Ese día Pedro entró a las 09:15 y salió a las 17:35: no debería
            // aparecer en ninguno de los 3 reportes.
            var resumen = _servicio.ObtenerResumen();

            bool apareceEnAlguno =
                resumen.Atrasos.Any(r => r.Email == "pedro.rojas@empresa.cl" && r.Fecha.ToString("yyyy-MM-dd") == "2026-08-18") ||
                resumen.SalidasAnticipadas.Any(r => r.Email == "pedro.rojas@empresa.cl" && r.Fecha.ToString("yyyy-MM-dd") == "2026-08-18") ||
                resumen.Inasistencias.Any(r => r.Email == "pedro.rojas@empresa.cl" && r.Fecha.ToString("yyyy-MM-dd") == "2026-08-18");

            Assert.False(apareceEnAlguno, "El día normal de Pedro no debería salir en ningún reporte");
        }
    }
}
