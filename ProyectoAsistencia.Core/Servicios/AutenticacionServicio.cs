using ProyectoAsistencia.Core.Dao;
using ProyectoAsistencia.Core.Modelo;
using ProyectoAsistencia.Core.Util;

namespace ProyectoAsistencia.Core.Servicios
{
    // CA-01 (parte de login): valida credenciales y entrega el usuario
    // autenticado, listo para que la interfaz decida qué panel mostrar.
    public class AutenticacionServicio
    {
        private readonly UsuarioDAO _usuarioDAO;

        public AutenticacionServicio() : this(new UsuarioDAO())
        {
        }

        public AutenticacionServicio(UsuarioDAO usuarioDAO)
        {
            _usuarioDAO = usuarioDAO;
        }

        public ResultadoLogin Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return new ResultadoLogin { Ok = false, Mensaje = "Debes ingresar email y contraseña." };
            }

            Usuario usuario = _usuarioDAO.BuscarPorEmail(email);

            if (usuario == null || usuario.PasswordHash != PasswordUtil.Encriptar(password))
            {
                return new ResultadoLogin { Ok = false, Mensaje = "Credenciales incorrectas." };
            }

            if (!usuario.Activo)
            {
                return new ResultadoLogin { Ok = false, Mensaje = "El usuario se encuentra inactivo." };
            }

            return new ResultadoLogin
            {
                Ok = true,
                Mensaje = $"Bienvenido/a {usuario.Nombre}",
                Usuario = UsuarioDTO.DesdeUsuario(usuario)
            };
        }
    }
}
