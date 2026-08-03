namespace SistemaDocente.Core;

public readonly record struct GrupoId
{
    private readonly Guid _value;

    internal GrupoId(Guid value)
    {
        _value = value;
    }

    internal static GrupoId Crear() => new(Guid.NewGuid());

    public override string ToString() => _value.ToString();
}