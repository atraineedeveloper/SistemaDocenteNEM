using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;

using SistemaDocente.App.Wpf.Demo;
using SistemaDocente.Application;
using SistemaDocente.Data;
using SistemaDocente.Presentation;

namespace SistemaDocente.App.Wpf;

public partial class App : System.Windows.Application
{
    public App()
    {
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
    }

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        LogException(e.Exception);
        e.Handled = true;
    }

    private static void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
            LogException(ex);
    }

    private static void LogException(Exception exception)
    {
        try
        {
            var logPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "SistemaDocenteNEM", "crash.log");
            Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
            File.AppendAllText(logPath, $"[{DateTime.Now:O}] {exception}\n\n");
        }
        catch
        {
            // Ignorar fallos de logging.
        }
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        try
        {
            var reiniciarDemo = e.Args.Any(x => string.Equals(x, "--demo-reset", StringComparison.OrdinalIgnoreCase));
            var modoDemo = reiniciarDemo
                || e.Args.Any(x => string.Equals(x, "--demo", StringComparison.OrdinalIgnoreCase));

            var rutas = RutasAplicacion.DesdeLocalApplicationData(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                modoDemo);
            if (reiniciarDemo) rutas.ReiniciarDemostracion();

            var persistencia = new PersistenciaGrupoSqlite(rutas.BaseSqlite);
            var persistenciaAsistencia = new PersistenciaAsistenciaSqlite(rutas.BaseSqlite);
            var persistenciaProyectos = new PersistenciaProyectosSqlite(rutas.BaseSqlite);
            var persistenciaExpediente = new PersistenciaExpedienteSqlite(rutas.BaseSqlite);
            var estado = new AlmacenamientoEstadoJson(rutas.EstadoAplicacion);

            if (modoDemo)
            {
                var grupoDemo = DemoDataSeeder.AsegurarDatos(
                    persistencia,
                    persistenciaAsistencia,
                    persistenciaProyectos,
                    persistenciaExpediente);
                estado.Guardar(grupoDemo);
            }

            var gestion = new GestionGrupoPresentacion(new GestionGrupoCasosUso(persistencia));
            var gestionAsistencia = new GestionAsistenciaPresentacion(
                new GestionAsistenciaCasosUso(persistencia, persistenciaAsistencia));
            var gestionProyectos = new GestionProyectosPresentacion(
                new GestionProyectosActividadesCasosUso(
                    persistencia, persistenciaProyectos, persistenciaProyectos));
            var mensajes = new WpfNotificationService();
            var viewModelGrupo = new GestionGrupoViewModel(
                gestion, estado, mensajes, new ServicioConfirmacionWpf());
            var viewModelAsistencia = new GestionAsistenciaViewModel(
                gestionAsistencia,
                new RelojLocalSistema(),
                new DialogoCambiosPendientesWpf(),
                mensajes);
            var viewModelMensual = new GestionAsistenciaMensualViewModel(
                gestionAsistencia,
                new RelojLocalSistema(),
                new DialogoCambiosPendientesWpf(),
                mensajes);
            var viewModelProyectos = new GestionProyectosViewModel(
                gestionProyectos,
                new DialogoCambiosPendientesWpf(),
                new ConfirmacionProyectosWpf(),
                mensajes);
            var viewModelEvaluacion = new EvaluacionActividadesViewModel(
                gestionProyectos,
                new DialogoCambiosPendientesWpf(),
                mensajes);
            var gestionExpedienteCasosUso = new GestionExpedienteCasosUso(
                persistencia, persistenciaAsistencia, persistenciaProyectos, persistenciaProyectos, persistenciaExpediente);
            var viewModelExpediente = new GestionExpedienteViewModel(gestionExpedienteCasosUso, mensajes);

            var viewModel = new MainWindowViewModel(
                viewModelGrupo,
                viewModelAsistencia,
                viewModelMensual,
                viewModelProyectos,
                viewModelEvaluacion,
                viewModelExpediente,
                modoDemo);
            var ventana = new MainWindow(viewModel);
            MainWindow = ventana;
            ventana.Show();
            viewModelGrupo.Inicializar();
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException or ErrorPersistenciaAplicacionException)
        {
            Debug.WriteLine($"Fallo de inicio de almacenamiento: {exception}");
            MessageBox.Show(
                "No fue posible iniciar el almacenamiento local. Cierra la aplicación e intenta nuevamente.",
                "Sistema Docente Local", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }
}