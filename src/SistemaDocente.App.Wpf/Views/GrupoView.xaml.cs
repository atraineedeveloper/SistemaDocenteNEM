using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using SistemaDocente.Presentation;

namespace SistemaDocente.App.Wpf.Views;

/// <summary>
/// Presentación del módulo Grupo: bienvenida/creación, lista de estudiantes,
/// búsqueda, editor de nombre y apertura de ventanas dedicadas.
/// </summary>
public partial class GrupoView : UserControl
{
    private GestionGrupoViewModel? _viewModelSuscrito;

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
    }

    private void OnUnloaded(object sender, RoutedEventArgs e) => CambiarSuscripcion(null);

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e) =>
        CambiarSuscripcion(e.NewValue as GestionGrupoViewModel);

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
