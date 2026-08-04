namespace SistemaDocente.Core;

public sealed record DatosEntregaActividadRehidratada(
    EstudianteId EstudianteId,
    EstadoEntrega Estado,
    string Observacion);