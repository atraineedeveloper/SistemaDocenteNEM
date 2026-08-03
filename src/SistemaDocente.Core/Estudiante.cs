namespace SistemaDocente.Core;

public sealed class Estudiante
{
    internal Estudiante(string nombreVisible, int numeroLista)
        : this(EstudianteId.Crear(), nombreVisible, numeroLista, true)
    {
    }

    internal Estudiante(
        EstudianteId id,
        string nombreVisible,
        int numeroLista,
        bool estaActivo)
    {
        Id = id;
        NombreVisible = nombreVisible;
        NumeroLista = numeroLista;
        EstaActivo = estaActivo;
    }

    public EstudianteId Id { get; }

    public string NombreVisible { get; private set; }

    public int NumeroLista { get; private set; }

    public bool EstaActivo { get; private set; }

    internal void Renombrar(string nombreVisible)
    {
        NombreVisible = nombreVisible;
    }

    internal void CambiarNumeroLista(int numeroLista)
    {
        NumeroLista = numeroLista;
    }

    internal void Desactivar()
    {
        EstaActivo = false;
    }

    internal void Reactivar()
    {
        EstaActivo = true;
    }
}