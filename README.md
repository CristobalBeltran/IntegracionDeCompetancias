# Sistema de Registro de Asistencia — Prototipo (Avance #4)

## Arquitectura

El proyecto quedó dividido en 3 capas / 3 proyectos dentro de la misma solución:

```
ProyectoAsistencia.sln
├── ProyectoAsistencia.Core/       Lógica de negocio, sin nada de UI.
│   ├── Modelo/                    Usuario, Rol, Asistencia, UsuarioDTO, ReporteAsistenciaDTO
│   ├── Util/                      ConexionBD, PasswordUtil (SHA-256)
│   ├── Dao/                       UsuarioDAO, AsistenciaDAO, ReporteDAO (acceso a SQL Server)
│   └── Servicios/                 AutenticacionServicio, UsuarioServicio,
│                                  AsistenciaServicio, ReporteServicio (validaciones + orquestación)
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
`modificarUsuario`, `eliminarUsuario`, `marcarEntrada`, `marcarSalida`,
`obtenerReportes`) están documentadas como comentario arriba de la clase
`Puente`.

## Reportes (Avance #4: RE-01, RE-02, RE-03)

El panel de administrador ahora tiene 2 pestañas: **Usuarios** (lo de antes)
y **Reportes**. La pestaña de Reportes pide los 3 listados de una sola vez
con la acción `obtenerReportes` y los muestra en 3 tablas:

- **Atrasos** (RE-01): entrada registrada después de las 09:30.
- **Salidas anticipadas** (RE-02): salida registrada antes de las 17:30.
- **Inasistencias** (RE-03): día con fila en `asistencia` pero sin entrada
  ni salida.

`ReporteDAO` filtra esto directo en SQL (mismas reglas que ya estaban
codificadas en `Modelo.Asistencia.EsAtraso() / EsSalidaAnticipada() /
EsInasistencia()`, solo que aquí se traducen a `WHERE` para no traer toda
la tabla a memoria).

## Tests: unitarios vs. de integración

- `AsistenciaTests.cs` y `ServiciosValidacionTests.cs` son **unitarios**:
  no tocan la base de datos, corren siempre y en cualquier máquina.
- `ReporteServicioIntegrationTests.cs` es de **integración**: sí necesita
  la base de datos real (LocalDB) con el script ya ejecutado, porque valida
  que `ReporteServicio` + `ReporteDAO` + SQL Server efectivamente devuelvan
  los 3 casos de prueba que trae el script (el atraso de Pedro, la salida
  anticipada de Maria y su inasistencia). Si la BD no está levantada, estos
  4 tests van a fallar por error de conexión — es esperable, hay que correr
  `script_bd_asistencia_sqlserver.sql` primero (ver SETUP_SQLSERVER.md).

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
- Manejo de errores de conexión a SQL Server más amigable en el HTML (hoy solo
  se ve el mensaje crudo de la excepción si la BD no está disponible).
- Filtrar los reportes por rango de fecha o por usuario (hoy `ReporteDAO`
  siempre trae el histórico completo).
