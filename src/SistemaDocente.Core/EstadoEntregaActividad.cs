namespace SistemaDocente.Core;

/// <summary>
/// Estado operativo de cumplimiento de una actividad. Es independiente del nivel de logro.
/// </summary>
public enum EstadoEntregaActividad
{
    Pendiente = 0,
    Entregada = 1,
    NoEntregada = 2,
}
