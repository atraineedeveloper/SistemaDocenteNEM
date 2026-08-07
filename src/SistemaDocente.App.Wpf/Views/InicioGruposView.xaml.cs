using System.Windows;
using System.Windows.Controls;

using SistemaDocente.Application;
using SistemaDocente.Presentation;

namespace SistemaDocente.App.Wpf.Views;

public partial class InicioGruposView : UserControl
{
    public InicioGruposView()
    {
        InitializeComponent();
    }

    private void OnAbrirGrupoClic(object sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel
            || sender is not FrameworkElement { Tag: GrupoDetalle grupo })
        {
            return;
        }

        viewModel.CambiarGrupo(grupo.GrupoId);
    }
}
