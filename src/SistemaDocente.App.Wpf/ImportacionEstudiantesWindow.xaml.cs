using System.Windows;

using Microsoft.Win32;

using SistemaDocente.Presentation;

namespace SistemaDocente.App.Wpf;

public partial class ImportacionEstudiantesWindow : Window
{
    private readonly ImportacionEstudiantesViewModel _viewModel;

    public ImportacionEstudiantesWindow(ImportacionEstudiantesViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        _viewModel = viewModel;
        InitializeComponent();
        DataContext = viewModel;
    }

    private void OnSeleccionarArchivoClic(object sender, RoutedEventArgs e)
    {
        var dialogo = new OpenFileDialog
        {
            Title = "Seleccionar lista de alumnos",
            Filter = "Listas compatibles (*.xlsx;*.csv)|*.xlsx;*.csv|Excel (*.xlsx)|*.xlsx|CSV (*.csv)|*.csv",
            CheckFileExists = true,
            Multiselect = false,
        };

        if (dialogo.ShowDialog(this) == true)
        {
            _viewModel.CargarArchivo(dialogo.FileName);
        }
    }

    private void OnCerrarClic(object sender, RoutedEventArgs e)
    {
        DialogResult = _viewModel.Importados > 0;
        Close();
    }
}
