using System.ComponentModel;
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
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
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
            Close();
        }
    }

    private void OnCancelarClic(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ConfiguracionGrupoViewModel.GuardadoCorrectamente)
            && ViewModel.GuardadoCorrectamente)
        {
            DialogResult = true;
            Close();
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
        base.OnClosed(e);
    }
}
