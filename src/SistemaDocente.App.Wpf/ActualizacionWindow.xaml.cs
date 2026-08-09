using System.Diagnostics;
using System.IO;
using System.Windows;

using SistemaDocente.Application;

namespace SistemaDocente.App.Wpf;

public partial class ActualizacionWindow : Window
{
    private readonly IServicioActualizacionesAplicacion _servicio;
    private readonly ActualizacionDisponible _actualizacion;
    private readonly MainWindow _ventanaPrincipal;
    private readonly bool _modoDemo;
    private ActualizacionVerificada? _verificada;
    private bool _descargando;

    public ActualizacionWindow(
        IServicioActualizacionesAplicacion servicio,
        ActualizacionDisponible actualizacion,
        MainWindow ventanaPrincipal,
        bool modoDemo)
    {
        ArgumentNullException.ThrowIfNull(servicio);
        ArgumentNullException.ThrowIfNull(actualizacion);
        ArgumentNullException.ThrowIfNull(ventanaPrincipal);

        _servicio = servicio;
        _actualizacion = actualizacion;
        _ventanaPrincipal = ventanaPrincipal;
        _modoDemo = modoDemo;

        InitializeComponent();
        VersionText.Text = $"Tienes {IdentidadProducto.VersionVisible} · Disponible v{actualizacion.Version}";
        NotasText.Text = string.IsNullOrWhiteSpace(actualizacion.Notas)
            ? "Esta versión no incluye notas adicionales."
            : actualizacion.Notas.Trim();
    }

    private void LaterButton_Click(object sender, RoutedEventArgs e)
    {
        if (_descargando) return;
        Close();
    }

    private async void PrimaryButton_Click(object sender, RoutedEventArgs e)
    {
        if (_descargando) return;

        if (_verificada is null)
        {
            await DescargarAsync();
            return;
        }

        CerrarEInstalar(_verificada);
    }

    private async Task DescargarAsync()
    {
        _descargando = true;
        PrimaryButton.IsEnabled = false;
        LaterButton.IsEnabled = false;
        DownloadProgress.Visibility = Visibility.Visible;
        DownloadProgress.IsIndeterminate = true;
        EstadoText.Text = "Descargando y verificando la actualización… puedes seguir viendo esta ventana mientras termina.";

        var progreso = new Progress<ProgresoDescargaActualizacion>(p =>
        {
            if (p.Porcentaje is { } porcentaje)
            {
                DownloadProgress.IsIndeterminate = false;
                DownloadProgress.Value = porcentaje;
                EstadoText.Text = $"Descargando actualización… {porcentaje}%";
            }
        });

        try
        {
            _verificada = await _servicio.DescargarYVerificarAsync(_actualizacion, progreso);
            DownloadProgress.IsIndeterminate = false;
            DownloadProgress.Value = 100;
            EstadoText.Text = "Actualización descargada y verificada. AulaRaíz debe cerrarse para instalarla y volverá a abrirse automáticamente.";
            PrimaryButton.Content = "Cerrar y actualizar";
            PrimaryButton.IsEnabled = true;
            LaterButton.IsEnabled = true;
        }
        catch (ErrorActualizacionException)
        {
            DownloadProgress.Visibility = Visibility.Collapsed;
            EstadoText.Text = "No fue posible descargar o verificar la actualización. Puedes continuar usando AulaRaíz e intentarlo más tarde.";
            PrimaryButton.Content = "Reintentar descarga";
            PrimaryButton.IsEnabled = true;
            LaterButton.IsEnabled = true;
        }
        finally
        {
            _descargando = false;
        }
    }

    private void CerrarEInstalar(ActualizacionVerificada verificada)
    {
        if (!_ventanaPrincipal.PrepararCierreParaActualizacion())
        {
            EstadoText.Text = "La actualización quedó preparada, pero el cierre fue cancelado porque hay cambios pendientes. Guarda o descarta esos cambios e inténtalo nuevamente.";
            return;
        }

        try
        {
            var updater = Path.Combine(AppContext.BaseDirectory, "AulaRaiz.Updater.exe");
            var app = Environment.ProcessPath;
            if (!File.Exists(updater) || string.IsNullOrWhiteSpace(app) || !File.Exists(app))
                throw new FileNotFoundException();

            var inicio = new ProcessStartInfo
            {
                FileName = updater,
                UseShellExecute = false,
            };
            inicio.ArgumentList.Add("--wait-pid");
            inicio.ArgumentList.Add(Environment.ProcessId.ToString(System.Globalization.CultureInfo.InvariantCulture));
            inicio.ArgumentList.Add("--installer");
            inicio.ArgumentList.Add(verificada.RutaInstalador);
            inicio.ArgumentList.Add("--sha256");
            inicio.ArgumentList.Add(verificada.Sha256);
            inicio.ArgumentList.Add("--app");
            inicio.ArgumentList.Add(app);
            inicio.ArgumentList.Add("--target-version");
            inicio.ArgumentList.Add(verificada.Version);
            if (_modoDemo) inicio.ArgumentList.Add("--demo");

            Process.Start(inicio);
            System.Windows.Application.Current.Shutdown();
        }
        catch
        {
            _ventanaPrincipal.CancelarCierrePreparadoParaActualizacion();
            EstadoText.Text = "No fue posible iniciar el actualizador. AulaRaíz seguirá abierto y tus datos no fueron modificados.";
        }
    }
}
