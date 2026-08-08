using System.IO;

namespace SistemaDocente.App.Wpf.Tests;

public sealed class PrivacyLoggingTests
{
    private static string Root() => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    [Fact]
    public void AppUsesStructuredSafeDiagnosticsInsteadOfRawExceptions()
    {
        var source = File.ReadAllText(Path.Combine(Root(), "src", "SistemaDocente.App.Wpf", "App.xaml.cs"));

        Assert.Contains("RegistroDiagnosticoSeguroArchivo", source, StringComparison.Ordinal);
        Assert.Contains("CategoriaEventoDiagnostico.FalloNoControlado", source, StringComparison.Ordinal);
        Assert.Contains("CategoriaEventoDiagnostico.FalloInicioAlmacenamiento", source, StringComparison.Ordinal);
        Assert.DoesNotContain("crash.log", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("File.AppendAllText", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Debug.WriteLine", source, StringComparison.Ordinal);
        Assert.DoesNotContain("exception.ToString", source, StringComparison.OrdinalIgnoreCase);
    }
}