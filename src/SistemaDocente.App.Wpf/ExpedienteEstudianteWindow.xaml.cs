using System.Windows;
using SistemaDocente.Presentation;

namespace SistemaDocente.App.Wpf;

public partial class ExpedienteEstudianteWindow : Window
{
    public ExpedienteEstudianteWindow(GestionExpedienteViewModel viewModel)
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
