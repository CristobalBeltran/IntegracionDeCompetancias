using ProyectoAsistencia.Core.Servicios;
using Xunit;

namespace ProyectoAsistencia.Tests
{
    // Prueba las validaciones de negocio de los Servicios que se ejecutan
    // ANTES de tocar la base de datos (por eso no requieren MySQL corriendo).
    public class ServiciosValidacionTests
    {
        [Fact]
        public void LoginConEmailVacioNoIntentaAutenticar()
        {
            var servicio = new AutenticacionServicio();
            var resultado = servicio.Login("", "cualquierClave");

            Assert.False(resultado.Ok);
            Assert.Equal("Debes ingresar email y contraseña.", resultado.Mensaje);
        }

        [Fact]
        public void CrearUsuarioConCamposVaciosFalla()
        {
            var servicio = new UsuarioServicio();
            var resultado = servicio.Crear("", "Rojas", "pedro@empresa.cl", "1234", "EMPLEADO");

            Assert.False(resultado.Ok);
            Assert.Equal("Todos los campos son obligatorios.", resultado.Mensaje);
        }

        [Fact]
        public void CrearUsuarioConRolInvalidoFalla()
        {
            var servicio = new UsuarioServicio();
            var resultado = servicio.Crear("Pedro", "Rojas", "pedro@empresa.cl", "1234", "SUPERVISOR");

            Assert.False(resultado.Ok);
            Assert.Equal("Rol inválido. Usa ADMINISTRADOR o EMPLEADO.", resultado.Mensaje);
        }
    }
}
