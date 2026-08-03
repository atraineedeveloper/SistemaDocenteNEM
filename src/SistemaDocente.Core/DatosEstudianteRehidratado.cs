namespace SistemaDocente.Core;

public readonly record struct DatosEstudianteRehidratado(
    EstudianteId Id,
    string NombreVisible,
    int NumeroLista,
    bool EstaActivo);