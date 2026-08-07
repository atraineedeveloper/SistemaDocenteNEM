using SistemaDocente.Core;

namespace SistemaDocente.Core.Tests;

public sealed class CatalogoPlaneacionNemTests
{
    [Fact]
    public void ExponeCuatroMetodologiasNemEspecificas()
    {
        Assert.Equal(4, CatalogoPlaneacionNem.MetodologiasProyecto.Count);
        Assert.DoesNotContain(
            MetodologiaProyectoNem.NoEspecificada,
            CatalogoPlaneacionNem.MetodologiasProyecto);
        Assert.Equal(
            "Aprendizaje Basado en Indagación · STEAM como enfoque",
            CatalogoPlaneacionNem.FormatearMetodologia(MetodologiaProyectoNem.IndagacionSteam));
    }

    [Fact]
    public void ExponeCuatroCamposFormativosEspecificos()
    {
        Assert.Equal(4, CatalogoPlaneacionNem.CamposFormativos.Count);
        Assert.DoesNotContain(
            CampoFormativoNem.NoEspecificado,
            CatalogoPlaneacionNem.CamposFormativos);
        Assert.Equal(
            "De lo Humano y lo Comunitario",
            CatalogoPlaneacionNem.FormatearCampo(CampoFormativoNem.DeLoHumanoYLoComunitario));
    }

    [Fact]
    public void NormalizaGradosObjetivoSinDuplicadosYEnOrden()
    {
        var grados = CatalogoPlaneacionNem.NormalizarGradosObjetivo(
            [GradoPrimaria.Tercero, GradoPrimaria.Primero, GradoPrimaria.Tercero]);

        Assert.Equal([GradoPrimaria.Primero, GradoPrimaria.Tercero], grados);
    }

    [Fact]
    public void RechazaGradoNoEspecificadoComoObjetivoExplicito()
    {
        Assert.Throws<DomainValidationException>(() =>
            CatalogoPlaneacionNem.NormalizarGradosObjetivo(
                [GradoPrimaria.Primero, GradoPrimaria.NoEspecificado]));
    }

    [Fact]
    public void ActividadNoPuedeSalirDeLosGradosExplicitosDelProyecto()
    {
        Assert.Throws<DomainValidationException>(() =>
            CatalogoPlaneacionNem.ValidarGradosActividadDentroDelProyecto(
                [GradoPrimaria.Primero, GradoPrimaria.Segundo],
                [GradoPrimaria.Tercero]));
    }

    [Fact]
    public void AlcanceLegacyVacioNoInventaRestricciones()
    {
        CatalogoPlaneacionNem.ValidarGradosActividadDentroDelProyecto(
            [],
            [GradoPrimaria.Sexto]);
    }
}