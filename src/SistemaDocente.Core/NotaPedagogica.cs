namespace SistemaDocente.Core;

public sealed record NotaPedagogica
{
    public NotaPedagogica(Guid notaId, TipoNotaPedagogica tipo, string contenido, DateTime fechaHoraRegistro)
    {
        if (notaId == Guid.Empty) throw new ArgumentException("El ID de la nota no puede estar vacío.", nameof(notaId));
        if (!Enum.IsDefined(tipo)) throw new DomainValidationException("El tipo de nota pedagógica no es válido.");
        ValidadorContenidoPedagogico.ValidarTextoPedagogico(contenido, nameof(contenido));
        NotaId = notaId;
        Tipo = tipo;
        Contenido = contenido.Trim();
        FechaHoraRegistro = fechaHoraRegistro;
    }

    public Guid NotaId { get; }
    public TipoNotaPedagogica Tipo { get; }
    public string Contenido { get; }
    public DateTime FechaHoraRegistro { get; }
}