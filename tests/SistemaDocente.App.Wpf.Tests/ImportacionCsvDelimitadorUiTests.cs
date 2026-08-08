using System.IO;

namespace SistemaDocente.App.Wpf.Tests;

public sealed class ImportacionCsvDelimitadorUiTests
{
    [Fact]
    public void AsistenteExponeResolucionExplicitaDeDelimitadorCsv()
    {
        var raiz = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        var xaml = File.ReadAllText(Path.Combine(
            raiz,
            "src",
            "SistemaDocente.App.Wpf",
            "ImportacionEstudiantesWindow.xaml"));

        Assert.Contains("RequiereDelimitadorCsv", xaml, StringComparison.Ordinal);
        Assert.Contains("OpcionesDelimitadoresCsv", xaml, StringComparison.Ordinal);
        Assert.Contains("DelimitadorCsvSeleccionado", xaml, StringComparison.Ordinal);
        Assert.Contains("ReintentarCsvCommand", xaml, StringComparison.Ordinal);
        Assert.Contains("Selecciona el delimitador del CSV", xaml, StringComparison.Ordinal);
        Assert.Contains("Coma", xaml, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Punto y coma", xaml, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("tabulador", xaml, StringComparison.OrdinalIgnoreCase);
    }
}