namespace SistemaDocente.Core;

public sealed record AcuerdoTutor
{
    public AcuerdoTutor(Guid acuerdoId, string motivo, string acuerdoConvenido, DateOnly fechaReunion, DateOnly? fechaSeguimiento = null)
    {
        if (acuerdoId == Guid.Empty) throw new ArgumentException("El ID del acuerdo no puede estar vacío.", nameof(acuerdoId));
        ValidadorContenidoPedagogico.ValidarTextoPedagogico(motivo, nameof(motivo));
        ValidadorContenidoPedagogico.ValidarTextoPedagogico(acuerdoConvenido, nameof(acuerdoConvenido));
        if (fechaSeguimiento.HasValue && fechaSeguimiento.Value < fechaReunion)
        {
            throw new DomainValidationException("La fecha de seguimiento no puede ser anterior a la fecha de la reunión.");
        }
        AcuerdoId = acuerdoId;
        Motivo = motivo.Trim();
        AcuerdoConvenido = acuerdoConvenido.Trim();
        FechaReunion = fechaReunion;
        FechaSeguimiento = fechaSeguimiento;
    }

    public Guid AcuerdoId { get; }
    public string Motivo { get; }
    public string AcuerdoConvenido { get; }
    public DateOnly FechaReunion { get; }
    public DateOnly? FechaSeguimiento { get; }
}