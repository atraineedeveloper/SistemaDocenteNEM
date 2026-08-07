using SistemaDocente.Core;
using SistemaDocente.Reporting;

namespace SistemaDocente.Reporting.Tests;

public sealed class GeneradorReportesTests
{
    [Fact]
    public void CumplimientoIgnoraPendientesEnElDenominador()
    {
        var actividades = new[]
        {
            Actividad(EstadoEntregaActividad.Entregada, NivelLogro.Domina),
            Actividad(EstadoEntregaActividad.Entregada, NivelLogro.Pendiente),
            Actividad(EstadoEntregaActividad.NoEntregada, NivelLogro.Pendiente),
            Actividad(EstadoEntregaActividad.Pendiente, NivelLogro.Pendiente),
        };

        var resumen = GeneradorReportes.CalcularCumplimiento(actividades);

        Assert.Equal(4, resumen.ActividadesAplicables);
        Assert.Equal(2, resumen.Entregadas);
        Assert.Equal(1, resumen.NoEntregadas);
        Assert.Equal(1, resumen.Pendientes);
        Assert.Equal(200d / 3d, resumen.PorcentajeCumplimiento!.Value, 8);
    }

    [Fact]
    public void SinEntregasDecididasMuestraPorcentajeIndefinido()
    {
        var resumen = GeneradorReportes.CalcularCumplimiento([
            Actividad(EstadoEntregaActividad.Pendiente, NivelLogro.Pendiente),
            Actividad(EstadoEntregaActividad.Pendiente, NivelLogro.Pendiente)]);

        Assert.Null(resumen.PorcentajeCumplimiento);
        Assert.Equal(2, resumen.Pendientes);
    }

    [Fact]
    public void DistribucionDeLogroSoloConsideraEntregadas()
    {
        var distribucion = GeneradorReportes.CalcularLogro([
            Actividad(EstadoEntregaActividad.Entregada, NivelLogro.Domina),
            Actividad(EstadoEntregaActividad.Entregada, NivelLogro.Pendiente),
            Actividad(EstadoEntregaActividad.Entregada, NivelLogro.RequiereApoyo),
            Actividad(EstadoEntregaActividad.NoEntregada, NivelLogro.Pendiente),
            Actividad(EstadoEntregaActividad.Pendiente, NivelLogro.Pendiente)]);

        Assert.Equal(1, distribucion.Pendientes);
        Assert.Equal(1, distribucion.Domina);
        Assert.Equal(0, distribucion.Suficiente);
        Assert.Equal(0, distribucion.EnProceso);
        Assert.Equal(1, distribucion.RequiereApoyo);
    }

    private static ActividadReporteFuente Actividad(
        EstadoEntregaActividad estado,
        NivelLogro nivel) => new(
            "Proyecto",
            "Actividad",
            new DateOnly(2026, 8, 7),
            estado,
            nivel,
            "");
}