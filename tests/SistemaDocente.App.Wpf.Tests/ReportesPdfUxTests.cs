using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;

using SistemaDocente.App.Wpf.Views;

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
    public void ReportsViewConstruyeAccionGuardarPdfReal()
    {
        Exception? capturada = null;
        bool? encontroAccion = null;
        var thread = new Thread(() =>
        {
            try
            {
                if (System.Windows.Application.Current is null)
                {
                    var app = new App();
                    app.InitializeComponent();
                }

                var vista = new ReportesView();
                vista.Measure(new System.Windows.Size(1280, 780));
                vista.Arrange(new System.Windows.Rect(0, 0, 1280, 780));
                vista.UpdateLayout();
                encontroAccion = EncontrarBotonPdf(vista);
            }
            catch (Exception exception)
            {
                capturada = exception;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(capturada);
        Assert.True(encontroAccion);
    }

    [Fact]
    public void CompositionInjectsConcretePdfExporterIntoReports()
    {
        var app = Read("src/SistemaDocente.App.Wpf/App.xaml.cs");

        Assert.Contains("new ExportadorReportesPdf()", app, StringComparison.Ordinal);
        Assert.Contains("GestionReportesViewModel", app, StringComparison.Ordinal);
    }

    private static bool EncontrarBotonPdf(DependencyObject raiz)
    {
        foreach (var hijo in LogicalTreeHelper.GetChildren(raiz).OfType<DependencyObject>())
        {
            if (hijo is Button boton
                && string.Equals(
                    AutomationProperties.GetName(boton),
                    "Guardar reporte actual como PDF",
                    StringComparison.Ordinal))
            {
                return true;
            }

            if (EncontrarBotonPdf(hijo)) return true;
        }
        return false;
    }
}