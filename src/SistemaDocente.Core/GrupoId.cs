namespace SistemaDocente.Core;

public readonly record struct GrupoId
{
    private readonly Guid _value;

    internal GrupoId(Guid value)
    {
        _value = value;
    }

    internal static GrupoId Crear() => new(Guid.NewGuid());

    public Guid Valor => _value;

    public static GrupoId DesdeGuid(Guid valor)
    {
        if (valor == Guid.Empty)
        {
            throw new DomainValidationException("La identidad del grupo no puede estar vacía.");
        }

        return new GrupoId(valor);
    }

    public override string ToString() => _value.ToString();
}