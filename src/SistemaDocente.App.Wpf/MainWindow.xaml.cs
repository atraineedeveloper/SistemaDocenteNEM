using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

using SistemaDocente.Core;
using SistemaDocente.Presentation;

namespace SistemaDocente.App.Wpf;

public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        InitializeComponent();
        DataContext = viewModel;
        viewModel.Grupo.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName is nameof(GestionGrupoViewModel.PanelActual)
                or nameof(GestionGrupoViewModel.MostrarBienvenida))
            {
                Dispatcher.BeginInvoke(AsignarFocoInicial);
            }
        };
        viewModel.AsistenciaMensual.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(GestionAsistenciaMensualViewModel.Dias))
            {
                Dispatcher.BeginInvoke(CrearColumnasMensuales);
            }
        };
        Loaded += (_, _) =>
        {
            AsignarFocoInicial();
            CrearColumnasMensuales();
        };
    }

    private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext;

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (ViewModel.MostrarAsistenciaMensual && e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
        {
            if (ViewModel.AsistenciaMensual.GuardarMesCommand.CanExecute(null))
            {
                ViewModel.AsistenciaMensual.GuardarMesCommand.Execute(null);
            }

            e.Handled = true;
            return;
        }

        if (ViewModel.MostrarAsistenciaMensual && e.Key is Key.PageUp or Key.PageDown)
        {
            var command = e.Key == Key.PageUp
                ? ViewModel.AsistenciaMensual.MesAnteriorCommand
                : ViewModel.AsistenciaMensual.MesSiguienteCommand;
            if (command.CanExecute(null)) command.Execute(null);
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Escape && ViewModel.Grupo.CancelarEdicionCommand.CanExecute(null))
        {
            ViewModel.Grupo.CancelarEdicionCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void CrearColumnasMensuales()
    {
        GrillaMensual.Columns.Clear();
        GrillaMensual.Columns.Add(new DataGridTextColumn
        {
            Header = "Núm.",
            Binding = new Binding(nameof(AsistenciaEstudianteMesVisual.NumeroLista)),
            Width = 62,
        });
        GrillaMensual.Columns.Add(new DataGridTextColumn
        {
            Header = "Nombre",
            Binding = new Binding(nameof(AsistenciaEstudianteMesVisual.Nombre)),
            Width = 190,
        });

        for (var indice = 0; indice < ViewModel.AsistenciaMensual.Dias.Count; indice++)
        {
            var dia = ViewModel.AsistenciaMensual.Dias[indice];
            GrillaMensual.Columns.Add(new DataGridTextColumn
            {
                Header = $"{dia.NumeroDia}\n{dia.AbreviaturaDiaSemana}",
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
            Header = encabezado,
            Binding = new Binding(propiedad),
            Width = ancho,
        });

    private static Style CrearEstiloCelda(int indice)
    {
        var estilo = new Style(typeof(TextBlock));
        estilo.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Center));
        estilo.Setters.Add(new Setter(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center));
        estilo.Setters.Add(new Setter(TextBlock.FontWeightProperty, FontWeights.SemiBold));
        AgregarColor(estilo, indice, "P", "#ECFDF3", "#027A48");
        AgregarColor(estilo, indice, "F", "#FEF3F2", "#B42318");
        AgregarColor(estilo, indice, "R", "#FFFAEB", "#B54708");
        AgregarColor(estilo, indice, "J", "#EFF8FF", "#175CD3");
        return estilo;
    }

    private static Style CrearEstiloContenedor(bool esCierreSemana)
    {
        var estilo = new Style(typeof(DataGridCell));
        if (esCierreSemana)
        {
            estilo.Setters.Add(new Setter(Control.BorderBrushProperty, System.Windows.Media.Brushes.SlateGray));
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
            estilo.Setters.Add(new Setter(Control.BorderBrushProperty, System.Windows.Media.Brushes.SlateGray));
            estilo.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(0, 0, 3, 0)));
        }

        return estilo;
    }

    private static void AgregarColor(Style estilo, int indice, string texto, string fondo, string frente)
    {
        var disparador = new DataTrigger
        {
            Binding = new Binding($"Celdas[{indice}].Texto"),
            Value = texto,
        };
        disparador.Setters.Add(new Setter(TextBlock.BackgroundProperty,
            new System.Windows.Media.BrushConverter().ConvertFromString(fondo)));
        disparador.Setters.Add(new Setter(TextBlock.ForegroundProperty,
            new System.Windows.Media.BrushConverter().ConvertFromString(frente)));
        estilo.Triggers.Add(disparador);
    }

    private void OnCeldaMensualSeleccionada(object? sender, EventArgs e)
    {
        if (ObtenerCeldaActual() is { } seleccion)
        {
            ViewModel.AsistenciaMensual.SeleccionarCelda(seleccion.Fila, seleccion.Fecha);
        }
    }

    private void OnCeldaMensualDobleClic(object sender, MouseButtonEventArgs e)
    {
        if (ObtenerCeldaActual() is not null) MostrarSelectorCompacto();
    }

    private void OnGrillaMensualPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key is Key.P or Key.F or Key.R or Key.J)
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
        }
        else if (e.Key == Key.Enter)
        {
            MostrarSelectorCompacto();
            e.Handled = true;
        }
        else if (e.Key == Key.Home || e.Key == Key.End)
        {
            MoverAExtremoDeDias(e.Key == Key.End);
            e.Handled = true;
        }
    }

    private void OnVerExpedienteEstudianteClic(object sender, RoutedEventArgs e)
    {
        if (ViewModel.Grupo.GrupoIdActual is not { } grupoId || ViewModel.Grupo.EstudianteSeleccionado is not { } estudiante)
        {
            return;
        }

        if (ViewModel.Expediente is not null)
        {
            ViewModel.Expediente.Cargar(grupoId, estudiante.Id);
            var ventanaExpediente = new ExpedienteEstudianteWindow(ViewModel.Expediente) { Owner = this };
            ventanaExpediente.ShowDialog();
        }
    }

    private void OnProyectoPrincipalDobleClic(object sender, MouseButtonEventArgs e)
    {
        AbrirDetalleProyecto();
    }

    private void OnAbrirDetalleProyectoClic(object sender, RoutedEventArgs e)
    {
        AbrirDetalleProyecto();
    }

    private void AbrirDetalleProyecto()
    {
        if (ViewModel.Proyectos?.ProyectoSeleccionado is null && !ViewModel.Proyectos?.TieneCambiosProyecto == true) return;
        if (ViewModel.Proyectos is not null)
        {
            var ventanaProyecto = new DetalleProyectoWindow(ViewModel.Proyectos) { Owner = this };
            ventanaProyecto.ShowDialog();
        }
    }

    private void OnGrillaEntregasEvaluacionPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key is not (Key.D or Key.S or Key.E or Key.R or Key.N or Key.P)
            || Keyboard.Modifiers != ModifierKeys.None
            || Keyboard.FocusedElement is TextBoxBase
            || Keyboard.FocusedElement is not DependencyObject foco
            || !GrillaEntregasEvaluacion.IsAncestorOf(foco))
        {
            return;
        }

        var command = e.Key switch
        {
            Key.D => ViewModel.Evaluacion?.MarcarDominaCommand,
            Key.S => ViewModel.Evaluacion?.MarcarSuficienteCommand,
            Key.E => ViewModel.Evaluacion?.MarcarEnProcesoCommand,
            Key.R => ViewModel.Evaluacion?.MarcarRequiereApoyoCommand,
            Key.N => ViewModel.Evaluacion?.MarcarNoEntregoCommand,
            _ => ViewModel.Evaluacion?.MarcarPendienteCommand,
        };
        if (command?.CanExecute(null) == true)
        {
            command.Execute(null);
            e.Handled = true;
        }
    }

    private void MostrarSelectorCompacto()
    {
        if (ObtenerCeldaActual() is null) return;
        var menu = new ContextMenu();
        AgregarOpcion(menu, "Presente (P)", EstadoAsistencia.Presente);
        AgregarOpcion(menu, "Falta (F)", EstadoAsistencia.Falta);
        AgregarOpcion(menu, "Retardo (R)", EstadoAsistencia.Retardo);
        AgregarOpcion(menu, "Falta justificada (J)", EstadoAsistencia.Justificada);
        menu.PlacementTarget = GrillaMensual;
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

    private bool AsignarEstadoActual(EstadoAsistencia estado) => ObtenerCeldaActual() is { } seleccion
        && ViewModel.AsistenciaMensual.AsignarEstado(seleccion.Fila, seleccion.Fecha, estado);

    private (AsistenciaEstudianteMesVisual Fila, DateOnly Fecha)? ObtenerCeldaActual()
    {
        var indiceDia = GrillaMensual.CurrentColumn?.DisplayIndex - 2;
        if (indiceDia is null || indiceDia < 0 || indiceDia >= ViewModel.AsistenciaMensual.Dias.Count
            || GrillaMensual.CurrentItem is not AsistenciaEstudianteMesVisual fila)
        {
            return null;
        }

        return (fila, ViewModel.AsistenciaMensual.Dias[indiceDia.Value].Fecha);
    }

    private void AvanzarFila()
    {
        var indice = GrillaMensual.Items.IndexOf(GrillaMensual.CurrentItem);
        if (indice < 0 || indice + 1 >= GrillaMensual.Items.Count) return;
        GrillaMensual.CurrentCell = new DataGridCellInfo(GrillaMensual.Items[indice + 1], GrillaMensual.CurrentColumn);
        GrillaMensual.ScrollIntoView(GrillaMensual.Items[indice + 1]);
    }

    private void MoverAExtremoDeDias(bool final)
    {
        if (GrillaMensual.CurrentItem is null || ViewModel.AsistenciaMensual.Dias.Count == 0) return;
        var indiceColumna = final ? ViewModel.AsistenciaMensual.Dias.Count + 1 : 2;
        var columna = GrillaMensual.Columns[indiceColumna];
        GrillaMensual.CurrentCell = new DataGridCellInfo(GrillaMensual.CurrentItem, columna);
        GrillaMensual.ScrollIntoView(GrillaMensual.CurrentItem, columna);
    }

    private void AsignarFocoInicial()
    {
        if (ViewModel.Grupo.MostrarBienvenida) NombreGrupoInicial.Focus();
        else if (ViewModel.Grupo.MostrarEditorGrupo) NombreGrupoEdicion.Focus();
        else if (ViewModel.Grupo.MostrarEditorEstudiante) PrimerApellidoEdicion.Focus();
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        if (!ViewModel.SolicitarCerrar())
        {
            e.Cancel = true;
        }
    }

    // ══ Toast de confirmación ══════════════════════════════

    private System.Windows.Threading.DispatcherTimer? _toastTimer;

    /// <summary>Muestra un toast flotante con auto-dismiss después de <paramref name="segundos"/> segundos.</summary>
    public void MostrarToast(string icono, string titulo, string mensaje,
        System.Windows.Media.Brush fondo, System.Windows.Media.Brush borde,
        System.Windows.Media.Brush colorTexto, int segundos = 3)
    {
        ToastIcon.Text = icono;
        ToastTitle.Text = titulo;
        ToastTitle.Foreground = colorTexto;
        ToastMessage.Text = mensaje;
        ToastMessage.Foreground = colorTexto;
        ToastBanner.Background = fondo;
        ToastBanner.BorderBrush = borde;
        ToastBanner.Visibility = Visibility.Visible;

        _toastTimer?.Stop();
        _toastTimer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(segundos)
        };
        _toastTimer.Tick += (_, _) =>
        {
            ToastBanner.Visibility = Visibility.Collapsed;
            _toastTimer.Stop();
        };
        _toastTimer.Start();
    }

    /// <summary>Toast de éxito verde estándar.</summary>
    public void MostrarToastExito(string mensaje, string titulo = "✅ Guardado exitosamente") =>
        MostrarToast("✅", titulo, mensaje,
            new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#ECFDF3")),
            new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#ABEFC6")),
            new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#027A48")));

    /// <summary>Toast de advertencia naranja estándar.</summary>
    public void MostrarToastAdvertencia(string mensaje, string titulo = "⚠️ Advertencia") =>
        MostrarToast("⚠️", titulo, mensaje,
            new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFAEB")),
            new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FEF0C7")),
            new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#B54708")));
}

// ══ Converter: bool → "activo" / "" para el indicador de pestaña ══════

/// <summary>Convierte un bool en el string "activo" o "", usado por NavTabButton para mostrar el indicador inferior.</summary>
[System.Windows.Data.ValueConversion(typeof(bool), typeof(string))]
public sealed class BoolToActiveTagConverter : System.Windows.Data.IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        => value is true ? "activo" : string.Empty;

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        => throw new NotImplementedException();
}
