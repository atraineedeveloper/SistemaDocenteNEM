using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using SistemaDocente.Presentation;

namespace SistemaDocente.App.Wpf.Views
{
    /// <summary>
    /// Presentación del módulo Grupo: bienvenida/creación, lista de estudiantes,
    /// búsqueda, editor de nombre y apertura de ventanas dedicadas de estudiante
    /// y expediente. El DataContext es <see cref="GestionGrupoViewModel"/>.
    /// </summary>
    public partial class GrupoView : UserControl
    {
        public GrupoView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            DataContextChanged += OnDataContextChanged;
        }

        private GestionGrupoViewModel? ViewModel => DataContext as GestionGrupoViewModel;

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is GestionGrupoViewModel anterior)
            {
                anterior.PropertyChanged -= OnGrupoPropertyChanged;
            }

            if (DataContext is GestionGrupoViewModel vm)
            {
                vm.PropertyChanged += OnGrupoPropertyChanged;
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            AsignarFocoInicial();
            AbrirEditorEstudianteVentana();
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

        private void AbrirExpedienteEstudiante()
        {
            if (ViewModel is not { } grupo
                || grupo.GrupoIdActual is not { } grupoId
                || grupo.EstudianteSeleccionado is not { } estudiante)
            {
                return;
            }

            // El expediente vive en el MainWindowViewModel; se resuelve desde la ventana propietaria.
            if (Window.GetWindow(this) is MainWindow { ViewModel.Expediente: { } expediente } ventana)
            {
                expediente.Cargar(grupoId, estudiante.Id);
                var ventanaExpediente = new ExpedienteEstudianteWindow(expediente) { Owner = ventana };
                ventanaExpediente.ShowDialog();
            }
        }

        private void AbrirEditorEstudianteVentana()
        {
            if (ViewModel is not { } grupo
                || grupo.PanelActual is not (PanelEdicion.AgregarEstudiante or PanelEdicion.EditarEstudiante))
            {
                return;
            }

            var ventana = new EditorEstudianteWindow(grupo) { Owner = Window.GetWindow(this) };
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
}