namespace SistemaDocente.Core;

public readonly record struct DatosRegistroAsistenciaRehidratado(
    EstudianteId EstudianteId,
    EstadoAsistencia Estado);