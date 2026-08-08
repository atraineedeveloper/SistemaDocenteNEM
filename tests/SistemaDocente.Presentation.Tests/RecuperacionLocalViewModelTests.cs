using SistemaDocente.Application;
using SistemaDocente.Presentation;

namespace SistemaDocente.Presentation.Tests;

public sealed class RecuperacionLocalViewModelTests
{
    [Fact]
    public void EstadoInicialAdvierteQueElRespaldoNoEstaCifrado()
    {
        var viewModel = CrearViewModel(out _);

        Assert.Contains("no está cifrada", viewModel.AdvertenciaSeguridad, StringComparison.OrdinalIgnoreCase);
        Assert.False(viewModel.TieneInspeccion);
        Assert.False(viewModel.PuedeRestaurar);
    }

    [Fact]
    public void InspeccionExponeMetadatosYExigeConfirmacionExacta()
    {
        var viewModel = CrearViewModel(out var servicio);
        servicio.Inspeccion = new InspeccionRespaldoLocal(
            "C:\\Respaldos\\a.sdocbackup",
            new DateTimeOffset(2026, 8, 8, 3, 30, 0, TimeSpan.Zero),
            "1.2.3",
            ModoAlmacenamientoLocal.Demostracion,
            6,
            2 * 1024 * 1024,
            [new("Base de datos SQLite", 1024, new string('a', 64), true)],
            ["Advertencia de prueba"],
            EsCompatible: true);

        viewModel.Inspeccionar(servicio.Inspeccion.RutaArchivo);

        Assert.True(viewModel.TieneInspeccion);
        Assert.Equal("Demostración", viewModel.ModoRespaldo);
        Assert.Equal("1.2.3", viewModel.VersionAplicacionRespaldo);
        Assert.Equal("6", viewModel.VersionBaseDatos);
        Assert.Equal("2 MB", viewModel.TamanoRespaldo);
        Assert.Contains("Base de datos SQLite", viewModel.ComponentesRespaldo, StringComparison.Ordinal);
        Assert.True(viewModel.TieneAdvertenciasInspeccion);
        Assert.False(viewModel.PuedeRestaurar);

        viewModel.Confirmacion = "restaurar";
        Assert.False(viewModel.PuedeRestaurar);

        viewModel.Confirmacion = GestionRespaldoCasosUso.ConfirmacionRestauracion;
        Assert.True(viewModel.PuedeRestaurar);
    }

    [Fact]
    public void RestauracionExitosaExponeRespaldoSeguridad()
    {
        var viewModel = CrearViewModel(out var servicio);
        servicio.Inspeccion = CrearInspeccionCompatible();
        viewModel.Inspeccionar(servicio.Inspeccion.RutaArchivo);
        viewModel.Confirmacion = GestionRespaldoCasosUso.ConfirmacionRestauracion;
        servicio.ResultadoRestauracion = new ResultadoRestauracionLocal(
            servicio.Inspeccion.RutaArchivo,
            "C:\\safety\\before-restore.sdocbackup",
            new DateTimeOffset(2026, 8, 8, 3, 35, 0, TimeSpan.Zero),
            ReinicioRequerido: true,
            []);

        var resultado = viewModel.Restaurar(
            new DateTimeOffset(2026, 8, 8, 3, 35, 0, TimeSpan.Zero),
            "1.2.3");

        Assert.True(resultado.ReinicioRequerido);
        Assert.True(viewModel.RestauracionCompletada);
        Assert.Equal(servicio.ResultadoRestauracion.RutaRespaldoSeguridad, viewModel.RutaRespaldoSeguridad);
        Assert.Equal(1, servicio.Restauraciones);
    }

    [Fact]
    public void CrearRespaldoPublicaResumenYAdvertencias()
    {
        var viewModel = CrearViewModel(out var servicio);
        servicio.ResultadoRespaldo = new ResultadoRespaldoLocal(
            "C:\\Respaldos\\manual.sdocbackup",
            new DateTimeOffset(2026, 8, 8, 3, 40, 0, TimeSpan.Zero),
            "1.2.3",
            ModoAlmacenamientoLocal.Demostracion,
            6,
            1536,
            [new("Base de datos SQLite", 1024, new string('b', 64), true)],
            ["Estado omitido"]);

        viewModel.CrearRespaldo(
            servicio.ResultadoRespaldo.RutaArchivo,
            servicio.ResultadoRespaldo.CreadoUtc,
            servicio.ResultadoRespaldo.VersionAplicacion);

        Assert.Equal(servicio.ResultadoRespaldo.RutaArchivo, viewModel.UltimoRespaldoRuta);
        Assert.Contains("1.5 KB", viewModel.UltimoRespaldoResumen, StringComparison.Ordinal);
        Assert.Contains("Base v6", viewModel.UltimoRespaldoResumen, StringComparison.Ordinal);
        Assert.Contains("Estado omitido", viewModel.UltimoRespaldoAdvertencias, StringComparison.Ordinal);
    }

    private static RecuperacionLocalViewModel CrearViewModel(out ServicioRecuperacionFalso servicio)
    {
        servicio = new ServicioRecuperacionFalso
        {
            ModoActual = ModoAlmacenamientoLocal.Demostracion,
            Inspeccion = CrearInspeccionCompatible(),
        };
        return new RecuperacionLocalViewModel(new GestionRespaldoCasosUso(servicio));
    }

    private static InspeccionRespaldoLocal CrearInspeccionCompatible() =>
        new(
            "C:\\Respaldos\\demo.sdocbackup",
            new DateTimeOffset(2026, 8, 8, 3, 30, 0, TimeSpan.Zero),
            "1.2.3",
            ModoAlmacenamientoLocal.Demostracion,
            6,
            2048,
            [new("Base de datos SQLite", 1024, new string('c', 64), true)],
            [],
            EsCompatible: true);

    private sealed class ServicioRecuperacionFalso : IServicioRecuperacionLocal
    {
        public ModoAlmacenamientoLocal ModoActual { get; set; }
        public InspeccionRespaldoLocal Inspeccion { get; set; } = null!;
        public ResultadoRespaldoLocal ResultadoRespaldo { get; set; } = null!;
        public ResultadoRestauracionLocal ResultadoRestauracion { get; set; } = null!;
        public int Restauraciones { get; private set; }

        public ResultadoRespaldoLocal CrearRespaldo(
            string rutaDestino,
            DateTimeOffset ahoraUtc,
            string versionAplicacion) => ResultadoRespaldo;

        public InspeccionRespaldoLocal Inspeccionar(string rutaRespaldo) => Inspeccion;

        public ResultadoRestauracionLocal Restaurar(
            string rutaRespaldo,
            DateTimeOffset ahoraUtc,
            string versionAplicacion)
        {
            Restauraciones++;
            return ResultadoRestauracion;
        }
    }
}