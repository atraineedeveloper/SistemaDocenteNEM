using SistemaDocente.Core;

namespace SistemaDocente.Application;

public sealed record AsistenciaDiaDetalle(
    GrupoId GrupoId,
    DateOnly Fecha,
    bool EsPersistido,
    IReadOnlyList<AsistenciaEstudianteDetalle> Estudiantes);