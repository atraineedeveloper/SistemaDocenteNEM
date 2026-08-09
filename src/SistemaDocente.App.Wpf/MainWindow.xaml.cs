using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

using SistemaDocente.Application;
using SistemaDocente.Presentation;

namespace SistemaDocente.App.Wpf;

/// <summary>
/// Shell principal de la aplicación. Se responsabiliza sólo de ventana, composición,
/// feedback global y cierre. Los módulos mantienen su presentación en vistas dedicadas.
/// </summary>
public partial class MainWindow : Window
{
    private DispatcherTimer? _toastTimer;
    private CoordinadorActualizacionesWpf? _actualizaciones;
    private bool _omitirConfirmacionCierre;

    public MainWindow(
        MainWindowViewModel viewModel,
        ConfiguracionGrupoViewModel configuracionGrupo,
        ImportacionEstudiantesViewModel importacionEstudiantes,
        ExportacionGrupoViewModel exportacionGrupo,
        RecuperacionLocalViewModel recuperacionLocal)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(configuracionGrupo);
        ArgumentNullException.ThrowIfNull(importacionEstudiantes);
        ArgumentNullException.ThrowIfNull(exportacionGrupo);
        ArgumentNullException.ThrowIfNull(recuperacionLocal);
        ConfiguracionGrupo = configuracionGrupo;
        ImportacionEstudiantes = importacionEstudiantes;
        ExportacionGrupo = exportacionGrupo;
        RecuperacionLocal = recuperacionLocal;
        InitializeComponent();
        GrupoModule.Importacion = importacionEstudiantes;
        GrupoModule.Exportacion = exportacionGrupo;
        DataContext = viewModel;
    }

    public MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext;
    public ConfiguracionGrupoViewModel ConfiguracionGrupo { get; }
    public ImportacionEstudiantesViewModel ImportacionEstudiantes { get; }
    public ExportacionGrupoViewModel ExportacionGrupo { get; }
    public RecuperacionLocalViewModel RecuperacionLocal { get; }

    public void ConfigurarActualizaciones(
        IServicioActualizacionesAplicacion servicio,
        bool modoDemo,
        IRegistroDiagnosticoSeguro? diagnostico) =>
        _actualizaciones = new CoordinadorActualizacionesWpf(this, servicio, modoDemo, diagnostico);

    public Task ComprobarActualizacionesAlInicioAsync() =>
        _actualizaciones?.ComprobarAlInicioAsync() ?? Task.CompletedTask;

    public bool PrepararCierreParaActualizacion()
    {
        if (!ViewModel.SolicitarCerrar()) return false;
        _omitirConfirmacionCierre = true;
        return true;
    }

    public void CancelarCierrePreparadoParaActualizacion() => _omitirConfirmacionCierre = false;

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        if (!_omitirConfirmacionCierre && !ViewModel.SolicitarCerrar())
        {
            e.Cancel = true;
            return;
        }

        _toastTimer?.Stop();
    }

    private void OnRecuperacionSolicitada(object? sender, EventArgs e)
    {
        if (!ViewModel.SolicitarCerrar()) return;

        var ventana = new RecuperacionLocalWindow(RecuperacionLocal)
        {
            Owner = this,
        };
        ventana.ShowDialog();
    }

    private async void OnActualizacionSolicitada(object? sender, EventArgs e)
    {
        if (_actualizaciones is not null) await _actualizaciones.ComprobarManualAsync();
    }

    /// <summary>Muestra un toast flotante con auto-dismiss.</summary>
    public void MostrarToast(
        string icono,
        string titulo,
        string mensaje,
        Brush fondo,
        Brush borde,
        Brush colorTexto,
        int segundos = 3)
    {
        ToastIcon.Text = icono;
        ToastTitle.Text = titulo;
        ToastTitle.Foreground = colorTexto;
        ToastMessage.Text = mensaje;
        ToastMessage.Foreground = colorTexto;
        ToastBanner.Background = fondo;
        ToastBanner.BorderBrush = borde;
        ToastBanner.Visibility = Visibility.Visible;

        _toastTimer?.Stop();
        _toastTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(segundos),
        };
        _toastTimer.Tick += OnToastTimerTick;
        _toastTimer.Start();
    }

    private void OnToastTimerTick(object? sender, EventArgs e)
    {
        ToastBanner.Visibility = Visibility.Collapsed;
        if (_toastTimer is not null)
        {
            _toastTimer.Tick -= OnToastTimerTick;
            _toastTimer.Stop();
        }
    }

    public void MostrarToastExito(string mensaje, string titulo = "✅ Guardado exitosamente") =>
        MostrarToast(
            "✅",
            titulo,
            mensaje,
            ObtenerBrush("SuccessBackgroundBrush"),
            ObtenerBrush("SuccessBorderBrush"),
            ObtenerBrush("SuccessBrush"));

    public void MostrarToastAdvertencia(string mensaje, string titulo = "⚠️ Advertencia") =>
        MostrarToast(
            "⚠️",
            titulo,
            mensaje,
            ObtenerBrush("WarningBackgroundBrush"),
            ObtenerBrush("WarningBorderBrush"),
            ObtenerBrush("WarningBrush"));

    public void MostrarToastError(string mensaje, string titulo = "❌ Error") =>
        MostrarToast(
            "❌",
            titulo,
            mensaje,
            ObtenerBrush("ErrorBackgroundBrush"),
            ObtenerBrush("ErrorBorderBrush"),
            ObtenerBrush("ErrorBrush"));

    public void MostrarToastInfo(string mensaje, string titulo = "ℹ️ Información") =>
        MostrarToast(
            "ℹ️",
            titulo,
            mensaje,
            ObtenerBrush("InfoBackgroundBrush"),
            ObtenerBrush("InfoBorderBrush"),
            ObtenerBrush("InfoBrush"));

    private Brush ObtenerBrush(string clave) =>
        TryFindResource(clave) as Brush ?? Brushes.Transparent;
}
