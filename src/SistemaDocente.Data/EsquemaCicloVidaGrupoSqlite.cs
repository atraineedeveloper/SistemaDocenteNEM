using Microsoft.Data.Sqlite;

namespace SistemaDocente.Data;

internal static class EsquemaCicloVidaGrupoSqlite
{
    internal const string NombreExtension = "group-lifecycle";
    internal const int VersionActual = 1;

    internal static void Inicializar(SqliteConnection conexion)
    {
        ArgumentNullException.ThrowIfNull(conexion);

        using var transaccion = conexion.BeginTransaction();
        try
        {
            using (var comando = conexion.CreateCommand())
            {
                comando.Transaction = transaccion;
                comando.CommandText = """
                    CREATE TABLE IF NOT EXISTS esquema_extensiones (
                        nombre TEXT NOT NULL PRIMARY KEY,
                        version INTEGER NOT NULL CHECK (version > 0)
                    );

                    CREATE TABLE IF NOT EXISTS ciclo_vida_grupo (
                        grupo_id TEXT NOT NULL PRIMARY KEY,
                        archivado INTEGER NOT NULL DEFAULT 0 CHECK (archivado IN (0,1)),
                        FOREIGN KEY (grupo_id) REFERENCES grupos(id) ON DELETE CASCADE
                    );
                    """;
                comando.ExecuteNonQuery();
            }

            var version = LeerVersion(conexion, transaccion);
            if (version > VersionActual)
            {
                throw new SchemaIncompatibleException(
                    $"La extensión '{NombreExtension}' tiene una versión no compatible: {version}.");
            }

            using (var completar = conexion.CreateCommand())
            {
                completar.Transaction = transaccion;
                completar.CommandText = """
                    INSERT OR IGNORE INTO ciclo_vida_grupo (grupo_id, archivado)
                    SELECT id, 0 FROM grupos;
                    """;
                completar.ExecuteNonQuery();
            }

            if (version == 0)
            {
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

    private static int LeerVersion(SqliteConnection conexion, SqliteTransaction transaccion)
    {
        using var comando = conexion.CreateCommand();
        comando.Transaction = transaccion;
        comando.CommandText = "SELECT version FROM esquema_extensiones WHERE nombre = $nombre;";
        comando.Parameters.AddWithValue("$nombre", NombreExtension);
        var valor = comando.ExecuteScalar();
        return valor is null
            ? 0
            : Convert.ToInt32(valor, System.Globalization.CultureInfo.InvariantCulture);
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
