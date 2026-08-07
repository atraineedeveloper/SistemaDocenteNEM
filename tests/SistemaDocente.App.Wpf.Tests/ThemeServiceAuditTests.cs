using System;
using System.IO;

namespace SistemaDocente.App.Wpf.Tests;

public sealed class ThemeServiceAuditTests
{
    private static string ObtenerRaiz() => Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static string Leer(string rutaRelativa) => File.ReadAllText(Path.Combine(
        ObtenerRaiz(), rutaRelativa.Replace('/', Path.DirectorySeparatorChar)));

    [Fact]
    public void ThemeServiceConservaDesignTokensYColocaTemaComoOverrideFinal()
    {
        var code = Leer("src/SistemaDocente.App.Wpf/Services/ThemeService.cs");

        Assert.Contains("!source.EndsWith(\"/DesignTokens.xaml\"", code, StringComparison.Ordinal);
        Assert.Contains("dictionaries.Remove(existingTheme)", code, StringComparison.Ordinal);
        Assert.Contains("dictionaries.Add(new ResourceDictionary", code, StringComparison.Ordinal);
        Assert.DoesNotContain("dictionaries.Insert(index", code, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("DesignTokens.xaml")]
    [InlineData("Dark.xaml")]
    [InlineData("HighContrast.xaml")]
    public void TemasDefinenTokenSemanticoDelIconoDeHeader(string archivo)
    {
        var xaml = Leer($"src/SistemaDocente.App.Wpf/Themes/{archivo}");

        Assert.Contains("HeaderIconBackgroundColor", xaml, StringComparison.Ordinal);
        Assert.Contains("HeaderIconBackgroundBrush", xaml, StringComparison.Ordinal);
    }
}
