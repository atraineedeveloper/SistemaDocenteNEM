using System.IO;
using System.Windows;

using Microsoft.Win32;

using SistemaDocente.Application;
using SistemaDocente.Presentation;

namespace SistemaDocente.App.Wpf;

public partial class RecuperacionLocalWindow : Window
{
    public RecuperacionLocalWindow(RecuperacionLocalViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        InitializeComponent();
        DataContext = viewModel;
    }

    private RecuperacionLocalViewModel ViewModel => (RecuperacionLocalViewModel)DataContext;

    private void OnCrearRespaldoClic(object sender, RoutedEventArgs e)
    {
        var proteger = ProtectBackupCheckBox.IsChecked == true;
        if (proteger && !ValidarContrasenaCreacion()) return;

        char[]? contrasena = null;
        try
        {
            if (proteger)
            {
                var confirmacion = MessageBox.Show(
                    this,
                    "AulaRaíz no guarda esta contraseña. Si la olvidas, no será posible recuperar el respaldo protegido.\n\n¿Deseas continuar?",
                    "Proteger respaldo con contraseña",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning,
                    MessageBoxResult.No);
                if (confirmacion != MessageBoxResult.Yes) return;
                contrasena = CreatePasswordBox.Password.ToCharArray();
            }

            var dialogo = new SaveFileDialog
            {
                Title = $"Crear respaldo de {IdentidadProducto.Nombre}",
                Filter = $"Respaldo de {IdentidadProducto.Nombre} (*.sdocbackup)|*.sdocbackup",
                DefaultExt = ".sdocbackup",
                AddExtension = true,
                FileName = ViewModel.CrearNombreArchivoSugerido(DateTimeOffset.Now),
                OverwritePrompt = true,
            };
            if (dialogo.ShowDialog(this) != true) return;

            var resultado = proteger
                ? ViewModel.CrearRespaldoProtegido(
                    dialogo.FileName,
                    DateTimeOffset.UtcNow,
                    ObtenerVersionAplicacion(),
                    contrasena!)
                : ViewModel.CrearRespaldo(
                    dialogo.FileName,
                    DateTimeOffset.UtcNow,
                    ObtenerVersionAplicacion());
            var detalle = resultado.Advertencias.Count == 0
                ? proteger
                    ? "El respaldo protegido v2 se creó correctamente."
                    : "El respaldo estándar v1 se creó correctamente."
                : "El respaldo de la base se creó, pero contiene advertencias que conviene revisar en la ventana.";
            MessageBox.Show(
                this,
                detalle,
                "Respaldo creado",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception exception) when (EsErrorOperacional(exception))
        {
            MostrarError(exception, "No fue posible crear el respaldo");
        }
        finally
        {
            if (contrasena is not null) Array.Clear(contrasena, 0, contrasena.Length);
            CreatePasswordBox.Clear();
            ConfirmCreatePasswordBox.Clear();
        }
    }

    private void OnSeleccionarRespaldoClic(object sender, RoutedEventArgs e)
    {
        var dialogo = new OpenFileDialog
        {
            Title = $"Seleccionar respaldo de {IdentidadProducto.Nombre}",
            Filter = $"Respaldo de {IdentidadProducto.Nombre} (*.sdocbackup)|*.sdocbackup|Todos los archivos (*.*)|*.*",
            CheckFileExists = true,
            Multiselect = false,
        };
        if (dialogo.ShowDialog(this) != true) return;

        RestorePasswordBox.Clear();
        try
        {
            _ = ViewModel.SeleccionarRespaldo(dialogo.FileName);
        }
        catch (Exception exception) when (EsErrorOperacional(exception))
        {
            ViewModel.LimpiarInspeccion();
            MostrarError(exception, "El respaldo no puede restaurarse");
        }
    }

    private void OnDesbloquearRespaldoClic(object sender, RoutedEventArgs e)
    {
        if (RestorePasswordBox.Password.Length < GestionRespaldoCasosUso.LongitudMinimaContrasena)
        {
            MessageBox.Show(
                this,
                $"Escribe la contraseña del respaldo (al menos {GestionRespaldoCasosUso.LongitudMinimaContrasena} caracteres).",
                "Contraseña requerida",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            RestorePasswordBox.Focus();
            return;
        }

        var contrasena = RestorePasswordBox.Password.ToCharArray();
        try
        {
            _ = ViewModel.InspeccionarProtegido(contrasena);
        }
        catch (Exception exception) when (EsErrorOperacional(exception))
        {
            RestorePasswordBox.Clear();
            MostrarError(exception, "No fue posible abrir el respaldo protegido");
        }
        finally
        {
            Array.Clear(contrasena, 0, contrasena.Length);
        }
    }

    private void OnRestaurarClic(object sender, RoutedEventArgs e)
    {
        char[]? contrasena = null;
        try
        {
            if (ViewModel.InspeccionProtegida)
            {
                if (RestorePasswordBox.Password.Length < GestionRespaldoCasosUso.LongitudMinimaContrasena)
                {
                    throw new InvalidOperationException(
                        "Escribe nuevamente la contraseña del respaldo protegido para restaurarlo.");
                }
                contrasena = RestorePasswordBox.Password.ToCharArray();
            }

            var resultado = ViewModel.Restaurar(
                DateTimeOffset.UtcNow,
                ObtenerVersionAplicacion(),
                contrasena);
            RestorePasswordBox.Clear();
            MessageBox.Show(
                this,
                $"La restauración terminó correctamente.\n\nSe conservó un respaldo de seguridad del estado anterior en:\n{resultado.RutaRespaldoSeguridad}\n\nLa aplicación se cerrará ahora. Vuelve a abrirla para trabajar con los datos restaurados.",
                "Restauración completada",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            System.Windows.Application.Current.Shutdown();
        }
        catch (Exception exception) when (EsErrorOperacional(exception))
        {
            if (ViewModel.InspeccionProtegida) RestorePasswordBox.Clear();
            MostrarError(exception, "No fue posible completar la restauración");
            if (exception is RecuperacionLocalException
                {
                    Categoria: CategoriaErrorRecuperacionLocal.Publicacion,
                })
            {
                System.Windows.Application.Current.Shutdown();
            }
        }
        finally
        {
            if (contrasena is not null) Array.Clear(contrasena, 0, contrasena.Length);
        }
    }

    private bool ValidarContrasenaCreacion()
    {
        if (CreatePasswordBox.Password.Length < GestionRespaldoCasosUso.LongitudMinimaContrasena)
        {
            MessageBox.Show(
                this,
                $"La contraseña debe tener al menos {GestionRespaldoCasosUso.LongitudMinimaContrasena} caracteres. Puedes usar una frase con espacios.",
                "Contraseña demasiado corta",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            CreatePasswordBox.Focus();
            return false;
        }

        if (!string.Equals(
                CreatePasswordBox.Password,
                ConfirmCreatePasswordBox.Password,
                StringComparison.Ordinal))
        {
            MessageBox.Show(
                this,
                "La contraseña y su confirmación no coinciden.",
                "Confirma la contraseña",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            ConfirmCreatePasswordBox.Focus();
            return false;
        }

        return true;
    }

    private void OnCerrarClic(object sender, RoutedEventArgs e) => Close();

    protected override void OnClosed(EventArgs e)
    {
        CreatePasswordBox.Clear();
        ConfirmCreatePasswordBox.Clear();
        RestorePasswordBox.Clear();
        base.OnClosed(e);
    }

    private static bool EsErrorOperacional(Exception exception) =>
        exception is RecuperacionLocalException
            or IOException
            or UnauthorizedAccessException
            or InvalidOperationException;

    private void MostrarError(Exception exception, string titulo)
    {
        MessageBox.Show(
            this,
            exception.Message,
            titulo,
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }

    private static string ObtenerVersionAplicacion() =>
        typeof(App).Assembly.GetName().Version?.ToString() ?? "desconocida";
}
