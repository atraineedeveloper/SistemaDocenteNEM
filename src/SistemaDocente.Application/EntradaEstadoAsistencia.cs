using SistemaDocente.Core;

namespace SistemaDocente.Application;

public sealed record EntradaEstadoAsistencia(
    EstudianteId EstudianteId,
    EstadoAsistencia Estado);