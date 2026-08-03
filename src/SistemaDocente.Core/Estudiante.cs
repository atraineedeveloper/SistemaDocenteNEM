namespace SistemaDocente.Core;

public sealed class Estudiante
{
    internal Estudiante(string nombreVisible, int numeroLista)
    {
        Id = EstudianteId.Crear();
        NombreVisible = nombreVisible;
        NumeroLista = numeroLista;
        EstaActivo = true;
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