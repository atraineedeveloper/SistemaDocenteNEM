using Microsoft.Data.Sqlite;

using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Data.Tests;

public sealed class ServicioRecuperacionLocalCompatibilidadTests : IDisposable
{
    private readonly string _directorio = Path.Combine(
        Path.GetTempPath(),
        "SistemaDocenteNEM-RecoveryCompat-" + Guid.NewGuid().ToString("N"));
    private readonly string _rutaBase;

    public ServicioRecuperacionLocalCompatibilidadTests()
    {
        Directory.CreateDirectory(_directorio);
        _rutaBase = Path.Combine(_directorio, "sistema-docente.db");
    }

    [Fact]
    public void InspeccionPreparaVersionCincoEnCopiaSinModificarOrigen()
    {
        var grupo = Grupo.Crear("Grupo de compatibilidad");
        grupo.AgregarEstudiante("Alumno de compatibilidad", 1);
        new PersistenciaGrupoSqlite(_rutaBase).Guardar(grupo);
        EstablecerVersion(5);

        var servicio = new ServicioRecuperacionLocalSqlite(
            _rutaBase,
            Path.Combine(_directorio, "app-state.json"),
            Path.Combine(_directorio, "backups", "safety"),
            ModoAlmacenamientoLocal.Produccion);
        var rutaRespaldo = Path.Combine(_directorio, "v5.sdocbackup");

        var creado = servicio.CrearRespaldo(
            rutaRespaldo,
            new DateTimeOffset(2026, 8, 8, 4, 0, 0, TimeSpan.Zero),
            "1.0-test");
        var inspeccion = servicio.Inspeccionar(rutaRespaldo);

        Assert.Equal(5, creado.VersionBaseDatos);
        Assert.Equal(5, inspeccion.VersionBaseDatos);
        Assert.True(inspeccion.EsCompatible);
        Assert.Equal(5, LeerVersion());
        Assert.Equal(
            "Grupo de compatibilidad",
            new PersistenciaGrupoSqlite(_rutaBase).Cargar(grupo.Id)!.NombreVisible);
    }

    private void EstablecerVersion(int version)
    {
        using var conexion = AbrirConexion(SqliteOpenMode.ReadWrite);
        using var comando = conexion.CreateCommand();
        comando.CommandText = $"PRAGMA user_version = {version};";
        comando.ExecuteNonQuery();
    }

    private int LeerVersion()
    {
        using var conexion = AbrirConexion(SqliteOpenMode.ReadOnly);
        using var comando = conexion.CreateCommand();
        comando.CommandText = "PRAGMA user_version;";
        return checked((int)(long)(comando.ExecuteScalar() ?? 0L));
    }

    private SqliteConnection AbrirConexion(SqliteOpenMode modo)
    {
        var conexion = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = _rutaBase,
            Mode = modo,
            Pooling = false,
        }.ToString());
        conexion.Open();
        return conexion;
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (Directory.Exists(_directorio))
        {
            Directory.Delete(_directorio, recursive: true);
        }
    }
}