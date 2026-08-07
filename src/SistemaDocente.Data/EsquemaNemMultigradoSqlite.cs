using Microsoft.Data.Sqlite;

using SistemaDocente.Core;

namespace SistemaDocente.Data;

internal static class EsquemaNemMultigradoSqlite
{
    internal const string NombreExtension = "nem-contexto-multigrado";
    internal const int VersionActual = 1;

    internal static void Inicializar(SqliteConnection conexion)
    {
        ArgumentNullException.ThrowIfNull(conexion);

        // The compatibility context table is owned by the reporting/context extension.
        EsquemaReportesSqlite.Inicializar(conexion);

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
                MigrarContextoLegacy(conexion, transaccion);
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

            CREATE TABLE IF NOT EXISTS contexto_nem_grupo (
                grupo_id TEXT NOT NULL PRIMARY KEY,
                organizacion_escolar INTEGER NOT NULL DEFAULT 0
                    CHECK (organizacion_escolar IN (0,1,2,3,4,5,6)),
                entidad_catalogo TEXT NOT NULL DEFAULT '',
                municipio_catalogo TEXT NOT NULL DEFAULT '',
                FOREIGN KEY (grupo_id) REFERENCES grupos(id) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS grados_grupo (
                grupo_id TEXT NOT NULL,
                grado INTEGER NOT NULL CHECK (grado BETWEEN 1 AND 6),
                PRIMARY KEY (grupo_id, grado),
                FOREIGN KEY (grupo_id) REFERENCES grupos(id) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS grados_estudiante (
                grupo_id TEXT NOT NULL,
                estudiante_id TEXT NOT NULL,
                grado INTEGER NOT NULL CHECK (grado BETWEEN 1 AND 6),
                PRIMARY KEY (grupo_id, estudiante_id),
                FOREIGN KEY (estudiante_id, grupo_id)
                    REFERENCES estudiantes(id, grupo_id) ON DELETE CASCADE
            );

            CREATE INDEX IF NOT EXISTS ix_grados_estudiante_grupo_grado
                ON grados_estudiante(grupo_id, grado);
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
        return valor is null
            ? 0
            : Convert.ToInt32(valor, System.Globalization.CultureInfo.InvariantCulture);
    }

    private static void MigrarContextoLegacy(SqliteConnection conexion, SqliteTransaction transaccion)
    {
        using (var copiarContexto = conexion.CreateCommand())
        {
            copiarContexto.Transaction = transaccion;
            copiarContexto.CommandText = """
                INSERT OR IGNORE INTO contexto_nem_grupo (
                    grupo_id, organizacion_escolar, entidad_catalogo, municipio_catalogo)
                SELECT grupo_id, 0, entidad_federativa, municipio
                FROM configuracion_grupo;
                """;
            copiarContexto.ExecuteNonQuery();
        }

        var candidatos = new List<(string GrupoId, string Grado)>();
        using (var leer = conexion.CreateCommand())
        {
            leer.Transaction = transaccion;
            leer.CommandText = "SELECT grupo_id, grado FROM configuracion_grupo;";
            using var lector = leer.ExecuteReader();
            while (lector.Read())
            {
                candidatos.Add((lector.GetString(0), lector.GetString(1)));
            }
        }

        foreach (var candidato in candidatos)
        {
            if (!CatalogoNemPrimaria.TryParseGradoLegacy(candidato.Grado, out var grado))
            {
                continue;
            }

            using (var guardarGradoGrupo = conexion.CreateCommand())
            {
                guardarGradoGrupo.Transaction = transaccion;
                guardarGradoGrupo.CommandText = """
                    INSERT OR IGNORE INTO grados_grupo (grupo_id, grado)
                    VALUES ($grupo, $grado);
                    """;
                guardarGradoGrupo.Parameters.AddWithValue("$grupo", candidato.GrupoId);
                guardarGradoGrupo.Parameters.AddWithValue("$grado", (int)grado);
                guardarGradoGrupo.ExecuteNonQuery();
            }

            using var guardarEstudiantes = conexion.CreateCommand();
            guardarEstudiantes.Transaction = transaccion;
            guardarEstudiantes.CommandText = """
                INSERT OR IGNORE INTO grados_estudiante (grupo_id, estudiante_id, grado)
                SELECT grupo_id, id, $grado
                FROM estudiantes
                WHERE grupo_id = $grupo;
                """;
            guardarEstudiantes.Parameters.AddWithValue("$grupo", candidato.GrupoId);
            guardarEstudiantes.Parameters.AddWithValue("$grado", (int)grado);
            guardarEstudiantes.ExecuteNonQuery();
        }
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