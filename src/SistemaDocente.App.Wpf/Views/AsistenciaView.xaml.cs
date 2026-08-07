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
                },
                Binding = new Binding($"Celdas[{indice}].Texto"),
                Width = 43,
                ElementStyle = CrearEstiloCelda(indice),
                CellStyle = CrearEstiloContenedor(dia.EsCierreSemana),
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

    private static Style CrearEstiloCelda(int indice)
    {
        var estilo = new Style(typeof(TextBlock));
        estilo.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Center));
        estilo.Setters.Add(new Setter(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center));
        estilo.Setters.Add(new Setter(TextBlock.FontWeightProperty, FontWeights.SemiBold));
        AgregarColor(estilo, indice, "P", "SuccessBackgroundBrush", "SuccessBrush");
        AgregarColor(estilo, indice, "F", "ErrorBackgroundBrush", "ErrorBrush");
        AgregarColor(estilo, indice, "R", "WarningBackgroundBrush", "WarningBrush");
        AgregarColor(estilo, indice, "J", "InfoBackgroundBrush", "InfoBrush");
        return estilo;
    }

    private static Style CrearEstiloContenedor(bool esCierreSemana)
    {
        var estilo = new Style(typeof(DataGridCell));
        if (esCierreSemana)
        {
            estilo.Setters.Add(new Setter(Control.BorderBrushProperty, ObtenerBrush("BorderDefaultBrush")));
            estilo.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(0, 0, 3, 0)));
        }

        return estilo;
    }

    private static Style CrearEstiloEncabezado(bool esCierreSemana)
    {
        var estilo = new Style(typeof(DataGridColumnHeader));
        estilo.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Center));
        if (esCierreSemana)
        {
            estilo.Setters.Add(new Setter(Control.BorderBrushProperty, ObtenerBrush("BorderDefaultBrush")));
            estilo.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(0, 0, 3, 0)));
        }

        return estilo;
    }

    private static void AgregarColor(Style estilo, int indice, string texto, string fondoKey, string frenteKey)
    {
        var disparador = new DataTrigger
        {
            Binding = new Binding($"Celdas[{indice}].Texto"),
            Value = texto,
        };
        disparador.Setters.Add(new Setter(TextBlock.BackgroundProperty, ObtenerBrush(fondoKey)));
        disparador.Setters.Add(new Setter(TextBlock.ForegroundProperty, ObtenerBrush(frenteKey)));
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

    private void MostrarSelectorCompacto()
    {
        if (ObtenerCeldaActual() is null) return;

        var menu = new ContextMenu { PlacementTarget = GrillaMensual };
        AgregarOpcion(menu, "Presente (P)", EstadoAsistencia.Presente);
        AgregarOpcion(menu, "Falta (F)", EstadoAsistencia.Falta);
        AgregarOpcion(menu, "Retardo (R)", EstadoAsistencia.Retardo);
        AgregarOpcion(menu, "Falta justificada (J)", EstadoAsistencia.Justificada);
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
}