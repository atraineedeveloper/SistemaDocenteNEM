using System.IO;

namespace SistemaDocente.App.Wpf.Tests;

public sealed class GroupCreationWizardStructureTests
{
    [Fact]
    public void CrearGrupoExponeWizardOpcionalDeCincoPasos()
    {
        var raiz = ObtenerRaizRepositorio();
        var wizard = File.ReadAllText(Path.Combine(
            raiz,
            "src",
            "SistemaDocente.App.Wpf",
            "Views",
            "CrearGrupoView.xaml"));

        Assert.Contains("Text=\"{Binding ProgresoCreacionGrupo}\"", wizard, StringComparison.Ordinal);
        Assert.Contains("Header=\"Nombre del grupo *\"", wizard, StringComparison.Ordinal);
        Assert.Contains("ConfiguracionGrupo.PrimerGrado", wizard, StringComparison.Ordinal);
        Assert.Contains("ConfiguracionGrupo.NombreEscuela", wizard, StringComparison.Ordinal);
        Assert.Contains("ConfiguracionGrupo.Cct", wizard, StringComparison.Ordinal);
        Assert.Contains("ConfiguracionGrupo.CicloEscolar", wizard, StringComparison.Ordinal);
        Assert.Contains("ConfiguracionGrupo.EntidadFederativa", wizard, StringComparison.Ordinal);
        Assert.Contains("ConfiguracionGrupo.Municipio", wizard, StringComparison.Ordinal);
        Assert.Contains("ConfiguracionGrupo.Localidad", wizard, StringComparison.Ordinal);
        Assert.Contains("Content=\"Omitir por ahora\"", wizard, StringComparison.Ordinal);
        Assert.Contains("Command=\"{Binding VolverCreacionGrupoCommand}\"", wizard, StringComparison.Ordinal);
        Assert.Contains("Command=\"{Binding ConfirmarCreacionGrupoCommand}\"", wizard, StringComparison.Ordinal);
        Assert.DoesNotContain("Olvidar referencia", wizard, StringComparison.Ordinal);
    }

    [Fact]
    public void EditorEstudiantePresentaFechaNacimientoComoOpcional()
    {
        var raiz = ObtenerRaizRepositorio();
        var editor = File.ReadAllText(Path.Combine(
            raiz,
            "src",
            "SistemaDocente.App.Wpf",
            "EditorEstudianteWindow.xaml"));

        Assert.Contains("Header=\"Fecha de nacimiento (opcional)\"", editor, StringComparison.Ordinal);
        Assert.DoesNotContain("Header=\"Fecha de nacimiento *\"", editor, StringComparison.Ordinal);
    }

    private static string ObtenerRaizRepositorio() => Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory,
        "..",
        "..",
        "..",
        "..",
        ".."));
}