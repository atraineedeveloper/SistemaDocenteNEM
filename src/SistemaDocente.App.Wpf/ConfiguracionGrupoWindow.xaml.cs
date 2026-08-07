using System.Windows;

using SistemaDocente.Presentation;

namespace SistemaDocente.App.Wpf;

public partial class ConfiguracionGrupoWindow : Window
{
    public ConfiguracionGrupoWindow(ConfiguracionGrupoViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        InitializeComponent();
        DataContext = viewModel;
    }

    private ConfiguracionGrupoViewModel ViewModel => (ConfiguracionGrupoViewModel)DataContext;

    private void OnGuardarClic(object sender, RoutedEventArgs e)
    {
        if (ViewModel.GuardarCommand.CanExecute(null))
        {
            ViewModel.GuardarCommand.Execute(null);
        }

        if (ViewModel.GuardadoCorrectamente)
        {
            DialogResult = true;
        }
    }

    private void OnCancelarClic(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
