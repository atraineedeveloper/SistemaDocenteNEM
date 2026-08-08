using System.IO;

namespace SistemaDocente.App.Wpf.Tests;

public sealed class ReportesPdfUxTests
{
    private static string Root() => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static string Read(string relativePath) =>
        File.ReadAllText(Path.Combine(Root(), relativePath.Replace('/', Path.DirectorySeparatorChar)));

    [Fact]
    public void ReportsUiOwnsOnlyPrivacyConfirmationAndNativePdfSaveDialog()
    {
        var code = Read("src/SistemaDocente.App.Wpf/Views/ReportesView.xaml.cs");

        Assert.Contains("Guardar PDF", code, StringComparison.Ordinal);
        Assert.Contains("SaveFileDialog", code, StringComparison.Ordinal);
        Assert.Contains("AdvertenciaPdf", code, StringComparison.Ordinal);
        Assert.Contains("MessageBoxButton.YesNo", code, StringComparison.Ordinal);
        Assert.Contains("viewModel.ExportarPdf", code, StringComparison.Ordinal);
        Assert.DoesNotContain("PdfSharp", code, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("MigraDoc", code, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CompositionInjectsConcretePdfExporterIntoReports()
    {
        var app = Read("src/SistemaDocente.App.Wpf/App.xaml.cs");

        Assert.Contains("new ExportadorReportesPdf()", app, StringComparison.Ordinal);
        Assert.Contains("GestionReportesViewModel", app, StringComparison.Ordinal);
    }
}