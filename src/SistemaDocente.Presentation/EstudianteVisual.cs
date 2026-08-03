using SistemaDocente.Core;

namespace SistemaDocente.Presentation;

public sealed class EstudianteVisual
{
    internal EstudianteVisual(EstudianteId id, string nombre, int numeroLista, bool estaActivo)
    {
        Id = id;
        Nombre = nombre;
        NumeroLista = numeroLista;
        EstaActivo = estaActivo;
    }

    internal EstudianteId Id { get; }

    public string Nombre { get; }

    public int NumeroLista { get; }

    public bool EstaActivo { get; }

    public string Estado => EstaActivo ? "Activo" : "Inactivo";
}