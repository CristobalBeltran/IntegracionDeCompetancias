using ProyectoAsistencia.Core.Util;
using Xunit;

namespace ProyectoAsistencia.Tests
{
    // Pruebas unitarias para validar que la encriptación de contraseñas
    // funcione de forma correcta y consistente (requisito nuevo del profe).
    public class PasswordUtilTests
    {
        [Fact]
        public void MismaPasswordGeneraElMismoHash()
        {
            string hash1 = PasswordUtil.Encriptar("empleado1234");
            string hash2 = PasswordUtil.Encriptar("empleado1234");
            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void PasswordsDistintasGeneranHashesDistintos()
        {
            string hash1 = PasswordUtil.Encriptar("empleado1234");
            string hash2 = PasswordUtil.Encriptar("otraPassword");
            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void ElHashNuncaEsIgualAlTextoPlano()
        {
            string passwordPlano = "empleado1234";
            string hash = PasswordUtil.Encriptar(passwordPlano);
            Assert.NotEqual(passwordPlano, hash);
        }

        [Fact]
        public void ElHashSHA256TieneSiempre64Caracteres()
        {
            string hash = PasswordUtil.Encriptar("cualquierPassword");
            Assert.Equal(64, hash.Length);
        }
    }
}
