namespace SistemaDocente.Application;

public enum CanalActualizacion
{
    Estable,
    Preview,
}

public sealed record ActualizacionDisponible(
    string Version,
    string Etiqueta,
    string Notas,
    Uri UrlInstalador,
    Uri UrlChecksums,
    DateTimeOffset? PublicadaEn);

public sealed record ActualizacionVerificada(
    string Version,
    string RutaInstalador,
    string Sha256);

public sealed record ProgresoDescargaActualizacion(long BytesRecibidos, long? TotalBytes)
{
    public int? Porcentaje => TotalBytes is > 0
        ? (int)Math.Clamp(BytesRecibidos * 100L / TotalBytes.Value, 0, 100)
        : null;
}

public interface IServicioActualizacionesAplicacion
{
    Task<ActualizacionDisponible?> BuscarAsync(
        string versionActual,
        CanalActualizacion canal,
        CancellationToken cancellationToken = default);

    Task<ActualizacionVerificada> DescargarYVerificarAsync(
        ActualizacionDisponible actualizacion,
        IProgress<ProgresoDescargaActualizacion>? progreso = null,
        CancellationToken cancellationToken = default);
}

public sealed class ErrorActualizacionException : Exception
{
    public ErrorActualizacionException(string codigo)
        : base("No fue posible completar la actualización de AulaRaíz.")
    {
        Codigo = codigo;
    }

    public ErrorActualizacionException(string codigo, Exception innerException)
        : base("No fue posible completar la actualización de AulaRaíz.", innerException)
    {
        Codigo = codigo;
    }

    public string Codigo { get; }
}
