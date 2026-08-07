using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;

using SistemaDocente.Application;
using SistemaDocente.Data;
using SistemaDocente.Presentation;

namespace SistemaDocente.App.Wpf.Tests;

public sealed class ImportacionEstudiantesWindowRuntimeTests
{
    [Fact]
    public void EstadoInicialYConvertidorOcultanPasosNoActivos()
    {
        var converter = new BooleanToVisibilityConverter();
        Assert.Equal(
            Visibility.Visible,
            converter.Convert(true, typeof(Visibility), null, CultureInfo.InvariantCulture));
        Assert.Equal(
            Visibility.Collapsed,
            converter.Convert(false, typeof(Visibility), null, CultureInfo.InvariantCulture));

        var viewModel = ConstruirViewModel();
        Assert.True(viewModel.MostrarArchivo);
        Assert.False(viewModel.MostrarColumnas);
        Assert.False(viewModel.MostrarPrevia);
        Assert.False(viewModel.MostrarConfirmacion);
        Assert.False(viewModel.MostrarResultado);
        Assert.False(viewModel.GenerarPreviaCommand.CanExecute(null));
        Assert.False(viewModel.PrepararConfirmacionCommand.CanExecute(null));
        Assert.False(viewModel.ConfirmarCommand.CanExecute(null));
        Assert.False(viewModel.VolverCommand.CanExecute(null));

        var xaml = File.ReadAllText(Path.Combine(
            ObtenerRaiz(),
            "src",
            "SistemaDocente.App.Wpf",
            "ImportacionEstudiantesWindow.xaml"));
        Assert.Contains("BooleanToVisibilityConverter x:Key=\"ImportBoolToVisibility\"", xaml, StringComparison.Ordinal);
        Assert.Contains("PasoArchivoPanel", xaml, StringComparison.Ordinal);
        Assert.Contains("PasoResultadoPanel", xaml, StringComparison.Ordinal);
        Assert.Contains("Converter={StaticResource ImportBoolToVisibility}", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Converter={StaticResource BoolToVisibility}", xaml, StringComparison.Ordinal);
    }

    private static ImportacionEstudiantesViewModel ConstruirViewModel()
    {
        var directorio = Path.Combine(
            Path.GetTempPath(),
            "SistemaDocenteNEM-ImportState-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directorio);
        var baseSqlite = Path.Combine(directorio, "sistema-docente.db");
        var grupos = new PersistenciaGrupoSqlite(baseSqlite);
        var contextos = new PersistenciaContextoGrupoSqlite(baseSqlite);

        return new ImportacionEstudiantesViewModel(
            new SistemaDocente.Interchange.LectorImportacionTabular(),
            new ImportacionEstudiantesCasosUso(grupos, contextos));
    }

    private static string ObtenerRaiz() => Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
}