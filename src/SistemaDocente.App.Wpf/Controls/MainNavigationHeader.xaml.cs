using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

using SistemaDocente.App.Wpf.Services;
using SistemaDocente.Presentation;

namespace SistemaDocente.App.Wpf.Controls
{
    /// <summary>
    /// Encabezado global del shell: branding, selector de grupo, navegación principal
    /// y selector de tema. No contiene lógica de módulos.
    /// </summary>
    public partial class MainNavigationHeader : UserControl
    {
        public MainNavigationHeader()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            DataContextChanged += OnDataContextChanged;
        }

        private MainWindowViewModel? ViewModel => DataContext as MainWindowViewModel;

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Suscribir();
            ActualizarPestañaActiva();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Limpia la suscripción anterior antes de enlazar el nuevo ViewModel.
            if (e.OldValue is MainWindowViewModel anterior)
            {
                anterior.PropertyChanged -= OnViewModelPropertyChanged;
            }

            Suscribir();
            ActualizarPestañaActiva();
        }

        private void Suscribir()
        {
            if (ViewModel is { } vm)
            {
                vm.PropertyChanged += OnViewModelPropertyChanged;
            }
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(MainWindowViewModel.MostrarGrupo)
                or nameof(MainWindowViewModel.MostrarAsistenciaDiaria)
                or nameof(MainWindowViewModel.MostrarAsistenciaMensual)
                or nameof(MainWindowViewModel.MostrarProyectos)
                or nameof(MainWindowViewModel.MostrarEvaluacion))
            {
                ActualizarPestañaActiva();
            }
        }

        private void ActualizarPestañaActiva()
        {
            if (ViewModel is not { } vm) return;
            NavBtnGrupo.Tag = vm.MostrarGrupo ? "activo" : "";
            NavBtnAsistencia.Tag = (vm.MostrarAsistenciaDiaria || vm.MostrarAsistenciaMensual) ? "activo" : "";
            NavBtnProyectos.Tag = vm.MostrarProyectos ? "activo" : "";
            NavBtnEvaluacion.Tag = vm.MostrarEvaluacion ? "activo" : "";
        }

        /// <summary>Cambia al tema claro.</summary>
        private void TemaClaro_Click(object sender, RoutedEventArgs e)
            => ThemeService.ApplyTheme(ThemeService.Light);

        /// <summary>Cambia al tema oscuro.</summary>
        private void TemaOscuro_Click(object sender, RoutedEventArgs e)
            => ThemeService.ApplyTheme(ThemeService.Dark);

        /// <summary>Cambia al tema de alto contraste.</summary>
        private void TemaAltoContraste_Click(object sender, RoutedEventArgs e)
            => ThemeService.ApplyTheme(ThemeService.HighContrast);
    }
}