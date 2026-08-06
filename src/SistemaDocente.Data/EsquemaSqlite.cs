using System.Text;

using Microsoft.Data.Sqlite;

namespace SistemaDocente.Data;

internal static class EsquemaSqlite
{
    internal const int VersionActual = 6;

    private const string TablaNotasPedagogicas = """
        CREATE TABLE notas_pedagogicas_estudiantes (
            nota_id TEXT NOT NULL PRIMARY KEY,
            estudiante_id TEXT NOT NULL,
            grupo_id TEXT NOT NULL,
            tipo INTEGER NOT NULL CHECK (tipo IN (0, 1, 2, 3)),
            contenido TEXT NOT NULL CHECK (length(trim(contenido)) > 0),
            fecha_hora_registro TEXT NOT NULL,
            FOREIGN KEY (estudiante_id, grupo_id) REFERENCES estudiantes(id, grupo_id) ON DELETE CASCADE
        )
        """;

    private const string TablaAcuerdosTutores = """
        CREATE TABLE acuerdos_tutores_estudiantes (
            acuerdo_id TEXT NOT NULL PRIMARY KEY,
            estudiante_id TEXT NOT NULL,
            grupo_id TEXT NOT NULL,
            motivo TEXT NOT NULL CHECK (length(trim(motivo)) > 0),
            acuerdo_convenido TEXT NOT NULL CHECK (length(trim(acuerdo_convenido)) > 0),
            fecha_reunion TEXT NOT NULL,
            fecha_seguimiento TEXT,
            FOREIGN KEY (estudiante_id, grupo_id) REFERENCES estudiantes(id, grupo_id) ON DELETE CASCADE
        )
        """;

    private const string IndiceNotasEstudiante = "CREATE INDEX ix_notas_pedagogicas_estudiante ON notas_pedagogicas_estudiantes(estudiante_id, tipo)";
    private const string IndiceAcuerdosEstudiante = "CREATE INDEX ix_acuerdos_tutores_estudiante ON acuerdos_tutores_estudiantes(estudiante_id)";

    private static readonly IReadOnlyDictionary<string, string> ObjetosExpedientes =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["notas_pedagogicas_estudiantes"] = TablaNotasPedagogicas,
            ["acuerdos_tutores_estudiantes"] = TablaAcuerdosTutores,
            ["ix_notas_pedagogicas_estudiante"] = IndiceNotasEstudiante,
            ["ix_acuerdos_tutores_estudiante"] = IndiceAcuerdosEstudiante,
        };

    private const string TablaGrupos = """
        CREATE TABLE grupos (
            id TEXT NOT NULL PRIMARY KEY,
            nombre TEXT NOT NULL CHECK (length(trim(nombre)) BETWEEN 1 AND 100)
        )
        """;

    private const string TablaEstudiantesV1 = """
        CREATE TABLE estudiantes (
            id TEXT NOT NULL PRIMARY KEY,
            grupo_id TEXT NOT NULL,
            nombre TEXT NOT NULL CHECK (length(trim(nombre)) BETWEEN 1 AND 150),
            numero_lista INTEGER NOT NULL CHECK (numero_lista > 0),
            activo INTEGER NOT NULL CHECK (activo IN (0, 1)),
            FOREIGN KEY (grupo_id) REFERENCES grupos(id) ON DELETE RESTRICT
        )
        """;

    private const string TablaEstudiantes = """
        CREATE TABLE estudiantes (
            id TEXT NOT NULL,
            grupo_id TEXT NOT NULL,
            nombre TEXT NOT NULL CHECK (length(trim(nombre)) BETWEEN 1 AND 150),
            primer_apellido TEXT NOT NULL DEFAULT '',
            segundo_apellido TEXT NOT NULL DEFAULT '',
            nombres TEXT NOT NULL DEFAULT '',
            fecha_nacimiento TEXT,
            genero INTEGER NOT NULL DEFAULT 0,
            fecha_ingreso TEXT,
            observaciones TEXT NOT NULL DEFAULT '',
            numero_lista INTEGER NOT NULL CHECK (numero_lista > 0),
            activo INTEGER NOT NULL CHECK (activo IN (0, 1)),
            PRIMARY KEY (id, grupo_id),
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

    private const string TablaProyectos = """
        CREATE TABLE proyectos_didacticos (
            proyecto_id TEXT NOT NULL PRIMARY KEY,
            grupo_id TEXT NOT NULL,
            nombre TEXT NOT NULL CHECK (length(trim(nombre)) BETWEEN 1 AND 150),
            descripcion TEXT NOT NULL CHECK (length(descripcion) <= 2000),
            fecha_inicio TEXT NOT NULL CHECK (length(fecha_inicio) = 10 AND fecha_inicio GLOB '[0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9]' AND date(fecha_inicio) = fecha_inicio),
            fecha_termino TEXT NOT NULL CHECK (length(fecha_termino) = 10 AND fecha_termino GLOB '[0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9]' AND date(fecha_termino) = fecha_termino AND fecha_inicio <= fecha_termino),
            estado INTEGER NOT NULL CHECK (estado IN (0, 1, 2)),
            observaciones TEXT NOT NULL CHECK (length(observaciones) <= 2000),
            version INTEGER NOT NULL CHECK (version > 0),
            UNIQUE (proyecto_id, grupo_id),
            FOREIGN KEY (grupo_id) REFERENCES grupos(id) ON DELETE RESTRICT
        )
        """;

    private const string TablaActividades = """
        CREATE TABLE actividades_proyecto (
            actividad_id TEXT NOT NULL PRIMARY KEY,
            proyecto_id TEXT NOT NULL,
            grupo_id TEXT NOT NULL,
            titulo TEXT NOT NULL CHECK (length(trim(titulo)) BETWEEN 1 AND 200),
            descripcion TEXT NOT NULL CHECK (length(descripcion) <= 2000),
            fecha_realizacion TEXT NOT NULL CHECK (length(fecha_realizacion) = 10 AND fecha_realizacion GLOB '[0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9]' AND date(fecha_realizacion) = fecha_realizacion),
            observaciones_generales TEXT NOT NULL CHECK (length(observaciones_generales) <= 2000),
            estado INTEGER NOT NULL CHECK (estado IN (0, 1)),
            version INTEGER NOT NULL CHECK (version > 0),
            UNIQUE (actividad_id, grupo_id),
            FOREIGN KEY (proyecto_id, grupo_id) REFERENCES proyectos_didacticos(proyecto_id, grupo_id) ON DELETE RESTRICT
        )
        """;

    private const string TablaEntregasV3 = """
        CREATE TABLE entregas_actividad (
            actividad_id TEXT NOT NULL,
            estudiante_id TEXT NOT NULL,
            grupo_id TEXT NOT NULL,
            estado_entrega INTEGER NOT NULL CHECK (estado_entrega IN (0, 1, 2)),
            observacion TEXT NOT NULL CHECK (length(observacion) <= 500),
            PRIMARY KEY (actividad_id, estudiante_id),
            FOREIGN KEY (actividad_id, grupo_id) REFERENCES actividades_proyecto(actividad_id, grupo_id) ON DELETE RESTRICT,
            FOREIGN KEY (estudiante_id, grupo_id) REFERENCES estudiantes(id, grupo_id) ON DELETE RESTRICT
        )
        """;

    private const string TablaEntregas = """
        CREATE TABLE entregas_actividad (
            actividad_id TEXT NOT NULL,
            estudiante_id TEXT NOT NULL,
            grupo_id TEXT NOT NULL,
            estado_entrega INTEGER NOT NULL CHECK (estado_entrega IN (0, 1, 2, 3, 4, 5)),
            observacion TEXT NOT NULL CHECK (length(observacion) <= 500),
            PRIMARY KEY (actividad_id, estudiante_id),
            FOREIGN KEY (actividad_id, grupo_id) REFERENCES actividades_proyecto(actividad_id, grupo_id) ON DELETE RESTRICT,
            FOREIGN KEY (estudiante_id, grupo_id) REFERENCES estudiantes(id, grupo_id) ON DELETE RESTRICT
        )
        """;

    private const string IndiceProyectos = "CREATE INDEX ix_proyectos_grupo_estado_fecha ON proyectos_didacticos(grupo_id, estado, fecha_inicio)";
    private const string IndiceActividades = "CREATE INDEX ix_actividades_proyecto_fecha ON actividades_proyecto(proyecto_id, fecha_realizacion)";
    private const string IndiceEntregas = "CREATE INDEX ix_entregas_estudiante ON entregas_actividad(estudiante_id)";

    private static readonly IReadOnlyDictionary<string, string> ObjetosVersionSeis =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["grupos"] = TablaGrupos,
            ["estudiantes"] = TablaEstudiantes,
            ["ix_estudiantes_grupo_id"] = IndiceGrupo,
            ["ux_estudiantes_grupo_numero_activo"] = IndiceNumeroActivo,
            ["ux_estudiantes_id_grupo_id"] = IndicePertenencia,
            ["asistencias_diarias"] = TablaAsistencias,
            ["registros_asistencia"] = TablaRegistros,
            ["ix_asistencias_diarias_grupo_fecha"] = IndiceAsistenciasGrupoFecha,
            ["ix_registros_asistencia_estudiante_id"] = IndiceRegistrosEstudiante,
            ["proyectos_didacticos"] = TablaProyectos,
            ["actividades_proyecto"] = TablaActividades,
            ["entregas_actividad"] = TablaEntregas,
            ["ix_proyectos_grupo_estado_fecha"] = IndiceProyectos,
            ["ix_actividades_proyecto_fecha"] = IndiceActividades,
            ["ix_entregas_estudiante"] = IndiceEntregas,
            ["notas_pedagogicas_estudiantes"] = TablaNotasPedagogicas,
            ["acuerdos_tutores_estudiantes"] = TablaAcuerdosTutores,
            ["ix_notas_pedagogicas_estudiante"] = IndiceNotasEstudiante,
            ["ix_acuerdos_tutores_estudiante"] = IndiceAcuerdosEstudiante,
        };

    private static readonly IReadOnlyDictionary<string, string> ObjetosVersionUno =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["grupos"] = TablaGrupos,
            ["estudiantes"] = TablaEstudiantesV1,
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

    private static readonly IReadOnlyDictionary<string, string> ObjetosProyectosV3 =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["proyectos_didacticos"] = TablaProyectos,
            ["actividades_proyecto"] = TablaActividades,
            ["entregas_actividad"] = TablaEntregasV3,
            ["ix_proyectos_grupo_estado_fecha"] = IndiceProyectos,
            ["ix_actividades_proyecto_fecha"] = IndiceActividades,
            ["ix_entregas_estudiante"] = IndiceEntregas,
        };

    private static readonly IReadOnlyDictionary<string, string> ObjetosProyectos =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["proyectos_didacticos"] = TablaProyectos,
            ["actividades_proyecto"] = TablaActividades,
            ["entregas_actividad"] = TablaEntregas,
            ["ix_proyectos_grupo_estado_fecha"] = IndiceProyectos,
            ["ix_actividades_proyecto_fecha"] = IndiceActividades,
            ["ix_entregas_estudiante"] = IndiceEntregas,
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

            CrearVersionSeis(conexion);
            return;
        }

        if (version == 1)
        {
            ValidarObjetos(conexion, ObjetosVersionUno);
            MigrarVersionUno(conexion);
            MigrarVersionDos(conexion);
            MigrarVersionTres(conexion);
            MigrarVersionCuatro(conexion);
            MigrarVersionCinco(conexion);
            return;
        }

        if (version == 2)
        {
            ValidarVersionDos(conexion);
            MigrarVersionDos(conexion);
            MigrarVersionTres(conexion);
            MigrarVersionCuatro(conexion);
            MigrarVersionCinco(conexion);
            return;
        }

        if (version == 3)
        {
            ValidarVersionTres(conexion);
            MigrarVersionTres(conexion);
            MigrarVersionCuatro(conexion);
            MigrarVersionCinco(conexion);
            return;
        }

        if (version == 4)
        {
            ValidarVersionCuatro(conexion);
            MigrarVersionCuatro(conexion);
            MigrarVersionCinco(conexion);
            return;
        }

        if (version == 5)
        {
            ValidarVersionCinco(conexion);
            MigrarVersionCinco(conexion);
            return;
        }

        if (version != VersionActual)
        {
            throw new SchemaIncompatibleException(
                $"La versión de esquema {version} no es compatible con la versión {VersionActual}.");
        }

        ValidarVersionSeis(conexion);
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

    private static void CrearVersionCinco(SqliteConnection conexion)
    {
        using var transaccion = conexion.BeginTransaction();
        CrearObjetos(conexion, transaccion, ObjetosVersionUno.Values);
        CrearObjetos(conexion, transaccion, ObjetosAsistencia.Values);
        CrearObjetos(conexion, transaccion, ObjetosProyectos.Values);
        CrearObjetos(conexion, transaccion, ObjetosExpedientes.Values);
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
            EstablecerVersion(conexion, transaccion, 2);
            transaccion.Commit();
        }
        catch
        {
            transaccion.Rollback();
            throw;
        }
    }

    private static void MigrarVersionDos(SqliteConnection conexion)
    {
        using var transaccion = conexion.BeginTransaction();
        try
        {
            CrearObjetos(conexion, transaccion, ObjetosProyectosV3.Values);
            ValidarObjetos(conexion, ObjetosProyectosV3, transaccion);
            EstablecerVersion(conexion, transaccion, 3);
            transaccion.Commit();
        }
        catch
        {
            transaccion.Rollback();
            throw;
        }
    }

    private static void MigrarVersionTres(SqliteConnection conexion)
    {
        using var transaccion = conexion.BeginTransaction();
        try
        {
            using (var cmd = conexion.CreateCommand())
            {
                cmd.Transaction = transaccion;
                cmd.CommandText = """
                    CREATE TABLE entregas_actividad_temp AS SELECT * FROM entregas_actividad;
                    DROP TABLE entregas_actividad;
                    """;
                cmd.ExecuteNonQuery();
            }

            using (var cmd = conexion.CreateCommand())
            {
                cmd.Transaction = transaccion;
                cmd.CommandText = TablaEntregas;
                cmd.ExecuteNonQuery();
            }

            using (var cmd = conexion.CreateCommand())
            {
                cmd.Transaction = transaccion;
                cmd.CommandText = """
                    INSERT INTO entregas_actividad SELECT actividad_id, estudiante_id, grupo_id, estado_entrega, observacion FROM entregas_actividad_temp;
                    DROP TABLE entregas_actividad_temp;
                    CREATE INDEX IF NOT EXISTS ix_entregas_estudiante ON entregas_actividad(estudiante_id);
                    """;
                cmd.ExecuteNonQuery();
            }

            ValidarObjetos(conexion, ObjetosProyectos, transaccion);
            EstablecerVersion(conexion, transaccion, 4);
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
        ValidarObjetos(conexion, ObjetosAsistencia);
    }

    private static void ValidarVersionTres(SqliteConnection conexion)
    {
        ValidarObjetos(conexion, ObjetosProyectosV3);
    }

    private static void MigrarVersionCuatro(SqliteConnection conexion)
    {
        using var transaccion = conexion.BeginTransaction();
        try
        {
            using (var cmd = conexion.CreateCommand())
            {
                cmd.Transaction = transaccion;
                cmd.CommandText = """
                    CREATE TABLE IF NOT EXISTS notas_pedagogicas_estudiantes (
                        nota_id TEXT NOT NULL PRIMARY KEY,
                        estudiante_id TEXT NOT NULL,
                        grupo_id TEXT NOT NULL,
                        tipo INTEGER NOT NULL CHECK (tipo IN (0, 1, 2, 3)),
                        contenido TEXT NOT NULL CHECK (length(trim(contenido)) > 0),
                        fecha_hora_registro TEXT NOT NULL,
                        FOREIGN KEY (estudiante_id, grupo_id) REFERENCES estudiantes(id, grupo_id) ON DELETE CASCADE
                    );
                    CREATE TABLE IF NOT EXISTS acuerdos_tutores_estudiantes (
                        acuerdo_id TEXT NOT NULL PRIMARY KEY,
                        estudiante_id TEXT NOT NULL,
                        grupo_id TEXT NOT NULL,
                        motivo TEXT NOT NULL CHECK (length(trim(motivo)) > 0),
                        acuerdo_convenido TEXT NOT NULL CHECK (length(trim(acuerdo_convenido)) > 0),
                        fecha_reunion TEXT NOT NULL,
                        fecha_seguimiento TEXT,
                        FOREIGN KEY (estudiante_id, grupo_id) REFERENCES estudiantes(id, grupo_id) ON DELETE CASCADE
                    );
                    CREATE INDEX IF NOT EXISTS ix_notas_pedagogicas_estudiante ON notas_pedagogicas_estudiantes(estudiante_id, tipo);
                    CREATE INDEX IF NOT EXISTS ix_acuerdos_tutores_estudiante ON acuerdos_tutores_estudiantes(estudiante_id);
                    """;
                cmd.ExecuteNonQuery();
            }
            ValidarObjetos(conexion, ObjetosExpedientes, transaccion);
            EstablecerVersion(conexion, transaccion, VersionActual);
            transaccion.Commit();
        }
        catch
        {
            transaccion.Rollback();
            throw;
        }
    }

    private static void CrearVersionSeis(SqliteConnection conexion)
    {
        using var transaccion = conexion.BeginTransaction();
        try
        {
            CrearObjetos(
                conexion,
                transaccion,
                [
                    TablaGrupos,
                    TablaEstudiantes,
                    IndiceGrupo,
                    IndiceNumeroActivo,
                    IndicePertenencia,
                    TablaAsistencias,
                    TablaRegistros,
                    IndiceAsistenciasGrupoFecha,
                    IndiceRegistrosEstudiante,
                    TablaProyectos,
                    TablaActividades,
                    TablaEntregas,
                    IndiceProyectos,
                    IndiceActividades,
                    IndiceEntregas,
                    TablaNotasPedagogicas,
                    TablaAcuerdosTutores,
                    IndiceNotasEstudiante,
                    IndiceAcuerdosEstudiante,
                ]);

            EstablecerVersion(conexion, transaccion, 6);
            transaccion.Commit();
        }
        catch
        {
            transaccion.Rollback();
            throw;
        }
    }

    private static void MigrarVersionCinco(SqliteConnection conexion)
    {
        using var transaccion = conexion.BeginTransaction();
        try
        {
            if (!ExisteColumna(conexion, transaccion, "estudiantes", "primer_apellido"))
            {
                using var cmd = conexion.CreateCommand();
                cmd.Transaction = transaccion;
                cmd.CommandText = """
                    ALTER TABLE estudiantes ADD COLUMN primer_apellido TEXT NOT NULL DEFAULT '';
                    ALTER TABLE estudiantes ADD COLUMN segundo_apellido TEXT NOT NULL DEFAULT '';
                    ALTER TABLE estudiantes ADD COLUMN nombres TEXT NOT NULL DEFAULT '';
                    ALTER TABLE estudiantes ADD COLUMN fecha_nacimiento TEXT;
                    ALTER TABLE estudiantes ADD COLUMN genero INTEGER NOT NULL DEFAULT 0;
                    ALTER TABLE estudiantes ADD COLUMN fecha_ingreso TEXT;
                    ALTER TABLE estudiantes ADD COLUMN observaciones TEXT NOT NULL DEFAULT '';
                    """;
                cmd.ExecuteNonQuery();
            }
            EstablecerVersion(conexion, transaccion, 6);
            transaccion.Commit();
        }
        catch
        {
            transaccion.Rollback();
            throw;
        }
    }

    private static bool ExisteColumna(SqliteConnection conexion, SqliteTransaction? transaccion, string tabla, string columna)
    {
        using var cmd = conexion.CreateCommand();
        cmd.Transaction = transaccion;
        cmd.CommandText = $"PRAGMA table_info('{tabla}');";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            if (string.Equals(reader.GetString(1), columna, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    private static void ValidarVersionCuatro(SqliteConnection conexion)
    {
        ValidarObjetos(conexion, ObjetosProyectos);
    }

    private static void ValidarVersionCinco(SqliteConnection conexion)
    {
        ValidarObjetos(conexion, ObjetosExpedientes);
    }

    private static void ValidarVersionSeis(SqliteConnection conexion)
    {
        string[] columnasV6 = ["id", "grupo_id", "nombre", "numero_lista", "activo", "primer_apellido", "segundo_apellido", "nombres", "fecha_nacimiento", "genero", "fecha_ingreso", "observaciones"];
        foreach (var col in columnasV6)
        {
            if (!ExisteColumna(conexion, null, "estudiantes", col))
            {
                throw new SchemaIncompatibleException($"La tabla 'estudiantes' no contiene la columna requerida '{col}'.");
            }
        }

        var objetosSinEstudiantes = ObjetosVersionSeis
            .Where(kvp => !string.Equals(kvp.Key, "estudiantes", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        ValidarObjetos(conexion, objetosSinEstudiantes);
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