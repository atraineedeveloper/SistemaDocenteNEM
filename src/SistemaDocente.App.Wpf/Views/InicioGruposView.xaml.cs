using System.IO;
using System.Windows;
using System.Windows.Controls;

using SistemaDocente.Application;
using SistemaDocente.Core;
using SistemaDocente.Presentation;

namespace SistemaDocente.App.Wpf.Views;

public partial class InicioGruposView : UserControl
{
    public InicioGruposView()
    {
        InitializeComponent();
    }

    private void OnVistaCargada(object sender, RoutedEventArgs e)
    {
        RefrescarArchivados();
    }

    private void OnAbrirGrupoClic(object sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel
            || ObtenerGrupo(sender) is not { } grupo)
        {
            return;
        }

        viewModel.CambiarGrupo(grupo.GrupoId);
    }

    private void OnArchivarGrupoClic(object sender, RoutedEventArgs e)
    {
        if (ObtenerGrupo(sender) is not { } grupo)
        {
            return;
        }

        var resultado = MostrarConfirmacion(
            $"¿Archivar “{grupo.NombreVisible}”?\n\nEl grupo dejará de aparecer entre los grupos activos, pero no se borrará ninguna información. Podrás restaurarlo cuando quieras.",
            "Archivar grupo",
            MessageBoxImage.Question);
        if (resultado != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            CicloVidaGruposWpf.Archivar(grupo.GrupoId);
            RefrescarDespuesDeCambio();
        }
        catch (Exception exception) when (EsErrorAdministracion(exception))
        {
            MostrarError("No fue posible archivar el grupo. No se eliminó información.");
        }
    }

    private void OnRestaurarGrupoClic(object sender, RoutedEventArgs e)
    {
        if (ObtenerGrupo(sender) is not { } grupo)
        {
            return;
        }

        try
        {
            CicloVidaGruposWpf.Restaurar(grupo.GrupoId);
            RefrescarDespuesDeCambio();
        }
        catch (Exception exception) when (EsErrorAdministracion(exception))
        {
            MostrarError("No fue posible restaurar el grupo. Intenta nuevamente.");
        }
    }

    private void OnEliminarGrupoClic(object sender, RoutedEventArgs e)
    {
        if (ObtenerGrupo(sender) is not { } grupo)
        {
            return;
        }

        ResumenEliminacionGrupo resumen;
        try
        {
            resumen = CicloVidaGruposWpf.ObtenerResumenEliminacion(grupo.GrupoId);
        }
        catch (Exception exception) when (EsErrorAdministracion(exception))
        {
            MostrarError("No fue posible revisar el contenido del grupo. No se eliminó nada.");
            return;
        }

        if (resumen.TieneDatos)
        {
            var dialogo = new ConfirmarEliminacionGrupoWindow(grupo, resumen);
            var owner = Window.GetWindow(this);
            if (owner is not null)
            {
                dialogo.Owner = owner;
            }

            if (dialogo.ShowDialog() != true || !dialogo.Confirmado)
            {
                return;
            }
        }
        else
        {
            var resultado = MostrarConfirmacion(
                $"¿Eliminar permanentemente “{grupo.NombreVisible}”?\n\nEl grupo no contiene estudiantes, asistencia, proyectos ni configuración significativa. Esta acción no se puede deshacer desde la aplicación.",
                "Eliminar grupo vacío",
                MessageBoxImage.Warning);
            if (resultado != MessageBoxResult.Yes)
            {
                return;
            }
        }

        try
        {
            CicloVidaGruposWpf.Eliminar(grupo.GrupoId);
            RefrescarDespuesDeCambio();
        }
        catch (RecuperacionLocalException)
        {
            MostrarError(
                "No se eliminó el grupo porque AulaRaíz no pudo crear primero el respaldo de seguridad. Revisa el almacenamiento disponible e intenta nuevamente.");
        }
        catch (Exception exception) when (EsErrorAdministracion(exception))
        {
            MostrarError("No fue posible eliminar el grupo de forma completa. No se confirmó una eliminación parcial.");
        }
    }

    private void RefrescarDespuesDeCambio()
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.Grupo.Inicializar();
        }

        RefrescarArchivados();
    }

    private void RefrescarArchivados()
    {
        try
        {
            var archivados = CicloVidaGruposWpf.ListarArchivados();
            ArchivadosItemsControl.ItemsSource = archivados;
            ArchivadosCantidadTextBlock.Text = $"({archivados.Count})";
            ArchivadosPanel.Visibility = archivados.Count > 0
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
        catch (InvalidOperationException)
        {
            ArchivadosItemsControl.ItemsSource = null;
            ArchivadosPanel.Visibility = Visibility.Collapsed;
        }
    }

    private static GrupoDetalle? ObtenerGrupo(object sender) =>
        sender is FrameworkElement { Tag: GrupoDetalle grupo } ? grupo : null;

    private MessageBoxResult MostrarConfirmacion(
        string mensaje,
        string titulo,
        MessageBoxImage icono)
    {
        var owner = Window.GetWindow(this);
        return owner is null
            ? MessageBox.Show(mensaje, titulo, MessageBoxButton.YesNo, icono, MessageBoxResult.No)
            : MessageBox.Show(owner, mensaje, titulo, MessageBoxButton.YesNo, icono, MessageBoxResult.No);
    }

    private void MostrarError(string mensaje)
    {
        var owner = Window.GetWindow(this);
        if (owner is null)
        {
            MessageBox.Show(mensaje, "AulaRaíz", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        else
        {
            MessageBox.Show(owner, mensaje, "AulaRaíz", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static bool EsErrorAdministracion(Exception exception) =>
        exception is ErrorPersistenciaAplicacionException
            or GrupoNoEncontradoException
            or GrupoArchivadoException
            or DomainValidationException
            or DomainConflictException
            or IOException
            or UnauthorizedAccessException;
}
