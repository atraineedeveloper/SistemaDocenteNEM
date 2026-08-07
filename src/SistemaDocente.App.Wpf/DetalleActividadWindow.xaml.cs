using System.Windows;

using SistemaDocente.Presentation;

namespace SistemaDocente.App.Wpf;

public partial class DetalleActividadWindow : Window
{
    public DetalleActividadWindow(GestionProyectosViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        InitializeComponent();
        DataContext = viewModel;
    }

    private void OnCerrarClic(object sender, RoutedEventArgs e)
    {
        Close();
    }
}