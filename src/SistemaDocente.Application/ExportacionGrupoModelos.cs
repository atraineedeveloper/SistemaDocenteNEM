using SistemaDocente.Core;

namespace SistemaDocente.Application;

public enum ConjuntoExportacionGrupo
{
    Contexto = 1,
    Alumnos = 2,
    Asistencia = 3,
    Proyectos = 4,
    Actividades = 5,
    Evaluacion = 6,
    Seguimiento = 7,
}

public sealed record SolicitudExportacionGrupo(
    GrupoId GrupoId,
    FormatoExportacionTabular Formato,
    IReadOnlyCollection<ConjuntoExportacionGrupo> Conjuntos,
    DateOnly? AsistenciaDesde = null,
    DateOnly? AsistenciaHasta = null,
    ProyectoId? ProyectoId = null,
    bool IncluirObservacionesEstudiante = false,
    bool IncluirObservacionesEvaluacion = false);

public sealed record ResumenConjuntoExportado(
    ConjuntoExportacionGrupo Conjunto,
    string Nombre,
    int Filas);

public sealed record PlanExportacionGrupo(
    GrupoId GrupoId,
    string NombreGrupo,
    FormatoExportacionTabular Formato,
    string NombreArchivoSugerido,
    DocumentoTabularSalida Documento,
    IReadOnlyList<ResumenConjuntoExportado> Conjuntos,
    bool ContieneDatosSensibles);

public sealed record ResultadoExportacionGrupo(
    string RutaArchivo,
    GrupoId GrupoId,
    string NombreGrupo,
    FormatoExportacionTabular Formato,
    IReadOnlyList<ResumenConjuntoExportado> Conjuntos,
    bool ContieneDatosSensibles);
