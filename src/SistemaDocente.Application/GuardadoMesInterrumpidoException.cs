namespace SistemaDocente.Application;

public sealed class GuardadoMesInterrumpidoException : Exception
{
    public GuardadoMesInterrumpidoException(
        DateOnly fechaFallida,
        IReadOnlyList<DateOnly> fechasGuardadas,
        IReadOnlyList<DateOnly> fechasPendientes,
        ErrorPersistenciaAplicacionException innerException)
        : base($"No fue posible guardar la asistencia del {fechaFallida:yyyy-MM-dd}.", innerException)
    {
        FechaFallida = fechaFallida;
        FechasGuardadas = fechasGuardadas.ToArray();
        FechasPendientes = fechasPendientes.ToArray();
    }

    public DateOnly FechaFallida { get; }

    public IReadOnlyList<DateOnly> FechasGuardadas { get; }

    public IReadOnlyList<DateOnly> FechasPendientes { get; }
}