namespace SistemaDocente.Core;

public readonly record struct ActividadId
{
    private readonly Guid _value;

    private ActividadId(Guid value) => _value = value;

    internal static ActividadId Crear() => new(Guid.NewGuid());

    public Guid Valor => _value;

    public static ActividadId DesdeGuid(Guid valor) => valor == Guid.Empty
        ? throw new DomainValidationException("La identidad de la actividad no puede estar vacía.")
        : new ActividadId(valor);

    public override string ToString() => _value.ToString();
}