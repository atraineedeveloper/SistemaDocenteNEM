namespace SistemaDocente.Core;

public enum NivelGravedadAlerta
{
    Informativa = 0,
    AtencionRequerida = 1,
    Prioritaria = 2,
}

public sealed record AlertaPedagogica
{
    public AlertaPedagogica(NivelGravedadAlerta gravedad, string mensaje)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mensaje);
        Gravedad = gravedad;
        Mensaje = mensaje.Trim();
    }

    public NivelGravedadAlerta Gravedad { get; }
    public string Mensaje { get; }
}