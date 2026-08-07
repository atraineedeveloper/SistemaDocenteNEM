using System.ComponentModel;
using System.Windows;

using SistemaDocente.Presentation;

namespace SistemaDocente.App.Wpf;

/// <summary>
/// Shell principal de la aplicación. Sólo se responsabiliza de asuntos globales:
/// ventana, encabezado/navegación (delegado a <see cref="Controls.MainNavigationHeader"/>),
/// feedback global (toast y progreso), ensamblado de vistas y cierre.
/// No conoce los detalles visuales internos de los módulos.
/// </summary>
public partial class MainWindow : Window
{
    private System.Windows.Threading.DispatcherTimer? _toastTimer;

    public MainWindow(MainWindowViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        InitializeComponent();
        DataContext = viewModel;
    }

    /// <summary>ViewModel raíz expuesto para que las vistas resuelvan coordinación global.</summary>
    public MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext;

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        if (!ViewModel.SolicitarCerrar())
        {
            e.Cancel = true;
        }
    }

    // ══ Toast de confirmación global ════════════════════════

    /// <summary>Muestra un toast flotante con auto-dismiss después de <paramref name="segundos"/> segundos.</summary>
    public void MostrarToast(string icono, string titulo, string mensaje,
        System.Windows.Media.Brush fondo, System.Windows.Media.Brush borde,
        System.Windows.Media.Brush colorTexto, int segundos = 3)
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
        _toastTimer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(segundos)
        };
        _toastTimer.Tick += (_, _) =>
        {
            ToastBanner.Visibility = Visibility.Collapsed;
            _toastTimer.Stop();
        };
        _toastTimer.Start();
    }

    /// <summary>Toast de éxito verde estándar.</summary>
    public void MostrarToastExito(string mensaje, string titulo = "✅ Guardado exitosamente") =>
        MostrarToast("✅", titulo, mensaje,
            new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#ECFDF3")),
            new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#ABEFC6")),
            new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#027A48")));

    /// <summary>Toast de advertencia naranja estándar.</summary>
    public void MostrarToastAdvertencia(string mensaje, string titulo = "⚠️ Advertencia") =>
        MostrarToast("⚠️", titulo, mensaje,
            new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFAEB")),
            new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FEF0C7")),
            new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#B54708")));

    /// <summary>Toast de error rojo estándar.</summary>
    public void MostrarToastError(string mensaje, string titulo = "❌ Error") =>
        MostrarToast("❌", titulo, mensaje,
            new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FEF3F2")),
            new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FECDCA")),
            new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#B42318")));

    /// <summary>Toast de información azul estándar.</summary>
    public void MostrarToastInfo(string mensaje, string titulo = "ℹ️ Información") =>
        MostrarToast("ℹ️", titulo, mensaje,
            new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#EFF8FF")),
            new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#B2DDFF")),
            new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#175CD3")));
}