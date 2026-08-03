namespace SistemaDocente.Core;

public readonly record struct EstadoEstudianteAsistencia(
    EstudianteId EstudianteId,
    EstadoAsistencia Estado);