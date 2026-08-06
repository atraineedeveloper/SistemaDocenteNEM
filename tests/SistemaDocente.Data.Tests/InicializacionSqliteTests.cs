using Microsoft.Data.Sqlite;

using SistemaDocente.Core;

namespace SistemaDocente.Data.Tests;

public sealed class InicializacionSqliteTests : IDisposable
{
    private readonly BaseSqliteTemporal _base = new();

    [Fact]
    public void InicializarCreaDirectorioBaseYVersionTres()
    {
        Assert.False(File.Exists(_base.Ruta));

        _base.Persistencia.Inicializar();

        Assert.True(File.Exists(_base.Ruta));
        using var conexion = _base.AbrirConexion();
        Assert.Equal(5L, EscalarLong(conexion, "PRAGMA user_version;"));
        Assert.Equal(
            19L,
            EscalarLong(
                conexion,
                """
                SELECT COUNT(*) FROM sqlite_master
                WHERE name IN (
                    'grupos',
                    'estudiantes',
                    'ix_estudiantes_grupo_id',
                    'ux_estudiantes_grupo_numero_activo',
                    'ux_estudiantes_id_grupo_id',
                    'asistencias_diarias',
                    'registros_asistencia',
                    'ix_asistencias_diarias_grupo_fecha',
                    'ix_registros_asistencia_estudiante_id',
                    'proyectos_didacticos',
                    'actividades_proyecto',
                    'entregas_actividad',
                    'ix_proyectos_grupo_estado_fecha',
                    'ix_actividades_proyecto_fecha',
                    'ix_entregas_estudiante',
                    'notas_pedagogicas_estudiantes',
                    'acuerdos_tutores_estudiantes',
                    'ix_notas_pedagogicas_estudiante',
                    'ix_acuerdos_tutores_estudiante');
                """));
    }

    [Fact]
    public void InicializarVersionDosCompatibleEsIdempotente()
    {
        _base.Persistencia.Inicializar();
        var grupo = Grupo.Crear("Primero A");
        _base.Persistencia.Guardar(grupo);

        _base.Persistencia.Inicializar();

        Assert.NotNull(_base.Persistencia.Cargar(grupo.Id));
    }

    [Fact]
    public void InicializarRechazaVersionPosteriorSinModificarla()
    {
        CrearBaseCruda("PRAGMA user_version = 5;");

        Assert.Throws<SchemaIncompatibleException>(() => _base.Persistencia.Inicializar());

        using var conexion = _base.AbrirConexion();
        Assert.Equal(5L, EscalarLong(conexion, "PRAGMA user_version;"));
    }

    [Fact]
    public void InicializarRechazaVersionCeroConObjetosPreexistentes()
    {
        CrearBaseCruda("CREATE TABLE legado (id INTEGER PRIMARY KEY);");

        Assert.Throws<SchemaIncompatibleException>(() => _base.Persistencia.Inicializar());

        using var conexion = _base.AbrirConexion();
        Assert.Equal(
            1L,
            EscalarLong(
                conexion,
                "SELECT COUNT(*) FROM sqlite_master WHERE name = 'legado';"));
        Assert.Equal(0L, EscalarLong(conexion, "PRAGMA user_version;"));
    }

    [Fact]
    public void InicializarRechazaVersionUnoConEstructuraIncompatible()
    {
        CrearBaseCruda(
            """
            CREATE TABLE grupos (id TEXT PRIMARY KEY);
            PRAGMA user_version = 1;
            """);

        Assert.Throws<SchemaIncompatibleException>(() => _base.Persistencia.Inicializar());

        using var conexion = _base.AbrirConexion();
        Assert.Equal(1L, EscalarLong(conexion, "PRAGMA user_version;"));
        Assert.Equal(
            1L,
            EscalarLong(
                conexion,
                "SELECT COUNT(*) FROM pragma_table_info('grupos');"));
    }

    [Fact]
    public void InicializarRechazaVersionDosConEstructuraIncompatible()
    {
        _base.Persistencia.Inicializar();
        using (var conexion = _base.AbrirConexion())
        using (var comando = conexion.CreateCommand())
        {
            comando.CommandText = """
                DROP INDEX ix_registros_asistencia_estudiante_id;
                CREATE INDEX ix_registros_asistencia_estudiante_id
                    ON registros_asistencia(grupo_id);
                """;
            comando.ExecuteNonQuery();
        }

        Assert.Throws<SchemaIncompatibleException>(() => _base.Persistencia.Inicializar());

        using var comprobacion = _base.AbrirConexion();
        Assert.Equal(5L, EscalarLong(comprobacion, "PRAGMA user_version;"));
    }

    [Fact]
    public void InicializarRechazaArchivoNoSqliteSinSobrescribirlo()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_base.Ruta)!);
        byte[] contenido = [1, 2, 3, 4, 5, 6, 7];
        File.WriteAllBytes(_base.Ruta, contenido);

        Assert.Throws<DataAccessException>(() => _base.Persistencia.Inicializar());

        Assert.Equal(contenido, File.ReadAllBytes(_base.Ruta));
    }

    [Fact]
    public void ReabrirConservaDatos()
    {
        var grupo = Grupo.Crear("Primero A");
        grupo.AgregarEstudiante("Ana", 1);
        _base.Persistencia.Guardar(grupo);

        SqliteConnection.ClearAllPools();
        var reabierta = new PersistenciaGrupoSqlite(_base.Ruta);
        var cargado = reabierta.Cargar(grupo.Id);

        Assert.NotNull(cargado);
        Assert.Single(cargado.Estudiantes);
    }

    [Fact]
    public void BasesTemporalesDistintasNoCompartenEstado()
    {
        using var otraBase = new BaseSqliteTemporal();
        var grupo = Grupo.Crear("Primero A");
        _base.Persistencia.Guardar(grupo);

        otraBase.Persistencia.Inicializar();

        Assert.Null(otraBase.Persistencia.Cargar(grupo.Id));
        Assert.NotEqual(_base.Ruta, otraBase.Ruta);
    }

    public void Dispose() => _base.Dispose();

    private void CrearBaseCruda(string sql)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_base.Ruta)!);
        using var conexion = new SqliteConnection($"Data Source={_base.Ruta}");
        conexion.Open();
        using var comando = conexion.CreateCommand();
        comando.CommandText = sql;
        comando.ExecuteNonQuery();
    }

    private static long EscalarLong(SqliteConnection conexion, string sql)
    {
        using var comando = conexion.CreateCommand();
        comando.CommandText = sql;
        return (long)(comando.ExecuteScalar() ?? 0L);
    }
}