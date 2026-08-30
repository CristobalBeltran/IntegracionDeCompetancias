using Microsoft.Data.SqlClient;

namespace ProyectoAsistencia.Core.Util
{
    // Clase encargada de entregar la conexión hacia SQL Server (Express / LocalDB).
    //
    // Por defecto apunta a LocalDB, que es la instancia liviana de SQL Server
    // que Visual Studio instala con el workload "Almacenamiento y procesamiento
    // de datos". Si en tu máquina tienes SQL Server Express "de verdad"
    // (instancia con nombre SQLEXPRESS), comenta la línea de LocalDB y usa la
    // otra cadena de conexión que dejamos abajo como ejemplo.
    public static class ConexionBD
    {
        // Opción 1 (por defecto): LocalDB, la que trae Visual Studio.
        private const string CadenaConexion =
            @"Server=(localdb)\MSSQLLocalDB;Database=registro_asistencia;Trusted_Connection=True;TrustServerCertificate=True;";

        // Opción 2: SQL Server Express con instancia con nombre (descomenta si aplica).
        // private const string CadenaConexion =
        //     @"Server=.\SQLEXPRESS;Database=registro_asistencia;Trusted_Connection=True;TrustServerCertificate=True;";

        public static SqlConnection ObtenerConexion()
        {
            var conexion = new SqlConnection(CadenaConexion);
            conexion.Open();
            return conexion;
        }
    }
}
