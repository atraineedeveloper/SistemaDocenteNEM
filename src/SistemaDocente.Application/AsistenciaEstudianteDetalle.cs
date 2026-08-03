using SistemaDocente.Core;

namespace SistemaDocente.Application;

public sealed record AsistenciaEstudianteDetalle(
    EstudianteId EstudianteId,
    string NombreVisible,
    int NumeroLista,
    EstadoAsistencia Estado,
    bool EstaActivoActualmente);