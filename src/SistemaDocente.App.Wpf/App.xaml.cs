using System.IO;
using System.Windows;
using System.Windows.Threading;

using SistemaDocente.App.Wpf.Demo;
using SistemaDocente.Application;
using SistemaDocente.Data;
using SistemaDocente.Interchange;
using SistemaDocente.Presentation;

namespace SistemaDocente.App.Wpf;

public partial class App : System.Windows.Application
{
    private static RegistroDiagnosticoSeguroArchivo? _registroDiagnostico;

    public App()
    {
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
    }

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        _registroDiagnostico?.Registrar(e.Exception, CategoriaEventoDiagnostico.FalloNoControlado);
        e.Handled = true;
    }

    private static void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception exception)
        {
            _registroDiagnostico?.Registrar(exception, CategoriaEventoDiagnostico.FalloNoControlado);
        }
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var reiniciarDemo = e.Args.Any(x => string.Equals(x, "--demo-reset", StringComparison.OrdinalIgnoreCase));
        var modoDemo = reiniciarDemo
            || e.Args.Any(x => string.Equals(x, "--demo", StringComparison.OrdinalIgnoreCase));
        var localApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _registroDiagnostico = RegistroDiagnosticoSeguroArchivo.DesdeLocalApplicationData(
            localApplicationData,
            modoDemo);

        try
        {
            var rutas = RutasAplicacion.DesdeLocalApplicationData(localApplicationData, modoDemo);
            if (reiniciarDemo) rutas.ReiniciarDemostracion();

            var persistencia = new PersistenciaGrupoSqlite(rutas.BaseSqlite);
            var persistenciaAsistencia = new PersistenciaAsistenciaSqlite(rutas.BaseSqlite);
            var persistenciaProyectos = new PersistenciaProyectosSqlite(rutas.BaseSqlite);
            var persistenciaExpediente = new PersistenciaExpedienteSqlite(rutas.BaseSqlite);
            var persistenciaContexto = new PersistenciaContextoGrupoSqlite(rutas.BaseSqlite);
            var estado = new AlmacenamientoEstadoJson(rutas.EstadoAplicacion);

            if (modoDemo)
            {
                var grupoDemo = DemoDataSeeder.AsegurarDatos(
                    persistencia,
                    persistenciaAsistencia,
                    persistenciaProyectos,
                    persistenciaExpediente);
                DemoContextSeeder.AsegurarContexto(persistenciaContexto, grupoDemo);
                estado.Guardar(grupoDemo);
            }

            var gestionGrupoCasosUso = new GestionGrupoCasosUso(persistencia);
            var gestion = new GestionGrupoPresentacion(gestionGrupoCasosUso);
            var gestionAsistencia = new GestionAsistenciaPresentacion(
                new GestionAsistenciaCasosUso(persistencia, persistenciaAsistencia));
            var gestionProyectosCasosUso = new GestionProyectosActividadesCasosUso(
                persistencia, persistenciaProyectos, persistenciaProyectos);
            var gestionProyectos = new GestionProyectosPresentacion(gestionProyectosCasosUso);
            var gestionExpedienteCasosUso = new GestionExpedienteCasosUso(
                persistencia,
                persistenciaAsistencia,
                persistenciaProyectos,
                persistenciaProyectos,
                persistenciaExpediente);
            var gestionContextoCasosUso = new GestionContextoGrupoCasosUso(
                persistencia,
                persistenciaContexto);
            var gestionReportesCasosUso = new GestionReportesCasosUso(
                persistencia,
                persistenciaAsistencia,
                persistenciaProyectos,
                persistenciaProyectos,
                persistenciaExpediente,
                persistenciaContexto);
            var importacionEstudiantesCasosUso = new ImportacionEstudiantesCasosUso(
                persistencia,
                persistenciaContexto);
            var exportacionGrupoCasosUso = new ExportacionGrupoCasosUso(
                persistencia,
                persistenciaAsistencia,
                persistenciaProyectos,
                persistenciaProyectos,
                persistenciaExpediente,
                persistenciaContexto,
                new ExportadorTabularArchivo());
            var consultaExportacionGrupo = new ConsultaExportacionGrupoCasosUso(persistenciaProyectos);
            var servicioRecuperacion = new ServicioRecuperacionLocalSqlite(
                rutas.BaseSqlite,
                rutas.EstadoAplicacion,
                rutas.DirectorioRespaldosSeguridad,
                modoDemo
                    ? ModoAlmacenamientoLocal.Demostracion
                    : ModoAlmacenamientoLocal.Produccion);
            var gestionRespaldo = new GestionRespaldoCasosUso(servicioRecuperacion);

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
            var viewModelExpediente = new GestionExpedienteViewModel(
                gestionExpedienteCasosUso,
                mensajes);
            var viewModelReportes = new GestionReportesViewModel(
                gestionGrupoCasosUso,
                gestionReportesCasosUso,
                new ExportadorReportesPdf());
            var viewModelConfiguracion = new ConfiguracionGrupoViewModel(
                gestionContextoCasosUso);
            var viewModelImportacion = new ImportacionEstudiantesViewModel(
                new LectorImportacionTabular(),
                importacionEstudiantesCasosUso);
            var viewModelExportacion = new ExportacionGrupoViewModel(
                exportacionGrupoCasosUso,
                consultaExportacionGrupo);
            var viewModelRecuperacion = new RecuperacionLocalViewModel(gestionRespaldo);

            var viewModel = new MainWindowViewModel(
                viewModelGrupo,
                viewModelAsistencia,
                viewModelMensual,
                viewModelProyectos,
                viewModelEvaluacion,
                viewModelExpediente,
                modoDemo,
                viewModelReportes);
            var ventana = new MainWindow(
                viewModel,
                viewModelConfiguracion,
                viewModelImportacion,
                viewModelExportacion,
                viewModelRecuperacion);
            MainWindow = ventana;
            ventana.Show();
            viewModelGrupo.Inicializar();
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException or ErrorPersistenciaAplicacionException)
        {
            _registroDiagnostico?.Registrar(
                exception,
                CategoriaEventoDiagnostico.FalloInicioAlmacenamiento);
            MessageBox.Show(
                "No fue posible iniciar el almacenamiento local. Cierra la aplicación e intenta nuevamente.",
                IdentidadProducto.Nombre, MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }
}