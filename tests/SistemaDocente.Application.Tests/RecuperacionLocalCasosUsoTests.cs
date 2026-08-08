namespace SistemaDocente.Application.Tests;

public sealed class RecuperacionLocalCasosUsoTests
{
    [Fact]
    public void NombreSugeridoIncluyeModoYFecha()
    {
        var servicio = new ServicioRecuperacionFalso
        {
            ModoActual = ModoAlmacenamientoLocal.Demostracion,
        };
        var casosUso = new GestionRespaldoCasosUso(servicio);

        var nombre = casosUso.CrearNombreArchivoSugerido(
            new DateTimeOffset(2026, 8, 8, 2, 15, 0, TimeSpan.Zero));

        Assert.Equal("SistemaDocenteNEM_Respaldo_Demo_2026-08-08_0215.sdocbackup", nombre);
    }

    [Fact]
    public void RestaurarSinConfirmacionExactaNoLlegaAlServicio()
    {
        var servicio = new ServicioRecuperacionFalso();
        var casosUso = new GestionRespaldoCasosUso(servicio);

        var error = Assert.Throws<InvalidOperationException>(() => casosUso.Restaurar(
            "respaldo.sdocbackup",
            "restaurar",
            DateTimeOffset.UtcNow,
            "1.0-test"));

        Assert.Contains("RESTAURAR", error.Message, StringComparison.Ordinal);
        Assert.Equal(0, servicio.Restauraciones);
    }

    [Fact]
    public void RestaurarConConfirmacionExactaDelegaUnaVez()
    {
        var servicio = new ServicioRecuperacionFalso();
        var casosUso = new GestionRespaldoCasosUso(servicio);
        var ahora = new DateTimeOffset(2026, 8, 8, 2, 20, 0, TimeSpan.Zero);

        var resultado = casosUso.Restaurar(
            "respaldo.sdocbackup",
            GestionRespaldoCasosUso.ConfirmacionRestauracion,
            ahora,
            "1.0-test");

        Assert.Equal(1, servicio.Restauraciones);
        Assert.True(resultado.ReinicioRequerido);
    }

    private sealed class ServicioRecuperacionFalso : IServicioRecuperacionLocal
    {
        public ModoAlmacenamientoLocal ModoActual { get; set; } = ModoAlmacenamientoLocal.Produccion;

        public int Restauraciones { get; private set; }

        public ResultadoRespaldoLocal CrearRespaldo(
            string rutaDestino,
            DateTimeOffset ahoraUtc,
            string versionAplicacion) => throw new NotSupportedException();

        public InspeccionRespaldoLocal Inspeccionar(string rutaRespaldo) =>
            throw new NotSupportedException();

        public ResultadoRestauracionLocal Restaurar(
            string rutaRespaldo,
            DateTimeOffset ahoraUtc,
            string versionAplicacion)
        {
            Restauraciones++;
            return new ResultadoRestauracionLocal(
                rutaRespaldo,
                "seguridad.sdocbackup",
                ahoraUtc,
                ReinicioRequerido: true,
                []);
        }
    }
}