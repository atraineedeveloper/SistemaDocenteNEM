using System.Windows;

using SistemaDocente.Presentation;

namespace SistemaDocente.App.Wpf;

public sealed class WpfNotificationService : INotificationService, IServicioMensajes
{
    public void Show(string message, string title, NotificationLevel level)
    {
        if (System.Windows.Application.Current?.MainWindow is not MainWindow mainWindow)
        {
            // Fallback en caso de que no haya ventana principal disponible.
            MessageBox.Show(message, title, MessageBoxButton.OK, MapIcon(level));
            return;
        }

        switch (level)
        {
            case NotificationLevel.Success:
                mainWindow.MostrarToastExito(message, title);
                break;
            case NotificationLevel.Warning:
                mainWindow.MostrarToastAdvertencia(message, title);
                break;
            case NotificationLevel.Error:
                mainWindow.MostrarToastError(message, title);
                break;
            default:
                mainWindow.MostrarToastInfo(message, title);
                break;
        }
    }

    public void ShowInfo(string message, string? title = null)
        => Show(message, title ?? "Información", NotificationLevel.Info);

    public void ShowSuccess(string message, string? title = null)
        => Show(message, title ?? "Éxito", NotificationLevel.Success);

    public void ShowWarning(string message, string? title = null)
        => Show(message, title ?? "Advertencia", NotificationLevel.Warning);

    public void ShowError(string message, string? title = null)
        => Show(message, title ?? "Error", NotificationLevel.Error);

    // Compatibilidad con IServicioMensajes existente.
    public void MostrarError(string mensaje) => ShowError(mensaje);

    private static MessageBoxImage MapIcon(NotificationLevel level) => level switch
    {
        NotificationLevel.Error => MessageBoxImage.Error,
        NotificationLevel.Warning => MessageBoxImage.Warning,
        NotificationLevel.Success => MessageBoxImage.Information,
        _ => MessageBoxImage.Information,
    };
}
