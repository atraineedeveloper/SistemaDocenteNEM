namespace SistemaDocente.Application;

public enum ModoAlmacenamientoLocal
{
    Produccion = 0,
    Demostracion = 1,
}

public enum CategoriaErrorRecuperacionLocal
{
    AccesoArchivo = 0,
    PaqueteInvalido = 1,
    PaqueteIncompatible = 2,
    IntegridadBaseDatos = 3,
    RespaldoSeguridad = 4,
    Publicacion = 5,
}

public sealed class RecuperacionLocalException : Exception
{
    public RecuperacionLocalException(
        CategoriaErrorRecuperacionLocal categoria,
        string message,
        Exception? innerException = null)
        : base(message, innerException)
    {
        Categoria = categoria;
    }

    public CategoriaErrorRecuperacionLocal Categoria { get; }
}

public sealed record ComponenteRespaldoLocal(
    string Nombre,
    long TamanoBytes,
    string Sha256,
    bool Requerido);

public sealed record ResultadoRespaldoLocal(
    string RutaArchivo,
    DateTimeOffset CreadoUtc,
    string VersionAplicacion,
    ModoAlmacenamientoLocal ModoOrigen,
    int VersionBaseDatos,
    long TamanoBytes,
    IReadOnlyList<ComponenteRespaldoLocal> Componentes,
    IReadOnlyList<string> Advertencias);

public sealed record InspeccionRespaldoLocal(
    string RutaArchivo,
    DateTimeOffset CreadoUtc,
    string VersionAplicacion,
    ModoAlmacenamientoLocal ModoOrigen,
    int VersionBaseDatos,
    long TamanoBytes,
    IReadOnlyList<ComponenteRespaldoLocal> Componentes,
    IReadOnlyList<string> Advertencias,
    bool EsCompatible);

public sealed record ResultadoRestauracionLocal(
    string RutaArchivoOrigen,
    string RutaRespaldoSeguridad,
    DateTimeOffset RestauradoUtc,
    bool ReinicioRequerido,
    IReadOnlyList<string> Advertencias);

public interface IServicioRecuperacionLocal
{
    ModoAlmacenamientoLocal ModoActual { get; }

    ResultadoRespaldoLocal CrearRespaldo(
        string rutaDestino,
        DateTimeOffset ahoraUtc,
        string versionAplicacion);

    InspeccionRespaldoLocal Inspeccionar(string rutaRespaldo);

    ResultadoRestauracionLocal Restaurar(
        string rutaRespaldo,
        DateTimeOffset ahoraUtc,
        string versionAplicacion);
}

public sealed class GestionRespaldoCasosUso
{
    public const string ConfirmacionRestauracion = "RESTAURAR";

    private readonly IServicioRecuperacionLocal _servicio;

    public GestionRespaldoCasosUso(IServicioRecuperacionLocal servicio)
    {
        _servicio = servicio ?? throw new ArgumentNullException(nameof(servicio));
    }

    public ModoAlmacenamientoLocal ModoActual => _servicio.ModoActual;

    public string CrearNombreArchivoSugerido(DateTimeOffset fechaLocal)
    {
        var modo = ModoActual == ModoAlmacenamientoLocal.Demostracion ? "Demo" : "Produccion";
        return $"SistemaDocenteNEM_Respaldo_{modo}_{fechaLocal:yyyy-MM-dd_HHmm}.sdocbackup";
    }

    public ResultadoRespaldoLocal CrearRespaldo(
        string rutaDestino,
        DateTimeOffset ahoraUtc,
        string versionAplicacion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rutaDestino);
        ArgumentException.ThrowIfNullOrWhiteSpace(versionAplicacion);
        return _servicio.CrearRespaldo(rutaDestino, ahoraUtc, versionAplicacion);
    }

    public InspeccionRespaldoLocal Inspeccionar(string rutaRespaldo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rutaRespaldo);
        return _servicio.Inspeccionar(rutaRespaldo);
    }

    public ResultadoRestauracionLocal Restaurar(
        string rutaRespaldo,
        string confirmacion,
        DateTimeOffset ahoraUtc,
        string versionAplicacion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rutaRespaldo);
        ArgumentException.ThrowIfNullOrWhiteSpace(versionAplicacion);

        if (!string.Equals(
                confirmacion?.Trim(),
                ConfirmacionRestauracion,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Escribe {ConfirmacionRestauracion} para confirmar la restauración.");
        }

        return _servicio.Restaurar(rutaRespaldo, ahoraUtc, versionAplicacion);
    }
}
