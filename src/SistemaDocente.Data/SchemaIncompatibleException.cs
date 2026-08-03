namespace SistemaDocente.Data;

public class SchemaIncompatibleException : DataAccessException
{
    public SchemaIncompatibleException()
    {
    }

    public SchemaIncompatibleException(string message)
        : base(message)
    {
    }

    public SchemaIncompatibleException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}