namespace SistemaDocente.Core;

public sealed class EntregaActividad
{
    internal EntregaActividad(
        EstudianteId estudianteId,
        EstadoEntregaActividad estadoEntrega,
        NivelLogro nivelLogro,
        string observacion)
    {
        EstudianteId = estudianteId;
        EstadoEntrega = estadoEntrega;
        NivelLogro = nivelLogro;
        Observacion = observacion;
    }

    internal EntregaActividad(EstudianteId estudianteId, NivelLogro nivelLogro, string observacion)
        : this(
            estudianteId,
            InferirEstadoLegado(nivelLogro),
            NormalizarNivelLegado(nivelLogro),
            observacion)
    {
    }

    public EstudianteId EstudianteId { get; }

    public EstadoEntregaActividad EstadoEntrega { get; internal set; }

    public NivelLogro NivelLogro { get; internal set; }

    public string Observacion { get; internal set; }

    private static EstadoEntregaActividad InferirEstadoLegado(NivelLogro nivelLogro) => nivelLogro switch
    {
        NivelLogro.NoEntrego => EstadoEntregaActividad.NoEntregada,
        NivelLogro.Pendiente => EstadoEntregaActividad.Pendiente,
        _ => EstadoEntregaActividad.Entregada,
    };

    private static NivelLogro NormalizarNivelLegado(NivelLogro nivelLogro) =>
        nivelLogro == NivelLogro.NoEntrego ? NivelLogro.Pendiente : nivelLogro;
}
