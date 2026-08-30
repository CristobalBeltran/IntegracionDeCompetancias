using System;
using System.Collections.Generic;
using System.Linq;
using ProyectoAsistencia.Core.Dao;
using ProyectoAsistencia.Core.Modelo;
using ProyectoAsistencia.Core.Util;

namespace ProyectoAsistencia.Core.Servicios
{
    // Gestión de usuarios: GU-01 (crear), GU-02 (modificar), GU-03 (eliminar).
    // Aquí van las validaciones de negocio; el HTML/JS solo manda datos crudos.
    public class UsuarioServicio
    {
        private readonly UsuarioDAO _usuarioDAO;

        public UsuarioServicio() : this(new UsuarioDAO())
        {
        }

        public UsuarioServicio(UsuarioDAO usuarioDAO)
        {
            _usuarioDAO = usuarioDAO;
        }

        // GU-01
        public ResultadoOperacion Crear(string nombre, string apellido, string email, string password, string rolTexto)
        {
            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(apellido)
                || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return ResultadoOperacion.Error("Todos los campos son obligatorios.");
            }

            if (!TryParsearRol(rolTexto, out Rol rol))
            {
                return ResultadoOperacion.Error("Rol inválido. Usa ADMINISTRADOR o EMPLEADO.");
            }

            var nuevo = new Usuario(nombre, apellido, email, PasswordUtil.Encriptar(password), rol);
            bool ok = _usuarioDAO.Crear(nuevo);

            return ok
                ? ResultadoOperacion.Exito("Usuario creado con éxito.")
                : ResultadoOperacion.Error("No se pudo crear el usuario (revisa que el email no esté repetido).");
        }

        // GU-02
        public ResultadoOperacion Modificar(int id, string nombre, string apellido, string email, string rolTexto)
        {
            if (!TryParsearRol(rolTexto, out Rol rol))
            {
                return ResultadoOperacion.Error("Rol inválido. Usa ADMINISTRADOR o EMPLEADO.");
            }

            var usuario = new Usuario(nombre, apellido, email, null, rol) { IdUsuario = id };
            bool ok = _usuarioDAO.Modificar(usuario);

            return ok
                ? ResultadoOperacion.Exito("Usuario modificado con éxito.")
                : ResultadoOperacion.Error("No se pudo modificar el usuario.");
        }

        // GU-03
        public ResultadoOperacion Eliminar(int id)
        {
            bool ok = _usuarioDAO.Eliminar(id);

            return ok
                ? ResultadoOperacion.Exito("Usuario eliminado con éxito.")
                : ResultadoOperacion.Error("No se pudo eliminar el usuario.");
        }

        public List<UsuarioDTO> ListarTodos()
        {
            return _usuarioDAO.ListarTodos()
                .Select(UsuarioDTO.DesdeUsuario)
                .ToList();
        }

        private static bool TryParsearRol(string rolTexto, out Rol rol)
        {
            return Enum.TryParse((rolTexto ?? "").ToUpperInvariant(), out rol);
        }
    }
}
