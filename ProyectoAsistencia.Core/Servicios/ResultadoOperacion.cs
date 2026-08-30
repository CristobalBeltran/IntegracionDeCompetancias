namespace ProyectoAsistencia.Core.Servicios
{
    // Resultado simple de una operación (crear, modificar, eliminar, marcar
    // entrada/salida, etc). Se serializa directo a JSON para el HTML.
    public class ResultadoOperacion
    {
        public bool Ok { get; set; }
        public string Mensaje { get; set; }

        public static ResultadoOperacion Exito(string mensaje = "")
            => new ResultadoOperacion { Ok = true, Mensaje = mensaje };

        public static ResultadoOperacion Error(string mensaje)
            => new ResultadoOperacion { Ok = false, Mensaje = mensaje };
    }

    // Resultado específico del login: además del ok/mensaje, trae el
    // usuario (sin password) para que la interfaz sepa qué mostrar.
    public class ResultadoLogin : ResultadoOperacion
    {
        public Modelo.UsuarioDTO Usuario { get; set; }
    }
}
