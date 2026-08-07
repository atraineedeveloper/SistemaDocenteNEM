using System.IO;

namespace SistemaDocente.App.Wpf.Tests;

public sealed class NemMultigradoUiTests
{
    private static string ObtenerRaiz() => Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static string Leer(string rutaRelativa) => File.ReadAllText(Path.Combine(
        ObtenerRaiz(), rutaRelativa.Replace('/', Path.DirectorySeparatorChar)));

    [Fact]
    public void ConfiguracionGrupoUsaCatalogosYDerivadosNem()
    {
        var xaml = Leer("src/SistemaDocente.App.Wpf/ConfiguracionGrupoWindow.xaml");

        Assert.Contains("ItemsSource=\"{Binding EntidadesFederativas}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("ItemsSource=\"{Binding MunicipiosDisponibles}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("ItemsSource=\"{Binding OrganizacionesEscolares}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("IsChecked=\"{Binding PrimerGrado}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("IsChecked=\"{Binding SextoGrado}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"{Binding ModalidadGrupo}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"{Binding FasesNemTexto}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"{Binding ReferenciaDesarrolloTexto}\"", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("SelectedValue=\"{Binding EtapaCognoscitiva", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void EditorYListaDeEstudiantesExponenGradoEstructurado()
    {
        var editor = Leer("src/SistemaDocente.App.Wpf/EditorEstudianteWindow.xaml");
        var grupo = Leer("src/SistemaDocente.App.Wpf/Views/GrupoView.xaml");
        var grupoCode = Leer("src/SistemaDocente.App.Wpf/Views/GrupoView.xaml.cs");

        Assert.Contains("ItemsSource=\"{Binding GradosDisponiblesEdicion}\"", editor, StringComparison.Ordinal);
        Assert.Contains("SelectedValue=\"{Binding GradoEdicion, Mode=TwoWay}\"", editor, StringComparison.Ordinal);
        Assert.Contains("Binding=\"{Binding GradoTexto}\"", grupo, StringComparison.Ordinal);
        Assert.Contains("ConfigurarGradosDisponibles(Configuracion.ObtenerGradosConfigurados())", grupoCode, StringComparison.Ordinal);
    }
}