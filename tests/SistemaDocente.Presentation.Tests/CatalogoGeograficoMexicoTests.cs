using SistemaDocente.Presentation;

namespace SistemaDocente.Presentation.Tests;

public sealed class CatalogoGeograficoMexicoTests
{
    [Fact]
    public void CatalogoContieneLas32EntidadesFederativas()
    {
        Assert.Equal(32, CatalogoGeograficoMexico.EntidadesFederativas.Count);
        Assert.Contains("Tabasco", CatalogoGeograficoMexico.EntidadesFederativas);
        Assert.Contains("Ciudad de México", CatalogoGeograficoMexico.EntidadesFederativas);
    }

    [Fact]
    public void TabascoContieneSus17MunicipiosYFiltraOtrosEstados()
    {
        var municipios = CatalogoGeograficoMexico.ObtenerMunicipios("Tabasco");

        Assert.Equal(17, municipios.Count);
        Assert.Contains("Centro", municipios);
        Assert.Contains("Tacotalpa", municipios);
        Assert.DoesNotContain("Monterrey", municipios);
    }

    [Fact]
    public void EntidadDesconocidaNoDevuelveMunicipios()
    {
        Assert.Empty(CatalogoGeograficoMexico.ObtenerMunicipios("Entidad inventada"));
        Assert.False(CatalogoGeograficoMexico.ContieneEntidad("Entidad inventada"));
    }
}