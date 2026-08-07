using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

using SistemaDocente.App.Wpf.Services;
using SistemaDocente.Presentation;

namespace SistemaDocente.App.Wpf.Controls;

/// <summary>
/// Encabezado global del shell: branding, selector de grupo, navegación principal
/// y selector de tema. No contiene lógica de módulos.
/// </summary>
public partial class MainNavigationHeader : UserControl
{
    private MainWindowViewModel? _viewModelSuscrito;

    public MainNavigationHeader()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        DataContextChanged += OnDataContextChanged;
    }

    private MainWindowViewModel? ViewModel => DataContext as MainWindowViewModel;

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        CambiarSuscripcion(ViewModel);
        ActualizarPestañaActiva();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e) => CambiarSuscripcion(null);

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        CambiarSuscripcion(e.NewValue as MainWindowViewModel);
        ActualizarPestañaActiva();
    }

    private void CambiarSuscripcion(MainWindowViewModel? nuevo)
    {
        if (ReferenceEquals(_viewModelSuscrito, nuevo)) return;

        if (_viewModelSuscrito is not null)
        {
            _viewModelSuscrito.PropertyChanged -= OnViewModelPropertyChanged;
        }

        _viewModelSuscrito = nuevo;

        if (_viewModelSuscrito is not null)
        {
            _viewModelSuscrito.PropertyChanged += OnViewModelPropertyChanged;
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(MainWindowViewModel.MostrarGrupo)
            or nameof(MainWindowViewModel.MostrarAsistenciaDiaria)
            or nameof(MainWindowViewModel.MostrarAsistenciaMensual)
            or nameof(MainWindowViewModel.MostrarProyectos)
            or nameof(MainWindowViewModel.MostrarEvaluacion)
            or nameof(MainWindowViewModel.MostrarReportes))
        {
            ActualizarPestañaActiva();
        }
    }

    private void ActualizarPestañaActiva()
    {
        if (ViewModel is not { } vm) return;

        NavBtnGrupo.Tag = vm.MostrarGrupo ? "activo" : string.Empty;
        NavBtnAsistencia.Tag = vm.MostrarAsistenciaDiaria || vm.MostrarAsistenciaMensual ? "activo" : string.Empty;
        NavBtnProyectos.Tag = vm.MostrarProyectos ? "activo" : string.Empty;
        NavBtnEvaluacion.Tag = vm.MostrarEvaluacion ? "activo" : string.Empty;
        NavBtnReportes.Tag = vm.MostrarReportes ? "activo" : string.Empty;
    }

    private void TemaClaro_Click(object sender, RoutedEventArgs e) =>
        ThemeService.ApplyTheme(ThemeService.Light);

    private void TemaOscuro_Click(object sender, RoutedEventArgs e) =>
        ThemeService.ApplyTheme(ThemeService.Dark);

    private void TemaAltoContraste_Click(object sender, RoutedEventArgs e) =>
        ThemeService.ApplyTheme(ThemeService.HighContrast);
}