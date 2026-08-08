using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;

using SistemaDocente.App.Wpf;
using SistemaDocente.Application;
using SistemaDocente.Data;
using SistemaDocente.Presentation;

namespace SistemaDocente.App.Wpf.Tests;

/// <summary>
/// Pruebas de regresión del refactor que convierte MainWindow en un shell
/// y extrae las vistas de Grupo, Asistencia, Proyectos y Evaluación.
/// </summary>
public sealed class RefactorMainWindowVistasTests
{
    private static string ObtenerRaiz() => Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static string LeerAplicacion(string nombre) => File.ReadAllText(Path.Combine(
        ObtenerRaiz(), "src", "SistemaDocente.App.Wpf", nombre));

    private static string LeerVista(string nombre) => File.ReadAllText(Path.Combine(
        ObtenerRaiz(), "src", "SistemaDocente.App.Wpf", "Views", nombre));

    private static string LeerControl(string nombre) => File.ReadAllText(Path.Combine(
        ObtenerRaiz(), "src", "SistemaDocente.App.Wpf", "Controls", nombre));

    private static string LeerPresentacion(string nombre) => File.ReadAllText(Path.Combine(
        ObtenerRaiz(), "src", "SistemaDocente.Presentation", nombre));

    [Fact]
    public void MainWindowEnsamblaVistasSeparadas()
    {
        var xaml = LeerAplicacion("MainWindow.xaml");

        Assert.Contains("views:GrupoView", xaml, StringComparison.Ordinal);
        Assert.Contains("views:AsistenciaView", xaml, StringComparison.Ordinal);
        Assert.Contains("views:ProyectosView", xaml, StringComparison.Ordinal);
        Assert.Contains("views:EvaluacionView", xaml, StringComparison.Ordinal);
        Assert.Contains("controls:MainNavigationHeader", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void MainWindowNoContieneLasPrincipalesDataGridDeModulos()
    {
        var xaml = LeerAplicacion("MainWindow.xaml");

        Assert.DoesNotContain("GrillaMensual", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("GrillaProyectosPrincipal", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("GrillaEntregasEvaluacion", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("<DataGrid.Columns>", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("EstudiantesFiltrados", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void BindingsDeModuloSeAnclanAlShellRaiz()
    {
        var xaml = LeerAplicacion("MainWindow.xaml");

        Assert.Contains("x:Name=\"RootWindow\"", xaml, StringComparison.Ordinal);
        Assert.Contains("DataContext=\"{Binding DataContext.Grupo, ElementName=RootWindow}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("DataContext=\"{Binding DataContext.ModuloAsistencia, ElementName=RootWindow}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("DataContext=\"{Binding DataContext.Proyectos, ElementName=RootWindow}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("DataContext=\"{Binding DataContext.Evaluacion, ElementName=RootWindow}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Visibility=\"{Binding DataContext.MostrarGrupo, ElementName=RootWindow", xaml, StringComparison.Ordinal);
        Assert.Contains("Visibility=\"{Binding DataContext.MostrarAsistencia, ElementName=RootWindow", xaml, StringComparison.Ordinal);
        Assert.Contains("Visibility=\"{Binding DataContext.MostrarProyectos, ElementName=RootWindow", xaml, StringComparison.Ordinal);
        Assert.Contains("Visibility=\"{Binding DataContext.MostrarEvaluacion, ElementName=RootWindow", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void AsistenciaUsaFronteraPropiaDeModulo()
    {
        var mainWindow = LeerAplicacion("MainWindow.xaml");
        var codeBehind = LeerVista("AsistenciaView.xaml.cs");
        var modulo = LeerPresentacion("ModuloAsistenciaViewModel.cs");

        Assert.Contains("DataContext=\"{Binding DataContext.ModuloAsistencia, ElementName=RootWindow}\"", mainWindow, StringComparison.Ordinal);
        Assert.Contains(nameof(ModuloAsistenciaViewModel), codeBehind, StringComparison.Ordinal);
        Assert.DoesNotContain(nameof(MainWindowViewModel), codeBehind, StringComparison.Ordinal);
        Assert.Contains("GestionAsistenciaViewModel Diaria", modulo, StringComparison.Ordinal);
        Assert.Contains("GestionAsistenciaMensualViewModel Mensual", modulo, StringComparison.Ordinal);
    }

    [Fact]
    public void GrupoViewRecibeExpedienteSinConocerMainWindowConcreto()
    {
        var mainWindow = LeerAplicacion("MainWindow.xaml");
        var grupoCs = LeerVista("GrupoView.xaml.cs");

        Assert.Contains("Expediente=\"{Binding DataContext.Expediente, ElementName=RootWindow}\"", mainWindow, StringComparison.Ordinal);
        Assert.Contains("DependencyProperty.Register", grupoCs, StringComparison.Ordinal);
        Assert.DoesNotContain("is MainWindow", grupoCs, StringComparison.Ordinal);
        Assert.DoesNotContain("MainWindowViewModel", grupoCs, StringComparison.Ordinal);
    }

    [Fact]
    public void GrupoViewMantieneDataGridFueraDeScrollViewerExterior()
    {
        var grupoXaml = LeerVista("GrupoView.xaml");

        Assert.DoesNotContain("<ScrollViewer", grupoXaml, StringComparison.Ordinal);
        Assert.Contains("Height=\"*\"", grupoXaml, StringComparison.Ordinal);
        Assert.Contains("ScrollViewer.CanContentScroll=\"True\"", grupoXaml, StringComparison.Ordinal);
        Assert.Contains("EnableRowVirtualization=\"True\"", grupoXaml, StringComparison.Ordinal);
    }

    [Fact]
    public void EncabezadoGestionaSuscripcionDeFormaIdempotente()
    {
        var codeBehind = LeerControl("MainNavigationHeader.xaml.cs");

        Assert.Contains("_viewModelSuscrito", codeBehind, StringComparison.Ordinal);
        Assert.Contains("ReferenceEquals(_viewModelSuscrito, nuevo)", codeBehind, StringComparison.Ordinal);
        Assert.Contains("Unloaded += OnUnloaded", codeBehind, StringComparison.Ordinal);
        Assert.Contains("PropertyChanged -= OnViewModelPropertyChanged", codeBehind, StringComparison.Ordinal);
    }

    [Fact]
    public void PageUpYPageDownSonContextualesALaGrillaMensual()
    {
        var xaml = LeerVista("AsistenciaView.xaml");
        var codeBehind = LeerVista("AsistenciaView.xaml.cs");

        Assert.DoesNotContain("PreviewKeyDown=\"OnPreviewKeyDown\"", xaml, StringComparison.Ordinal);
        Assert.Contains("PreviewKeyDown=\"OnGrillaMensualPreviewKeyDown\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Key.PageUp", codeBehind, StringComparison.Ordinal);
        Assert.Contains("Key.PageDown", codeBehind, StringComparison.Ordinal);
        Assert.Contains("GrillaMensual.IsAncestorOf(foco)", codeBehind, StringComparison.Ordinal);
        Assert.Contains("Keyboard.FocusedElement is TextBoxBase", codeBehind, StringComparison.Ordinal);
    }

    [Fact]
    public void VistasExtraidasNoHardcodeanColoresSemanticos()
    {
        var archivos = new[]
        {
            LeerAplicacion("MainWindow.xaml"), LeerAplicacion("MainWindow.xaml.cs"),
            LeerVista("GrupoView.xaml"), LeerVista("GrupoView.xaml.cs"),
            LeerVista("AsistenciaView.xaml"), LeerVista("AsistenciaView.xaml.cs"),
            LeerVista("ProyectosView.xaml"), LeerVista("ProyectosView.xaml.cs"),
            LeerVista("EvaluacionView.xaml"), LeerVista("EvaluacionView.xaml.cs"),
            LeerControl("MainNavigationHeader.xaml"), LeerControl("MainNavigationHeader.xaml.cs"),
        };

        var patronColor = new Regex("#[0-9A-Fa-f]{6,8}", RegexOptions.CultureInvariant);
        foreach (var archivo in archivos)
        {
            Assert.DoesNotMatch(patronColor, archivo);
        }
    }

    [Fact]
    public void GrupoViewReferenciaGestionGrupoViewModelPorDataContext()
    {
        var mainWindow = LeerAplicacion("MainWindow.xaml");
        var grupoXaml = LeerVista("GrupoView.xaml");
        var grupoCs = LeerVista("GrupoView.xaml.cs");

        Assert.Contains("DataContext=\"{Binding DataContext.Grupo, ElementName=RootWindow}\"", mainWindow, StringComparison.Ordinal);
        Assert.Contains("xmlns:controls=\"clr-namespace:SistemaDocente.App.Wpf.Controls\"", grupoXaml, StringComparison.Ordinal);
        Assert.Contains(nameof(GestionGrupoViewModel), grupoCs, StringComparison.Ordinal);
        Assert.Contains("OnVerExpedienteEstudianteClic", grupoXaml, StringComparison.Ordinal);
    }

    [Fact]
    public void ProyectosViewConservaAperturaDeDetalle()
    {
        var xaml = LeerVista("ProyectosView.xaml");
        var codeBehind = LeerVista("ProyectosView.xaml.cs");

        Assert.Contains("GrillaProyectosPrincipal", xaml, StringComparison.Ordinal);
        Assert.Contains("MouseDoubleClick=\"OnAbrirDetalleProyectoClic\"", xaml, StringComparison.Ordinal);
        Assert.Contains("NuevoProyectoCommand", xaml, StringComparison.Ordinal);
        Assert.Contains(nameof(DetalleProyectoWindow), codeBehind, StringComparison.Ordinal);
        Assert.Contains("Window.GetWindow(this)", codeBehind, StringComparison.Ordinal);
    }

    [Fact]
    public void LosCuatroModulosSiguenSiendoNavegables()
    {
        var header = LeerControl("MainNavigationHeader.xaml");

        Assert.Contains("NavBtnGrupo", header, StringComparison.Ordinal);
        Assert.Contains("NavBtnAsistencia", header, StringComparison.Ordinal);
        Assert.Contains("NavBtnProyectos", header, StringComparison.Ordinal);
        Assert.Contains("NavBtnEvaluacion", header, StringComparison.Ordinal);
        Assert.Contains("IrAGrupoCommand", header, StringComparison.Ordinal);
        Assert.Contains("IrAAsistenciaCommand", header, StringComparison.Ordinal);
        Assert.Contains("IrAProyectosCommand", header, StringComparison.Ordinal);
        Assert.Contains("IrAEvaluacionCommand", header, StringComparison.Ordinal);
    }

    [Fact]
    public void LosTemasSiguenDisponiblesDesdeElEncabezado()
    {
        var header = LeerControl("MainNavigationHeader.xaml");
        var codeBehind = LeerControl("MainNavigationHeader.xaml.cs");

        Assert.Contains("Tema", header, StringComparison.Ordinal);
        Assert.Contains("Claro", header, StringComparison.Ordinal);
        Assert.Contains("Oscuro", header, StringComparison.Ordinal);
        Assert.Contains("Alto contraste", header, StringComparison.Ordinal);
        Assert.Contains("ThemeService.ApplyTheme", codeBehind, StringComparison.Ordinal);
        Assert.Contains("HeaderIconBackgroundBrush", header, StringComparison.Ordinal);
    }

    [Fact]
    public void VistasYCodeBehindNoContienenSqlNiReglasDeNegocio()
    {
        var archivos = new[]
        {
            LeerVista("GrupoView.xaml"), LeerVista("GrupoView.xaml.cs"),
            LeerVista("AsistenciaView.xaml"), LeerVista("AsistenciaView.xaml.cs"),
            LeerVista("ProyectosView.xaml"), LeerVista("ProyectosView.xaml.cs"),
            LeerVista("EvaluacionView.xaml"), LeerVista("EvaluacionView.xaml.cs"),
            LeerControl("MainNavigationHeader.xaml"), LeerControl("MainNavigationHeader.xaml.cs"),
            LeerAplicacion("MainWindow.xaml"), LeerAplicacion("MainWindow.xaml.cs"),
        };

        foreach (var archivo in archivos)
        {
            Assert.DoesNotContain("SELECT ", archivo, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("INSERT ", archivo, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("UPDATE ", archivo, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("DELETE ", archivo, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Sqlite", archivo, StringComparison.OrdinalIgnoreCase);
        }

        var codeBehind = LeerAplicacion("MainWindow.xaml.cs");
        Assert.DoesNotContain("NivelLogro.", codeBehind, StringComparison.Ordinal);
        Assert.DoesNotContain("EstadoEntrega.", codeBehind, StringComparison.Ordinal);
    }

    /// <summary>
    /// Smoke test STA: construye MainWindow, fuerza InitializeComponent y layout.
    /// No afirma validez visual automática.
    /// </summary>
    [Fact]
    public void MainWindowPuedeInstanciarseSinExcepcionesDeBindings()
    {
        Exception? capturada = null;
        var thread = new Thread(() =>
        {
            try
            {
                if (System.Windows.Application.Current is null)
                {
                    var app = new App();
                    app.InitializeComponent();
                }

                var viewModel = ConstruirViewModel();
                var ventana = new MainWindow(
                    viewModel,
                    ConstruirConfiguracionGrupo(),
                    ConstruirImportacionEstudiantes(),
                    ConstruirExportacionGrupo(),
                    ConstruirRecuperacionLocal());
                ventana.Measure(new System.Windows.Size(1280, 780));
                ventana.Arrange(new System.Windows.Rect(0, 0, 1280, 780));
                ventana.UpdateLayout();
            }
            catch (Exception ex)
            {
                capturada = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(capturada);
    }

    private static ImportacionEstudiantesViewModel ConstruirImportacionEstudiantes()
    {
        var directorio = Path.Combine(Path.GetTempPath(), "SistemaDocenteNEM-ImportSmoke-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directorio);
        var baseSqlite = Path.Combine(directorio, "sistema-docente.db");
        var grupos = new PersistenciaGrupoSqlite(baseSqlite);
        var contextos = new PersistenciaContextoGrupoSqlite(baseSqlite);
        return new ImportacionEstudiantesViewModel(
            new SistemaDocente.Interchange.LectorImportacionTabular(),
            new ImportacionEstudiantesCasosUso(grupos, contextos));
    }

    private static ExportacionGrupoViewModel ConstruirExportacionGrupo()
    {
        var directorio = Path.Combine(Path.GetTempPath(), "SistemaDocenteNEM-ExportSmoke-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directorio);
        var baseSqlite = Path.Combine(directorio, "sistema-docente.db");
        var grupos = new PersistenciaGrupoSqlite(baseSqlite);
        var asistencias = new PersistenciaAsistenciaSqlite(baseSqlite);
        var proyectos = new PersistenciaProyectosSqlite(baseSqlite);
        var expedientes = new PersistenciaExpedienteSqlite(baseSqlite);
        var contextos = new PersistenciaContextoGrupoSqlite(baseSqlite);
        return new ExportacionGrupoViewModel(
            new ExportacionGrupoCasosUso(
                grupos,
                asistencias,
                proyectos,
                proyectos,
                expedientes,
                contextos,
                new SistemaDocente.Interchange.ExportadorTabularArchivo()),
            new ConsultaExportacionGrupoCasosUso(proyectos));
    }

    private static RecuperacionLocalViewModel ConstruirRecuperacionLocal()
    {
        var directorio = Path.Combine(Path.GetTempPath(), "SistemaDocenteNEM-RecoverySmoke-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directorio);
        var baseSqlite = Path.Combine(directorio, "sistema-docente.db");
        return new RecuperacionLocalViewModel(
            new GestionRespaldoCasosUso(
                new ServicioRecuperacionLocalSqlite(
                    baseSqlite,
                    Path.Combine(directorio, "app-state.json"),
                    Path.Combine(directorio, "backups", "safety"),
                    ModoAlmacenamientoLocal.Produccion)));
    }

    private static MainWindowViewModel ConstruirViewModel()
    {
        var directorio = Path.Combine(Path.GetTempPath(), "SistemaDocenteNEM-SmokeTest-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directorio);
        var baseSqlite = Path.Combine(directorio, "sistema-docente.db");
        var estadoAplicacion = Path.Combine(directorio, "app-state.json");

        var persistencia = new PersistenciaGrupoSqlite(baseSqlite);
        var persistenciaAsistencia = new PersistenciaAsistenciaSqlite(baseSqlite);
        var persistenciaProyectos = new PersistenciaProyectosSqlite(baseSqlite);
        var gestion = new GestionGrupoPresentacion(new GestionGrupoCasosUso(persistencia));
        var gestionAsistencia = new GestionAsistenciaPresentacion(
            new GestionAsistenciaCasosUso(persistencia, persistenciaAsistencia));
        var gestionProyectos = new GestionProyectosPresentacion(
            new GestionProyectosActividadesCasosUso(
                persistencia, persistenciaProyectos, persistenciaProyectos));
        var estado = new AlmacenamientoEstadoJson(estadoAplicacion);
        var mensajes = new WpfNotificationService();

        var viewModelGrupo = new GestionGrupoViewModel(
            gestion, estado, mensajes, new ServicioConfirmacionWpf());
        var viewModelAsistencia = new GestionAsistenciaViewModel(
            gestionAsistencia, new RelojLocalSistema(), new DialogoCambiosPendientesWpf(), mensajes);
        var viewModelMensual = new GestionAsistenciaMensualViewModel(
            gestionAsistencia, new RelojLocalSistema(), new DialogoCambiosPendientesWpf(), mensajes);
        var viewModelProyectos = new GestionProyectosViewModel(
            gestionProyectos, new DialogoCambiosPendientesWpf(), new ConfirmacionProyectosWpf(), mensajes);
        var viewModelEvaluacion = new EvaluacionActividadesViewModel(
            gestionProyectos, new DialogoCambiosPendientesWpf(), mensajes);

        var persistenciaExpediente = new PersistenciaExpedienteSqlite(baseSqlite);
        var gestionExpedienteCasosUso = new GestionExpedienteCasosUso(
            persistencia, persistenciaAsistencia, persistenciaProyectos, persistenciaProyectos, persistenciaExpediente);
        var viewModelExpediente = new GestionExpedienteViewModel(gestionExpedienteCasosUso, mensajes);

        return new MainWindowViewModel(
            viewModelGrupo, viewModelAsistencia, viewModelMensual,
            viewModelProyectos, viewModelEvaluacion, viewModelExpediente);
    }

    private static ConfiguracionGrupoViewModel ConstruirConfiguracionGrupo()
    {
        var directorio = Path.Combine(Path.GetTempPath(), "SistemaDocenteNEM-SmokeConfig-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directorio);
        var baseSqlite = Path.Combine(directorio, "sistema-docente.db");
        var grupos = new PersistenciaGrupoSqlite(baseSqlite);
        var contextos = new PersistenciaContextoGrupoSqlite(baseSqlite);
        return new ConfiguracionGrupoViewModel(new GestionContextoGrupoCasosUso(grupos, contextos));
    }
}