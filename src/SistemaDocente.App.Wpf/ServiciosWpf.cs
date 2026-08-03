using System.Windows;

using SistemaDocente.Presentation;

namespace SistemaDocente.App.Wpf;

public sealed class ServicioMensajesWpf : IServicioMensajes
{
    public void MostrarError(string mensaje) => MessageBox.Show(
        mensaje, "Sistema Docente Local", MessageBoxButton.OK, MessageBoxImage.Warning);
}

public sealed class ServicioConfirmacionWpf : IServicioConfirmacion
{
    public bool ConfirmarDesactivacion(string nombreEstudiante) => MessageBox.Show(
        $"¿Deseas desactivar a {nombreEstudiante}? Sus datos se conservarán.",
        "Confirmar desactivación", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
}

public sealed class DialogoCambiosPendientesWpf : IDialogoCambiosPendientes
{
    public DecisionCambiosPendientes ConfirmarCambiosPendientes() => MessageBox.Show(
        "Hay cambios pendientes. ¿Deseas guardarlos antes de continuar?\n\nSí: Guardar  ·  No: Descartar  ·  Cancelar: Permanecer aquí",
        "Cambios sin guardar",
        MessageBoxButton.YesNoCancel,
        MessageBoxImage.Question) switch
    {
        MessageBoxResult.Yes => DecisionCambiosPendientes.Guardar,
        MessageBoxResult.No => DecisionCambiosPendientes.Descartar,
        _ => DecisionCambiosPendientes.Cancelar,
    };
}

public sealed class RelojLocalSistema : IRelojLocal
{
    public DateOnly Hoy => DateOnly.FromDateTime(DateTime.Now);
}