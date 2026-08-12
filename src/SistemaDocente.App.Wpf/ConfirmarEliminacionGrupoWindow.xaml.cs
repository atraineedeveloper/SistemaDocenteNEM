using System.Windows;
using System.Windows.Controls;

using SistemaDocente.Application;

namespace SistemaDocente.App.Wpf;

public partial class ConfirmarEliminacionGrupoWindow : Window
{
    private readonly string _nombreGrupo;

    public ConfirmarEliminacionGrupoWindow(
        GrupoDetalle grupo,
        ResumenEliminacionGrupo resumen)
    {
        ArgumentNullException.ThrowIfNull(grupo);
        ArgumentNullException.ThrowIfNull(resumen);

        InitializeComponent();
        _nombreGrupo = grupo.NombreVisible;
        GrupoTextBlock.Text = grupo.NombreVisible;
        ImpactoTextBlock.Text = CrearResumenImpacto(resumen);
        InstruccionTextBlock.Text = $"Escribe “{grupo.NombreVisible}” para confirmar:";
    }

    public bool Confirmado { get; private set; }

    private void OnConfirmacionTextChanged(object sender, TextChangedEventArgs e)
    {
        EliminarButton.IsEnabled = string.Equals(
            ConfirmacionTextBox.Text,
            _nombreGrupo,
            StringComparison.Ordinal);
    }

    private void OnCancelarClic(object sender, RoutedEventArgs e)
    {
        Confirmado = false;
        DialogResult = false;
    }

    private void OnEliminarClic(object sender, RoutedEventArgs e)
    {
        if (!EliminarButton.IsEnabled)
        {
            return;
        }

        Confirmado = true;
        DialogResult = true;
    }

    private static string CrearResumenImpacto(ResumenEliminacionGrupo resumen)
    {
        var partes = new List<string>();
        if (resumen.Estudiantes > 0) partes.Add($"{resumen.Estudiantes} estudiantes");
        if (resumen.DiasAsistencia > 0) partes.Add($"{resumen.DiasAsistencia} días de asistencia");
        if (resumen.Proyectos > 0) partes.Add($"{resumen.Proyectos} proyectos");
        if (resumen.Actividades > 0) partes.Add($"{resumen.Actividades} actividades");
        if (resumen.Entregas > 0) partes.Add($"{resumen.Entregas} registros de evaluación/entrega");
        if (resumen.ConfiguracionSignificativa > 0) partes.Add("configuración escolar o NEM");

        return partes.Count == 0
            ? "El grupo contiene información asociada."
            : "Se eliminarán: " + string.Join(" · ", partes) + ".";
    }
}
