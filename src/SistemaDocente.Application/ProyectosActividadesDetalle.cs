using SistemaDocente.Core;

namespace SistemaDocente.Application;

public sealed record ProyectoResumen(
    ProyectoId ProyectoId,
    string Nombre,
    DateOnly FechaInicio,
    DateOnly FechaTermino,
    EstadoProyecto Estado,
    int NumeroActividades,
    int Version,
    MetodologiaProyectoNem Metodologia = MetodologiaProyectoNem.NoEspecificada,
    IReadOnlyList<GradoPrimaria>? GradosObjetivo = null);

public sealed record ProyectoDetalle(
    ProyectoId ProyectoId,
    GrupoId GrupoId,
    string Nombre,
    string Descripcion,
    DateOnly FechaInicio,
    DateOnly FechaTermino,
    EstadoProyecto Estado,
    string Observaciones,
    int NumeroActividades,
    int Version,
    bool DuracionAtipica,
    MetodologiaProyectoNem Metodologia = MetodologiaProyectoNem.NoEspecificada,
    IReadOnlyList<GradoPrimaria>? GradosObjetivo = null);

public sealed record EntradaProyecto(
    string Nombre,
    string Descripcion,
    DateOnly FechaInicio,
    DateOnly FechaTermino,
    string Observaciones,
    MetodologiaProyectoNem Metodologia = MetodologiaProyectoNem.NoEspecificada,
    IReadOnlyCollection<GradoPrimaria>? GradosObjetivo = null);

public sealed record EntregaActividadDetalle(
    EstudianteId EstudianteId,
    int NumeroLista,
    string NombreVisible,
    bool EstaActivoActualmente,
    EstadoEntregaActividad EstadoEntrega,
    NivelLogro NivelLogro,
    string Observacion,
    GradoPrimaria Grado = GradoPrimaria.NoEspecificado)
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
            observacion,
            GradoPrimaria.NoEspecificado)
    {
    }
}

public sealed record ActividadProyectoResumen(
    ActividadId ActividadId,
    ProyectoId ProyectoId,
    string Titulo,
    DateOnly FechaRealizacion,
    EstadoActividad Estado,
    int Total,
    int Pendientes,
    int Domina,
    int Suficiente,
    int EnProceso,
    int RequiereApoyo,
    int NoEntrego,
    int Version,
    CampoFormativoNem CampoFormativo = CampoFormativoNem.NoEspecificado,
    IReadOnlyList<GradoPrimaria>? GradosObjetivo = null);

public sealed record ActividadProyectoDetalle(
    ActividadId ActividadId,
    ProyectoId ProyectoId,
    GrupoId GrupoId,
    string Titulo,
    string Descripcion,
    DateOnly FechaRealizacion,
    string ObservacionesGenerales,
    EstadoActividad Estado,
    IReadOnlyList<EntregaActividadDetalle> Entregas,
    int Total,
    int Pendientes,
    int Domina,
    int Suficiente,
    int EnProceso,
    int RequiereApoyo,
    int NoEntrego,
    int Version,
    CampoFormativoNem CampoFormativo = CampoFormativoNem.NoEspecificado,
    IReadOnlyList<GradoPrimaria>? GradosObjetivo = null);

public sealed record EntradaEntregaActividad(
    EstudianteId EstudianteId,
    EstadoEntregaActividad EstadoEntrega,
    NivelLogro NivelLogro,
    string Observacion)
{
    /// <summary>
    /// Distinguishes new calls that intentionally express delivery state from legacy calls
    /// that only supplied NivelLogro. This preserves historical delivery state when an old
    /// screen edits metadata without knowing about the separate delivery dimension.
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
    string Titulo,
    string Descripcion,
    DateOnly FechaRealizacion,
    string ObservacionesGenerales,
    IReadOnlyCollection<EntradaEntregaActividad> Entregas,
    CampoFormativoNem CampoFormativo = CampoFormativoNem.NoEspecificado,
    IReadOnlyCollection<GradoPrimaria>? GradosObjetivo = null);