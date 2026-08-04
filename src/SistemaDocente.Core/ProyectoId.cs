namespace SistemaDocente.Core;

public readonly record struct ProyectoId
{
    private readonly Guid _value;

    private ProyectoId(Guid value) => _value = value;

    internal static ProyectoId Crear() => new(Guid.NewGuid());

    public Guid Valor => _value;

    public static ProyectoId DesdeGuid(Guid valor) => valor == Guid.Empty
        ? throw new DomainValidationException("La identidad del proyecto no puede estar vacía.")
        : new ProyectoId(valor);

    public override string ToString() => _value.ToString();
}