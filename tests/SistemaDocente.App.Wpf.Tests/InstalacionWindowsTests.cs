using System.IO;

namespace SistemaDocente.App.Wpf.Tests;

public sealed class InstalacionWindowsTests
{
    private static string Root() => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static string Read(string relativePath) =>
        File.ReadAllText(Path.Combine(Root(), relativePath.Replace('/', Path.DirectorySeparatorChar)));

    [Fact]
    public void PublishProfileIsSelfContainedX64WithoutTrimming()
    {
        var profile = Read("src/SistemaDocente.App.Wpf/Properties/PublishProfiles/win-x64-self-contained.pubxml");

        Assert.Contains("<RuntimeIdentifier>win-x64</RuntimeIdentifier>", profile, StringComparison.Ordinal);
        Assert.Contains("<SelfContained>true</SelfContained>", profile, StringComparison.Ordinal);
        Assert.Contains("<PublishSingleFile>false</PublishSingleFile>", profile, StringComparison.Ordinal);
        Assert.Contains("<PublishTrimmed>false</PublishTrimmed>", profile, StringComparison.Ordinal);
    }

    [Fact]
    public void InstallerUsesStablePerUserIdentityAndDoesNotOwnLegacyDataPath()
    {
        var installer = Read("installer/AulaRaiz.iss");

        Assert.Contains("AppId={{7A2B71C7-3BC4-4D54-A7A2-97A0D56D4E5B}", installer, StringComparison.Ordinal);
        Assert.Contains("PrivilegesRequired=lowest", installer, StringComparison.Ordinal);
        Assert.Contains("DefaultDirName={localappdata}\\Programs\\AulaRaiz", installer, StringComparison.Ordinal);
        Assert.Contains("UsePreviousAppDir=yes", installer, StringComparison.Ordinal);
        Assert.Contains("Tasks: desktopicon", installer, StringComparison.Ordinal);
        Assert.DoesNotContain("SistemaDocenteNEM", installer, StringComparison.Ordinal);
        Assert.DoesNotContain("sistema-docente.db", installer, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void InstalledVersionIsVisibleInTheMainHeader()
    {
        var header = Read("src/SistemaDocente.App.Wpf/Controls/MainNavigationHeader.xaml");
        var props = Read("Directory.Build.props");

        Assert.Contains("IdentidadProducto.VersionVisible", header, StringComparison.Ordinal);
        Assert.Contains("Versión instalada de AulaRaíz", header, StringComparison.Ordinal);
        Assert.Contains("<VersionPrefix>0.1.0</VersionPrefix>", props, StringComparison.Ordinal);
    }

    [Fact]
    public void InstallerCiProvesUserDataSurvivesUninstall()
    {
        var workflow = Read(".github/workflows/installer.yml");

        Assert.Contains("gh release verify-asset", workflow, StringComparison.Ordinal);
        Assert.Contains("installer-ci-sentinel.txt", workflow, StringComparison.Ordinal);
        Assert.Contains("Smoke-test install, upgrade and uninstall", workflow, StringComparison.Ordinal);
        Assert.Contains("Uninstall deleted the legacy user-data sentinel", workflow, StringComparison.Ordinal);
        Assert.Contains("actions/upload-artifact@v7", workflow, StringComparison.Ordinal);
    }
}
