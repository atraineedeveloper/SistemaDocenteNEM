using SistemaDocente.Core;

namespace SistemaDocente.Application;

public sealed record GrupoDetalle(
    GrupoId GrupoId,
    string NombreVisible,
    IReadOnlyList<EstudianteDetalle> Estudiantes,
    bool EstaArchivado = false);
