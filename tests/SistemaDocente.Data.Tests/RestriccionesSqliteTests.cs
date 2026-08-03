using Microsoft.Data.Sqlite;

namespace SistemaDocente.Data.Tests;

public sealed class RestriccionesSqliteTests : IDisposable
{
    private readonly BaseSqliteTemporal _base = new();

    [Fact]
    public void ClaveForaneaRechazaEstudianteHuerfano()
    {
        _base.Persistencia.Inicializar();
        using var conexion = _base.AbrirConexion();

        Assert.Throws<SqliteException>(
            () => InsertarEstudiante(
                conexion,
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Ana",
                1,
                1));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void NombreDeGrupoVacioSeRechaza(string nombre)
    {
        _base.Persistencia.Inicializar();
        using var conexion = _base.AbrirConexion();

        Assert.Throws<SqliteException>(() => InsertarGrupo(conexion, Guid.NewGuid(), nombre));
    }

    [Fact]
    public void LimitesDeNombresSeAplicanEnSqlite()
    {
        _base.Persistencia.Inicializar();
        using var conexion = _base.AbrirConexion();
        var grupoId = Guid.NewGuid();

        InsertarGrupo(conexion, grupoId, new string('g', 100));
        InsertarEstudiante(
            conexion,
            Guid.NewGuid(),
            grupoId,
            new string('e', 150),
            1,
            1);

        Assert.Throws<SqliteException>(
            () => InsertarGrupo(conexion, Guid.NewGuid(), new string('g', 101)));
        Assert.Throws<SqliteException>(
            () => InsertarEstudiante(
                conexion,
                Guid.NewGuid(),
                grupoId,
                new string('e', 151),
                2,
                1));
        Assert.Throws<SqliteException>(
            () => InsertarEstudiante(
                conexion,
                Guid.NewGuid(),
                grupoId,
                "   ",
                2,
                1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void NumeroNoPositivoSeRechaza(int numero)
    {
        _base.Persistencia.Inicializar();
        using var conexion = _base.AbrirConexion();
        var grupoId = Guid.NewGuid();
        InsertarGrupo(conexion, grupoId, "Primero A");

        Assert.Throws<SqliteException>(
            () => InsertarEstudiante(
                conexion,
                Guid.NewGuid(),
                grupoId,
                "Ana",
                numero,
                1));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void EstadoFueraDeCeroYUnoSeRechaza(int estado)
    {
        _base.Persistencia.Inicializar();
        using var conexion = _base.AbrirConexion();
        var grupoId = Guid.NewGuid();
        InsertarGrupo(conexion, grupoId, "Primero A");

        Assert.Throws<SqliteException>(
            () => InsertarEstudiante(
                conexion,
                Guid.NewGuid(),
                grupoId,
                "Ana",
                1,
                estado));
    }

    [Fact]
    public void DuplicadoActivoDelMismoGrupoSeRechaza()
    {
        _base.Persistencia.Inicializar();
        using var conexion = _base.AbrirConexion();
        var grupoId = Guid.NewGuid();
        InsertarGrupo(conexion, grupoId, "Primero A");
        InsertarEstudiante(conexion, Guid.NewGuid(), grupoId, "Ana", 1, 1);

        Assert.Throws<SqliteException>(
            () => InsertarEstudiante(
                conexion,
                Guid.NewGuid(),
                grupoId,
                "Luis",
                1,
                1));
    }

    [Fact]
    public void CoincidenciasEntreInactivosYGruposDistintosSePermiten()
    {
        _base.Persistencia.Inicializar();
        using var conexion = _base.AbrirConexion();
        var primerGrupo = Guid.NewGuid();
        var segundoGrupo = Guid.NewGuid();
        InsertarGrupo(conexion, primerGrupo, "Primero A");
        InsertarGrupo(conexion, segundoGrupo, "Primero B");

        InsertarEstudiante(conexion, Guid.NewGuid(), primerGrupo, "Ana", 1, 0);
        InsertarEstudiante(conexion, Guid.NewGuid(), primerGrupo, "Luis", 1, 0);
        InsertarEstudiante(conexion, Guid.NewGuid(), primerGrupo, "Eva", 1, 1);
        InsertarEstudiante(conexion, Guid.NewGuid(), segundoGrupo, "Leo", 1, 1);

        using var comando = conexion.CreateCommand();
        comando.CommandText = "SELECT COUNT(*) FROM estudiantes;";
        Assert.Equal(4L, comando.ExecuteScalar());
    }

    public void Dispose() => _base.Dispose();

    internal static void InsertarGrupo(
        SqliteConnection conexion,
        Guid id,
        string nombre)
    {
        using var comando = conexion.CreateCommand();
        comando.CommandText = "INSERT INTO grupos (id, nombre) VALUES ($id, $nombre);";
        comando.Parameters.AddWithValue("$id", id.ToString("D"));
        comando.Parameters.AddWithValue("$nombre", nombre);
        comando.ExecuteNonQuery();
    }

    internal static void InsertarEstudiante(
        SqliteConnection conexion,
        Guid id,
        Guid grupoId,
        string nombre,
        int numero,
        int estado)
    {
        using var comando = conexion.CreateCommand();
        comando.CommandText = """
            INSERT INTO estudiantes (
                id,
                grupo_id,
                nombre,
                numero_lista,
                activo)
            VALUES ($id, $grupoId, $nombre, $numero, $estado);
            """;
        comando.Parameters.AddWithValue("$id", id.ToString("D"));
        comando.Parameters.AddWithValue("$grupoId", grupoId.ToString("D"));
        comando.Parameters.AddWithValue("$nombre", nombre);
        comando.Parameters.AddWithValue("$numero", numero);
        comando.Parameters.AddWithValue("$estado", estado);
        comando.ExecuteNonQuery();
    }
}