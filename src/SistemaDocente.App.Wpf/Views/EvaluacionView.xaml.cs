using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

using SistemaDocente.App.Wpf.Services;
using SistemaDocente.Presentation;

namespace SistemaDocente.App.Wpf.Views;

/// <summary>
/// Matriz de evaluación estudiante × actividad. Las dos primeras columnas permanecen
/// congeladas y las actividades se generan dinámicamente con códigos visuales estables.
/// </summary>
public partial class EvaluacionView : UserControl
{
    private EvaluacionActividadesViewModel? _viewModelSuscrito;
    private bool _temaSuscrito;

    public EvaluacionView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        DataContextChanged += OnDataContextChanged;
    }

    private EvaluacionActividadesViewModel? ViewModel => DataContext as EvaluacionActividadesViewModel;

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        CambiarSuscripcion(ViewModel);
        if (!_temaSuscrito)
        {
            ThemeService.ThemeChanged += OnThemeChanged;
            _temaSuscrito = true;
        }
        CrearColumnasActividades();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        CambiarSuscripcion(null);
        if (_temaSuscrito)
        {
            ThemeService.ThemeChanged -= OnThemeChanged;
            _temaSuscrito = false;
        }
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        CambiarSuscripcion(e.NewValue as EvaluacionActividadesViewModel);
        Dispatcher.BeginInvoke(CrearColumnasActividades);
    }

    private void CambiarSuscripcion(EvaluacionActividadesViewModel? nuevo)
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
        if (e.PropertyName == nameof(EvaluacionActividadesViewModel.ColumnasActividades))
            Dispatcher.BeginInvoke(CrearColumnasActividades);
    }

    private void OnThemeChanged(object? sender, string themeName) =>
        Dispatcher.BeginInvoke(CrearColumnasActividades);

    private void CrearColumnasActividades()
    {
        while (GrillaEvaluacionMatriz.Columns.Count > 2)
            GrillaEvaluacionMatriz.Columns.RemoveAt(2);

        if (ViewModel is not { } vm) return;
        for (var indice = 0; indice < vm.ColumnasActividades.Count; indice++)
        {
            var actividad = vm.ColumnasActividades[indice];
            var header = new TextBlock
            {
                Text = actividad.Codigo,
                ToolTip = actividad.DescripcionAccesible,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            AutomationProperties.SetName(header, actividad.DescripcionAccesible);

            GrillaEvaluacionMatriz.Columns.Add(new DataGridTextColumn
            {
                Header = header,
                Binding = new Binding($"Celdas[{indice}].EtiquetaNivel"),
                Width = 58,
                IsReadOnly = true,
                ElementStyle = CrearEstiloTextoCelda(indice),
                CellStyle = CrearEstiloCelda(indice),
            });
        }
    }

    private static Style CrearEstiloTextoCelda(int indice)
    {
        var estilo = new Style(typeof(TextBlock));
        estilo.Setters.Add(new Setter(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Stretch));
        estilo.Setters.Add(new Setter(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Stretch));
        estilo.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Center));
        estilo.Setters.Add(new Setter(TextBlock.FontWeightProperty, FontWeights.Bold));
        estilo.Setters.Add(new Setter(TextBlock.PaddingProperty, new Thickness(4, 11, 4, 0)));
        estilo.Setters.Add(new Setter(TextBlock.ToolTipProperty, new Binding($"Celdas[{indice}].DescripcionAccesible")));
        return estilo;
    }

    private static Style CrearEstiloCelda(int indice)
    {
        var estilo = new Style(typeof(DataGridCell));
        estilo.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch));
        estilo.Setters.Add(new Setter(Control.VerticalContentAlignmentProperty, VerticalAlignment.Stretch));
        estilo.Setters.Add(new Setter(Control.BorderBrushProperty, ObtenerBrush("BorderLightBrush")));

        AgregarEstado(estilo, indice, "D", "SuccessBackgroundBrush", "SuccessBrush");
        AgregarEstado(estilo, indice, "S", "InfoBackgroundBrush", "InfoBrush");
        AgregarEstado(estilo, indice, "E", "WarningBackgroundBrush", "WarningBrush");
        AgregarEstado(estilo, indice, "R", "ErrorBackgroundBrush", "ErrorBrush");
        AgregarEstado(estilo, indice, "✓", "InfoBackgroundBrush", "InfoBrush");
        AgregarEstado(estilo, indice, "P", "WarningBackgroundBrush", "WarningBrush");
        AgregarEstado(estilo, indice, "N", "DisabledBackgroundBrush", "TextMutedBrush");

        var noAplicable = new DataTrigger
        {
            Binding = new Binding($"Celdas[{indice}].EsAplicable"),
            Value = false,
        };
        noAplicable.Setters.Add(new Setter(Control.BackgroundProperty, ObtenerBrush("SectionBackgroundBrush")));
        noAplicable.Setters.Add(new Setter(Control.ForegroundProperty, ObtenerBrush("TextDisabledBrush")));
        estilo.Triggers.Add(noAplicable);
        return estilo;
    }

    private static void AgregarEstado(
        Style estilo,
        int indice,
        string etiqueta,
        string fondoKey,
        string frenteKey)
    {
        var trigger = new DataTrigger
        {
            Binding = new Binding($"Celdas[{indice}].EtiquetaNivel"),
            Value = etiqueta,
        };
        trigger.Setters.Add(new Setter(Control.BackgroundProperty, ObtenerBrush(fondoKey)));
        trigger.Setters.Add(new Setter(Control.ForegroundProperty, ObtenerBrush(frenteKey)));
        estilo.Triggers.Add(trigger);
    }

    private static Brush ObtenerBrush(string clave) =>
        System.Windows.Application.Current?.TryFindResource(clave) as Brush ?? Brushes.Transparent;

    private void OnCeldaActualCambiada(object? sender, EventArgs e) => SeleccionarCeldaActual();

    private bool SeleccionarCeldaActual()
    {
        if (ViewModel is not { } vm
            || GrillaEvaluacionMatriz.CurrentItem is not EvaluacionEstudianteFilaVisual fila
            || GrillaEvaluacionMatriz.CurrentColumn is null)
        {
            return false;
        }

        var indiceActividad = GrillaEvaluacionMatriz.CurrentColumn.DisplayIndex - 2;
        if (indiceActividad < 0 || indiceActividad >= vm.ColumnasActividades.Count) return false;
        vm.SeleccionarCelda(fila, indiceActividad);
        return true;
    }

    private void OnGrillaEvaluacionClick(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is not DependencyObject origen) return;
        var celda = BuscarAncestro<DataGridCell>(origen);
        if (celda?.Column is null || celda.Column.DisplayIndex < 2) return;

        GrillaEvaluacionMatriz.CurrentCell = new DataGridCellInfo(celda.DataContext, celda.Column);
        GrillaEvaluacionMatriz.SelectedCells.Clear();
        GrillaEvaluacionMatriz.SelectedCells.Add(GrillaEvaluacionMatriz.CurrentCell);
        if (!SeleccionarCeldaActual()) return;

        MostrarMenuResultadoCompacto(celda);
        e.Handled = true;
    }

    private void MostrarMenuResultadoCompacto(FrameworkElement destino)
    {
        if (ViewModel is not { } vm
            || vm.CeldaSeleccionada is not { EsAplicable: true, EsEditable: true })
        {
            return;
        }

        var menu = new ContextMenu
        {
            PlacementTarget = destino,
            Placement = PlacementMode.Bottom,
        };
        AgregarComando(menu, "Pendiente (P)", vm.MarcarPendienteEntregaCommand);
        AgregarComando(menu, "Entregada · evaluar después (T)", vm.MarcarEntregadaCommand);
        menu.Items.Add(new Separator());
        AgregarComando(menu, "Domina (D)", vm.MarcarDominaCommand);
        AgregarComando(menu, "Suficiente (S)", vm.MarcarSuficienteCommand);
        AgregarComando(menu, "En proceso (E)", vm.MarcarEnProcesoCommand);
        AgregarComando(menu, "Requiere apoyo (R)", vm.MarcarRequiereApoyoCommand);
        menu.Items.Add(new Separator());
        AgregarComando(menu, "No entregó (N)", vm.MarcarNoEntregadaCommand);
        menu.Items.Add(new Separator());

        var masOpciones = new MenuItem { Header = "Más opciones…" };
        masOpciones.Click += (_, _) => AbrirEditorCelda();
        menu.Items.Add(masOpciones);
        menu.IsOpen = true;
    }

    private static void AgregarComando(ContextMenu menu, string texto, RelayCommand command)
    {
        var opcion = new MenuItem
        {
            Header = texto,
            Command = command,
        };
        menu.Items.Add(opcion);
    }

    private void OnGrillaEvaluacionPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (ViewModel is not { } vm
            || Keyboard.FocusedElement is TextBoxBase
            || Keyboard.FocusedElement is not DependencyObject foco
            || !GrillaEvaluacionMatriz.IsAncestorOf(foco))
        {
            return;
        }

        if (Keyboard.Modifiers == ModifierKeys.None
            && e.Key is Key.D or Key.S or Key.E or Key.R or Key.T or Key.N or Key.P)
        {
            if (!SeleccionarCeldaActual()) return;
            var command = e.Key switch
            {
                Key.D => vm.MarcarDominaCommand,
                Key.S => vm.MarcarSuficienteCommand,
                Key.E => vm.MarcarEnProcesoCommand,
                Key.R => vm.MarcarRequiereApoyoCommand,
                Key.T => vm.MarcarEntregadaCommand,
                Key.N => vm.MarcarNoEntregadaCommand,
                _ => vm.MarcarPendienteEntregaCommand,
            };
            if (command.CanExecute(null))
            {
                command.Execute(null);
                AvanzarFila();
                e.Handled = true;
            }
            return;
        }

        if (Keyboard.Modifiers == ModifierKeys.None && e.Key is Key.Enter or Key.F2)
        {
            if (SeleccionarCeldaActual()) AbrirEditorCelda();
            e.Handled = true;
        }
    }

    private void OnGrillaEvaluacionDobleClic(object sender, MouseButtonEventArgs e)
    {
        if (SeleccionarCeldaActual()) AbrirEditorCelda();
    }

    private void AbrirEditorCelda()
    {
        if (ViewModel is not { } vm
            || vm.CeldaSeleccionada is not { EsAplicable: true, EsEditable: true } celda
            || vm.ActividadColumnaSeleccionada is not { } actividad
            || GrillaEvaluacionMatriz.CurrentItem is not EvaluacionEstudianteFilaVisual fila)
        {
            return;
        }

        var ventana = new EditarEvaluacionCeldaWindow(fila, actividad, celda)
        {
            Owner = Window.GetWindow(this),
        };
        ventana.ShowDialog();
    }

    private void AvanzarFila()
    {
        var indiceFila = GrillaEvaluacionMatriz.Items.IndexOf(GrillaEvaluacionMatriz.CurrentItem);
        if (indiceFila < 0 || indiceFila + 1 >= GrillaEvaluacionMatriz.Items.Count
            || GrillaEvaluacionMatriz.CurrentColumn is null)
        {
            return;
        }

        var siguiente = GrillaEvaluacionMatriz.Items[indiceFila + 1];
        GrillaEvaluacionMatriz.CurrentCell = new DataGridCellInfo(siguiente, GrillaEvaluacionMatriz.CurrentColumn);
        GrillaEvaluacionMatriz.ScrollIntoView(siguiente, GrillaEvaluacionMatriz.CurrentColumn);
        SeleccionarCeldaActual();
    }

    private static T? BuscarAncestro<T>(DependencyObject origen) where T : DependencyObject
    {
        DependencyObject? actual = origen;
        while (actual is not null)
        {
            if (actual is T encontrado) return encontrado;
            actual = VisualTreeHelper.GetParent(actual);
        }

        return null;
    }
}