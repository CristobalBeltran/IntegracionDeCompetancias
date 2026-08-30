# Sistema de Registro de Asistencia — Prototipo (Avance #3)

## Arquitectura

El proyecto quedó dividido en 3 capas / 3 proyectos dentro de la misma solución:

```
ProyectoAsistencia.sln
├── ProyectoAsistencia.Core/       Lógica de negocio, sin nada de UI.
│   ├── Modelo/                    Usuario, Rol, Asistencia, UsuarioDTO
│   ├── Util/                      ConexionBD, PasswordUtil (SHA-256)
│   ├── Dao/                       UsuarioDAO, AsistenciaDAO (acceso a MySQL)
│   └── Servicios/                 AutenticacionServicio, UsuarioServicio,
│                                  AsistenciaServicio (validaciones + orquestación)
│
├── ProyectoAsistencia.Desktop/    App de escritorio (WinForms + WebView2)
│   ├── Program.cs                 Punto de entrada
│   ├── MainForm.cs                Form con el control WebView2
│   ├── Puente.cs                  Traduce mensajes JSON JS <-> Servicios de Core
│   └── wwwroot/                   HTML + CSS + JS de la interfaz
│       ├── index.html
│       ├── css/estilos.css
│       └── js/app.js
│
└── ProyectoAsistencia.Tests/      Tests unitarios (xUnit), apuntan a Core
```

**Por qué esta separación:** `Core` no sabe que existe WinForms ni WebView2, así
que se puede testear sin levantar ninguna ventana. `Desktop` no tiene lógica de
negocio, solo dibuja HTML y reenvía mensajes. Si el día de mañana cambian de
WebView2 a otra UI (o a una web real), `Core` no se toca.

## Cómo se comunican el HTML y el C#

El HTML/JS vive dentro del `WebView2` y **no tiene acceso directo a la base de
datos ni al C#**. Todo pasa por mensajes JSON:

1. El JS llama `window.chrome.webview.postMessage({ accion: 'login', email, password })`.
2. `MainForm.cs` escucha `CoreWebView2.WebMessageReceived` y le pasa el JSON a `Puente.cs`.
3. `Puente.cs` mira el campo `"accion"`, llama al servicio de `Core` que corresponda
   (ej. `AutenticacionServicio.Login(...)`) y arma la respuesta.
4. `MainForm.cs` devuelve la respuesta al HTML con `CoreWebView2.PostWebMessageAsJson(...)`.
5. El JS la recibe en `window.chrome.webview.addEventListener('message', ...)`.

Todas las acciones soportadas (`login`, `listarUsuarios`, `crearUsuario`,
`modificarUsuario`, `eliminarUsuario`, `marcarEntrada`, `marcarSalida`) están
documentadas como comentario arriba de la clase `Puente`.

Un detalle importante: al HTML **nunca** se le manda el hash de la contraseña.
`UsuarioDTO` es una versión "limpia" de `Usuario` sin ese campo.

## Cómo correrlo (Visual Studio 2022, en Windows)

1. Abrir `ProyectoAsistencia.sln`.
2. Restaurar paquetes NuGet (Visual Studio lo hace solo al abrir, o botón derecho
   en la solución > Restaurar paquetes NuGet).
3. Instalar el **runtime de WebView2** si no lo tienes (viene preinstalado en
   Windows 11 y en la mayoría de Windows 10 actualizados; si falta, se descarga
   gratis del sitio de Microsoft Edge WebView2).
4. Ejecutar el script `script_bd_asistencia.txt` en MySQL para crear la base
   `registro_asistencia`.
5. Ajustar usuario/contraseña de MySQL en
   `ProyectoAsistencia.Core/Util/ConexionBD.cs`.
6. Marcar `ProyectoAsistencia.Desktop` como proyecto de inicio (botón derecho >
   "Set as Startup Project") y presionar F5.
7. Para correr los tests: menú Test > Run All Tests (o `dotnet test` en la
   carpeta `ProyectoAsistencia.Tests`).

## Usuarios de prueba (vienen del script SQL)

| Email                     | Password     | Rol           |
|---------------------------|--------------|---------------|
| ana.soto@empresa.cl       | admin1234    | ADMINISTRADOR |
| pedro.rojas@empresa.cl    | pedro1234    | EMPLEADO      |
| maria.diaz@empresa.cl     | maria1234    | EMPLEADO      |

## Pendientes / ideas para el próximo avance

- Cambiar contraseña desde el panel de administrador (hoy `Modificar` no la toca).
- Mostrar el historial de asistencia y los reportes (RE-01/02/03) en el panel admin.
- Manejo de errores de conexión a MySQL más amigable en el HTML (hoy solo se ve
  el mensaje crudo de la excepción si la BD no está disponible).
