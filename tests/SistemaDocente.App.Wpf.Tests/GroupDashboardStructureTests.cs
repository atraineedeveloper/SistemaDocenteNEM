using System.IO;

namespace SistemaDocente.App.Wpf.Tests;

public sealed class GroupDashboardStructureTests
{
    [Fact]
    public void ResumenUsaDosMetricasYNoMantieneLaBarraInferior()
    {
        var raiz = ObtenerRaizRepositorio();
        var xaml = File.ReadAllText(Path.Combine(
            raiz,
            "src",
            "SistemaDocente.App.Wpf",
            "Views",
            "GrupoView.xaml"));

        Assert.Contains("Text=\"Total\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"Activos\"", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Text=\"Promedio de edad\"", xaml, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Acciones masivas", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Content=\"▱  Ver expediente\"", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void ToolbarMantieneAgregarVisibleYDatosEnMenuSecundario()
    {
        var raiz = ObtenerRaizRepositorio();
        var xaml = File.ReadAllText(Path.Combine(
            raiz,
            "src",
            "SistemaDocente.App.Wpf",
            "Views",
            "GrupoView.xaml"));

        Assert.Contains("Content=\"＋  Agregar estudiante\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Header=\"Importar alumnos…\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Header=\"Exportar datos…\"", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"StudentStatusFilterCombo\"", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"StudentSortCombo\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Content=\"Activos\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Content=\"Inactivos\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Content=\"Nombre A–Z\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Content=\"Nombre Z–A\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Content=\"Número de lista\"", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void FilasExponenOverflowClicDerechoYDobleClic()
    {
        var raiz = ObtenerRaizRepositorio();
        var xaml = File.ReadAllText(Path.Combine(
            raiz,
            "src",
            "SistemaDocente.App.Wpf",
            "Views",
            "GrupoView.xaml"));
        var codeBehind = File.ReadAllText(Path.Combine(
            raiz,
            "src",
            "SistemaDocente.App.Wpf",
            "Views",
            "GrupoView.xaml.cs"));

        Assert.Contains("x:Name=\"StudentGrid\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Click=\"OnStudentMoreClick\"", xaml, StringComparison.Ordinal);
        Assert.Contains("PreviewMouseRightButtonUp=\"OnStudentGridPreviewMouseRightButtonUp\"", xaml, StringComparison.Ordinal);
        Assert.Contains("MouseDoubleClick=\"OnStudentGridMouseDoubleClick\"", xaml, StringComparison.Ordinal);

        Assert.Contains("SeleccionarEstudiante(estudiante);", codeBehind, StringComparison.Ordinal);
        Assert.Contains("AbrirMenuContextualEstudiante", codeBehind, StringComparison.Ordinal);
        Assert.Contains("Header = \"Ver expediente\"", codeBehind, StringComparison.Ordinal);
        Assert.Contains("Header = \"Editar estudiante\"", codeBehind, StringComparison.Ordinal);
        Assert.Contains("Header = \"Desactivar estudiante\"", codeBehind, StringComparison.Ordinal);
        Assert.Contains("Header = \"Reactivar estudiante\"", codeBehind, StringComparison.Ordinal);
        Assert.Contains("AbrirExpedienteEstudiante();", codeBehind, StringComparison.Ordinal);
    }

    [Fact]
    public void DashboardUsaRecursosSemanticosEnLugarDeColoresFijos()
    {
        var raiz = ObtenerRaizRepositorio();
        var xaml = File.ReadAllText(Path.Combine(
            raiz,
            "src",
            "SistemaDocente.App.Wpf",
            "Views",
            "GrupoView.xaml"));

        Assert.Contains("{DynamicResource CardBackgroundBrush}", xaml, StringComparison.Ordinal);
        Assert.Contains("{DynamicResource TextPrimaryBrush}", xaml, StringComparison.Ordinal);
        Assert.Contains("{DynamicResource SuccessBackgroundBrush}", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Background=\"#", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Foreground=\"#", xaml, StringComparison.Ordinal);
    }

    private static string ObtenerRaizRepositorio() => Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory,
        "..",
        "..",
        "..",
        "..",
        ".."));
}