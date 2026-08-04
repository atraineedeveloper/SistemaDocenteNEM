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
    bool EstaActivoActualmente, EstadoEntrega Estado, string Observacion);

public sealed record ActividadProyectoResumen(
    ActividadId ActividadId, ProyectoId ProyectoId, string Titulo, DateOnly FechaRealizacion,
    EstadoActividad Estado, int Total, int Pendientes, int Entregadas, int NoEntregadas, int Version);

public sealed record ActividadProyectoDetalle(
    ActividadId ActividadId, ProyectoId ProyectoId, GrupoId GrupoId, string Titulo,
    string Descripcion, DateOnly FechaRealizacion, string ObservacionesGenerales,
    EstadoActividad Estado, IReadOnlyList<EntregaActividadDetalle> Entregas,
    int Total, int Pendientes, int Entregadas, int NoEntregadas, int Version);

public sealed record EntradaEntregaActividad(EstudianteId EstudianteId, EstadoEntrega Estado, string Observacion);

public sealed record EntradaActividad(
    string Titulo, string Descripcion, DateOnly FechaRealizacion,
    string ObservacionesGenerales, IReadOnlyCollection<EntradaEntregaActividad> Entregas);