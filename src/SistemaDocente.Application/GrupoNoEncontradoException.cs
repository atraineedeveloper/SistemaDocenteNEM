namespace SistemaDocente.Application;

public sealed class GrupoNoEncontradoException : Exception
{
    public GrupoNoEncontradoException(string message)
        : base(message)
    {
    }
}