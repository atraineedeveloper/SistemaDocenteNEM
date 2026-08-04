using Microsoft.Data.Sqlite;

namespace SistemaDocente.Data.Tests;

public sealed class MigracionAsistenciaSqliteTests : IDisposable
{
    private readonly BaseSqliteTemporal _base = new();

    [Fact]
    public void MigraBaseVersionUnoAutenticaConDatosSinAlterarlos()
    {
        var grupoId = Guid.NewGuid();
        var estudianteId = Guid.NewGuid();
        CrearVersionUno(grupoId, estudianteId);

        _base.Persistencia.Inicializar();

        using var conexion = _base.AbrirConexion();
        Assert.Equal(3L, Escalar(conexion, "PRAGMA user_version;"));
        Assert.Equal(1L, Escalar(conexion, "SELECT COUNT(*) FROM grupos;"));
        Assert.Equal(1L, Escalar(conexion, "SELECT COUNT(*) FROM estudiantes;"));
        Assert.Equal(1L, Escalar(
            conexion,
            "SELECT COUNT(*) FROM sqlite_master WHERE name = 'asistencias_diarias';"));
    }

    [Fact]
    public void FalloDeMigracionConservaVersionUnoSinObjetosParciales()
    {
        var grupoId = Guid.NewGuid();
        var estudianteId = Guid.NewGuid();
        CrearVersionUno(grupoId, estudianteId);
        using (var conexion = _base.AbrirConexion())
        using (var comando = conexion.CreateCommand())
        {
            comando.CommandText = """
                CREATE INDEX ux_estudiantes_id_grupo_id ON estudiantes(grupo_id);
                """;
            comando.ExecuteNonQuery();
        }

        Assert.Throws<DataAccessException>(() => _base.Persistencia.Inicializar());

        using var comprobacion = _base.AbrirConexion();
        Assert.Equal(1L, Escalar(comprobacion, "PRAGMA user_version;"));
        Assert.Equal(1L, Escalar(comprobacion, "SELECT COUNT(*) FROM grupos;"));
        Assert.Equal(1L, Escalar(comprobacion, "SELECT COUNT(*) FROM estudiantes;"));
        Assert.Equal(0L, Escalar(
            comprobacion,
            "SELECT COUNT(*) FROM sqlite_master WHERE name IN ('asistencias_diarias', 'registros_asistencia');"));
    }

    private void CrearVersionUno(Guid grupoId, Guid estudianteId)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_base.Ruta)!);
        using var conexion = _base.AbrirConexion();
        using var comando = conexion.CreateCommand();
        comando.CommandText = """
            CREATE TABLE grupos (
                id TEXT NOT NULL PRIMARY KEY,
                nombre TEXT NOT NULL CHECK (length(trim(nombre)) BETWEEN 1 AND 100)
            );
            CREATE TABLE estudiantes (
                id TEXT NOT NULL PRIMARY KEY,
                grupo_id TEXT NOT NULL,
                nombre TEXT NOT NULL CHECK (length(trim(nombre)) BETWEEN 1 AND 150),
                numero_lista INTEGER NOT NULL CHECK (numero_lista > 0),
                activo INTEGER NOT NULL CHECK (activo IN (0, 1)),
                FOREIGN KEY (grupo_id) REFERENCES grupos(id) ON DELETE RESTRICT
            );
            CREATE INDEX ix_estudiantes_grupo_id ON estudiantes(grupo_id);
            CREATE UNIQUE INDEX ux_estudiantes_grupo_numero_activo
                ON estudiantes(grupo_id, numero_lista) WHERE activo = 1;
            INSERT INTO grupos (id, nombre) VALUES ($grupoId, 'Primero A');
            INSERT INTO estudiantes (id, grupo_id, nombre, numero_lista, activo)
                VALUES ($estudianteId, $grupoId, 'Ana', 1, 1);
            PRAGMA user_version = 1;
            """;
        comando.Parameters.AddWithValue("$grupoId", grupoId.ToString("D"));
        comando.Parameters.AddWithValue("$estudianteId", estudianteId.ToString("D"));
        comando.ExecuteNonQuery();
    }

    private static long Escalar(SqliteConnection conexion, string sql)
    {
        using var comando = conexion.CreateCommand();
        comando.CommandText = sql;
        return (long)(comando.ExecuteScalar() ?? 0L);
    }

    public void Dispose() => _base.Dispose();
}