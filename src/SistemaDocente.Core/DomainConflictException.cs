namespace SistemaDocente.Core;

public class DomainConflictException : Exception
{
    public DomainConflictException()
    {
    }

    public DomainConflictException(string message)
        : base(message)
    {
    }

    public DomainConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}