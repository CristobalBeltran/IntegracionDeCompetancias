# Cómo dejar el proyecto corriendo con SQL Server Express (Visual Studio)

Este proyecto originalmente estaba armado para MySQL. Ya quedó adaptado para
usar **SQL Server**, usando por defecto **LocalDB** (la instancia liviana de
SQL Server Express que se instala junto con Visual Studio).

## 1. Asegúrate de tener LocalDB instalado

1. Abre **Visual Studio Installer** (buscarlo en el menú de inicio de Windows).
2. Click en **Modificar** sobre tu instalación de Visual Studio 2022.
3. En la pestaña **Cargas de trabajo**, marca **"Almacenamiento y
   procesamiento de datos"** (o revisa en "Componentes individuales" que esté
   marcado **"SQL Server Express LocalDB"**).
4. Click en **Modificar** para instalar. Con esto ya tienes SQL Server Express
   (LocalDB) disponible, sin instalar nada aparte.

Si prefieres usar una instancia "completa" de SQL Server Express (no LocalDB),
instálala desde el sitio de Microsoft y luego cambia la cadena de conexión en
`ProyectoAsistencia.Core/Util/ConexionBD.cs` a la opción 2 que está comentada
ahí mismo (`Server=.\SQLEXPRESS`).

## 2. Crear la base de datos

1. En Visual Studio, abre el menú **Ver > SQL Server Object Explorer**
   (o "SQL Server Object Explorer" en el menú Vista).
2. Click derecho en **SQL Server** > **Agregar SQL Server...**
3. Como nombre del servidor escribe: `(localdb)\MSSQLLocalDB`
   Autenticación: **Windows Authentication**. Click en **Conectar**.
4. Ya conectado, click derecho sobre esa instancia > **Nueva consulta**.
5. Abre el archivo `script_bd_asistencia_sqlserver.sql` (está en la raíz del
   proyecto), copia todo su contenido y pégalo en la consulta nueva.
6. Ejecuta el script completo (botón ▷ Ejecutar, o F5 dentro de esa ventana de
   consulta). Esto crea la base `registro_asistencia`, sus tablas y los datos
   de prueba.

## 3. Verificar la cadena de conexión

`ProyectoAsistencia.Core/Util/ConexionBD.cs` ya viene configurado por defecto
para `(localdb)\MSSQLLocalDB`, así que si seguiste el paso 2 tal cual, no
necesitas tocar nada.

## 4. Correr el proyecto

1. Click derecho sobre **ProyectoAsistencia.Desktop** en el Explorador de
   soluciones > **"Establecer como proyecto de inicio"**.
2. Presiona **F5**.
3. Prueba iniciar sesión con alguno de estos usuarios de prueba (vienen del
   script):

   | Email                     | Password     | Rol           |
   |---------------------------|--------------|---------------|
   | ana.soto@empresa.cl       | admin1234    | ADMINISTRADOR |
   | pedro.rojas@empresa.cl    | pedro1234    | EMPLEADO      |
   | maria.diaz@empresa.cl     | maria1234    | EMPLEADO      |

## 5. Ver los registros que se van generando

Cada vez que un empleado marca entrada/salida desde la app, el registro cae en
la tabla `asistencia`. Para verlo:

- Dentro de Visual Studio: **SQL Server Object Explorer** > tu servidor >
  **Databases > registro_asistencia > Tables**. Click derecho en la tabla
  `asistencia` (o `usuarios`, `reportes`) > **View Data**. Esa vista se
  refresca si cierras y vuelves a abrir "View Data" (no es en vivo).
- También puedes hacer click derecho sobre la base `registro_asistencia` >
  **New Query** y correr, por ejemplo:

  ```sql
  SELECT * FROM asistencia ORDER BY fecha_registro DESC;
  ```

- Si tienes **SQL Server Management Studio (SSMS)** instalado aparte (ese es
  el programa con el logo rojo/anaranjado, distinto de Visual Studio), puedes
  conectarte ahí también con el mismo nombre de servidor `(localdb)\MSSQLLocalDB`
  y navegar las tablas igual.

## Qué cambió en el código (por si te preguntan en la entrega)

- `ConexionBD.cs`: ahora usa `Microsoft.Data.SqlClient` en vez de
  `MySql.Data.MySqlClient`, con la cadena de conexión apuntando a LocalDB.
- `ProyectoAsistencia.Core.csproj`: el paquete NuGet cambió de `MySql.Data` a
  `Microsoft.Data.SqlClient`.
- `UsuarioDAO.cs`: mismo SQL, pero usando `SqlCommand`/`SqlDataReader`. Se
  agregó un método `LeerUsuario` porque `SqlDataReader` no tiene los métodos
  `GetInt32("columna")` por nombre que sí trae el driver de MySQL; hay que
  resolver primero el índice de la columna con `GetOrdinal`.
- `AsistenciaDAO.cs`: SQL Server no tiene `ON DUPLICATE KEY UPDATE` (eso es
  específico de MySQL), así que se reemplazó por `IF EXISTS (...) UPDATE ...
  ELSE INSERT ...`, que hace exactamente lo mismo.
- `script_bd_asistencia_sqlserver.sql` (nuevo archivo): mismo modelo de datos
  que el script de MySQL, pero en T-SQL (`IDENTITY` en vez de
  `AUTO_INCREMENT`, `CHECK` en vez de `ENUM`, `HASHBYTES` en vez de `SHA2`,
  etc.). El script original (`script_bd_asistencia.txt`) sigue ahí por si
  algún día vuelven a MySQL, pero ya no se usa.
