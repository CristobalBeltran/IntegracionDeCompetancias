using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using ProyectoAsistencia.Core.Servicios;

namespace ProyectoAsistencia.Desktop
{
    // Traduce los mensajes JSON que llegan desde el HTML/JS (a través de
    // window.chrome.webview.postMessage) en llamadas a los servicios del
    // Core, y arma la respuesta JSON que se devuelve al HTML.
    //
    // Contrato de mensajes (todos con la forma { "accion": "..." , ...}):
    //   -> { accion: "login", email, password }
    //   <- { accion: "loginResultado", ok, mensaje, usuario? }
    //
    //   -> { accion: "listarUsuarios" }
    //   <- { accion: "listarUsuariosResultado", ok, usuarios: [...] }
    //
    //   -> { accion: "crearUsuario", nombre, apellido, email, password, rol }
    //   <- { accion: "crearUsuarioResultado", ok, mensaje }
    //
    //   -> { accion: "modificarUsuario", id, nombre, apellido, email, rol }
    //   <- { accion: "modificarUsuarioResultado", ok, mensaje }
    //
    //   -> { accion: "eliminarUsuario", id }
    //   <- { accion: "eliminarUsuarioResultado", ok, mensaje }
    //
    //   -> { accion: "marcarEntrada", idUsuario }
    //   <- { accion: "marcarEntradaResultado", ok, mensaje }
    //
    //   -> { accion: "marcarSalida", idUsuario }
    //   <- { accion: "marcarSalidaResultado", ok, mensaje }
    public class Puente
    {
        private readonly AutenticacionServicio _autenticacionServicio = new AutenticacionServicio();
        private readonly UsuarioServicio _usuarioServicio = new UsuarioServicio();
        private readonly AsistenciaServicio _asistenciaServicio = new AsistenciaServicio();

        private static readonly JsonSerializerOptions OpcionesJson = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // Punto de entrada único: recibe el JSON crudo que llegó del WebView2
        // y devuelve el JSON (como string) que hay que postear de vuelta.
        public string ProcesarMensaje(string jsonRecibido)
        {
            try
            {
                JsonNode nodo = JsonNode.Parse(jsonRecibido);
                string accion = nodo?["accion"]?.GetValue<string>() ?? "";

                object respuesta = accion switch
                {
                    "login" => ManejarLogin(nodo),
                    "listarUsuarios" => ManejarListarUsuarios(),
                    "crearUsuario" => ManejarCrearUsuario(nodo),
                    "modificarUsuario" => ManejarModificarUsuario(nodo),
                    "eliminarUsuario" => ManejarEliminarUsuario(nodo),
                    "marcarEntrada" => ManejarMarcarEntrada(nodo),
                    "marcarSalida" => ManejarMarcarSalida(nodo),
                    _ => new { accion = "error", ok = false, mensaje = $"Acción desconocida: {accion}" }
                };

                return JsonSerializer.Serialize(respuesta, OpcionesJson);
            }
            catch (Exception ex)
            {
                var error = new { accion = "error", ok = false, mensaje = "Error procesando el mensaje: " + ex.Message };
                return JsonSerializer.Serialize(error, OpcionesJson);
            }
        }

        private object ManejarLogin(JsonNode nodo)
        {
            string email = nodo["email"]?.GetValue<string>();
            string password = nodo["password"]?.GetValue<string>();

            ResultadoLogin resultado = _autenticacionServicio.Login(email, password);

            return new
            {
                accion = "loginResultado",
                ok = resultado.Ok,
                mensaje = resultado.Mensaje,
                usuario = resultado.Usuario
            };
        }

        private object ManejarListarUsuarios()
        {
            var usuarios = _usuarioServicio.ListarTodos();
            return new { accion = "listarUsuariosResultado", ok = true, usuarios };
        }

        private object ManejarCrearUsuario(JsonNode nodo)
        {
            var resultado = _usuarioServicio.Crear(
                nodo["nombre"]?.GetValue<string>(),
                nodo["apellido"]?.GetValue<string>(),
                nodo["email"]?.GetValue<string>(),
                nodo["password"]?.GetValue<string>(),
                nodo["rol"]?.GetValue<string>()
            );

            return new { accion = "crearUsuarioResultado", ok = resultado.Ok, mensaje = resultado.Mensaje };
        }

        private object ManejarModificarUsuario(JsonNode nodo)
        {
            var resultado = _usuarioServicio.Modificar(
                nodo["id"]?.GetValue<int>() ?? 0,
                nodo["nombre"]?.GetValue<string>(),
                nodo["apellido"]?.GetValue<string>(),
                nodo["email"]?.GetValue<string>(),
                nodo["rol"]?.GetValue<string>()
            );

            return new { accion = "modificarUsuarioResultado", ok = resultado.Ok, mensaje = resultado.Mensaje };
        }

        private object ManejarEliminarUsuario(JsonNode nodo)
        {
            var resultado = _usuarioServicio.Eliminar(nodo["id"]?.GetValue<int>() ?? 0);
            return new { accion = "eliminarUsuarioResultado", ok = resultado.Ok, mensaje = resultado.Mensaje };
        }

        private object ManejarMarcarEntrada(JsonNode nodo)
        {
            var resultado = _asistenciaServicio.MarcarEntrada(nodo["idUsuario"]?.GetValue<int>() ?? 0);
            return new { accion = "marcarEntradaResultado", ok = resultado.Ok, mensaje = resultado.Mensaje };
        }

        private object ManejarMarcarSalida(JsonNode nodo)
        {
            var resultado = _asistenciaServicio.MarcarSalida(nodo["idUsuario"]?.GetValue<int>() ?? 0);
            return new { accion = "marcarSalidaResultado", ok = resultado.Ok, mensaje = resultado.Mensaje };
        }
    }
}
