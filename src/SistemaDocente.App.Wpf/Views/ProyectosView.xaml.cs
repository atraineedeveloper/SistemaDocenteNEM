using System.Windows;
using System.Windows.Input;

using SistemaDocente.Presentation;

namespace SistemaDocente.App.Wpf.Views
{
    /// <summary>
    /// Vista principal de proyectos: lista amplia, filtros y apertura de la
    /// ventana dedicada <see cref="DetalleProyectoWindow"/>. El DataContext es
    /// <see cref="GestionProyectosViewModel"/>. No reintroduce master-detail.
    /// </summary>
    public partial class ProyectosView : System.Windows.Controls.UserControl
    {
        public ProyectosView()
        {
            InitializeComponent();
        }

        private GestionProyectosViewModel? ViewModel => DataContext as GestionProyectosViewModel;

        private void OnAbrirDetalleProyectoClic(object sender, RoutedEventArgs e) => AbrirDetalleProyecto();

        private void AbrirDetalleProyecto()
        {
            if (ViewModel is not { } vm) return;
            if (vm.ProyectoSeleccionado is null && !vm.TieneCambiosProyecto) return;

            var ventanaProyecto = new DetalleProyectoWindow(vm) { Owner = Window.GetWindow(this) };
            ventanaProyecto.ShowDialog();
        }
    }
}