using System.ComponentModel;
using System.Windows;

using SistemaDocente.Presentation;

namespace SistemaDocente.App.Wpf;

public partial class EditorEstudianteWindow : Window
{
    public EditorEstudianteWindow(GestionGrupoViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        InitializeComponent();
        DataContext = viewModel;

        viewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    private GestionGrupoViewModel ViewModel => (GestionGrupoViewModel)DataContext;

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(GestionGrupoViewModel.PanelActual) && ViewModel.PanelActual == PanelEdicion.Ninguno)
        {
            DialogResult = true;
            Close();
        }
    }

    private void OnGuardarClic(object sender, RoutedEventArgs e)
    {
        if (ViewModel.GuardarEstudianteCommand.CanExecute(null))
        {
            ViewModel.GuardarEstudianteCommand.Execute(null);
        }
    }

    private void OnCancelarClic(object sender, RoutedEventArgs e)
    {
        if (ViewModel.CancelarEdicionCommand.CanExecute(null))
        {
            ViewModel.CancelarEdicionCommand.Execute(null);
        }

        DialogResult = false;
        Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
        base.OnClosed(e);
    }
}
