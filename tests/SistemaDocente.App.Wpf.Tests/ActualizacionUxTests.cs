using System.IO;

namespace SistemaDocente.App.Wpf.Tests;

public sealed class ActualizacionUxTests
{
    private static string Root() => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static string Read(string relativePath) =>
        File.ReadAllText(Path.Combine(Root(), relativePath.Replace('/', Path.DirectorySeparatorChar)));

    [Fact]
    public void ShellExposesManualUpdateAndAutomaticCheckWithoutReplacingWpfInPlace()
    {
        var header = Read("src/SistemaDocente.App.Wpf/Controls/MainNavigationHeader.xaml");
        var app = Read("src/SistemaDocente.App.Wpf/App.xaml.cs");
        var dialog = Read("src/SistemaDocente.App.Wpf/ActualizacionWindow.xaml.cs");

        Assert.Contains("Buscar actualizaciones de AulaRaíz", header, StringComparison.Ordinal);
        Assert.Contains("ComprobarActualizacionesAlInicioAsync", app, StringComparison.Ordinal);
        Assert.Contains("AulaRaiz.Updater.run.exe", dialog, StringComparison.Ordinal);
        Assert.Contains("Path.GetDirectoryName(verificada.RutaInstalador)", dialog, StringComparison.Ordinal);
        Assert.DoesNotContain("File.Copy(app", dialog, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void UpdateDialogRequiresDownloadThenExplicitCloseAndUpdate()
    {
        var xaml = Read("src/SistemaDocente.App.Wpf/ActualizacionWindow.xaml");
        var code = Read("src/SistemaDocente.App.Wpf/ActualizacionWindow.xaml.cs");

        Assert.Contains("Descargar e instalar", xaml, StringComparison.Ordinal);
        Assert.Contains("Más tarde", xaml, StringComparison.Ordinal);
        Assert.Contains("Cerrar y actualizar", code, StringComparison.Ordinal);
        Assert.Contains("PrepararCierreParaActualizacion", code, StringComparison.Ordinal);
        Assert.Contains("--sha256", code, StringComparison.Ordinal);
        Assert.Contains("--target-version", code, StringComparison.Ordinal);
        Assert.DoesNotContain("--demo-reset", code, StringComparison.Ordinal);
    }

    [Fact]
    public void RestartFeedbackUsesOnlyTechnicalVersionArgument()
    {
        var app = Read("src/SistemaDocente.App.Wpf/App.xaml.cs");
        var updater = Read("src/SistemaDocente.Updater/Program.cs");

        Assert.Contains("--updated-to", app, StringComparison.Ordinal);
        Assert.Contains("--updated-to", updater, StringComparison.Ordinal);
        Assert.Contains("--demo", updater, StringComparison.Ordinal);
        Assert.DoesNotContain("--demo-reset", updater, StringComparison.Ordinal);
    }
}
