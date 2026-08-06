namespace SistemaDocente.Core;

public sealed class EntregaActividad
{
    internal EntregaActividad(EstudianteId estudianteId, NivelLogro nivelLogro, string observacion)
    {
        EstudianteId = estudianteId;
        NivelLogro = nivelLogro;
        Observacion = observacion;
    }

    public EstudianteId EstudianteId { get; }
    public NivelLogro NivelLogro { get; internal set; }
    public string Observacion { get; internal set; }
}