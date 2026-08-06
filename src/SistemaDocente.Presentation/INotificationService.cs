namespace SistemaDocente.Presentation;

public enum NotificationLevel
{
    Info,
    Success,
    Warning,
    Error,
}

public interface INotificationService
{
    void Show(string message, string title, NotificationLevel level);

    void ShowInfo(string message, string? title = null);

    void ShowSuccess(string message, string? title = null);

    void ShowWarning(string message, string? title = null);

    void ShowError(string message, string? title = null);
}
