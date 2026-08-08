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
        var dialogo = new SaveFileDialog
        {
            Title = "Crear respaldo de Sistema Docente",
            Filter = "Respaldo de Sistema Docente (*.sdocbackup)|*.sdocbackup",
            DefaultExt = ".sdocbackup",
            AddExtension = true,
            FileName = ViewModel.CrearNombreArchivoSugerido(DateTimeOffset.Now),
            OverwritePrompt = true,
        };
        if (dialogo.ShowDialog(this) != true) return;

        try
        {
            var resultado = ViewModel.CrearRespaldo(
                dialogo.FileName,
                DateTimeOffset.UtcNow,
                ObtenerVersionAplicacion());
            var detalle = resultado.Advertencias.Count == 0
                ? "El respaldo completo se creó correctamente."
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
    }

    private void OnSeleccionarRespaldoClic(object sender, RoutedEventArgs e)
    {
        var dialogo = new OpenFileDialog
        {
            Title = "Seleccionar respaldo de Sistema Docente",
            Filter = "Respaldo de Sistema Docente (*.sdocbackup)|*.sdocbackup|Todos los archivos (*.*)|*.*",
            CheckFileExists = true,
            Multiselect = false,
        };
        if (dialogo.ShowDialog(this) != true) return;

        try
        {
            ViewModel.Inspeccionar(dialogo.FileName);
        }
        catch (Exception exception) when (EsErrorOperacional(exception))
        {
            ViewModel.LimpiarInspeccion();
            MostrarError(exception, "El respaldo no puede restaurarse");
        }
    }

    private void OnRestaurarClic(object sender, RoutedEventArgs e)
    {
        try
        {
            var resultado = ViewModel.Restaurar(
                DateTimeOffset.UtcNow,
                ObtenerVersionAplicacion());
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
            MostrarError(exception, "No fue posible completar la restauración");
            if (exception is RecuperacionLocalException
                {
                    Categoria: CategoriaErrorRecuperacionLocal.Publicacion,
                })
            {
                System.Windows.Application.Current.Shutdown();
            }
        }
    }

    private void OnCerrarClic(object sender, RoutedEventArgs e) => Close();

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