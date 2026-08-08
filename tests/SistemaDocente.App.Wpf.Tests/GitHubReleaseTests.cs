using System.IO;

namespace SistemaDocente.App.Wpf.Tests;

public sealed class GitHubReleaseTests
{
    private static string Root() => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static string Read(string relativePath) =>
        File.ReadAllText(Path.Combine(Root(), relativePath.Replace('/', Path.DirectorySeparatorChar)));

    [Fact]
    public void ReleaseWorkflowRequiresVersionedTagMatchingRepositoryVersion()
    {
        var workflow = Read(".github/workflows/release.yml");

        Assert.Contains("v*.*.*", workflow, StringComparison.Ordinal);
        Assert.Contains("contents: write", workflow, StringComparison.Ordinal);
        Assert.Contains("vMAJOR.MINOR.PATCH", workflow, StringComparison.Ordinal);
        Assert.Contains("VersionPrefix", workflow, StringComparison.Ordinal);
        Assert.Contains("RELEASE_IS_PRERELEASE", workflow, StringComparison.Ordinal);
    }

    [Fact]
    public void ReleaseWorkflowRunsQualityGatesAndReusesInstallerBuild()
    {
        var workflow = Read(".github/workflows/release.yml");

        Assert.Contains("dotnet format SistemaDocente.sln --verify-no-changes --no-restore", workflow, StringComparison.Ordinal);
        Assert.Contains("dotnet build SistemaDocente.sln --configuration Release --no-restore", workflow, StringComparison.Ordinal);
        Assert.Contains("dotnet test SistemaDocente.sln --configuration Release --no-build", workflow, StringComparison.Ordinal);
        Assert.Contains("openspec validate --all", workflow, StringComparison.Ordinal);
        Assert.Contains("gh release verify-asset", workflow, StringComparison.Ordinal);
        Assert.Contains("scripts\\build-installer.ps1", workflow, StringComparison.Ordinal);
    }

    [Fact]
    public void ReleaseWorkflowPublishesInstallerChecksumAndGeneratedNotes()
    {
        var workflow = Read(".github/workflows/release.yml");

        Assert.Contains("SHA256SUMS.txt", workflow, StringComparison.Ordinal);
        Assert.Contains("gh @arguments", workflow, StringComparison.Ordinal);
        Assert.Contains("'--verify-tag'", workflow, StringComparison.Ordinal);
        Assert.Contains("'--generate-notes'", workflow, StringComparison.Ordinal);
        Assert.Contains("'--prerelease'", workflow, StringComparison.Ordinal);
        Assert.Contains("'--latest=false'", workflow, StringComparison.Ordinal);
        Assert.Contains("sin firma Authenticode", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("dotnet nuget push", workflow, StringComparison.OrdinalIgnoreCase);
    }
}
