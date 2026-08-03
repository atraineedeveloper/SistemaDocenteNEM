using SistemaDocente.Core;

namespace SistemaDocente.Application;

public sealed record EstudianteDetalle(
    EstudianteId EstudianteId,
    string NombreVisible,
    int NumeroLista,
    bool EstaActivo);