namespace SistemaDocente.Data;

public class DataIntegrityException : DataAccessException
{
    public DataIntegrityException()
    {
    }

    public DataIntegrityException(string message)
        : base(message)
    {
    }

    public DataIntegrityException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}