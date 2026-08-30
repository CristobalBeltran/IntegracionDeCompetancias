namespace ProyectoAsistencia.Core.Modelo
{
    // Versión "segura" de Usuario para mandar al HTML/JS a través del WebView2.
    // A propósito NO incluye PasswordHash: lo que muestra la interfaz nunca
    // debería tener ni siquiera el hash de la contraseña.
    public class UsuarioDTO
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Rol { get; set; }
        public bool Activo { get; set; }

        public static UsuarioDTO DesdeUsuario(Usuario usuario)
        {
            if (usuario == null) return null;

            return new UsuarioDTO
            {
                IdUsuario = usuario.IdUsuario,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Email = usuario.Email,
                Rol = usuario.Rol.ToString(),
                Activo = usuario.Activo
            };
        }
    }
}
