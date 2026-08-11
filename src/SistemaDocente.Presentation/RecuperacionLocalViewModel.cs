using System.Globalization;

using SistemaDocente.Application;

namespace SistemaDocente.Presentation;

public sealed class RecuperacionLocalViewModel : ViewModelBase
{
    private static readonly CultureInfo CulturaEsMx = CultureInfo.GetCultureInfo("es-MX");

    private readonly GestionRespaldoCasosUso _casosUso;
    private InspeccionRespaldoLocal? _inspeccion;
    private ResultadoRespaldoLocal? _ultimoRespaldo;
    private ResultadoRestauracionLocal? _ultimaRestauracion;
    private string? _rutaProtegidaPendiente;
    private bool _inspeccionProtegida;
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
        $"El respaldo de {ModoActual} contiene datos personales y pedagógicos. La copia estándar v1 no está cifrada; puedes activar protección con contraseña para crear un v2 cifrado.";

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
    public bool RequiereContrasenaInspeccion =>
        _inspeccion is null && !string.IsNullOrWhiteSpace(_rutaProtegidaPendiente);
    public bool InspeccionProtegida => _inspeccionProtegida;
    public string RutaProtegidaPendiente => _rutaProtegidaPendiente ?? string.Empty;
    public string TipoProteccionInspeccion => _inspeccion is null
        ? string.Empty
        : _inspeccionProtegida
            ? "Protegido con contraseña (v2)"
            : "Sin contraseña (v1)";

    public bool PuedeRestaurar =>
        _inspeccion?.EsCompatible == true
        && string.Equals(
            Confirmacion.Trim(),
            GestionRespaldoCasosUso.ConfirmacionRestauracion,
            StringComparison.Ordinal);

    public string RutaInspeccion => _inspeccion?.RutaArchivo ?? string.Empty;
    public string FechaRespaldo => _inspeccion?.CreadoUtc.ToLocalTime().ToString("g", CulturaEsMx) ?? string.Empty;
    public string VersionAplicacionRespaldo => _inspeccion?.VersionAplicacion ?? string.Empty;
    public string ModoRespaldo => _inspeccion is null
        ? string.Empty
        : _inspeccion.ModoOrigen == ModoAlmacenamientoLocal.Demostracion
            ? "Demostración"
            : "Producción";
    public string VersionBaseDatos => _inspeccion?.VersionBaseDatos.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
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
        Mensaje = "Respaldo estándar v1 creado correctamente.";
        NotificarRespaldo();
        return resultado;
    }

    public ResultadoRespaldoLocal CrearRespaldoProtegido(
        string rutaDestino,
        DateTimeOffset ahoraUtc,
        string versionAplicacion,
        char[] contrasena)
    {
        var resultado = _casosUso.CrearRespaldoProtegido(
            rutaDestino,
            ahoraUtc,
            versionAplicacion,
            contrasena);
        _ultimoRespaldo = resultado;
        Mensaje = "Respaldo protegido v2 creado correctamente.";
        NotificarRespaldo();
        return resultado;
    }

    public TipoProteccionRespaldoLocal SeleccionarRespaldo(string rutaRespaldo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rutaRespaldo);
        LimpiarInspeccion();

        var proteccion = _casosUso.DetectarProteccion(rutaRespaldo);
        if (proteccion == TipoProteccionRespaldoLocal.Contrasena)
        {
            _rutaProtegidaPendiente = rutaRespaldo;
            Mensaje = "El respaldo está protegido. Escribe su contraseña para inspeccionarlo.";
            NotificarInspeccion();
            return proteccion;
        }

        Inspeccionar(rutaRespaldo);
        return proteccion;
    }

    public InspeccionRespaldoLocal Inspeccionar(string rutaRespaldo)
    {
        _inspeccion = _casosUso.Inspeccionar(rutaRespaldo);
        _rutaProtegidaPendiente = null;
        _inspeccionProtegida = false;
        _ultimaRestauracion = null;
        Confirmacion = string.Empty;
        Mensaje = "El respaldo v1 es compatible y está listo para revisión.";
        NotificarInspeccion();
        OnPropertyChanged(nameof(RestauracionCompletada));
        OnPropertyChanged(nameof(RutaRespaldoSeguridad));
        return _inspeccion;
    }

    public InspeccionRespaldoLocal InspeccionarProtegido(char[] contrasena)
    {
        if (string.IsNullOrWhiteSpace(_rutaProtegidaPendiente))
        {
            throw new InvalidOperationException("Selecciona un respaldo protegido antes de escribir la contraseña.");
        }

        var ruta = _rutaProtegidaPendiente;
        _inspeccion = _casosUso.InspeccionarProtegido(ruta, contrasena);
        _rutaProtegidaPendiente = null;
        _inspeccionProtegida = true;
        _ultimaRestauracion = null;
        Confirmacion = string.Empty;
        Mensaje = "El respaldo protegido es compatible y está listo para revisión.";
        NotificarInspeccion();
        OnPropertyChanged(nameof(RestauracionCompletada));
        OnPropertyChanged(nameof(RutaRespaldoSeguridad));
        return _inspeccion;
    }

    public ResultadoRestauracionLocal Restaurar(
        DateTimeOffset ahoraUtc,
        string versionAplicacion)
    {
        return Restaurar(ahoraUtc, versionAplicacion, contrasena: null);
    }

    public ResultadoRestauracionLocal Restaurar(
        DateTimeOffset ahoraUtc,
        string versionAplicacion,
        char[]? contrasena)
    {
        if (_inspeccion is null)
        {
            throw new InvalidOperationException("Selecciona e inspecciona un respaldo antes de restaurar.");
        }

        ResultadoRestauracionLocal resultado;
        if (_inspeccionProtegida)
        {
            if (contrasena is null)
            {
                throw new InvalidOperationException("Escribe la contraseña del respaldo protegido para restaurarlo.");
            }
            resultado = _casosUso.RestaurarProtegido(
                _inspeccion.RutaArchivo,
                Confirmacion,
                ahoraUtc,
                versionAplicacion,
                contrasena);
        }
        else
        {
            resultado = _casosUso.Restaurar(
                _inspeccion.RutaArchivo,
                Confirmacion,
                ahoraUtc,
                versionAplicacion);
        }

        _ultimaRestauracion = resultado;
        Mensaje = "Restauración completada. La aplicación debe cerrarse antes de continuar.";
        OnPropertyChanged(nameof(RestauracionCompletada));
        OnPropertyChanged(nameof(RutaRespaldoSeguridad));
        return resultado;
    }

    public void LimpiarInspeccion()
    {
        _inspeccion = null;
        _rutaProtegidaPendiente = null;
        _inspeccionProtegida = false;
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
        OnPropertyChanged(nameof(RequiereContrasenaInspeccion));
        OnPropertyChanged(nameof(InspeccionProtegida));
        OnPropertyChanged(nameof(RutaProtegidaPendiente));
        OnPropertyChanged(nameof(TipoProteccionInspeccion));
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
        if (bytes < 1024) return FormattableString.Invariant($"{bytes} B");
        if (bytes < 1024L * 1024) return FormattableString.Invariant($"{bytes / 1024d:0.#} KB");
        if (bytes < 1024L * 1024 * 1024) return FormattableString.Invariant($"{bytes / (1024d * 1024):0.#} MB");
        return FormattableString.Invariant($"{bytes / (1024d * 1024 * 1024):0.#} GB");
    }
}