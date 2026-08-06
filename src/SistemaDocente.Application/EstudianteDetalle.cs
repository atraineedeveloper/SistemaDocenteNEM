using SistemaDocente.Core;

namespace SistemaDocente.Application;

public sealed record EstudianteDetalle(
    EstudianteId EstudianteId,
    string NombreVisible,
    string PrimerApellido,
    string SegundoApellido,
    string Nombres,
    DateOnly? FechaNacimiento,
    int? Edad,
    GeneroEstudiante Genero,
    DateOnly? FechaIngreso,
    string Observaciones,
    int NumeroLista,
    bool EstaActivo)
{
    public EstudianteDetalle(EstudianteId estudianteId, string nombreVisible, int numeroLista, bool estaActivo)
        : this(estudianteId, nombreVisible, "", "", "", null, null, GeneroEstudiante.NoEspecificado, null, "", numeroLista, estaActivo)
    {
    }
}