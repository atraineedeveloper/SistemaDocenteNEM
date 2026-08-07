using System.Reflection;

using SistemaDocente.Core;

namespace SistemaDocente.Presentation.Tests;

public sealed class EvaluacionResultadoVisualTests
{
    [Theory]
    [InlineData(ResultadoEvaluacionVisual.Pendiente, EstadoEntregaActividad.Pendiente, NivelLogro.Pendiente, "P")]
    [InlineData(ResultadoEvaluacionVisual.EntregadaSinEvaluar, EstadoEntregaActividad.Entregada, NivelLogro.Pendiente, "✓")]
    [InlineData(ResultadoEvaluacionVisual.Domina, EstadoEntregaActividad.Entregada, NivelLogro.Domina, "D")]
    [InlineData(ResultadoEvaluacionVisual.Suficiente, EstadoEntregaActividad.Entregada, NivelLogro.Suficiente, "S")]
    [InlineData(ResultadoEvaluacionVisual.EnProceso, EstadoEntregaActividad.Entregada, NivelLogro.EnProceso, "E")]
    [InlineData(ResultadoEvaluacionVisual.RequiereApoyo, EstadoEntregaActividad.Entregada, NivelLogro.RequiereApoyo, "R")]
    [InlineData(ResultadoEvaluacionVisual.NoEntregada, EstadoEntregaActividad.NoEntregada, NivelLogro.Pendiente, "N")]
    public void ResultadoVisibleActualizaEstadoYNivelCorrectamente(
        ResultadoEvaluacionVisual resultado,
        EstadoEntregaActividad estadoEsperado,
        NivelLogro nivelEsperado,
        string etiquetaEsperada)
    {
        var celda = CrearCelda();

        celda.Resultado = resultado;

        Assert.Equal(resultado, celda.Resultado);
        Assert.Equal(estadoEsperado, celda.EstadoEntrega);
        Assert.Equal(nivelEsperado, celda.NivelLogro);
        Assert.Equal(etiquetaEsperada, celda.EtiquetaNivel);
        Assert.True(celda.TieneCambios);
    }

    [Fact]
    public void CambiarDeNoEntregadaALogroConvierteEntregaAutomaticamente()
    {
        var celda = CrearCelda();
        celda.Resultado = ResultadoEvaluacionVisual.NoEntregada;

        celda.Resultado = ResultadoEvaluacionVisual.Suficiente;

        Assert.Equal(EstadoEntregaActividad.Entregada, celda.EstadoEntrega);
        Assert.Equal(NivelLogro.Suficiente, celda.NivelLogro);
        Assert.Equal(ResultadoEvaluacionVisual.Suficiente, celda.Resultado);
    }

    [Fact]
    public void EntregadaSinEvaluarMantieneLogroPendiente()
    {
        var celda = CrearCelda();

        celda.Resultado = ResultadoEvaluacionVisual.EntregadaSinEvaluar;

        Assert.Equal(EstadoEntregaActividad.Entregada, celda.EstadoEntrega);
        Assert.Equal(NivelLogro.Pendiente, celda.NivelLogro);
        Assert.Equal("Entregada · pendiente de evaluación", celda.NombreNivel);
    }

    private static EvaluacionCeldaVisual CrearCelda()
    {
        var constructor = typeof(EvaluacionCeldaVisual).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            [
                typeof(ActividadId),
                typeof(EstudianteId),
                typeof(bool),
                typeof(bool),
                typeof(EstadoEntregaActividad),
                typeof(NivelLogro),
                typeof(string),
            ],
            modifiers: null);

        Assert.NotNull(constructor);
        return (EvaluacionCeldaVisual)constructor.Invoke(
        [
            ActividadId.DesdeGuid(Guid.NewGuid()),
            EstudianteId.DesdeGuid(Guid.NewGuid()),
            true,
            true,
            EstadoEntregaActividad.Pendiente,
            NivelLogro.Pendiente,
            string.Empty,
        ]);
    }
}