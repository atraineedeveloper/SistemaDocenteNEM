using Microsoft.Data.Sqlite;

using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Data.Tests;

public sealed class ServicioRecuperacionLocalSqliteTests : IDisposable
{
    private readonly string _directorio = Path.Combine(
        Path.GetTempPath(),
        "SistemaDocenteNEM-RecoveryTests-" + Guid.NewGuid().ToString("N"));
    private readonly string _rutaBase;
    private readonly string _rutaEstado;
    private readonly string _directorioSeguridad;

    public ServicioRecuperacionLocalSqliteTests()
    {
        Directory.CreateDirectory(_directorio);
        _rutaBase = Path.Combine(_directorio, "sistema-docente.db");
        _rutaEstado = Path.Combine(_directorio, "app-state.json");
        _directorioSeguridad = Path.Combine(_directorio, "backups", "safety");
    }

    [Fact]
    public void CrearEInspeccionarRespaldoConservaBaseYEstado()
    {
        var grupo = CrearGrupo("Cuarto A", "Ana", 1);
        File.WriteAllText(_rutaEstado, "{\"grupo\":\"cuarto-a\"}");
        var servicio = CrearServicio(ModoAlmacenamientoLocal.Produccion);
        var rutaRespaldo = Path.Combine(_directorio, "manual.sdocbackup");
        var instante = new DateTimeOffset(2026, 8, 8, 2, 30, 0, TimeSpan.Zero);

        var resultado = servicio.CrearRespaldo(rutaRespaldo, instante, "1.0-test");
        var inspeccion = servicio.Inspeccionar(rutaRespaldo);

        Assert.True(File.Exists(rutaRespaldo));
        Assert.Equal(instante, resultado.CreadoUtc);
        Assert.Equal(6, resultado.VersionBaseDatos);
        Assert.Equal(2, resultado.Componentes.Count);
        Assert.Empty(resultado.Advertencias);
        Assert.True(resultado.TamanoBytes > 0);

        Assert.True(inspeccion.EsCompatible);
        Assert.Equal(ModoAlmacenamientoLocal.Produccion, inspeccion.ModoOrigen);
        Assert.Equal("1.0-test", inspeccion.VersionAplicacion);
        Assert.Equal(2, inspeccion.Componentes.Count);
        Assert.Empty(inspeccion.Advertencias);

        var vivo = new PersistenciaGrupoSqlite(_rutaBase).Cargar(grupo.Id);
        Assert.NotNull(vivo);
        Assert.Equal("Cuarto A", vivo.NombreVisible);
        Assert.Equal("Ana", Assert.Single(vivo.Estudiantes).NombreVisible);
    }

    [Fact]
    public void EstadoInvalidoSeOmiteSinBloquearRespaldoDeBase()
    {
        CrearGrupo("Quinto A", "Luis", 1);
        File.WriteAllText(_rutaEstado, "{esto no es json");
        var servicio = CrearServicio(ModoAlmacenamientoLocal.Produccion);
        var rutaRespaldo = Path.Combine(_directorio, "sin-estado.sdocbackup");

        var resultado = servicio.CrearRespaldo(
            rutaRespaldo,
            new DateTimeOffset(2026, 8, 8, 2, 31, 0, TimeSpan.Zero),
            "1.0-test");
        var inspeccion = servicio.Inspeccionar(rutaRespaldo);

        Assert.Single(resultado.Componentes);
        Assert.Single(resultado.Advertencias);
        Assert.Contains("omitido", resultado.Advertencias[0], StringComparison.OrdinalIgnoreCase);
        Assert.Single(inspeccion.Componentes);
        Assert.Single(inspeccion.Advertencias);
    }

    [Fact]
    public void RespaldoDemoNoPuedeInspeccionarseComoProduccion()
    {
        CrearGrupo("Sexto A", "Marta", 1);
        var rutaRespaldo = Path.Combine(_directorio, "demo.sdocbackup");
        CrearServicio(ModoAlmacenamientoLocal.Demostracion).CrearRespaldo(
            rutaRespaldo,
            new DateTimeOffset(2026, 8, 8, 2, 32, 0, TimeSpan.Zero),
            "1.0-test");

        var error = Assert.Throws<RecuperacionLocalException>(() =>
            CrearServicio(ModoAlmacenamientoLocal.Produccion).Inspeccionar(rutaRespaldo));

        Assert.Equal(CategoriaErrorRecuperacionLocal.PaqueteIncompatible, error.Categoria);
        Assert.NotNull(new PersistenciaGrupoSqlite(_rutaBase).ListarTodos().SingleOrDefault());
    }

    [Fact]
    public void RestaurarRecuperaSnapshotYCreaRespaldoSeguridad()
    {
        var grupo = CrearGrupo("Cuarto A", "Ana", 1);
        File.WriteAllText(_rutaEstado, "{\"estado\":\"original\"}");
        var servicio = CrearServicio(ModoAlmacenamientoLocal.Produccion);
        var rutaRespaldo = Path.Combine(_directorio, "original.sdocbackup");
        servicio.CrearRespaldo(
            rutaRespaldo,
            new DateTimeOffset(2026, 8, 8, 2, 33, 0, TimeSpan.Zero),
            "1.0-test");

        var mutado = new PersistenciaGrupoSqlite(_rutaBase).Cargar(grupo.Id)!;
        mutado.Renombrar("Cuarto A mutado");
        mutado.AgregarEstudiante("Bruno", 2);
        new PersistenciaGrupoSqlite(_rutaBase).Guardar(mutado);
        File.WriteAllText(_rutaEstado, "{\"estado\":\"mutado\"}");

        var resultado = servicio.Restaurar(
            rutaRespaldo,
            new DateTimeOffset(2026, 8, 8, 2, 34, 0, TimeSpan.Zero),
            "1.0-test");

        var restaurado = new PersistenciaGrupoSqlite(_rutaBase).Cargar(grupo.Id)!;
        Assert.Equal("Cuarto A", restaurado.NombreVisible);
        var estudiante = Assert.Single(restaurado.Estudiantes);
        Assert.Equal("Ana", estudiante.NombreVisible);
        Assert.Equal("{\"estado\":\"original\"}", File.ReadAllText(_rutaEstado));
        Assert.True(resultado.ReinicioRequerido);
        Assert.True(File.Exists(resultado.RutaRespaldoSeguridad));
        Assert.StartsWith(_directorioSeguridad, resultado.RutaRespaldoSeguridad, StringComparison.OrdinalIgnoreCase);
        Assert.True(servicio.Inspeccionar(resultado.RutaRespaldoSeguridad).EsCompatible);
        Assert.Empty(Directory.GetFiles(_directorio, "*.restore-old-*", SearchOption.AllDirectories));
    }

    [Fact]
    public void RestaurarRespaldoSinEstadoEliminaEstadoVivoAnterior()
    {
        var grupo = CrearGrupo("Tercero A", "Sofía", 1);
        var servicio = CrearServicio(ModoAlmacenamientoLocal.Produccion);
        var rutaRespaldo = Path.Combine(_directorio, "base-solamente.sdocbackup");
        servicio.CrearRespaldo(
            rutaRespaldo,
            new DateTimeOffset(2026, 8, 8, 2, 35, 0, TimeSpan.Zero),
            "1.0-test");
        File.WriteAllText(_rutaEstado, "{\"estado\":\"posterior\"}");

        servicio.Restaurar(
            rutaRespaldo,
            new DateTimeOffset(2026, 8, 8, 2, 36, 0, TimeSpan.Zero),
            "1.0-test");

        Assert.False(File.Exists(_rutaEstado));
        Assert.Equal("Tercero A", new PersistenciaGrupoSqlite(_rutaBase).Cargar(grupo.Id)!.NombreVisible);
    }

    private ServicioRecuperacionLocalSqlite CrearServicio(ModoAlmacenamientoLocal modo) =>
        new(_rutaBase, _rutaEstado, _directorioSeguridad, modo);

    private Grupo CrearGrupo(
        string nombreGrupo,
        string nombreEstudiante,
        int numeroLista)
    {
        var grupo = Grupo.Crear(nombreGrupo);
        grupo.AgregarEstudiante(nombreEstudiante, numeroLista);
        new PersistenciaGrupoSqlite(_rutaBase).Guardar(grupo);
        return grupo;
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