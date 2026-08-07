using Microsoft.Data.Sqlite;

namespace SistemaDocente.Data;

internal static class EsquemaPlaneacionNemSqlite
{
    internal const string NombreExtension = "nem-planeacion-proyectos";
    internal const int VersionActual = 1;

    internal static void Inicializar(SqliteConnection conexion)
    {
        ArgumentNullException.ThrowIfNull(conexion);

        using var transaccion = conexion.BeginTransaction();
        try
        {
            CrearTablas(conexion, transaccion);
            var version = LeerVersion(conexion, transaccion);
            if (version > VersionActual)
            {
                throw new SchemaIncompatibleException(
                    $"La extensión '{NombreExtension}' tiene una versión no compatible: {version}.");
            }

            if (version == 0)
            {
                MigrarRegistrosLegacy(conexion, transaccion);
                EstablecerVersion(conexion, transaccion, VersionActual);
            }

            transaccion.Commit();
        }
        catch
        {
            transaccion.Rollback();
            throw;
        }
    }

    private static void CrearTablas(SqliteConnection conexion, SqliteTransaction transaccion)
    {
        using var comando = conexion.CreateCommand();
        comando.Transaction = transaccion;
        comando.CommandText = """
            CREATE TABLE IF NOT EXISTS esquema_extensiones (
                nombre TEXT NOT NULL PRIMARY KEY,
                version INTEGER NOT NULL CHECK (version > 0)
            );

            CREATE TABLE IF NOT EXISTS proyectos_nem (
                proyecto_id TEXT NOT NULL PRIMARY KEY,
                metodologia INTEGER NOT NULL DEFAULT 0
                    CHECK (metodologia BETWEEN 0 AND 4),
                FOREIGN KEY (proyecto_id)
                    REFERENCES proyectos_didacticos(proyecto_id) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS grados_proyecto (
                proyecto_id TEXT NOT NULL,
                grado INTEGER NOT NULL CHECK (grado BETWEEN 1 AND 6),
                PRIMARY KEY (proyecto_id, grado),
                FOREIGN KEY (proyecto_id)
                    REFERENCES proyectos_didacticos(proyecto_id) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS actividades_nem (
                actividad_id TEXT NOT NULL PRIMARY KEY,
                campo_formativo INTEGER NOT NULL DEFAULT 0
                    CHECK (campo_formativo BETWEEN 0 AND 4),
                FOREIGN KEY (actividad_id)
                    REFERENCES actividades_proyecto(actividad_id) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS grados_actividad (
                actividad_id TEXT NOT NULL,
                grado INTEGER NOT NULL CHECK (grado BETWEEN 1 AND 6),
                PRIMARY KEY (actividad_id, grado),
                FOREIGN KEY (actividad_id)
                    REFERENCES actividades_proyecto(actividad_id) ON DELETE CASCADE
            );

            CREATE INDEX IF NOT EXISTS ix_grados_proyecto_grado
                ON grados_proyecto(grado, proyecto_id);

            CREATE INDEX IF NOT EXISTS ix_grados_actividad_grado
                ON grados_actividad(grado, actividad_id);
            """;
        comando.ExecuteNonQuery();
    }

    private static int LeerVersion(SqliteConnection conexion, SqliteTransaction transaccion)
    {
        using var comando = conexion.CreateCommand();
        comando.Transaction = transaccion;
        comando.CommandText = "SELECT version FROM esquema_extensiones WHERE nombre=$nombre;";
        comando.Parameters.AddWithValue("$nombre", NombreExtension);
        var valor = comando.ExecuteScalar();
        return valor is null
            ? 0
            : Convert.ToInt32(valor, System.Globalization.CultureInfo.InvariantCulture);
    }

    private static void MigrarRegistrosLegacy(
        SqliteConnection conexion,
        SqliteTransaction transaccion)
    {
        using var comando = conexion.CreateCommand();
        comando.Transaction = transaccion;
        comando.CommandText = """
            INSERT OR IGNORE INTO proyectos_nem (proyecto_id, metodologia)
            SELECT proyecto_id, 0
            FROM proyectos_didacticos;

            INSERT OR IGNORE INTO actividades_nem (actividad_id, campo_formativo)
            SELECT actividad_id, 0
            FROM actividades_proyecto;
            """;
        comando.ExecuteNonQuery();
    }

    private static void EstablecerVersion(
        SqliteConnection conexion,
        SqliteTransaction transaccion,
        int version)
    {
        using var comando = conexion.CreateCommand();
        comando.Transaction = transaccion;
        comando.CommandText = """
            INSERT INTO esquema_extensiones (nombre, version)
            VALUES ($nombre, $version)
            ON CONFLICT(nombre) DO UPDATE SET version=excluded.version;
            """;
        comando.Parameters.AddWithValue("$nombre", NombreExtension);
        comando.Parameters.AddWithValue("$version", version);
        comando.ExecuteNonQuery();
    }
}