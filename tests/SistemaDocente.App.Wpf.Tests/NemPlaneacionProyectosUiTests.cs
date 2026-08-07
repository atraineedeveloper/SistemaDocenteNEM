using System.IO;

namespace SistemaDocente.App.Wpf.Tests;

public sealed class NemPlaneacionProyectosUiTests
{
    private static string ObtenerRaiz() => Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static string Leer(string rutaRelativa) => File.ReadAllText(Path.Combine(
        ObtenerRaiz(), rutaRelativa.Replace('/', Path.DirectorySeparatorChar)));

    [Fact]
    public void ShellEntregaConfiguracionDeGrupoAlModuloProyectos()
    {
        var main = Leer("src/SistemaDocente.App.Wpf/MainWindow.xaml");
        var proyectosCode = Leer("src/SistemaDocente.App.Wpf/Views/ProyectosView.xaml.cs");

        Assert.Contains("<views:ProyectosView", main, StringComparison.Ordinal);
        Assert.Contains("Configuracion=\"{Binding ConfiguracionGrupo, ElementName=RootWindow}\"", main, StringComparison.Ordinal);
        Assert.Contains("Configuracion.Inicializar(grupoId)", proyectosCode, StringComparison.Ordinal);
        Assert.Contains("vm.ConfigurarGradosDisponibles(Configuracion.ObtenerGradosConfigurados())", proyectosCode, StringComparison.Ordinal);
    }

    [Fact]
    public void EditorProyectoExponeMetodologiaYGradosObjetivo()
    {
        var xaml = Leer("src/SistemaDocente.App.Wpf/DetalleProyectoWindow.xaml");

        Assert.Contains("ItemsSource=\"{Binding MetodologiasProyecto}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("SelectedValue=\"{Binding MetodologiaProyecto, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("ItemsSource=\"{Binding GradosProyecto}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("IsChecked=\"{Binding Seleccionado, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}\"", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void EditorActividadExponeCampoFormativoYProtegeGradosHistoricos()
    {
        var xaml = Leer("src/SistemaDocente.App.Wpf/DetalleActividadWindow.xaml");

        Assert.Contains("ItemsSource=\"{Binding CamposFormativos}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("SelectedValue=\"{Binding CampoFormativoActividad, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("ItemsSource=\"{Binding GradosActividad}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("DataContext.PuedeEditarGradosActividad", xaml, StringComparison.Ordinal);
        Assert.Contains("el alcance queda fijo para proteger su padrón histórico", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void ListasMuestranMetadatosNemCompactos()
    {
        var proyectos = Leer("src/SistemaDocente.App.Wpf/Views/ProyectosView.xaml");
        var detalle = Leer("src/SistemaDocente.App.Wpf/DetalleProyectoWindow.xaml");
        var converters = Leer("src/SistemaDocente.App.Wpf/Converters/NemPlaneacionConverters.cs");

        Assert.Contains("MetodologiaProyectoNemConverter", proyectos, StringComparison.Ordinal);
        Assert.Contains("Binding Metodologia, Converter={StaticResource MetodologiaNem}", proyectos, StringComparison.Ordinal);
        Assert.Contains("GradosObjetivoConverter", proyectos, StringComparison.Ordinal);
        Assert.Contains("CampoFormativoNemConverter", detalle, StringComparison.Ordinal);
        Assert.Contains("Binding CampoFormativo, Converter={StaticResource CampoFormativoNem}", detalle, StringComparison.Ordinal);
        Assert.Contains("Sin grados definidos", converters, StringComparison.Ordinal);
    }
}