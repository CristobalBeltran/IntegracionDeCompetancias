using System.Security.Cryptography;
using System.Text;

namespace ProyectoAsistencia.Core.Util
{
    // Encripta las contraseñas con SHA-256, para que coincida con el
    // hash generado en el script SQL con SHA2(password, 256).
    public static class PasswordUtil
    {
        public static string Encriptar(string passwordPlano)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(passwordPlano);
                byte[] hashBytes = sha256.ComputeHash(bytes);

                var sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}
