using System.Windows;

using Microsoft.Win32;

using SistemaDocente.Application;
using SistemaDocente.Presentation;

namespace SistemaDocente.App.Wpf;

public partial class ExportacionGrupoWindow : Window
{
    private readonly ExportacionGrupoViewModel _viewModel;

    public ExportacionGrupoWindow(ExportacionGrupoViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        _viewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
    }

    private void OnGuardarArchivoClic(object sender, RoutedEventArgs e)
    {
        var esXlsx = _viewModel.Formato == FormatoExportacionTabular.Xlsx;
        var dialogo = new SaveFileDialog
        {
            Title = "Guardar exportación del grupo",
            Filter = esXlsx
                ? "Libro de Excel (*.xlsx)|*.xlsx"
                : "CSV UTF-8 (*.csv)|*.csv",
            DefaultExt = esXlsx ? ".xlsx" : ".csv",
            AddExtension = true,
            OverwritePrompt = true,
            FileName = _viewModel.NombreArchivoSugerido,
        };

        if (dialogo.ShowDialog(this) == true)
        {
            _viewModel.ExportarA(dialogo.FileName);
        }
    }

    private void OnCerrarClic(object sender, RoutedEventArgs e) => Close();
}
