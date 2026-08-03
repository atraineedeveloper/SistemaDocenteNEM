using System.Text;

using Microsoft.Data.Sqlite;

namespace SistemaDocente.Data;

internal static class EsquemaSqlite
{
    internal const int VersionActual = 1;

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

    private static readonly IReadOnlyDictionary<string, string> ObjetosEsperados =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["grupos"] = TablaGrupos,
            ["estudiantes"] = TablaEstudiantes,
            ["ix_estudiantes_grupo_id"] = IndiceGrupo,
            ["ux_estudiantes_grupo_numero_activo"] = IndiceNumeroActivo,
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

            CrearVersionUno(conexion);
            return;
        }

        if (version != VersionActual)
        {
            throw new SchemaIncompatibleException(
                $"La versión de esquema {version} no es compatible con la versión {VersionActual}.");
        }

        ValidarVersionUno(conexion);
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

    private static void CrearVersionUno(SqliteConnection conexion)
    {
        using var transaccion = conexion.BeginTransaction();

        foreach (var sql in ObjetosEsperados.Values)
        {
            using var comando = conexion.CreateCommand();
            comando.Transaction = transaccion;
            comando.CommandText = sql;
            comando.ExecuteNonQuery();
        }

        using (var comandoVersion = conexion.CreateCommand())
        {
            comandoVersion.Transaction = transaccion;
            comandoVersion.CommandText = $"PRAGMA user_version = {VersionActual};";
            comandoVersion.ExecuteNonQuery();
        }

        transaccion.Commit();
    }

    private static void ValidarVersionUno(SqliteConnection conexion)
    {
        using var comando = conexion.CreateCommand();
        comando.CommandText = """
            SELECT name, sql
            FROM sqlite_master
            WHERE name IN (
                'grupos',
                'estudiantes',
                'ix_estudiantes_grupo_id',
                'ux_estudiantes_grupo_numero_activo');
            """;

        var encontrados = new Dictionary<string, string>(StringComparer.Ordinal);
        using var lector = comando.ExecuteReader();

        while (lector.Read())
        {
            encontrados.Add(lector.GetString(0), lector.GetString(1));
        }

        foreach (var esperado in ObjetosEsperados)
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