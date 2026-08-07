using SistemaDocente.Core;

namespace SistemaDocente.Application;

public sealed record ProyectoResumen(
    ProyectoId ProyectoId, string Nombre, DateOnly FechaInicio, DateOnly FechaTermino,
    EstadoProyecto Estado, int NumeroActividades, int Version);

public sealed record ProyectoDetalle(
    ProyectoId ProyectoId, GrupoId GrupoId, string Nombre, string Descripcion,
    DateOnly FechaInicio, DateOnly FechaTermino, EstadoProyecto Estado,
    string Observaciones, int NumeroActividades, int Version, bool DuracionAtipica);

public sealed record EntradaProyecto(
    string Nombre, string Descripcion, DateOnly FechaInicio, DateOnly FechaTermino, string Observaciones);

public sealed record EntregaActividadDetalle(
    EstudianteId EstudianteId, int NumeroLista, string NombreVisible,
    bool EstaActivoActualmente, EstadoEntregaActividad EstadoEntrega, NivelLogro NivelLogro, string Observacion)
{
    public EntregaActividadDetalle(
        EstudianteId estudianteId,
        int numeroLista,
        string nombreVisible,
        bool estaActivoActualmente,
        NivelLogro nivelLogro,
        string observacion)
        : this(
            estudianteId,
            numeroLista,
            nombreVisible,
            estaActivoActualmente,
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

public sealed record ActividadProyectoResumen(
    ActividadId ActividadId, ProyectoId ProyectoId, string Titulo, DateOnly FechaRealizacion,
    EstadoActividad Estado, int Total, int Pendientes, int Domina, int Suficiente,
    int EnProceso, int RequiereApoyo, int NoEntrego, int Version);

public sealed record ActividadProyectoDetalle(
    ActividadId ActividadId, ProyectoId ProyectoId, GrupoId GrupoId, string Titulo,
    string Descripcion, DateOnly FechaRealizacion, string ObservacionesGenerales,
    EstadoActividad Estado, IReadOnlyList<EntregaActividadDetalle> Entregas,
    int Total, int Pendientes, int Domina, int Suficiente, int EnProceso,
    int RequiereApoyo, int NoEntrego, int Version);

public sealed record EntradaEntregaActividad(
    EstudianteId EstudianteId,
    EstadoEntregaActividad EstadoEntrega,
    NivelLogro NivelLogro,
    string Observacion)
{
    /// <summary>
    /// Distingue llamadas nuevas, que expresan el estado de entrega de forma intencional,
    /// de llamadas legacy que sólo proporcionaban NivelLogro. Permite conservar el estado
    /// histórico cuando una pantalla antigua edita metadatos sin conocer esta dimensión.
    /// </summary>
    public bool EstadoEntregaEsExplicito { get; private init; } = true;

    public EntradaEntregaActividad(EstudianteId estudianteId, NivelLogro nivelLogro, string observacion)
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
        EstadoEntregaEsExplicito = false;
    }
}

public sealed record EntradaActividad(
    string Titulo, string Descripcion, DateOnly FechaRealizacion,
    string ObservacionesGenerales, IReadOnlyCollection<EntradaEntregaActividad> Entregas);