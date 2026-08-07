using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

using SistemaDocente.Presentation;

namespace SistemaDocente.App.Wpf.Views
{
    /// <summary>
    /// Presentación del módulo Evaluación: selectores de proyecto/actividad,
    /// grilla de entregas y atajos D/S/E/R/N/P (sólo con foco en la grilla).
    /// El DataContext es <see cref="EvaluacionActividadesViewModel"/>.
    /// </summary>
    public partial class EvaluacionView : UserControl
    {
        public EvaluacionView()
        {
            InitializeComponent();
        }

        private EvaluacionActividadesViewModel? ViewModel => DataContext as EvaluacionActividadesViewModel;

        // Atajos simples sólo funcionan cuando el foco está dentro de la grilla
        // de evaluación y nunca mientras se escribe en controles de texto.
        private void OnGrillaEntregasEvaluacionPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (ViewModel is not { } vm) return;
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
                Key.D => vm.MarcarDominaCommand,
                Key.S => vm.MarcarSuficienteCommand,
                Key.E => vm.MarcarEnProcesoCommand,
                Key.R => vm.MarcarRequiereApoyoCommand,
                Key.N => vm.MarcarNoEntregoCommand,
                _ => vm.MarcarPendienteCommand,
            };
            if (command?.CanExecute(null) == true)
            {
                command.Execute(null);
                e.Handled = true;
            }
        }
    }
}