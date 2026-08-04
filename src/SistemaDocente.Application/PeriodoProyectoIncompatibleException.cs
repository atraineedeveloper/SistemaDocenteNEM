namespace SistemaDocente.Application;

public sealed class PeriodoProyectoIncompatibleException : Exception
{
    public PeriodoProyectoIncompatibleException(IReadOnlyList<DateOnly> fechas)
        : base("El periodo no puede cambiar porque existen actividades fuera del nuevo rango.")
    {
        FechasIncompatibles = fechas.ToArray();
    }

    public IReadOnlyList<DateOnly> FechasIncompatibles { get; }
}