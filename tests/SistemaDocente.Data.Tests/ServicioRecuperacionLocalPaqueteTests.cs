using System.IO.Compression;

using Microsoft.Data.Sqlite;

using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Data.Tests;

public sealed class ServicioRecuperacionLocalPaqueteTests : IDisposable
{
    private readonly string _directorio = Path.Combine(
        Path.GetTempPath(),
        "SistemaDocenteNEM-RecoveryPackage-" + Guid.NewGuid().ToString("N"));
    private readonly string _rutaBase;
    private readonly string _rutaEstado;
    private readonly string _directorioSeguridad;

    public ServicioRecuperacionLocalPaqueteTests()
    {
        Directory.CreateDirectory(_directorio);
        _rutaBase = Path.Combine(_directorio, "sistema-docente.db");
        _rutaEstado = Path.Combine(_directorio, "app-state.json");
        _directorioSeguridad = Path.Combine(_directorio, "backups", "safety");

        var grupo = Grupo.Crear("Grupo Ñandú");
        grupo.AgregarEstudiante("Ángela de prueba", 1);
        new PersistenciaGrupoSqlite(_rutaBase).Guardar(grupo);
    }

    [Fact]
    public void PaqueteConRutaYMetadatosUnicodeHaceRoundTrip()
    {
        var servicio = CrearServicio();
        var subdirectorio = Path.Combine(_directorio, "Respaldos-áé-测试");
        Directory.CreateDirectory(subdirectorio);
        var ruta = Path.Combine(subdirectorio, "respaldo-ñ-数据.sdocbackup");
        const string versionAplicacion = "versión-prueba-ñ-测试";

        var creado = servicio.CrearRespaldo(
            ruta,
            new DateTimeOffset(2026, 8, 8, 4, 30, 0, TimeSpan.Zero),
            versionAplicacion);
        var inspeccion = servicio.Inspeccionar(ruta);

        Assert.Equal(Path.GetFullPath(ruta), creado.RutaArchivo);
        Assert.Equal(Path.GetFullPath(ruta), inspeccion.RutaArchivo);
        Assert.Equal(versionAplicacion, inspeccion.VersionAplicacion);
        Assert.True(inspeccion.EsCompatible);
        Assert.Contains(
            inspeccion.Componentes,
            componente => componente.Nombre == "Base de datos SQLite"
                && componente.Sha256.Length == 64);
    }

    [Fact]
    public void PaqueteSinBaseDeDatosEsRechazado()
    {
        var servicio = CrearServicio();
        var ruta = Path.Combine(_directorio, "sin-base.sdocbackup");
        servicio.CrearRespaldo(
            ruta,
            new DateTimeOffset(2026, 8, 8, 4, 31, 0, TimeSpan.Zero),
            "1.0-test");

        using (var archivo = ZipFile.Open(ruta, ZipArchiveMode.Update))
        {
            var entradaBase = Assert.Single(
                archivo.Entries,
                entrada => entrada.FullName == "data/sistema-docente.db");
            entradaBase.Delete();
        }

        var error = Assert.Throws<RecuperacionLocalException>(() => servicio.Inspeccionar(ruta));

        Assert.Equal(CategoriaErrorRecuperacionLocal.PaqueteInvalido, error.Categoria);
    }

    private ServicioRecuperacionLocalSqlite CrearServicio() =>
        new(
            _rutaBase,
            _rutaEstado,
            _directorioSeguridad,
            ModoAlmacenamientoLocal.Produccion);

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (Directory.Exists(_directorio))
        {
            Directory.Delete(_directorio, recursive: true);
        }
    }
}