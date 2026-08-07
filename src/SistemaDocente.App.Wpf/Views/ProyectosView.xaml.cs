using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

using SistemaDocente.Application;
using SistemaDocente.Presentation;

namespace SistemaDocente.App.Wpf.Views;

/// <summary>
/// Vista principal de proyectos: lista amplia, filtros y apertura de la ventana dedicada
/// <see cref="DetalleProyectoWindow"/>. La búsqueda es una preocupación visual local y
/// no altera los datos ni reintroduce master-detail.
/// </summary>
public partial class ProyectosView : System.Windows.Controls.UserControl
{
    private GestionProyectosViewModel? _viewModelSuscrito;

    public ProyectosView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        DataContextChanged += OnDataContextChanged;
    }

    private GestionProyectosViewModel? ViewModel => DataContext as GestionProyectosViewModel;

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        CambiarSuscripcion(ViewModel);
        AplicarBusqueda();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e) => CambiarSuscripcion(null);

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        CambiarSuscripcion(e.NewValue as GestionProyectosViewModel);
        Dispatcher.BeginInvoke(AplicarBusqueda);
    }

    private void CambiarSuscripcion(GestionProyectosViewModel? nuevo)
    {
        if (ReferenceEquals(_viewModelSuscrito, nuevo)) return;
        if (_viewModelSuscrito is not null)
            _viewModelSuscrito.PropertyChanged -= OnViewModelPropertyChanged;
        _viewModelSuscrito = nuevo;
        if (_viewModelSuscrito is not null)
            _viewModelSuscrito.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(GestionProyectosViewModel.ProyectosVisibles))
            Dispatcher.BeginInvoke(AplicarBusqueda);
    }

    private void OnBusquedaProyectoCambiada(object sender, System.Windows.Controls.TextChangedEventArgs e) =>
        AplicarBusqueda();

    private void AplicarBusqueda()
    {
        if (GrillaProyectosPrincipal?.ItemsSource is null) return;
        var vista = CollectionViewSource.GetDefaultView(GrillaProyectosPrincipal.ItemsSource);
        var texto = BusquedaProyectoTextBox?.Text?.Trim() ?? string.Empty;
        vista.Filter = item => item is ProyectoResumen proyecto
            && (texto.Length == 0
                || proyecto.Nombre.Contains(texto, StringComparison.CurrentCultureIgnoreCase));
        vista.Refresh();
    }

    private void OnAbrirDetalleProyectoClic(object sender, RoutedEventArgs e) => AbrirDetalleProyecto();

    private void AbrirDetalleProyecto()
    {
        if (ViewModel is not { } vm) return;
        if (vm.ProyectoSeleccionado is null && !vm.TieneCambiosProyecto) return;

        var ventanaProyecto = new DetalleProyectoWindow(vm) { Owner = Window.GetWindow(this) };
        ventanaProyecto.ShowDialog();
    }
}