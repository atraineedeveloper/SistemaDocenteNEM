using System.IO;
using System.Diagnostics;
using System.Windows;

using SistemaDocente.Application;
using SistemaDocente.Data;
using SistemaDocente.Presentation;

namespace SistemaDocente.App.Wpf;

public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        try
        {
            var rutas = RutasAplicacion.DesdeLocalApplicationData(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
            var persistencia = new PersistenciaGrupoSqlite(rutas.BaseSqlite);
            var persistenciaAsistencia = new PersistenciaAsistenciaSqlite(rutas.BaseSqlite);
            var persistenciaProyectos = new PersistenciaProyectosSqlite(rutas.BaseSqlite);
            var gestion = new GestionGrupoPresentacion(new GestionGrupoCasosUso(persistencia));
            var gestionAsistencia = new GestionAsistenciaPresentacion(
                new GestionAsistenciaCasosUso(persistencia, persistenciaAsistencia));
            var gestionProyectos = new GestionProyectosPresentacion(
                new GestionProyectosActividadesCasosUso(
                    persistencia, persistenciaProyectos, persistenciaProyectos));
            var estado = new AlmacenamientoEstadoJson(rutas.EstadoAplicacion);
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
            var persistenciaExpediente = new PersistenciaExpedienteSqlite(rutas.BaseSqlite);
            var gestionExpedienteCasosUso = new GestionExpedienteCasosUso(
                persistencia, persistenciaAsistencia, persistenciaProyectos, persistenciaProyectos, persistenciaExpediente);
            var viewModelExpediente = new GestionExpedienteViewModel(gestionExpedienteCasosUso, mensajes);

            var viewModel = new MainWindowViewModel(
                viewModelGrupo, viewModelAsistencia, viewModelMensual, viewModelProyectos, viewModelEvaluacion, viewModelExpediente);
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
