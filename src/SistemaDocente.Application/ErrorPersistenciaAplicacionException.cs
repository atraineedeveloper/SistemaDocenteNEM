namespace SistemaDocente.Application;

public sealed class ErrorPersistenciaAplicacionException : Exception
{
    public ErrorPersistenciaAplicacionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}