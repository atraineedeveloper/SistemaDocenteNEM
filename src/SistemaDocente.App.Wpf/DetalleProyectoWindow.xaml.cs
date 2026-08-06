using System.Windows;
using System.Windows.Input;
using SistemaDocente.Presentation;

namespace SistemaDocente.App.Wpf;

public partial class DetalleProyectoWindow : Window
{
    public DetalleProyectoWindow(GestionProyectosViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        InitializeComponent();
        DataContext = viewModel;
    }

    private GestionProyectosViewModel ViewModel => (GestionProyectosViewModel)DataContext;

    private void OnActividadDobleClic(object sender, MouseButtonEventArgs e)
    {
        AbrirDetalleActividad();
    }

    private void OnEditarActividadClic(object sender, RoutedEventArgs e)
    {
        AbrirDetalleActividad();
    }

    private void AbrirDetalleActividad()
    {
        if (ViewModel.ActividadSeleccionada is null && !ViewModel.TieneCambiosActividad) return;
        var ventanaActividad = new DetalleActividadWindow(ViewModel) { Owner = this };
        ventanaActividad.ShowDialog();
    }

    private void OnCerrarClic(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
