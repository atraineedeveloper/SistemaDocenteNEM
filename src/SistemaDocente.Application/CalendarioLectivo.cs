namespace SistemaDocente.Application;

public interface ICalendarioLectivo
{
    bool EsLaborable(DateOnly fecha);
}

public sealed class CalendarioLectivoLunesAViernes : ICalendarioLectivo
{
    public bool EsLaborable(DateOnly fecha) =>
        fecha.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday;
}