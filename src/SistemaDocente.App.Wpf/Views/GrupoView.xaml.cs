using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

using SistemaDocente.Presentation;

namespace SistemaDocente.App.Wpf.Views;

/// <summary>
/// Presentación del módulo Resumen/Grupo: lista de estudiantes, búsqueda,
/// filtros/orden, acciones contextuales y apertura de ventanas dedicadas.
/// </summary>
public partial class GrupoView : UserControl
{
    private GestionGrupoViewModel? _viewModelSuscrito;
    private ICollectionView? _vistaEstudiantes;

    public GrupoView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        DataContextChanged += OnDataContextChanged;
    }

    public static readonly DependencyProperty ExpedienteProperty = DependencyProperty.Register(
        nameof(Expediente),
        typeof(GestionExpedienteViewModel),
        typeof(GrupoView),
        new PropertyMetadata(null));

    public GestionExpedienteViewModel? Expediente
    {
        get => (GestionExpedienteViewModel?)GetValue(ExpedienteProperty);
        set => SetValue(ExpedienteProperty, value);
    }

    public static readonly DependencyProperty ConfiguracionProperty = DependencyProperty.Register(
        nameof(Configuracion),
        typeof(ConfiguracionGrupoViewModel),
        typeof(GrupoView),
        new PropertyMetadata(null));

    public ConfiguracionGrupoViewModel? Configuracion
    {
        get => (ConfiguracionGrupoViewModel?)GetValue(ConfiguracionProperty);
        set => SetValue(ConfiguracionProperty, value);
    }

    public static readonly DependencyProperty ImportacionProperty = DependencyProperty.Register(
        nameof(Importacion),
        typeof(ImportacionEstudiantesViewModel),
        typeof(GrupoView),
        new PropertyMetadata(null));

    public ImportacionEstudiantesViewModel? Importacion
    {
        get => (ImportacionEstudiantesViewModel?)GetValue(ImportacionProperty);
        set => SetValue(ImportacionProperty, value);
    }

    public static readonly DependencyProperty ExportacionProperty = DependencyProperty.Register(
        nameof(Exportacion),
        typeof(ExportacionGrupoViewModel),
        typeof(GrupoView),
        new PropertyMetadata(null));

    public ExportacionGrupoViewModel? Exportacion
    {
        get => (ExportacionGrupoViewModel?)GetValue(ExportacionProperty);
        set => SetValue(ExportacionProperty, value);
    }

    private GestionGrupoViewModel? ViewModel => DataContext as GestionGrupoViewModel;

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        CambiarSuscripcion(ViewModel);
        AsignarFocoInicial();
        AbrirEditorEstudianteVentana();
        ActualizarVistaEstudiantes();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        CambiarSuscripcion(null);
        _vistaEstudiantes = null;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        CambiarSuscripcion(e.NewValue as GestionGrupoViewModel);
        if (IsLoaded)
        {
            Dispatcher.BeginInvoke(ActualizarVistaEstudiantes);
        }
    }

    private void CambiarSuscripcion(GestionGrupoViewModel? nuevo)
    {
        if (ReferenceEquals(_viewModelSuscrito, nuevo)) return;

        if (_viewModelSuscrito is not null)
        {
            _viewModelSuscrito.PropertyChanged -= OnGrupoPropertyChanged;
        }

        _viewModelSuscrito = nuevo;

        if (_viewModelSuscrito is not null)
        {
            _viewModelSuscrito.PropertyChanged += OnGrupoPropertyChanged;
        }
    }

    private void OnGrupoPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName is nameof(GestionGrupoViewModel.PanelActual)
            or nameof(GestionGrupoViewModel.MostrarBienvenida))
        {
            Dispatcher.BeginInvoke(AsignarFocoInicial);
            if (ViewModel is { } vm && vm.PanelActual is PanelEdicion.AgregarEstudiante or PanelEdicion.EditarEstudiante)
            {
                Dispatcher.BeginInvoke(AbrirEditorEstudianteVentana);
            }
        }

        if (args.PropertyName is nameof(GestionGrupoViewModel.Estudiantes)
            or nameof(GestionGrupoViewModel.FiltroBusqueda))
        {
            Dispatcher.BeginInvoke(ActualizarVistaEstudiantes);
        }
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && ViewModel is { } vm && vm.CancelarEdicionCommand.CanExecute(null))
        {
            vm.CancelarEdicionCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void OnVerExpedienteEstudianteClic(object sender, RoutedEventArgs e) => AbrirExpedienteEstudiante();

    private void OnConfigurarGrupoClic(object sender, RoutedEventArgs e)
    {
        if (ViewModel?.GrupoIdActual is not { } grupoId || Configuracion is null) return;

        Configuracion.Inicializar(grupoId);
        var ventana = new ConfiguracionGrupoWindow(Configuracion)
        {
            Owner = Window.GetWindow(this),
        };
        ventana.ShowDialog();
    }

    private void OnImportarAlumnosClic(object sender, RoutedEventArgs e)
    {
        if (ViewModel is not { } grupo ||
            grupo.GrupoIdActual is not { } grupoId ||
            Importacion is not { } importacion)
        {
            return;
        }

        importacion.Inicializar(grupoId);
        var ventana = new ImportacionEstudiantesWindow(importacion)
        {
            Owner = Window.GetWindow(this),
        };

        var importacionCompletada = ventana.ShowDialog() == true;
        if (importacionCompletada && importacion.Importados > 0)
        {
            grupo.CargarGrupoPorId(grupoId);
        }
    }

    private void OnExportarDatosClic(object sender, RoutedEventArgs e)
    {
        if (ViewModel?.GrupoIdActual is not { } grupoId || Exportacion is not { } exportacion)
        {
            return;
        }

        exportacion.Inicializar(grupoId, DateOnly.FromDateTime(DateTime.Today));
        var ventana = new ExportacionGrupoWindow(exportacion)
        {
            Owner = Window.GetWindow(this),
        };
        ventana.ShowDialog();
    }

    private void OnFiltroEstadoCambiado(object sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded)
        {
            ActualizarVistaEstudiantes();
        }
    }

    private void OnOrdenEstudiantesCambiado(object sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded)
        {
            ActualizarVistaEstudiantes();
        }
    }

    private void ActualizarVistaEstudiantes()
    {
        if (StudentGrid.ItemsSource is null) return;

        _vistaEstudiantes = CollectionViewSource.GetDefaultView(StudentGrid.ItemsSource);
        using (_vistaEstudiantes.DeferRefresh())
        {
            _vistaEstudiantes.Filter = FiltrarEstudiante;
            _vistaEstudiantes.SortDescriptions.Clear();

            var orden = StudentSortCombo.SelectedIndex switch
            {
                1 => new SortDescription(nameof(EstudianteVisual.Nombre), ListSortDirection.Descending),
                2 => new SortDescription(nameof(EstudianteVisual.NumeroLista), ListSortDirection.Ascending),
                _ => new SortDescription(nameof(EstudianteVisual.Nombre), ListSortDirection.Ascending),
            };
            _vistaEstudiantes.SortDescriptions.Add(orden);
        }
    }

    private bool FiltrarEstudiante(object item)
    {
        if (item is not EstudianteVisual estudiante) return false;

        var busqueda = ViewModel?.FiltroBusqueda?.Trim() ?? string.Empty;
        var coincideBusqueda = string.IsNullOrWhiteSpace(busqueda)
            || estudiante.Nombre.Contains(busqueda, StringComparison.CurrentCultureIgnoreCase)
            || estudiante.NumeroLista.ToString(System.Globalization.CultureInfo.CurrentCulture)
                .Contains(busqueda, StringComparison.Ordinal)
            || estudiante.GradoTexto.Contains(busqueda, StringComparison.CurrentCultureIgnoreCase);

        if (!coincideBusqueda) return false;

        return StudentStatusFilterCombo.SelectedIndex switch
        {
            1 => estudiante.EstaActivo,
            2 => !estudiante.EstaActivo,
            _ => true,
        };
    }

    private void OnStudentGridPreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        var fila = ObtenerFilaEstudiante(StudentGrid, e.OriginalSource as DependencyObject);
        if (fila?.Item is not EstudianteVisual estudiante) return;

        SeleccionarEstudiante(estudiante);
        AbrirMenuContextualEstudiante(fila, PlacementMode.MousePoint);
        e.Handled = true;
    }

    private void OnStudentMoreClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: EstudianteVisual estudiante } boton) return;

        SeleccionarEstudiante(estudiante);
        AbrirMenuContextualEstudiante(boton, PlacementMode.Bottom);
        e.Handled = true;
    }

    private void OnStudentGridMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left) return;

        var fila = ObtenerFilaEstudiante(StudentGrid, e.OriginalSource as DependencyObject);
        if (fila?.Item is not EstudianteVisual estudiante) return;

        SeleccionarEstudiante(estudiante);
        AbrirExpedienteEstudiante();
        e.Handled = true;
    }

    private void SeleccionarEstudiante(EstudianteVisual estudiante)
    {
        StudentGrid.SelectedItem = estudiante;
        if (ViewModel is { } grupo)
        {
            grupo.EstudianteSeleccionado = estudiante;
        }
    }

    private void AbrirMenuContextualEstudiante(FrameworkElement destino, PlacementMode ubicacion)
    {
        if (ViewModel is not { EstudianteSeleccionado: { } estudiante } grupo) return;

        var menu = new ContextMenu
        {
            PlacementTarget = destino,
            Placement = ubicacion,
        };

        var verExpediente = new MenuItem { Header = "Ver expediente" };
        verExpediente.Click += (_, _) => AbrirExpedienteEstudiante();
        menu.Items.Add(verExpediente);

        menu.Items.Add(new MenuItem
        {
            Header = "Editar estudiante",
            Command = grupo.AbrirEditarEstudianteCommand,
        });

        menu.Items.Add(new Separator());

        if (estudiante.EstaActivo)
        {
            menu.Items.Add(new MenuItem
            {
                Header = "Desactivar estudiante",
                Command = grupo.DesactivarEstudianteCommand,
            });
        }
        else
        {
            menu.Items.Add(new MenuItem
            {
                Header = "Reactivar estudiante",
                Command = grupo.ReactivarEstudianteCommand,
            });
        }

        menu.IsOpen = true;
    }

    private static DataGridRow? ObtenerFilaEstudiante(DataGrid grid, DependencyObject? origen)
    {
        var actual = origen;
        while (actual is not null && actual is not DataGridRow)
        {
            actual = VisualTreeHelper.GetParent(actual);
        }

        return actual as DataGridRow;
    }

    private void AbrirExpedienteEstudiante()
    {
        if (ViewModel is not { } grupo
            || grupo.GrupoIdActual is not { } grupoId
            || grupo.EstudianteSeleccionado is not { } estudiante
            || Expediente is not { } expediente)
        {
            return;
        }

        expediente.Cargar(grupoId, estudiante.Id);
        var ventanaExpediente = new ExpedienteEstudianteWindow(expediente)
        {
            Owner = Window.GetWindow(this),
        };
        ventanaExpediente.ShowDialog();
    }

    private void AbrirEditorEstudianteVentana()
    {
        if (ViewModel is not { } grupo
            || grupo.PanelActual is not (PanelEdicion.AgregarEstudiante or PanelEdicion.EditarEstudiante))
        {
            return;
        }

        if (Configuracion is not null && grupo.GrupoIdActual is { } grupoId)
        {
            Configuracion.Inicializar(grupoId);
            grupo.ConfigurarGradosDisponibles(Configuracion.ObtenerGradosConfigurados());
        }
        else
        {
            grupo.ConfigurarGradosDisponibles(null);
        }

        var ventana = new EditorEstudianteWindow(grupo)
        {
            Owner = Window.GetWindow(this),
        };
        ventana.ShowDialog();
    }

    private void AsignarFocoInicial()
    {
        if (ViewModel is not { } grupo) return;

        UIElement? target = null;
        if (grupo.MostrarBienvenida)
        {
            target = FindFirstFocusableControl(GrupoBienvenidaPanel);
        }
        else if (grupo.MostrarEditorGrupo)
        {
            target = FindFirstFocusableControl(GrupoEditorPanel);
        }

        if (target is not null)
        {
            Dispatcher.BeginInvoke(() => target.Focus(), System.Windows.Threading.DispatcherPriority.Render);
        }
    }

    private static UIElement? FindFirstFocusableControl(DependencyObject parent)
    {
        var count = VisualTreeHelper.GetChildrenCount(parent);
        for (var i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is TextBox or ComboBox or DatePicker or Button { IsDefault: true })
            {
                return (UIElement)child;
            }

            var descendant = FindFirstFocusableControl(child);
            if (descendant is not null)
            {
                return descendant;
            }
        }

        return null;
    }
}