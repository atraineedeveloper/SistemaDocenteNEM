namespace SistemaDocente.Core;

public readonly record struct EstudianteId
{
    private readonly Guid _value;

    internal EstudianteId(Guid value)
    {
        _value = value;
    }

    internal static EstudianteId Crear() => new(Guid.NewGuid());

    public override string ToString() => _value.ToString();
}