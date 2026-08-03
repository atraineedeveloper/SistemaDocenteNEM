namespace SistemaDocente.Core;

public sealed class RegistroAsistencia
{
    internal RegistroAsistencia(EstudianteId estudianteId, EstadoAsistencia estado)
    {
        EstudianteId = estudianteId;
        Estado = estado;
    }

    public EstudianteId EstudianteId { get; }

    public EstadoAsistencia Estado { get; private set; }

    internal void CambiarEstado(EstadoAsistencia estado)
    {
        Estado = estado;
    }
}