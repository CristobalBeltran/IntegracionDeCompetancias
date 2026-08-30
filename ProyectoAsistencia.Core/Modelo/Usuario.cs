namespace ProyectoAsistencia.Core.Modelo
{
    // Representa a un trabajador o trabajadora de la empresa.
    // Cubre GU-01, GU-02 y GU-03 (crear, modificar y eliminar usuarios).
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; } // nunca se guarda ni se envía en texto plano
        public Rol Rol { get; set; }
        public bool Activo { get; set; }

        public Usuario()
        {
            Activo = true;
        }

        public Usuario(string nombre, string apellido, string email, string passwordHash, Rol rol)
        {
            Nombre = nombre;
            Apellido = apellido;
            Email = email;
            PasswordHash = passwordHash;
            Rol = rol;
            Activo = true;
        }

        public bool EsAdministrador()
        {
            return Rol == Rol.ADMINISTRADOR;
        }

        public override string ToString()
        {
            return $"Usuario{{id={IdUsuario}, nombre='{Nombre} {Apellido}', email='{Email}', rol={Rol}, activo={Activo}}}";
        }
    }
}
