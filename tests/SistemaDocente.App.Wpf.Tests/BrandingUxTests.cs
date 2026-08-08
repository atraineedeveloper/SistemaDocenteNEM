using System.IO;

namespace SistemaDocente.App.Wpf.Tests;

public sealed class BrandingUxTests
{
    private static string Root() => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static string Read(string relativePath) =>
        File.ReadAllText(Path.Combine(Root(), relativePath.Replace('/', Path.DirectorySeparatorChar)));

    [Fact]
    public void HeaderAndWindowTitleUseCentralAulaRaizBrand()
    {
        var header = Read("src/SistemaDocente.App.Wpf/Controls/MainNavigationHeader.xaml");
        var shell = Read("src/SistemaDocente.Presentation/MainWindowViewModel.cs");

        Assert.Contains("IdentidadProducto.Nombre", header, StringComparison.Ordinal);
        Assert.Contains("IdentidadProducto.Subtitulo", header, StringComparison.Ordinal);
        Assert.Contains("Text=\"AR\"", header, StringComparison.Ordinal);
        Assert.Contains("IdentidadProducto.Nombre", shell, StringComparison.Ordinal);
        Assert.DoesNotContain("Sistema Docente Local", header, StringComparison.Ordinal);
        Assert.DoesNotContain("Sistema Docente Local", shell, StringComparison.Ordinal);
    }

    [Fact]
    public void RecoveryUiUsesAulaRaizWithoutChangingLegacyBackupIdentity()
    {
        var window = Read("src/SistemaDocente.App.Wpf/RecuperacionLocalWindow.xaml");
        var codeBehind = Read("src/SistemaDocente.App.Wpf/RecuperacionLocalWindow.xaml.cs");
        var recoveryData = Read("src/SistemaDocente.Data/ServicioRecuperacionLocalSqlite.cs");

        Assert.Contains("AulaRaíz", window, StringComparison.Ordinal);
        Assert.Contains("IdentidadProducto.Nombre", codeBehind, StringComparison.Ordinal);
        Assert.Contains("SistemaDocenteNEM.Backup", recoveryData, StringComparison.Ordinal);
    }
}