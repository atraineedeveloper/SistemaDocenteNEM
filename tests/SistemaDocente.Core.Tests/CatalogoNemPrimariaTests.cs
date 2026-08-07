using SistemaDocente.Core;

namespace SistemaDocente.Core.Tests;

public sealed class CatalogoNemPrimariaTests
{
    [Theory]
    [InlineData(GradoPrimaria.Primero, FaseNem.Fase3)]
    [InlineData(GradoPrimaria.Segundo, FaseNem.Fase3)]
    [InlineData(GradoPrimaria.Tercero, FaseNem.Fase4)]
    [InlineData(GradoPrimaria.Cuarto, FaseNem.Fase4)]
    [InlineData(GradoPrimaria.Quinto, FaseNem.Fase5)]
    [InlineData(GradoPrimaria.Sexto, FaseNem.Fase5)]
    public void ObtenerFaseMapeaGradoDePrimaria(GradoPrimaria grado, FaseNem faseEsperada)
    {
        Assert.Equal(faseEsperada, CatalogoNemPrimaria.ObtenerFase(grado));
    }

    [Fact]
    public void GrupoMultigradoPuedeAbarcarVariasFases()
    {
        var contexto = ContextoGrupo.Crear(
            GrupoId.DesdeGuid(Guid.NewGuid()),
            grado: "valor legacy ignorado",
            gradosAtendidos: [GradoPrimaria.Segundo, GradoPrimaria.Tercero]);

        Assert.True(contexto.EsMultigrado);
        Assert.Equal("Multigrado", contexto.ModalidadGrupo);
        Assert.Equal([FaseNem.Fase3, FaseNem.Fase4], contexto.FasesNem);
        Assert.Equal("2.º · 3.º", contexto.GradosTexto);
        Assert.Equal("Fase 3 · Fase 4", contexto.FasesNemTexto);
    }

    [Theory]
    [InlineData("4", GradoPrimaria.Cuarto)]
    [InlineData("4.º", GradoPrimaria.Cuarto)]
    [InlineData("Cuarto", GradoPrimaria.Cuarto)]
    [InlineData("4.º A", GradoPrimaria.Cuarto)]
    public void TryParseGradoLegacyAceptaFormasDeterministicas(string texto, GradoPrimaria esperado)
    {
        Assert.True(CatalogoNemPrimaria.TryParseGradoLegacy(texto, out var grado));
        Assert.Equal(esperado, grado);
    }

    [Theory]
    [InlineData("primero y segundo")]
    [InlineData("multigrado")]
    [InlineData("grupo superior")]
    public void TryParseGradoLegacyNoAdivinaValoresAmbiguos(string texto)
    {
        Assert.False(CatalogoNemPrimaria.TryParseGradoLegacy(texto, out var grado));
        Assert.Equal(GradoPrimaria.NoEspecificado, grado);
    }

    [Fact]
    public void ReferenciaPiagetDeSextoEsGeneralYNoDiagnostica()
    {
        var etapas = CatalogoNemPrimaria.ObtenerReferenciaPiaget([GradoPrimaria.Sexto]);
        var texto = CatalogoNemPrimaria.DescribirReferenciaPiaget([GradoPrimaria.Sexto]);

        Assert.Contains(EtapaDesarrolloCognoscitivo.OperacionesConcretas, etapas);
        Assert.Contains(EtapaDesarrolloCognoscitivo.OperacionesFormales, etapas);
        Assert.Contains("no diagnóstica", texto, StringComparison.OrdinalIgnoreCase);
    }
}