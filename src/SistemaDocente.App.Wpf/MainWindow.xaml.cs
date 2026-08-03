using System.Windows;
using System.Windows.Input;

using SistemaDocente.Presentation;

namespace SistemaDocente.App.Wpf;

public partial class MainWindow : Window
{
    public MainWindow(GestionGrupoViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        InitializeComponent();
        DataContext = viewModel;
        viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName is nameof(GestionGrupoViewModel.PanelActual)
                or nameof(GestionGrupoViewModel.MostrarBienvenida))
            {
                Dispatcher.BeginInvoke(AsignarFocoInicial);
            }
        };
        Loaded += (_, _) => AsignarFocoInicial();
    }

    private GestionGrupoViewModel ViewModel => (GestionGrupoViewModel)DataContext;

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && ViewModel.CancelarEdicionCommand.CanExecute(null))
        {
            ViewModel.CancelarEdicionCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void AsignarFocoInicial()
    {
        if (ViewModel.MostrarBienvenida) NombreGrupoInicial.Focus();
        else if (ViewModel.MostrarEditorGrupo) NombreGrupoEdicion.Focus();
        else if (ViewModel.MostrarEditorEstudiante) NombreEstudianteEdicion.Focus();
    }
}