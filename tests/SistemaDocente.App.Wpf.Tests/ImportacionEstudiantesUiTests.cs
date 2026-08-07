using System.IO;

namespace SistemaDocente.App.Wpf.Tests;

public sealed class ImportacionEstudiantesUiTests
{
    private static string ObtenerRaiz() => Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static string Leer(string rutaRelativa) => File.ReadAllText(Path.Combine(
        ObtenerRaiz(), rutaRelativa.Replace('/', Path.DirectorySeparatorChar)));

    [Fact]
    public void GrupoExponeImportacionYShellEntregaViewModelDeFormaExplicita()
    {
        var grupo = Leer("src/SistemaDocente.App.Wpf/Views/GrupoView.xaml");
        var main = Leer("src/SistemaDocente.App.Wpf/MainWindow.xaml");
        var mainCodeBehind = Leer("src/SistemaDocente.App.Wpf/MainWindow.xaml.cs");
        var codigoGrupo = Leer("src/SistemaDocente.App.Wpf/Views/GrupoView.xaml.cs");

        Assert.Contains("Importar alumnos…", grupo, StringComparison.Ordinal);
        Assert.Contains("OnImportarAlumnosClic", grupo, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"GrupoModule\"", main, StringComparison.Ordinal);
        Assert.DoesNotContain("Importacion=\"{Binding ImportacionEstudiantes, ElementName=RootWindow}\"", main, StringComparison.Ordinal);
        Assert.Contains("GrupoModule.Importacion = importacionEstudiantes;", mainCodeBehind, StringComparison.Ordinal);
        Assert.Contains("new ImportacionEstudiantesWindow(importacion)", codigoGrupo, StringComparison.Ordinal);
    }

    [Fact]
    public void AsistenteMantieneFlujoPreviewFirstEnUnaSolaVentana()
    {
        var xaml = Leer("src/SistemaDocente.App.Wpf/ImportacionEstudiantesWindow.xaml");

        Assert.Contains("Selecciona una lista de alumnos", xaml, StringComparison.Ordinal);
        Assert.Contains("Relaciona las columnas", xaml, StringComparison.Ordinal);
        Assert.Contains("Vista previa", xaml, StringComparison.Ordinal);
        Assert.Contains("Confirma la importación", xaml, StringComparison.Ordinal);
        Assert.Contains("Importación completada", xaml, StringComparison.Ordinal);
        Assert.Contains("nada se guardará al seleccionar el archivo", xaml, StringComparison.Ordinal);
        Assert.Contains("Los alumnos existentes no se modificarán", xaml, StringComparison.Ordinal);
        Assert.Contains("EstadoTexto", xaml, StringComparison.Ordinal);
        Assert.Contains("Revalidar cambios", xaml, StringComparison.Ordinal);
        Assert.Contains("Incluir / excluir fila", xaml, StringComparison.Ordinal);
        Assert.Contains("Importar duplicado como nuevo", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void MapeoDeclaraCurpFueraDeDestinosYPermiteCorregirFila()
    {
        var xaml = Leer("src/SistemaDocente.App.Wpf/ImportacionEstudiantesWindow.xaml");
        var presentacion = Leer("src/SistemaDocente.Presentation/ImportacionEstudiantesViewModel.cs");

        Assert.Contains("CURP no es un destino disponible", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("CampoImportacionEstudiante.Curp", presentacion, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("FilaSeleccionada.PrimerApellido", xaml, StringComparison.Ordinal);
        Assert.Contains("FilaSeleccionada.SegundoApellido", xaml, StringComparison.Ordinal);
        Assert.Contains("FilaSeleccionada.Nombres", xaml, StringComparison.Ordinal);
        Assert.Contains("FilaSeleccionada.FechaNacimientoTexto", xaml, StringComparison.Ordinal);
        Assert.Contains("FilaSeleccionada.GeneroTexto", xaml, StringComparison.Ordinal);
        Assert.Contains("FilaSeleccionada.Observaciones", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void SelectorNativoAdmiteSoloXlsxYCsvYEntregaRutaAlViewModel()
    {
        var codigo = Leer("src/SistemaDocente.App.Wpf/ImportacionEstudiantesWindow.xaml.cs");

        Assert.Contains("OpenFileDialog", codigo, StringComparison.Ordinal);
        Assert.Contains("*.xlsx;*.csv", codigo, StringComparison.Ordinal);
        Assert.Contains("_viewModel.CargarArchivo(dialogo.FileName)", codigo, StringComparison.Ordinal);
        Assert.DoesNotContain("SpreadsheetDocument", codigo, StringComparison.Ordinal);
        Assert.DoesNotContain("Sqlite", codigo, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ComposicionMantieneInterchangeFueraDeCodeBehind()
    {
        var app = Leer("src/SistemaDocente.App.Wpf/App.xaml.cs");
        var proyecto = Leer("src/SistemaDocente.App.Wpf/SistemaDocente.App.Wpf.csproj");

        Assert.Contains("new ImportacionEstudiantesCasosUso", app, StringComparison.Ordinal);
        Assert.Contains("new LectorImportacionTabular()", app, StringComparison.Ordinal);
        Assert.Contains("new ImportacionEstudiantesViewModel", app, StringComparison.Ordinal);
        Assert.Contains("SistemaDocente.Interchange", proyecto, StringComparison.Ordinal);
    }
}