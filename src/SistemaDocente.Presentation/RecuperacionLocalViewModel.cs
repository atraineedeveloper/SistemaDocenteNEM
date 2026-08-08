using SistemaDocente.Application;

namespace SistemaDocente.Presentation;

public sealed class RecuperacionLocalViewModel : ViewModelBase
{
    private readonly GestionRespaldoCasosUso _casosUso;
    private InspeccionRespaldoLocal? _inspeccion;
    private ResultadoRespaldoLocal? _ultimoRespaldo;
    private ResultadoRestauracionLocal? _ultimaRestauracion;
    private string _confirmacion = string.Empty;
    private string _mensaje = string.Empty;

    public RecuperacionLocalViewModel(GestionRespaldoCasosUso casosUso)
    {
        _casosUso = casosUso ?? throw new ArgumentNullException(nameof(casosUso));
    }

    public string ModoActual => _casosUso.ModoActual == ModoAlmacenamientoLocal.Demostracion
        ? "Demostración"
        : "Producción";

    public string AdvertenciaSeguridad =>
        "El respaldo contiene datos personales y pedagógicos. La versión 1 no está cifrada; guárdala sólo en una ubicación segura.";

    public string Confirmacion
    {
        get => _confirmacion;
        set
        {
            if (!SetProperty(ref _confirmacion, value)) return;
            OnPropertyChanged(nameof(PuedeRestaurar));
        }
    }

    public bool TieneInspeccion => _inspeccion is not null;
    public bool PuedeRestaurar =>
        _inspeccion?.EsCompatible == true
        && string.Equals(
            Confirmacion.Trim(),
            GestionRespaldoCasosUso.ConfirmacionRestauracion,
            StringComparison.Ordinal);

    public string RutaInspeccion => _inspeccion?.RutaArchivo ?? string.Empty;
    public string FechaRespaldo => _inspeccion?.CreadoUtc.ToLocalTime().ToString("g") ?? string.Empty;
    public string VersionAplicacionRespaldo => _inspeccion?.VersionAplicacion ?? string.Empty;
    public string ModoRespaldo => _inspeccion is null
        ? string.Empty
        : _inspeccion.ModoOrigen == ModoAlmacenamientoLocal.Demostracion
            ? "Demostración"
            : "Producción";
    public string VersionBaseDatos => _inspeccion?.VersionBaseDatos.ToString() ?? string.Empty;
    public string TamanoRespaldo => _inspeccion is null ? string.Empty : FormatearTamano(_inspeccion.TamanoBytes);
    public string ComponentesRespaldo => _inspeccion is null
        ? string.Empty
        : string.Join(
            " · ",
            _inspeccion.Componentes.Select(componente =>
                $"{componente.Nombre}: {FormatearTamano(componente.TamanoBytes)}"));
    public string AdvertenciasInspeccion => _inspeccion is null
        ? string.Empty
        : string.Join(Environment.NewLine, _inspeccion.Advertencias);
    public bool TieneAdvertenciasInspeccion => _inspeccion?.Advertencias.Count > 0;

    public string UltimoRespaldoRuta => _ultimoRespaldo?.RutaArchivo ?? string.Empty;
    public string UltimoRespaldoResumen => _ultimoRespaldo is null
        ? string.Empty
        : $"{FormatearTamano(_ultimoRespaldo.TamanoBytes)} · Base v{_ultimoRespaldo.VersionBaseDatos}";
    public string UltimoRespaldoAdvertencias => _ultimoRespaldo is null
        ? string.Empty
        : string.Join(Environment.NewLine, _ultimoRespaldo.Advertencias);

    public string RutaRespaldoSeguridad => _ultimaRestauracion?.RutaRespaldoSeguridad ?? string.Empty;
    public bool RestauracionCompletada => _ultimaRestauracion is not null;

    public string Mensaje
    {
        get => _mensaje;
        private set => SetProperty(ref _mensaje, value);
    }

    public string CrearNombreArchivoSugerido(DateTimeOffset fechaLocal) =>
        _casosUso.CrearNombreArchivoSugerido(fechaLocal);

    public ResultadoRespaldoLocal CrearRespaldo(
        string rutaDestino,
        DateTimeOffset ahoraUtc,
        string versionAplicacion)
    {
        var resultado = _casosUso.CrearRespaldo(rutaDestino, ahoraUtc, versionAplicacion);
        _ultimoRespaldo = resultado;
        Mensaje = "Respaldo creado correctamente.";
        NotificarRespaldo();
        return resultado;
    }

    public InspeccionRespaldoLocal Inspeccionar(string rutaRespaldo)
    {
        _inspeccion = _casosUso.Inspeccionar(rutaRespaldo);
        _ultimaRestauracion = null;
        Confirmacion = string.Empty;
        Mensaje = "El respaldo es compatible y está listo para revisión.";
        NotificarInspeccion();
        OnPropertyChanged(nameof(RestauracionCompletada));
        OnPropertyChanged(nameof(RutaRespaldoSeguridad));
        return _inspeccion;
    }

    public ResultadoRestauracionLocal Restaurar(
        DateTimeOffset ahoraUtc,
        string versionAplicacion)
    {
        if (_inspeccion is null)
        {
            throw new InvalidOperationException("Selecciona e inspecciona un respaldo antes de restaurar.");
        }

        var resultado = _casosUso.Restaurar(
            _inspeccion.RutaArchivo,
            Confirmacion,
            ahoraUtc,
            versionAplicacion);
        _ultimaRestauracion = resultado;
        Mensaje = "Restauración completada. La aplicación debe cerrarse antes de continuar.";
        OnPropertyChanged(nameof(RestauracionCompletada));
        OnPropertyChanged(nameof(RutaRespaldoSeguridad));
        return resultado;
    }

    public void LimpiarInspeccion()
    {
        _inspeccion = null;
        _ultimaRestauracion = null;
        Confirmacion = string.Empty;
        Mensaje = string.Empty;
        NotificarInspeccion();
        OnPropertyChanged(nameof(RestauracionCompletada));
        OnPropertyChanged(nameof(RutaRespaldoSeguridad));
    }

    private void NotificarRespaldo()
    {
        OnPropertyChanged(nameof(UltimoRespaldoRuta));
        OnPropertyChanged(nameof(UltimoRespaldoResumen));
        OnPropertyChanged(nameof(UltimoRespaldoAdvertencias));
    }

    private void NotificarInspeccion()
    {
        OnPropertyChanged(nameof(TieneInspeccion));
        OnPropertyChanged(nameof(PuedeRestaurar));
        OnPropertyChanged(nameof(RutaInspeccion));
        OnPropertyChanged(nameof(FechaRespaldo));
        OnPropertyChanged(nameof(VersionAplicacionRespaldo));
        OnPropertyChanged(nameof(ModoRespaldo));
        OnPropertyChanged(nameof(VersionBaseDatos));
        OnPropertyChanged(nameof(TamanoRespaldo));
        OnPropertyChanged(nameof(ComponentesRespaldo));
        OnPropertyChanged(nameof(AdvertenciasInspeccion));
        OnPropertyChanged(nameof(TieneAdvertenciasInspeccion));
    }

    private static string FormatearTamano(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024L * 1024) return $"{bytes / 1024d:0.#} KB";
        if (bytes < 1024L * 1024 * 1024) return $"{bytes / (1024d * 1024):0.#} MB";
        return $"{bytes / (1024d * 1024 * 1024):0.#} GB";
    }
}