using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Application.Tests;

public sealed class EstadoEntregaCompatibilidadTests
{
    [Fact]
    public void EntradaExplicitaConservaEstadoYNivelIndependientes()
    {
        var estudianteId = EstudianteId.DesdeGuid(Guid.NewGuid());

        var entrada = new EntradaEntregaActividad(
            estudianteId,
            EstadoEntregaActividad.Entregada,
            NivelLogro.Pendiente,
            "Trabajo recibido");

        Assert.True(entrada.EstadoEntregaEsExplicito);
        Assert.Equal(EstadoEntregaActividad.Entregada, entrada.EstadoEntrega);
        Assert.Equal(NivelLogro.Pendiente, entrada.NivelLogro);
    }

    [Fact]
    public void ConstructorLegacyMarcaEntradaComoNoExplicita()
    {
        var estudianteId = EstudianteId.DesdeGuid(Guid.NewGuid());

        var entrada = new EntradaEntregaActividad(
            estudianteId,
            NivelLogro.Pendiente,
            "Edición desde flujo legacy");

        Assert.False(entrada.EstadoEntregaEsExplicito);
        Assert.Equal(EstadoEntregaActividad.Pendiente, entrada.EstadoEntrega);
        Assert.Equal(NivelLogro.Pendiente, entrada.NivelLogro);
    }

    [Fact]
    public void NoEntregoLegacySeNormalizaAEstadoNoEntregada()
    {
        var estudianteId = EstudianteId.DesdeGuid(Guid.NewGuid());

        var entrada = new EntradaEntregaActividad(
            estudianteId,
            NivelLogro.NoEntrego,
            "");

        Assert.False(entrada.EstadoEntregaEsExplicito);
        Assert.Equal(EstadoEntregaActividad.NoEntregada, entrada.EstadoEntrega);
        Assert.Equal(NivelLogro.Pendiente, entrada.NivelLogro);
    }
}