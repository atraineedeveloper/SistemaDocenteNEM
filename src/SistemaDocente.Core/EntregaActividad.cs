namespace SistemaDocente.Core;

public sealed class EntregaActividad
{
    internal EntregaActividad(EstudianteId estudianteId, EstadoEntrega estado, string observacion)
    {
        EstudianteId = estudianteId;
        Estado = estado;
        Observacion = observacion;
    }

    public EstudianteId EstudianteId { get; }
    public EstadoEntrega Estado { get; internal set; }
    public string Observacion { get; internal set; }
}