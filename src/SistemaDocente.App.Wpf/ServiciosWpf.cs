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