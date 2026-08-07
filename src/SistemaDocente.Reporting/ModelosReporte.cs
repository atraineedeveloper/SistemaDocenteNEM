using SistemaDocente.Core;

namespace SistemaDocente.Reporting;

public sealed record MesAsistenciaReporte(int Anio, int Mes, int Dias, int Presentes, int Faltas, int Retardos, int Justificadas, double? Porcentaje);

public sealed record ResumenCumplimientoReporte(
    int ActividadesAplicables,
    int Entregadas,
    int NoEntregadas,
    int Pendientes,
    double? PorcentajeCumplimiento);

public sealed record DistribucionLogroReporte(
    int Pendientes,
    int Domina,
    int Suficiente,
    int EnProceso,
    int RequiereApoyo);

public sealed record ActividadReporteFuente(
    string Proyecto,
    string Actividad,
    DateOnly Fecha,
    EstadoEntregaActividad EstadoEntrega,
    NivelLogro NivelLogro,
    string Observacion);

public sealed record AsistenciaMesFuente(
    int Anio,
    int Mes,
    IReadOnlyList<EstadoAsistencia> Registros);

public sealed record EstudianteReporteFuente(
    EstudianteId EstudianteId,
    int NumeroLista,
    string Nombre,
    GeneroEstudiante Genero,
    int? Edad,
    bool EstaActivo,
    IReadOnlyList<AsistenciaMesFuente> AsistenciaMensual,
    IReadOnlyList<ActividadReporteFuente> Actividades,
    IReadOnlyList<string> Fortalezas,
    IReadOnlyList<string> Dificultades,
    IReadOnlyList<string> Apoyos,
    IReadOnlyList<string> Observaciones,
    IReadOnlyList<string> Acuerdos);

public sealed record ReporteIndividualAlumno(
    ContextoGrupo Contexto,
    string NombreGrupo,
    EstudianteId EstudianteId,
    int NumeroLista,
    string Nombre,
    GeneroEstudiante Genero,
    int? Edad,
    bool EstaActivo,
    double? PorcentajeAsistencia,
    IReadOnlyList<MesAsistenciaReporte> AsistenciaMensual,
    ResumenCumplimientoReporte Cumplimiento,
    DistribucionLogroReporte Logro,
    IReadOnlyList<ActividadReporteFuente> Actividades,
    IReadOnlyList<string> Fortalezas,
    IReadOnlyList<string> Dificultades,
    IReadOnlyList<string> Apoyos,
    IReadOnlyList<string> Observaciones,
    IReadOnlyList<string> Acuerdos);

public sealed record SeguimientoAlumnoReporte(
    EstudianteId EstudianteId,
    int NumeroLista,
    string Nombre,
    bool EstaActivo,
    double? PorcentajeAsistencia,
    ResumenCumplimientoReporte Cumplimiento,
    int RequiereApoyo);

public sealed record ReporteGrupal(
    ContextoGrupo Contexto,
    string NombreGrupo,
    int AlumnosHistoricos,
    int AlumnosActivos,
    double? PorcentajeAsistencia,
    ResumenCumplimientoReporte Cumplimiento,
    DistribucionLogroReporte Logro,
    IReadOnlyList<MesAsistenciaReporte> AsistenciaMensual,
    IReadOnlyList<SeguimientoAlumnoReporte> Seguimiento);
