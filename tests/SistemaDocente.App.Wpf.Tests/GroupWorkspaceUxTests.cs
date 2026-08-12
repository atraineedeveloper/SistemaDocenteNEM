using System.IO;
using System.Text.RegularExpressions;

namespace SistemaDocente.App.Wpf.Tests;

public sealed class GroupWorkspaceUxTests
{
    private static string Root() => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static string Read(string relativePath) => File.ReadAllText(Path.Combine(Root(), relativePath.Replace('/', Path.DirectorySeparatorChar)));

    [Fact]
    public void MainWindowMountsExplicitGroupWorkspace()
    {
        var main = Read("src/SistemaDocente.App.Wpf/MainWindow.xaml");
        var shell = Read("src/SistemaDocente.Presentation/MainWindowViewModel.cs");
        var workspace = Read("src/SistemaDocente.App.Wpf/Views/InicioGruposView.xaml");

        Assert.Contains("InicioGruposView", main, StringComparison.Ordinal);
        Assert.Contains("MostrarInicio", main, StringComparison.Ordinal);
        Assert.Contains("public bool CambiarGrupo", shell, StringComparison.Ordinal);
        Assert.Contains("Mis grupos", workspace, StringComparison.Ordinal);
        Assert.Contains("GroupWorkspaceCard", workspace, StringComparison.Ordinal);
        Assert.Contains("Content=\"Abrir grupo\"", workspace, StringComparison.Ordinal);
    }

    [Fact]
    public void HeaderUsesCompactContextSwitcherInsteadOfWideGroupComboBox()
    {
        var xaml = Read("src/SistemaDocente.App.Wpf/Controls/MainNavigationHeader.xaml");
        var code = Read("src/SistemaDocente.App.Wpf/Controls/MainNavigationHeader.xaml.cs");

        Assert.Contains("GrupoContextMenu", xaml, StringComparison.Ordinal);
        Assert.Contains("Cambiar grupo", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("DisplayMemberPath=\"NombreGrupo\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Mis grupos", code, StringComparison.Ordinal);
        Assert.Contains("vm.CambiarGrupo", code, StringComparison.Ordinal);
    }

    [Fact]
    public void AttendanceAndEvaluationExposeDirectCellMenus()
    {
        var attendance = Read("src/SistemaDocente.App.Wpf/Views/AsistenciaView.xaml.cs");
        var evaluationXaml = Read("src/SistemaDocente.App.Wpf/Views/EvaluacionView.xaml");
        var evaluationCode = Read("src/SistemaDocente.App.Wpf/Views/EvaluacionView.xaml.cs");

        Assert.Contains("PreviewMouseLeftButtonUp += OnGrillaMensualClick", attendance, StringComparison.Ordinal);
        Assert.Contains("Presente (P)", attendance, StringComparison.Ordinal);
        Assert.Contains("RowHeight = 42", attendance, StringComparison.Ordinal);
        Assert.Contains("PreviewMouseLeftButtonUp=\"OnGrillaEvaluacionClick\"", evaluationXaml, StringComparison.Ordinal);
        Assert.Contains("Entregada · evaluar después (T)", evaluationCode, StringComparison.Ordinal);
        Assert.Contains("Más opciones…", evaluationCode, StringComparison.Ordinal);
    }

    [Fact]
    public void StudentRecordUsesFollowUpInformationHierarchyAndSemanticResources()
    {
        var xaml = Read("src/SistemaDocente.App.Wpf/ExpedienteEstudianteWindow.xaml");

        Assert.Contains("Header=\"Resumen\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Header=\"Seguimiento\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Header=\"Actividades\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Header=\"Familia\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Historial de seguimiento", xaml, StringComparison.Ordinal);
        Assert.Contains("Styles/PopupStyles.xaml", xaml, StringComparison.Ordinal);
        Assert.False(Regex.IsMatch(xaml, "#[0-9A-Fa-f]{6,8}", RegexOptions.CultureInvariant));
    }
}
