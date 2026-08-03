namespace SistemaDocente.Core;

public readonly record struct EstudianteId
{
    private readonly Guid _value;

    internal EstudianteId(Guid value)
    {
        _value = value;
    }

    internal static EstudianteId Crear() => new(Guid.NewGuid());

    public Guid Valor => _value;

    public static EstudianteId DesdeGuid(Guid valor)
    {
        if (valor == Guid.Empty)
        {
            throw new DomainValidationException("La identidad del estudiante no puede estar vacía.");
        }

        return new EstudianteId(valor);
    }

    public override string ToString() => _value.ToString();
}