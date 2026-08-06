namespace SistemaDocente.Core;

public sealed record DatosEntregaActividadRehidratada(
    EstudianteId EstudianteId,
    NivelLogro NivelLogro,
    string Observacion);