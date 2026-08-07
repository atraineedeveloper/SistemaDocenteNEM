namespace SistemaDocente.Core;

public readonly record struct DatosEstudianteRehidratado(
    EstudianteId Id,
    string NombreVisible,
    string PrimerApellido,
    string SegundoApellido,
    string Nombres,
    DateOnly? FechaNacimiento,
    GeneroEstudiante Genero,
    DateOnly? FechaIngreso,
    string Observaciones,
    int NumeroLista,
    bool EstaActivo,
    GradoPrimaria Grado = GradoPrimaria.NoEspecificado)
{
    public DatosEstudianteRehidratado(EstudianteId id, string nombreVisible, int numeroLista, bool estaActivo)
        : this(id, nombreVisible, "", "", "", null, GeneroEstudiante.NoEspecificado, null, "", numeroLista, estaActivo)
    {
    }
}