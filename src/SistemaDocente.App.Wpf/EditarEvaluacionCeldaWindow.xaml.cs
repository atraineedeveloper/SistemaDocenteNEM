using System.ComponentModel;
using System.Windows;

using SistemaDocente.Core;
using SistemaDocente.Presentation;

namespace SistemaDocente.App.Wpf;

public partial class EditarEvaluacionCeldaWindow : Window
{
    private readonly EvaluacionCeldaVisual _celda;
    private readonly NivelLogro _nivelOriginal;
    private readonly string _observacionOriginal;
    private bool _aceptado;

    public EditarEvaluacionCeldaWindow(
        EvaluacionEstudianteFilaVisual fila,
        ActividadEvaluacionColumnaVisual actividad,
        EvaluacionCeldaVisual celda)
    {
        ArgumentNullException.ThrowIfNull(fila);
        ArgumentNullException.ThrowIfNull(actividad);
        ArgumentNullException.ThrowIfNull(celda);

        InitializeComponent();
        _celda = celda;
        _nivelOriginal = celda.NivelLogro;
        _observacionOriginal = celda.Observacion;
        DataContext = celda;
        ActividadText.Text = actividad.DescripcionAccesible;
        EstudianteText.Text = fila.Nombre;
        Closing += OnClosing;
    }

    private void OnAceptar(object sender, RoutedEventArgs e)
    {
        _aceptado = true;
        DialogResult = true;
    }

    private void OnCancelar(object sender, RoutedEventArgs e)
    {
        RestaurarEdicionLocal();
        DialogResult = false;
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        if (!_aceptado) RestaurarEdicionLocal();
    }

    private void RestaurarEdicionLocal()
    {
        _celda.NivelLogro = _nivelOriginal;
        _celda.Observacion = _observacionOriginal;
    }
}