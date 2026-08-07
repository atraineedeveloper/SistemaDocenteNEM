namespace SistemaDocente.Core;

public sealed record DatosEntregaActividadRehidratada(
    EstudianteId EstudianteId,
    EstadoEntregaActividad EstadoEntrega,
    NivelLogro NivelLogro,
    string Observacion)
{
    public DatosEntregaActividadRehidratada(
        EstudianteId estudianteId,
        NivelLogro nivelLogro,
        string observacion)
        : this(
            estudianteId,
            nivelLogro switch
            {
                NivelLogro.NoEntrego => EstadoEntregaActividad.NoEntregada,
                NivelLogro.Pendiente => EstadoEntregaActividad.Pendiente,
                _ => EstadoEntregaActividad.Entregada,
            },
            nivelLogro == NivelLogro.NoEntrego ? NivelLogro.Pendiente : nivelLogro,
            observacion)
    {
    }
}
