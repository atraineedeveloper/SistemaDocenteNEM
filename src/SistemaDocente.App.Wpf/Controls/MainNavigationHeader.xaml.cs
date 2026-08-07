using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

using SistemaDocente.App.Wpf.Services;
using SistemaDocente.Presentation;

namespace SistemaDocente.App.Wpf.Controls;

/// <summary>
/// Encabezado global del shell: branding, contexto de grupo, navegación principal
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
        ActualizarSelectorGrupo();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e) => CambiarSuscripcion(null);

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        CambiarSuscripcion(e.NewValue as MainWindowViewModel);
        ActualizarPestañaActiva();
        ActualizarSelectorGrupo();
    }

    private void CambiarSuscripcion(MainWindowViewModel? nuevo)
    {
        if (ReferenceEquals(_viewModelSuscrito, nuevo)) return;

        if (_viewModelSuscrito is not null)
        {
            _viewModelSuscrito.PropertyChanged -= OnViewModelPropertyChanged;
            _viewModelSuscrito.Grupo.PropertyChanged -= OnGrupoPropertyChanged;
        }

        _viewModelSuscrito = nuevo;

        if (_viewModelSuscrito is not null)
        {
            _viewModelSuscrito.PropertyChanged += OnViewModelPropertyChanged;
            _viewModelSuscrito.Grupo.PropertyChanged += OnGrupoPropertyChanged;
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

    private void OnGrupoPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(GestionGrupoViewModel.GrupoIdActual)
            or nameof(GestionGrupoViewModel.GruposDisponibles)
            or nameof(GestionGrupoViewModel.NombreGrupo))
        {
            ActualizarSelectorGrupo();
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

    private void ActualizarSelectorGrupo()
    {
        if (ViewModel is not { } vm || GrupoContextMenu is null) return;

        GrupoContextMenu.Items.Clear();
        foreach (var grupo in vm.Grupo.GruposDisponibles)
        {
            var opcion = new MenuItem
            {
                Header = grupo.NombreVisible,
                IsCheckable = true,
                IsChecked = grupo.GrupoId == vm.Grupo.GrupoIdActual,
            };
            AutomationProperties.SetName(opcion, $"Cambiar al grupo {grupo.NombreVisible}");
            opcion.Click += (_, _) => vm.CambiarGrupo(grupo.GrupoId);
            GrupoContextMenu.Items.Add(opcion);
        }

        if (vm.Grupo.GruposDisponibles.Count > 0)
        {
            GrupoContextMenu.Items.Add(new Separator());
        }

        GrupoContextMenu.Items.Add(new MenuItem
        {
            Header = "Mis grupos",
            Command = vm.IrAInicioCommand,
        });
        GrupoContextMenu.Items.Add(new MenuItem
        {
            Header = "Crear grupo…",
            Command = vm.CrearGrupoDesdeInicioCommand,
        });
    }

    private void TemaClaro_Click(object sender, RoutedEventArgs e) =>
        ThemeService.ApplyTheme(ThemeService.Light);

    private void TemaOscuro_Click(object sender, RoutedEventArgs e) =>
        ThemeService.ApplyTheme(ThemeService.Dark);

    private void TemaAltoContraste_Click(object sender, RoutedEventArgs e) =>
        ThemeService.ApplyTheme(ThemeService.HighContrast);
}
