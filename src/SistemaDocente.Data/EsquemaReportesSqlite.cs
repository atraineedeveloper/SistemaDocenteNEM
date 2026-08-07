using Microsoft.Data.Sqlite;

namespace SistemaDocente.Data;

/// <summary>
/// Additive extension of the base schema. It keeps the core PRAGMA user_version unchanged
/// for already validated v6 databases and versions this capability independently.
/// </summary>
internal static class EsquemaReportesSqlite
{
    private const string NombreExtension = "reportes-contexto-entregas";
    private const int VersionActual = 1;

    internal static void Inicializar(SqliteConnection conexion)
    {
        ArgumentNullException.ThrowIfNull(conexion);
        using var transaccion = conexion.BeginTransaction();
        try
        {
            CrearMeta(conexion, transaccion);
            var version = LeerVersion(conexion, transaccion);
            if (version > VersionActual)
            {
                throw new SchemaIncompatibleException(
                    $"La extensión '{NombreExtension}' tiene una versión no compatible: {version}.");
            }

            if (version == 0)
            {
                CrearVersionUno(conexion, transaccion);
                EstablecerVersion(conexion, transaccion, VersionActual);
            }

            // Keep the compatibility bridge self-healing. An older application can still
            // write legacy NivelLogro.NoEntrego (5) because the base user_version remains 6.
            // Reconciliation is idempotent and prevents those writes from surviving once a
            // current build opens the database again.
            ReconciliarEntregasLegacy(conexion, transaccion);

            transaccion.Commit();
        }
        catch
        {
            transaccion.Rollback();
            throw;
        }
    }

    private static void CrearMeta(SqliteConnection conexion, SqliteTransaction transaccion)
    {
        using var comando = conexion.CreateCommand();
        comando.Transaction = transaccion;
        comando.CommandText = """
            CREATE TABLE IF NOT EXISTS esquema_extensiones (
                nombre TEXT NOT NULL PRIMARY KEY,
                version INTEGER NOT NULL CHECK (version > 0)
            );
            """;
        comando.ExecuteNonQuery();
    }

    private static int LeerVersion(SqliteConnection conexion, SqliteTransaction transaccion)
    {
        using var comando = conexion.CreateCommand();
        comando.Transaction = transaccion;
        comando.CommandText = "SELECT version FROM esquema_extensiones WHERE nombre = $nombre;";
        comando.Parameters.AddWithValue("$nombre", NombreExtension);
        var valor = comando.ExecuteScalar();
        return valor is null ? 0 : Convert.ToInt32(valor, System.Globalization.CultureInfo.InvariantCulture);
    }

    private static void CrearVersionUno(SqliteConnection conexion, SqliteTransaction transaccion)
    {
        using var comando = conexion.CreateCommand();
        comando.Transaction = transaccion;
        comando.CommandText = """
            CREATE TABLE IF NOT EXISTS configuracion_grupo (
                grupo_id TEXT NOT NULL PRIMARY KEY,
                ciclo_escolar TEXT NOT NULL DEFAULT '',
                nombre_escuela TEXT NOT NULL DEFAULT '',
                cct TEXT NOT NULL DEFAULT '',
                entidad_federativa TEXT NOT NULL DEFAULT '',
                municipio TEXT NOT NULL DEFAULT '',
                localidad TEXT NOT NULL DEFAULT '',
                grado TEXT NOT NULL DEFAULT '',
                grupo TEXT NOT NULL DEFAULT '',
                turno TEXT NOT NULL DEFAULT '',
                etapa_cognoscitiva INTEGER NOT NULL DEFAULT 0 CHECK (etapa_cognoscitiva IN (0,1,2,3,4)),
                docente_responsable TEXT NOT NULL DEFAULT '',
                responsable_desde TEXT,
                responsable_hasta TEXT,
                hora_entrada TEXT,
                hora_salida TEXT,
                FOREIGN KEY (grupo_id) REFERENCES grupos(id) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS estados_entrega_actividad (
                actividad_id TEXT NOT NULL,
                estudiante_id TEXT NOT NULL,
                estado_entrega INTEGER NOT NULL CHECK (estado_entrega IN (0,1,2)),
                PRIMARY KEY (actividad_id, estudiante_id),
                FOREIGN KEY (actividad_id, estudiante_id)
                    REFERENCES entregas_actividad(actividad_id, estudiante_id) ON DELETE CASCADE
            );
            """;
        comando.ExecuteNonQuery();
    }

    private static void ReconciliarEntregasLegacy(
        SqliteConnection conexion,
        SqliteTransaction transaccion)
    {
        using var comando = conexion.CreateCommand();
        comando.Transaction = transaccion;
        comando.CommandText = """
            INSERT OR IGNORE INTO estados_entrega_actividad (actividad_id, estudiante_id, estado_entrega)
            SELECT actividad_id,
                   estudiante_id,
                   CASE estado_entrega
                       WHEN 5 THEN 2
                       WHEN 0 THEN 0
                       ELSE 1
                   END
            FROM entregas_actividad;

            UPDATE estados_entrega_actividad
            SET estado_entrega = 2
            WHERE EXISTS (
                SELECT 1
                FROM entregas_actividad e
                WHERE e.actividad_id = estados_entrega_actividad.actividad_id
                  AND e.estudiante_id = estados_entrega_actividad.estudiante_id
                  AND e.estado_entrega = 5
            );

            UPDATE entregas_actividad
            SET estado_entrega = 0
            WHERE estado_entrega = 5;
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
            ON CONFLICT(nombre) DO UPDATE SET version = excluded.version;
            """;
        comando.Parameters.AddWithValue("$nombre", NombreExtension);
        comando.Parameters.AddWithValue("$version", version);
        comando.ExecuteNonQuery();
    }
}