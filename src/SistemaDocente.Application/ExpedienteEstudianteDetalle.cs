using SistemaDocente.Core;

namespace SistemaDocente.Application;

public sealed record ResumenAsistenciaEstudiante(
    int TotalDias,
    int Presentes,
    int Faltas,
    int Retardos,
    int Justificadas,
    double PorcentajeAsistencia);

public sealed record HistorialEntregaEstudiante(
    string NombreProyecto,
    string TituloActividad,
    DateOnly Fecha,
    NivelLogro NivelLogro,
    string Observacion);

public sealed record ExpedienteEstudianteDetalle(
    EstudianteId EstudianteId,
    GrupoId GrupoId,
    string NombreEstudiante,
    int NumeroLista,
    bool EstaActivo,
    ResumenAsistenciaEstudiante Asistencia,
    IReadOnlyList<HistorialEntregaEstudiante> Entregas,
    IReadOnlyList<NotaPedagogica> Fortalezas,
    IReadOnlyList<NotaPedagogica> Dificultades,
    IReadOnlyList<NotaPedagogica> ApoyosAplicados,
    IReadOnlyList<NotaPedagogica> ObservacionesCronologicas,
    IReadOnlyList<AcuerdoTutor> AcuerdosTutores,
    IReadOnlyList<AlertaPedagogica> AlertasPedagogicas);
