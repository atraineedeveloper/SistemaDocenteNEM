using Microsoft.Data.Sqlite;

namespace SistemaDocente.Data.Tests;

public sealed class RestriccionesAsistenciaSqliteTests : IDisposable
{
    private readonly BaseSqliteTemporal _base = new();

    [Theory]
    [InlineData("")]
    [InlineData("03/08/2026")]
    [InlineData("2026-13-01")]
    [InlineData("2026-01-00")]
    [InlineData("2026-02-31")]
    [InlineData("2025-02-29")]
    [InlineData("2026-08-03T08:00:00")]
    public void RechazaFechasNoCanonicasOImposibles(string fecha)
    {
        var grupoId = PrepararGrupo();
        using var conexion = _base.AbrirConexion();

        Assert.Throws<SqliteException>(() => InsertarDia(conexion, grupoId, fecha));
    }

    [Fact]
    public void AceptaFechaBisiestaValida()
    {
        var grupoId = PrepararGrupo();
        using var conexion = _base.AbrirConexion();

        InsertarDia(conexion, grupoId, "2024-02-29");

        Assert.Equal(1L, Escalar(conexion, "SELECT COUNT(*) FROM asistencias_diarias;"));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(4)]
    public void RechazaEstadoFueraDeRango(int estado)
    {
        var grupoId = PrepararGrupo();
        var estudianteId = Guid.NewGuid();
        using var conexion = _base.AbrirConexion();
        RestriccionesSqliteTests.InsertarEstudiante(
            conexion, estudianteId, grupoId, "Ana", 1, 1);
        InsertarDia(conexion, grupoId, "2026-08-03");

        Assert.Throws<SqliteException>(() =>
            InsertarRegistro(conexion, grupoId, estudianteId, estado));
    }

    [Fact]
    public void RechazaDuplicadoYEstudianteDeOtroGrupo()
    {
        var grupoId = PrepararGrupo();
        var otroGrupoId = Guid.NewGuid();
        var estudianteId = Guid.NewGuid();
        using var conexion = _base.AbrirConexion();
        RestriccionesSqliteTests.InsertarGrupo(conexion, otroGrupoId, "Otro");
        RestriccionesSqliteTests.InsertarEstudiante(
            conexion, estudianteId, otroGrupoId, "Ajeno", 1, 1);
        InsertarDia(conexion, grupoId, "2026-08-03");

        Assert.Throws<SqliteException>(() =>
            InsertarRegistro(conexion, grupoId, estudianteId, 0));

        var propio = Guid.NewGuid();
        RestriccionesSqliteTests.InsertarEstudiante(
            conexion, propio, grupoId, "Ana", 1, 1);
        InsertarRegistro(conexion, grupoId, propio, 0);
        Assert.Throws<SqliteException>(() =>
            InsertarRegistro(conexion, grupoId, propio, 1));
    }

    private Guid PrepararGrupo()
    {
        _base.Persistencia.Inicializar();
        var grupoId = Guid.NewGuid();
        using var conexion = _base.AbrirConexion();
        RestriccionesSqliteTests.InsertarGrupo(conexion, grupoId, "Primero A");
        return grupoId;
    }

    private static void InsertarDia(SqliteConnection conexion, Guid grupoId, string fecha)
    {
        using var comando = conexion.CreateCommand();
        comando.CommandText = """
            INSERT INTO asistencias_diarias (grupo_id, fecha) VALUES ($grupoId, $fecha);
            """;
        comando.Parameters.AddWithValue("$grupoId", grupoId.ToString("D"));
        comando.Parameters.AddWithValue("$fecha", fecha);
        comando.ExecuteNonQuery();
    }

    private static void InsertarRegistro(
        SqliteConnection conexion,
        Guid grupoId,
        Guid estudianteId,
        int estado)
    {
        using var comando = conexion.CreateCommand();
        comando.CommandText = """
            INSERT INTO registros_asistencia (grupo_id, fecha, estudiante_id, estado)
            VALUES ($grupoId, '2026-08-03', $estudianteId, $estado);
            """;
        comando.Parameters.AddWithValue("$grupoId", grupoId.ToString("D"));
        comando.Parameters.AddWithValue("$estudianteId", estudianteId.ToString("D"));
        comando.Parameters.AddWithValue("$estado", estado);
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