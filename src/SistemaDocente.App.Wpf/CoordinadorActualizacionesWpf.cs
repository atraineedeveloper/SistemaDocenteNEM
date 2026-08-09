using SistemaDocente.Application;

namespace SistemaDocente.App.Wpf;

public sealed class CoordinadorActualizacionesWpf
{
    private readonly MainWindow _ventana;
    private readonly IServicioActualizacionesAplicacion _servicio;
    private readonly IRegistroDiagnosticoSeguro? _diagnostico;
    private readonly bool _modoDemo;
    private bool _comprobando;
    private ActualizacionWindow? _dialogoAbierto;

    public CoordinadorActualizacionesWpf(
        MainWindow ventana,
        IServicioActualizacionesAplicacion servicio,
        bool modoDemo,
        IRegistroDiagnosticoSeguro? diagnostico)
    {
        _ventana = ventana;
        _servicio = servicio;
        _modoDemo = modoDemo;
        _diagnostico = diagnostico;
    }

    public async Task ComprobarAlInicioAsync()
    {
        await Task.Delay(TimeSpan.FromSeconds(1.5));
        await ComprobarAsync(false);
    }

    public Task ComprobarManualAsync() => ComprobarAsync(true);

    private async Task ComprobarAsync(bool mostrarSinNovedades)
    {
        if (_comprobando) return;
        _comprobando = true;
        try
        {
            var actualizacion = await _servicio.BuscarAsync(
                IdentidadProducto.Version,
                CanalActualizacion.Preview);
            if (actualizacion is null)
            {
                if (mostrarSinNovedades)
                {
                    _ventana.MostrarToastInfo(
                        $"Ya tienes la versión más reciente ({IdentidadProducto.VersionVisible}).",
                        "AulaRaíz está actualizado");
                }
                return;
            }

            if (_dialogoAbierto is { IsVisible: true })
            {
                _dialogoAbierto.Activate();
                return;
            }

            _dialogoAbierto = new ActualizacionWindow(
                _servicio,
                actualizacion,
                _ventana,
                _modoDemo)
            {
                Owner = _ventana,
            };
            _dialogoAbierto.Closed += (_, _) => _dialogoAbierto = null;
            _dialogoAbierto.Show();
        }
        catch (Exception exception)
        {
            _diagnostico?.Registrar(exception, CategoriaEventoDiagnostico.FalloActualizacion);
            if (mostrarSinNovedades)
            {
                _ventana.MostrarToastAdvertencia(
                    "No fue posible consultar actualizaciones. AulaRaíz puede seguir utilizándose sin conexión.",
                    "Actualización no disponible");
            }
        }
        finally
        {
            _comprobando = false;
        }
    }
}
