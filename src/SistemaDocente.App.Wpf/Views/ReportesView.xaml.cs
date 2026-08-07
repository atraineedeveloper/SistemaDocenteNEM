using System.Windows;
using System.Windows.Controls;

using SistemaDocente.Presentation;

namespace SistemaDocente.App.Wpf.Views;

public partial class ReportesView : UserControl
{
    public ReportesView()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty ConfiguracionProperty = DependencyProperty.Register(
        nameof(Configuracion),
        typeof(ConfiguracionGrupoViewModel),
        typeof(ReportesView),
        new PropertyMetadata(null));

    public ConfiguracionGrupoViewModel? Configuracion
    {
        get => (ConfiguracionGrupoViewModel?)GetValue(ConfiguracionProperty);
        set => SetValue(ConfiguracionProperty, value);
    }

    private GestionReportesViewModel? ViewModel => DataContext as GestionReportesViewModel;

    private void OnConfigurarGrupoClic(object sender, RoutedEventArgs e)
    {
        if (ViewModel?.GrupoIdActual is not { } grupoId || Configuracion is null) return;
        Configuracion.Inicializar(grupoId);
        var ventana = new ConfiguracionGrupoWindow(Configuracion)
        {
            Owner = Window.GetWindow(this),
        };
        ventana.ShowDialog();
        ViewModel.Refrescar();
    }
}
