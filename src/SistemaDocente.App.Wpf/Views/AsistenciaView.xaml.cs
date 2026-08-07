using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

using SistemaDocente.App.Wpf.Services;
using SistemaDocente.Core;
using SistemaDocente.Presentation;

namespace SistemaDocente.App.Wpf.Views;

/// <summary>
/// Presentación del módulo Asistencia. Consume una frontera propia de módulo y mantiene
/// en WPF únicamente comportamiento visual: columnas dinámicas, foco y teclado contextual.
/// </summary>
public partial class AsistenciaView : UserControl
{
    private ModuloAsistenciaViewModel? _viewModelSuscrito;
    private bool _temaSuscrito;

    public AsistenciaView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        DataContextChanged += OnDataContextChanged;
        GrillaMensual.PreviewMouseLeftButtonUp += OnGrillaMensualClick;
    }

    private ModuloAsistenciaViewModel? ViewModel => DataContext as ModuloAsistenciaViewModel;

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        CambiarSuscripcion(ViewModel);
        if (!_temaSuscrito)
        {
            ThemeService.ThemeChanged += OnThemeChanged;
            _temaSuscrito = true;
        }

        GrillaMensual.RowHeight = 42;
        GrillaMensual.ColumnHeaderHeight = 48;
        GrillaMensual.GridLinesVisibility = DataGridGridLinesVisibility.Horizontal;
        CrearColumnasMensuales();
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

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e) =>
        CambiarSuscripcion(e.NewValue as ModuloAsistenciaViewModel);

    private void CambiarSuscripcion(ModuloAsistenciaViewModel? nuevo)
    {
        if (ReferenceEquals(_viewModelSuscrito, nuevo)) return;

        if (_viewModelSuscrito is not null)
        {
            _viewModelSuscrito.Mensual.PropertyChanged -= OnAsistenciaMensualPropertyChanged;
        }

        _viewModelSuscrito = nuevo;

        if (_viewModelSuscrito is not null)
        {
            _viewModelSuscrito.Mensual.PropertyChanged += OnAsistenciaMensualPropertyChanged;
        }
    }

    private void OnAsistenciaMensualPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(GestionAsistenciaMensualViewModel.Dias))
        {
            Dispatcher.BeginInvoke(CrearColumnasMensuales);
        }
    }

    private void OnThemeChanged(object? sender, string themeName) =>
        Dispatcher.BeginInvoke(CrearColumnasMensuales);

    private void CrearColumnasMensuales()
    {
        if (ViewModel is not { } vm) return;

        GrillaMensual.Columns.Clear();
        GrillaMensual.Columns.Add(new DataGridTextColumn
        {
            Header = "Núm.",
            Binding = new Binding(nameof(AsistenciaEstudianteMesVisual.NumeroLista)),
            Width = 62,
        });
        GrillaMensual.Columns.Add(new DataGridTextColumn
        {
            Header = new TextBlock { Text = "Nombre", ToolTip = "Nombre completo del estudiante" },
            Binding = new Binding(nameof(AsistenciaEstudianteMesVisual.Nombre)),
            Width = 190,
        });

        for (var indice = 0; indice < vm.Mensual.Dias.Count; indice++)
        {
            var dia = vm.Mensual.Dias[indice];
            GrillaMensual.Columns.Add(new DataGridTextColumn
            {
                Header = new TextBlock
                {
                    Text = $"{dia.NumeroDia}\n{dia.AbreviaturaDiaSemana}",
                    ToolTip = $"Día {dia.NumeroDia} - {dia.Fecha:dd/MM/yyyy}",
                    TextAlignment = TextAlignment.Center,
                },
                Binding = new Binding($"Celdas[{indice}].Texto"),
                Width = 48,
                ElementStyle = CrearEstiloCeldaTexto(),
                CellStyle = CrearEstiloContenedor(indice, dia.EsCierreSemana),
                HeaderStyle = CrearEstiloEncabezado(dia.EsCierreSemana),
            });
        }

        AgregarResumen("P", nameof(AsistenciaEstudianteMesVisual.Presentes));
        AgregarResumen("F", nameof(AsistenciaEstudianteMesVisual.Faltas));
        AgregarResumen("R", nameof(AsistenciaEstudianteMesVisual.Retardos));
        AgregarResumen("J", nameof(AsistenciaEstudianteMesVisual.Justificadas));
        AgregarResumen("%", nameof(AsistenciaEstudianteMesVisual.PorcentajeTexto), 64);
    }

    private void AgregarResumen(string encabezado, string propiedad, double ancho = 42) =>
        GrillaMensual.Columns.Add(new DataGridTextColumn
        {
            Header = new TextBlock
            {
                Text = encabezado,
                ToolTip = encabezado switch
                {
                    "P" => "Total presentes",
                    "F" => "Total faltas",
                    "R" => "Total retardos",
                    "J" => "Total justificadas",
                    "%" => "Porcentaje de asistencia",
                    _ => encabezado,
                },
            },
            Binding = new Binding(propiedad),
            Width = ancho,
        });

    private static Style CrearEstiloCeldaTexto()
    {
        var estilo = new Style(typeof(TextBlock));
        estilo.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Center));
        estilo.Setters.Add(new Setter(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Stretch));
        estilo.Setters.Add(new Setter(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center));
        estilo.Setters.Add(new Setter(TextBlock.FontWeightProperty, FontWeights.SemiBold));
        estilo.Setters.Add(new Setter(TextBlock.FontSizeProperty, 13d));
        return estilo;
    }

    private static Style CrearEstiloContenedor(int indice, bool esCierreSemana)
    {
        var estilo = new Style(typeof(DataGridCell));
        estilo.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch));
        estilo.Setters.Add(new Setter(Control.VerticalContentAlignmentProperty, VerticalAlignment.Stretch));
        estilo.Setters.Add(new Setter(Control.BorderBrushProperty, ObtenerBrush("BorderLightBrush")));
        estilo.Setters.Add(new Setter(Control.BorderThicknessProperty, esCierreSemana
            ? new Thickness(0, 0, 3, 1)
            : new Thickness(0, 0, 1, 1)));

        AgregarColorCelda(estilo, indice, "P", "SuccessBackgroundBrush", "SuccessBrush");
        AgregarColorCelda(estilo, indice, "F", "ErrorBackgroundBrush", "ErrorBrush");
        AgregarColorCelda(estilo, indice, "R", "WarningBackgroundBrush", "WarningBrush");
        AgregarColorCelda(estilo, indice, "J", "InfoBackgroundBrush", "InfoBrush");

        var hover = new Trigger { Property = UIElement.IsMouseOverProperty, Value = true };
        hover.Setters.Add(new Setter(Control.BorderBrushProperty, ObtenerBrush("PrimaryBrush")));
        estilo.Triggers.Add(hover);

        var seleccion = new Trigger { Property = DataGridCell.IsSelectedProperty, Value = true };
        seleccion.Setters.Add(new Setter(Control.BorderBrushProperty, ObtenerBrush("PrimaryBrush")));
        seleccion.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(2)));
        estilo.Triggers.Add(seleccion);
        return estilo;
    }

    private static Style CrearEstiloEncabezado(bool esCierreSemana)
    {
        var estilo = new Style(typeof(DataGridColumnHeader));
        estilo.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Center));
        estilo.Setters.Add(new Setter(Control.VerticalContentAlignmentProperty, VerticalAlignment.Center));
        estilo.Setters.Add(new Setter(Control.FontWeightProperty, FontWeights.SemiBold));
        if (esCierreSemana)
        {
            estilo.Setters.Add(new Setter(Control.BorderBrushProperty, ObtenerBrush("BorderDefaultBrush")));
            estilo.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(0, 0, 3, 0)));
        }

        return estilo;
    }

    private static void AgregarColorCelda(Style estilo, int indice, string texto, string fondoKey, string frenteKey)
    {
        var disparador = new DataTrigger
        {
            Binding = new Binding($"Celdas[{indice}].Texto"),
            Value = texto,
        };
        disparador.Setters.Add(new Setter(Control.BackgroundProperty, ObtenerBrush(fondoKey)));
        disparador.Setters.Add(new Setter(Control.ForegroundProperty, ObtenerBrush(frenteKey)));
        estilo.Triggers.Add(disparador);
    }

    private static Brush ObtenerBrush(string clave) =>
        System.Windows.Application.Current?.TryFindResource(clave) as Brush ?? Brushes.Transparent;

    private void OnCeldaMensualSeleccionada(object? sender, EventArgs e)
    {
        if (ObtenerCeldaActual() is { } seleccion && ViewModel is { } vm)
        {
            vm.Mensual.SeleccionarCelda(seleccion.Fila, seleccion.Fecha);
        }
    }

    private void OnGrillaMensualClick(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is not DependencyObject origen) return;
        var celda = BuscarAncestro<DataGridCell>(origen);
        if (celda?.Column is null || ViewModel is not { } vm) return;

        var indiceDia = celda.Column.DisplayIndex - 2;
        if (indiceDia < 0 || indiceDia >= vm.Mensual.Dias.Count) return;

        GrillaMensual.CurrentCell = new DataGridCellInfo(celda.DataContext, celda.Column);
        GrillaMensual.SelectedCells.Clear();
        GrillaMensual.SelectedCells.Add(GrillaMensual.CurrentCell);
        OnCeldaMensualSeleccionada(sender, EventArgs.Empty);
        MostrarSelectorCompacto(celda);
        e.Handled = true;
    }

    private void OnCeldaMensualDobleClic(object sender, MouseButtonEventArgs e)
    {
        if (ObtenerCeldaActual() is not null) MostrarSelectorCompacto();
    }

    /// <summary>
    /// Los atajos simples y PageUp/PageDown sólo operan cuando el foco está realmente
    /// dentro de la grilla mensual. Nunca interceptan entrada en controles de texto.
    /// </summary>
    private void OnGrillaMensualPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (Keyboard.FocusedElement is TextBoxBase
            || Keyboard.FocusedElement is not DependencyObject foco
            || !GrillaMensual.IsAncestorOf(foco))
        {
            return;
        }

        if (Keyboard.Modifiers == ModifierKeys.None && e.Key is Key.P or Key.F or Key.R or Key.J)
        {
            var estado = e.Key switch
            {
                Key.P => EstadoAsistencia.Presente,
                Key.F => EstadoAsistencia.Falta,
                Key.R => EstadoAsistencia.Retardo,
                _ => EstadoAsistencia.Justificada,
            };
            if (AsignarEstadoActual(estado)) AvanzarFila();
            e.Handled = true;
            return;
        }

        if (Keyboard.Modifiers == ModifierKeys.None && e.Key is Key.PageUp or Key.PageDown)
        {
            if (ViewModel is { } vm)
            {
                var command = e.Key == Key.PageUp
                    ? vm.Mensual.MesAnteriorCommand
                    : vm.Mensual.MesSiguienteCommand;
                if (command.CanExecute(null)) command.Execute(null);
                e.Handled = true;
            }
            return;
        }

        if (Keyboard.Modifiers == ModifierKeys.None && e.Key == Key.Enter)
        {
            MostrarSelectorCompacto();
            e.Handled = true;
        }
        else if (Keyboard.Modifiers == ModifierKeys.None && e.Key is Key.Home or Key.End)
        {
            MoverAExtremoDeDias(e.Key == Key.End);
            e.Handled = true;
        }
    }

    private void MostrarSelectorCompacto(FrameworkElement? destino = null)
    {
        if (ObtenerCeldaActual() is null) return;

        var menu = new ContextMenu
        {
            PlacementTarget = destino ?? GrillaMensual,
            Placement = destino is null ? PlacementMode.MousePoint : PlacementMode.Bottom,
        };
        AgregarOpcion(menu, "Presente (P)", EstadoAsistencia.Presente);
        AgregarOpcion(menu, "Falta (F)", EstadoAsistencia.Falta);
        AgregarOpcion(menu, "Retardo (R)", EstadoAsistencia.Retardo);
        AgregarOpcion(menu, "Justificada (J)", EstadoAsistencia.Justificada);
        menu.IsOpen = true;
    }

    private void AgregarOpcion(ContextMenu menu, string texto, EstadoAsistencia estado)
    {
        var opcion = new MenuItem { Header = texto };
        opcion.Click += (_, _) =>
        {
            if (AsignarEstadoActual(estado)) AvanzarFila();
        };
        menu.Items.Add(opcion);
    }

    private bool AsignarEstadoActual(EstadoAsistencia estado) =>
        ObtenerCeldaActual() is { } seleccion
        && ViewModel is { } vm
        && vm.Mensual.AsignarEstado(seleccion.Fila, seleccion.Fecha, estado);

    private (AsistenciaEstudianteMesVisual Fila, DateOnly Fecha)? ObtenerCeldaActual()
    {
        if (ViewModel is not { } vm) return null;

        var indiceDia = GrillaMensual.CurrentColumn?.DisplayIndex - 2;
        if (indiceDia is null || indiceDia < 0 || indiceDia >= vm.Mensual.Dias.Count
            || GrillaMensual.CurrentItem is not AsistenciaEstudianteMesVisual fila)
        {
            return null;
        }

        return (fila, vm.Mensual.Dias[indiceDia.Value].Fecha);
    }

    private void AvanzarFila()
    {
        var indice = GrillaMensual.Items.IndexOf(GrillaMensual.CurrentItem);
        if (indice < 0 || indice + 1 >= GrillaMensual.Items.Count) return;

        GrillaMensual.CurrentCell = new DataGridCellInfo(
            GrillaMensual.Items[indice + 1], GrillaMensual.CurrentColumn);
        GrillaMensual.ScrollIntoView(GrillaMensual.Items[indice + 1]);
    }

    private void MoverAExtremoDeDias(bool final)
    {
        if (ViewModel is not { } vm || GrillaMensual.CurrentItem is null || vm.Mensual.Dias.Count == 0) return;

        var indiceColumna = final ? vm.Mensual.Dias.Count + 1 : 2;
        var columna = GrillaMensual.Columns[indiceColumna];
        GrillaMensual.CurrentCell = new DataGridCellInfo(GrillaMensual.CurrentItem, columna);
        GrillaMensual.ScrollIntoView(GrillaMensual.CurrentItem, columna);
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
