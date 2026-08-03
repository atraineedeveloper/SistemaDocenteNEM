using System.Text;

using Microsoft.Data.Sqlite;

namespace SistemaDocente.Data;

internal static class EsquemaSqlite
{
    internal const int VersionActual = 2;

    private const string TablaGrupos = """
        CREATE TABLE grupos (
            id TEXT NOT NULL PRIMARY KEY,
            nombre TEXT NOT NULL CHECK (length(trim(nombre)) BETWEEN 1 AND 100)
        )
        """;

    private const string TablaEstudiantes = """
        CREATE TABLE estudiantes (
            id TEXT NOT NULL PRIMARY KEY,
            grupo_id TEXT NOT NULL,
            nombre TEXT NOT NULL CHECK (length(trim(nombre)) BETWEEN 1 AND 150),
            numero_lista INTEGER NOT NULL CHECK (numero_lista > 0),
            activo INTEGER NOT NULL CHECK (activo IN (0, 1)),
            FOREIGN KEY (grupo_id) REFERENCES grupos(id) ON DELETE RESTRICT
        )
        """;

    private const string IndiceGrupo = """
        CREATE INDEX ix_estudiantes_grupo_id
        ON estudiantes(grupo_id)
        """;

    private const string IndiceNumeroActivo = """
        CREATE UNIQUE INDEX ux_estudiantes_grupo_numero_activo
        ON estudiantes(grupo_id, numero_lista)
        WHERE activo = 1
        """;

    private const string IndicePertenencia = """
        CREATE UNIQUE INDEX ux_estudiantes_id_grupo_id
        ON estudiantes(id, grupo_id)
        """;

    private const string TablaAsistencias = """
        CREATE TABLE asistencias_diarias (
            grupo_id TEXT NOT NULL,
            fecha TEXT NOT NULL CHECK (
                length(fecha) = 10
                AND fecha GLOB '[0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9]'
                AND CAST(substr(fecha, 6, 2) AS INTEGER) BETWEEN 1 AND 12
                AND CAST(substr(fecha, 9, 2) AS INTEGER) BETWEEN 1 AND CASE
                    WHEN CAST(substr(fecha, 6, 2) AS INTEGER) IN (1, 3, 5, 7, 8, 10, 12) THEN 31
                    WHEN CAST(substr(fecha, 6, 2) AS INTEGER) IN (4, 6, 9, 11) THEN 30
                    WHEN CAST(substr(fecha, 1, 4) AS INTEGER) % 400 = 0
                      OR (CAST(substr(fecha, 1, 4) AS INTEGER) % 4 = 0
                          AND CAST(substr(fecha, 1, 4) AS INTEGER) % 100 <> 0) THEN 29
                    ELSE 28
                END),
            PRIMARY KEY (grupo_id, fecha),
            FOREIGN KEY (grupo_id) REFERENCES grupos(id) ON DELETE RESTRICT
        )
        """;

    private const string TablaRegistros = """
        CREATE TABLE registros_asistencia (
            grupo_id TEXT NOT NULL,
            fecha TEXT NOT NULL,
            estudiante_id TEXT NOT NULL,
            estado INTEGER NOT NULL CHECK (estado IN (0, 1, 2, 3)),
            PRIMARY KEY (grupo_id, fecha, estudiante_id),
            FOREIGN KEY (grupo_id, fecha)
                REFERENCES asistencias_diarias(grupo_id, fecha) ON DELETE RESTRICT,
            FOREIGN KEY (estudiante_id, grupo_id)
                REFERENCES estudiantes(id, grupo_id) ON DELETE RESTRICT
        )
        """;

    private const string IndiceAsistenciasGrupoFecha = """
        CREATE INDEX ix_asistencias_diarias_grupo_fecha
        ON asistencias_diarias(grupo_id, fecha)
        """;

    private const string IndiceRegistrosEstudiante = """
        CREATE INDEX ix_registros_asistencia_estudiante_id
        ON registros_asistencia(estudiante_id)
        """;

    private static readonly IReadOnlyDictionary<string, string> ObjetosVersionUno =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["grupos"] = TablaGrupos,
            ["estudiantes"] = TablaEstudiantes,
            ["ix_estudiantes_grupo_id"] = IndiceGrupo,
            ["ux_estudiantes_grupo_numero_activo"] = IndiceNumeroActivo,
        };

    private static readonly IReadOnlyDictionary<string, string> ObjetosAsistencia =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["ux_estudiantes_id_grupo_id"] = IndicePertenencia,
            ["asistencias_diarias"] = TablaAsistencias,
            ["registros_asistencia"] = TablaRegistros,
            ["ix_asistencias_diarias_grupo_fecha"] = IndiceAsistenciasGrupoFecha,
            ["ix_registros_asistencia_estudiante_id"] = IndiceRegistrosEstudiante,
        };

    internal static void Inicializar(SqliteConnection conexion)
    {
        var version = LeerVersion(conexion);

        if (version == 0)
        {
            if (!EstaVacia(conexion))
            {
                throw new SchemaIncompatibleException(
                    "Una base sin versión no puede contener objetos preexistentes.");
            }

            CrearVersionDos(conexion);
            return;
        }

        if (version == 1)
        {
            ValidarObjetos(conexion, ObjetosVersionUno);
            MigrarVersionUno(conexion);
            return;
        }

        if (version != VersionActual)
        {
            throw new SchemaIncompatibleException(
                $"La versión de esquema {version} no es compatible con la versión {VersionActual}.");
        }

        ValidarVersionDos(conexion);
    }

    private static long LeerVersion(SqliteConnection conexion)
    {
        using var comando = conexion.CreateCommand();
        comando.CommandText = "PRAGMA user_version;";
        return (long)(comando.ExecuteScalar() ?? 0L);
    }

    private static bool EstaVacia(SqliteConnection conexion)
    {
        using var comando = conexion.CreateCommand();
        comando.CommandText = """
            SELECT COUNT(*)
            FROM sqlite_master
            WHERE name NOT LIKE 'sqlite_%'
              AND type IN ('table', 'index', 'view', 'trigger');
            """;
        return (long)(comando.ExecuteScalar() ?? 0L) == 0;
    }

    private static void CrearVersionDos(SqliteConnection conexion)
    {
        using var transaccion = conexion.BeginTransaction();
        CrearObjetos(conexion, transaccion, ObjetosVersionUno.Values);
        CrearObjetos(conexion, transaccion, ObjetosAsistencia.Values);
        EstablecerVersion(conexion, transaccion, VersionActual);
        transaccion.Commit();
    }

    private static void MigrarVersionUno(SqliteConnection conexion)
    {
        using var transaccion = conexion.BeginTransaction();

        try
        {
            CrearObjetos(conexion, transaccion, ObjetosAsistencia.Values);
            ValidarObjetos(conexion, ObjetosAsistencia, transaccion);
            EstablecerVersion(conexion, transaccion, VersionActual);
            transaccion.Commit();
        }
        catch
        {
            transaccion.Rollback();
            throw;
        }
    }

    private static void ValidarVersionDos(SqliteConnection conexion)
    {
        ValidarObjetos(conexion, ObjetosVersionUno);
        ValidarObjetos(conexion, ObjetosAsistencia);
    }

    private static void CrearObjetos(
        SqliteConnection conexion,
        SqliteTransaction transaccion,
        IEnumerable<string> instrucciones)
    {
        foreach (var sql in instrucciones)
        {
            using var comando = conexion.CreateCommand();
            comando.Transaction = transaccion;
            comando.CommandText = sql;
            comando.ExecuteNonQuery();
        }
    }

    private static void EstablecerVersion(
        SqliteConnection conexion,
        SqliteTransaction transaccion,
        int version)
    {
        using var comando = conexion.CreateCommand();
        comando.Transaction = transaccion;
        comando.CommandText = $"PRAGMA user_version = {version};";
        comando.ExecuteNonQuery();
    }

    private static void ValidarObjetos(
        SqliteConnection conexion,
        IReadOnlyDictionary<string, string> esperados,
        SqliteTransaction? transaccion = null)
    {
        using var comando = conexion.CreateCommand();
        comando.Transaction = transaccion;
        var parametros = esperados.Keys.Select((_, indice) => $"$nombre{indice}").ToArray();
        comando.CommandText = $"""
            SELECT name, sql
            FROM sqlite_master
            WHERE name IN ({string.Join(", ", parametros)});
            """;

        var indiceParametro = 0;
        foreach (var nombre in esperados.Keys)
        {
            comando.Parameters.AddWithValue(parametros[indiceParametro++], nombre);
        }

        var encontrados = new Dictionary<string, string>(StringComparer.Ordinal);
        using var lector = comando.ExecuteReader();

        while (lector.Read())
        {
            encontrados.Add(lector.GetString(0), lector.GetString(1));
        }

        foreach (var esperado in esperados)
        {
            if (!encontrados.TryGetValue(esperado.Key, out var sqlEncontrado)
                || !string.Equals(
                    NormalizarSql(esperado.Value),
                    NormalizarSql(sqlEncontrado),
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new SchemaIncompatibleException(
                    $"El objeto de esquema '{esperado.Key}' no es compatible.");
            }
        }
    }

    private static string NormalizarSql(string sql)
    {
        var resultado = new StringBuilder(sql.Length);
        var espacioPendiente = false;

        foreach (var caracter in sql.Trim().TrimEnd(';'))
        {
            if (char.IsWhiteSpace(caracter))
            {
                espacioPendiente = resultado.Length > 0;
                continue;
            }

            if (espacioPendiente)
            {
                resultado.Append(' ');
                espacioPendiente = false;
            }

            resultado.Append(caracter);
        }

        return resultado.ToString();
    }
}