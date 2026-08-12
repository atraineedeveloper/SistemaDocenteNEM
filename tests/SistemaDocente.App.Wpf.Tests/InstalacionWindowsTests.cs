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
        Assert.Contains("<VersionPrefix>0.2.6</VersionPrefix>", props, StringComparison.Ordinal);
        Assert.Contains("Buscar actualizaciones de AulaRaíz", header, StringComparison.Ordinal);
    }

    [Fact]
    public void InstallerCiProvesPublishedUpgradeAddsCliUpdaterAndPreservesUserData()
    {
        var workflow = Read(".github/workflows/installer.yml");
        var buildScript = Read("scripts/build-installer.ps1");

        Assert.Contains("gh release verify-asset", workflow, StringComparison.Ordinal);
        Assert.Contains("UPGRADE_BASELINE_VERSION: \"0.1.0\"", workflow, StringComparison.Ordinal);
        Assert.Contains("Acquire published upgrade baseline", workflow, StringComparison.Ordinal);
        Assert.Contains("gh release download $tag", workflow, StringComparison.Ordinal);
        Assert.Contains("SHA256SUMS.txt", workflow, StringComparison.Ordinal);
        Assert.Contains("Smoke-test published upgrade, CLI, updater and uninstall", workflow, StringComparison.Ordinal);
        Assert.Contains("Assert-WpfInstalledVersion $env:UPGRADE_BASELINE_VERSION", workflow, StringComparison.Ordinal);
        Assert.Contains("unexpectedly contains the future AulaRaíz CLI", workflow, StringComparison.Ordinal);
        Assert.Contains("unexpectedly contains the future AulaRaíz updater", workflow, StringComparison.Ordinal);
        Assert.Contains("Assert-WpfInstalledVersion $version", workflow, StringComparison.Ordinal);
        Assert.Contains("Assert-CliStatus $version", workflow, StringComparison.Ordinal);
        Assert.Contains("Assert-UpdaterInstalledVersion $version", workflow, StringComparison.Ordinal);
        Assert.Contains("aularaiz.exe", workflow, StringComparison.Ordinal);
        Assert.Contains("AulaRaiz.Updater.exe", workflow, StringComparison.Ordinal);
        Assert.Contains("installer-ci-sentinel.txt", workflow, StringComparison.Ordinal);
        Assert.Contains("Uninstall deleted the legacy user-data sentinel", workflow, StringComparison.Ordinal);
        Assert.Contains("actions/upload-artifact@v7", workflow, StringComparison.Ordinal);
        Assert.Contains("[string]$VersionOverride", buildScript, StringComparison.Ordinal);
        Assert.Contains("$updaterProjectPath", buildScript, StringComparison.Ordinal);
        Assert.Contains("PublishSingleFile=true", buildScript, StringComparison.Ordinal);
    }

    [Fact]
    public void InstallerPackagesLicensesAndThirdPartyProvenance()
    {
        var installer = Read("installer/AulaRaiz.iss");
        var notices = Read("THIRD-PARTY-NOTICES.txt");
        var montserratLicense = Read("third-party/montserrat/OFL.txt");
        var catalogSource = Read("src/SistemaDocente.Presentation/Data/estados-municipios.SOURCE.md");
        var inventory = Read("docs/packaged-component-license-inventory.md");
        var readiness = Read("docs/signpath-readiness.md");
        var dataProject = Read("src/SistemaDocente.Data/SistemaDocente.Data.csproj");

        Assert.Contains("DestName: \"LICENSE.txt\"", installer, StringComparison.Ordinal);
        Assert.Contains("THIRD-PARTY-NOTICES.txt", installer, StringComparison.Ordinal);
        Assert.Contains("DestName: \"Montserrat-OFL.txt\"", installer, StringComparison.Ordinal);
        Assert.Contains("Montserrat 9.000", notices, StringComparison.Ordinal);
        Assert.Contains("SIL Open Font License, Version 1.1", montserratLicense, StringComparison.Ordinal);
        Assert.Contains("Fuente: INEGI", notices, StringComparison.Ordinal);
        Assert.Contains("2,478 municipalities", notices, StringComparison.Ordinal);
        Assert.Contains("https://www.inegi.org.mx/servicios/catalogounico.html", catalogSource, StringComparison.Ordinal);
        Assert.Contains("2025-06-17", catalogSource, StringComparison.Ordinal);
        Assert.Contains("<PackageReference Include=\"SQLitePCLRaw.bundle_e_sqlite3\" Version=\"3.0.5\" />", dataProject, StringComparison.Ordinal);
        Assert.Contains("| AulaRaíz project code and original materials | 0.2.6 |", inventory, StringComparison.Ordinal);
        Assert.Contains("| SQLitePCLRaw.bundle_e_sqlite3 | 3.0.5 |", inventory, StringComparison.Ordinal);
        Assert.Contains("native `SQLite` package 3.53.4", inventory, StringComparison.Ordinal);
        Assert.Contains("| SQLitePCLRaw.bundle_e_sqlite3 | 3.0.5 |", readiness, StringComparison.Ordinal);
    }
}