namespace SistemaDocente.Core;

public sealed record ExpedienteEstudiante
{
    public ExpedienteEstudiante(
        EstudianteId estudianteId,
        GrupoId grupoId,
        IReadOnlyList<NotaPedagogica>? notas = null,
        IReadOnlyList<AcuerdoTutor>? acuerdos = null,
        IReadOnlyList<AlertaPedagogica>? alertas = null)
    {
        EstudianteId = estudianteId;
        GrupoId = grupoId;
        Notas = notas?.ToArray() ?? [];
        Acuerdos = acuerdos?.ToArray() ?? [];
        Alertas = alertas?.ToArray() ?? [];
    }

    public EstudianteId EstudianteId { get; }
    public GrupoId GrupoId { get; }
    public IReadOnlyList<NotaPedagogica> Notas { get; }
    public IReadOnlyList<AcuerdoTutor> Acuerdos { get; }
    public IReadOnlyList<AlertaPedagogica> Alertas { get; }

    public IReadOnlyList<NotaPedagogica> ObtenerNotasPorTipo(TipoNotaPedagogica tipo) =>
        Notas.Where(n => n.Tipo == tipo).OrderByDescending(n => n.FechaHoraRegistro).ToArray();
}