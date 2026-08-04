namespace SistemaDocente.Application;

public sealed class ConflictoConcurrenciaException : Exception
{
    public ConflictoConcurrenciaException(string message) : base(message) { }
}