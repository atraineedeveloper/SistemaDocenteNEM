using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

using SistemaDocente.Core;
using SistemaDocente.Presentation;

namespace SistemaDocente.App.Wpf.Views
{
    /// <summary>
    /// Presentación del módulo Asistencia: vista diaria y mensual, atajos
    /// P/F/R/J (sólo con foco en la grilla), Ctrl+S y navegación contextual.
    /// El DataContext es <see cref="MainWindowViewModel"/>.
    /// </summary>
    public partial class AsistenciaView : UserControl
    {
        public AsistenciaView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            DataContextChanged += OnDataContextChanged;
        }

        private MainWindowViewModel? ViewModel => DataContext as MainWindowViewModel;

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is MainWindowViewModel anterior)
            {
                anterior.AsistenciaMensual.PropertyChanged -= OnAsistenciaMensualPropertyChanged;
            }

            if (DataContext is MainWindowViewModel vm)
            {
                vm.AsistenciaMensual.PropertyChanged += OnAsistenciaMensualPropertyChanged;
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e) => CrearColumnasMensuales();

        private void OnAsistenciaMensualPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GestionAsistenciaMensualViewModel.Dias))
            {
                Dispatcher.BeginInvoke(CrearColumnasMensuales);
            }
        }

        // Ctrl+S y PageUp/PageDown sólo aplican a la vista mensual.
        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (ViewModel is not { } vm || !vm.MostrarAsistenciaMensual) return;

            if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (vm.AsistenciaMensual.GuardarMesCommand.CanExecute(null))
                {
                    vm.AsistenciaMensual.GuardarMesCommand.Execute(null);
                }
                e.Handled = true;
                return;
            }

            if (e.Key is Key.PageUp or Key.PageDown)
            {
                var command = e.Key == Key.PageUp
                    ? vm.AsistenciaMensual.MesAnteriorCommand
                    : vm.AsistenciaMensual.MesSiguienteCommand;
                if (command.CanExecute(null)) command.Execute(null);
                e.Handled = true;
            }
        }

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

            for (var indice = 0; indice < vm.AsistenciaMensual.Dias.Count; indice++)
            {
                var dia = vm.AsistenciaMensual.Dias[indice];
                GrillaMensual.Columns.Add(new DataGridTextColumn
                {
                    Header = new TextBlock { Text = $"{dia.NumeroDia}\n{dia.AbreviaturaDiaSemana}", ToolTip = $"Dia {dia.NumeroDia} - {dia.Fecha:dd/MM/yyyy}" },
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
                Header = new TextBlock { Text = encabezado, ToolTip = encabezado switch { "P" => "Total presentes", "F" => "Total faltas", "R" => "Total retardos", "J" => "Total justificadas", "%" => "Porcentaje asistencia", _ => encabezado } },
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
            if (ObtenerCeldaActual() is { } seleccion && ViewModel is { } vm)
            {
                vm.AsistenciaMensual.SeleccionarCelda(seleccion.Fila, seleccion.Fecha);
            }
        }

        private void OnCeldaMensualDobleClic(object sender, MouseButtonEventArgs e)
        {
            if (ObtenerCeldaActual() is not null) MostrarSelectorCompacto();
        }

        // P/F/R/J sólo funcionan con foco en la grilla de asistencia.
        private void OnGrillaMensualPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.FocusedElement is TextBoxBase) return;

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

        private bool AsignarEstadoActual(EstadoAsistencia estado) =>
            ObtenerCeldaActual() is { } seleccion
            && ViewModel is { } vm
            && vm.AsistenciaMensual.AsignarEstado(seleccion.Fila, seleccion.Fecha, estado);

        private (AsistenciaEstudianteMesVisual Fila, DateOnly Fecha)? ObtenerCeldaActual()
        {
            if (ViewModel is not { } vm) return null;
            var indiceDia = GrillaMensual.CurrentColumn?.DisplayIndex - 2;
            if (indiceDia is null || indiceDia < 0 || indiceDia >= vm.AsistenciaMensual.Dias.Count
                || GrillaMensual.CurrentItem is not AsistenciaEstudianteMesVisual fila)
            {
                return null;
            }

            return (fila, vm.AsistenciaMensual.Dias[indiceDia.Value].Fecha);
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
            if (ViewModel is not { } vm || GrillaMensual.CurrentItem is null || vm.AsistenciaMensual.Dias.Count == 0) return;
            var indiceColumna = final ? vm.AsistenciaMensual.Dias.Count + 1 : 2;
            var columna = GrillaMensual.Columns[indiceColumna];
            GrillaMensual.CurrentCell = new DataGridCellInfo(GrillaMensual.CurrentItem, columna);
            GrillaMensual.ScrollIntoView(GrillaMensual.CurrentItem, columna);
        }
    }
}