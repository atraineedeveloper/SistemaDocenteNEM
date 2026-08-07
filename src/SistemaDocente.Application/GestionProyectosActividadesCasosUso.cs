using SistemaDocente.Core;

namespace SistemaDocente.Application;

public sealed class GestionProyectosActividadesCasosUso
{
    private readonly IAlmacenamientoGrupos _grupos;
    private readonly IAlmacenamientoProyectos _proyectos;
    private readonly IAlmacenamientoActividadesProyecto _actividades;

    public GestionProyectosActividadesCasosUso(IAlmacenamientoGrupos grupos,
        IAlmacenamientoProyectos proyectos, IAlmacenamientoActividadesProyecto actividades)
    {
        _grupos = grupos ?? throw new ArgumentNullException(nameof(grupos));
        _proyectos = proyectos ?? throw new ArgumentNullException(nameof(proyectos));
        _actividades = actividades ?? throw new ArgumentNullException(nameof(actividades));
    }

    public ProyectoDetalle CrearProyecto(GrupoId grupoId, EntradaProyecto entrada)
    {
        CargarGrupo(grupoId);
        var proyecto = ProyectoDidactico.Crear(grupoId, entrada.Nombre, entrada.Descripcion,
            entrada.FechaInicio, entrada.FechaTermino, entrada.Observaciones);
        _proyectos.Guardar(proyecto, null);
        return ProyectarProyecto(_proyectos.Cargar(proyecto.Id) ?? proyecto);
    }

    public ProyectoDetalle ObtenerProyecto(ProyectoId id) => ProyectarProyecto(CargarProyecto(id));

    public IReadOnlyList<ProyectoResumen> ListarProyectosDelGrupo(GrupoId grupoId)
    {
        CargarGrupo(grupoId);
        return _proyectos.ListarPorGrupo(grupoId)
            .OrderBy(x => x.Estado switch { EstadoProyecto.EnCurso => 0, EstadoProyecto.Borrador => 1, _ => 2 })
            .ThenByDescending(x => x.FechaInicio).ThenBy(x => x.Nombre, StringComparer.Ordinal)
            .ThenBy(x => x.Id.Valor)
            .Select(x => new ProyectoResumen(x.Id, x.Nombre, x.FechaInicio, x.FechaTermino,
                x.Estado, _actividades.ListarPorProyecto(x.Id).Count, x.Version)).ToArray();
    }

    public ProyectoDetalle ActualizarProyecto(ProyectoId id, int version, EntradaProyecto entrada)
    {
        var proyecto = CargarProyecto(id);
        VerificarVersion(proyecto.Version, version);
        var fechas = _proyectos.FechasActividadesFueraDeRango(id, entrada.FechaInicio, entrada.FechaTermino)
            .Distinct().OrderBy(x => x).ToArray();
        if (fechas.Length > 0) throw new PeriodoProyectoIncompatibleException(fechas);
        proyecto.Actualizar(entrada.Nombre, entrada.Descripcion, entrada.FechaInicio,
            entrada.FechaTermino, entrada.Observaciones);
        _proyectos.Guardar(proyecto, version);
        return ProyectarProyecto(CargarProyecto(id));
    }

    public ProyectoDetalle CambiarEstadoProyecto(ProyectoId id, int version, EstadoProyecto destino)
    {
        var proyecto = CargarProyecto(id); VerificarVersion(proyecto.Version, version);
        if (destino == EstadoProyecto.EnCurso && proyecto.Estado == EstadoProyecto.Borrador) proyecto.Iniciar();
        else if (destino == EstadoProyecto.Finalizado) proyecto.Finalizar();
        else throw new DomainConflictException("La transición solicitada no es válida.");
        _proyectos.Guardar(proyecto, version);
        return ProyectarProyecto(CargarProyecto(id));
    }

    public ProyectoDetalle ReabrirProyecto(ProyectoId id, int version)
    {
        var proyecto = CargarProyecto(id); VerificarVersion(proyecto.Version, version); proyecto.Reabrir();
        _proyectos.Guardar(proyecto, version); return ProyectarProyecto(CargarProyecto(id));
    }

    public void EliminarProyectoBorradorSinActividades(ProyectoId id, int version)
    {
        var proyecto = CargarProyecto(id); VerificarVersion(proyecto.Version, version);
        if (proyecto.Estado != EstadoProyecto.Borrador || _proyectos.TieneActividades(id))
            throw new DomainConflictException("Sólo puede eliminarse un proyecto Borrador sin actividades.");
        _proyectos.Eliminar(id, version);
    }

    public ActividadProyectoDetalle PrepararNuevaActividad(ProyectoId proyectoId, string titulo,
        string descripcion, DateOnly fecha, string observaciones)
    {
        var proyecto = CargarProyectoEditable(proyectoId); var grupo = CargarGrupo(proyecto.GrupoId);
        var actividad = ActividadProyecto.Crear(proyecto.Id, proyecto.GrupoId, titulo, descripcion,
            fecha, observaciones, proyecto.FechaInicio, proyecto.FechaTermino,
            grupo.EstudiantesActivos.Select(x => x.Id).ToArray());
        return ProyectarActividad(actividad, grupo);
    }

    public ActividadProyectoDetalle CrearActividad(ProyectoId proyectoId, EntradaActividad entrada)
    {
        var proyecto = CargarProyectoEditable(proyectoId); var grupo = CargarGrupo(proyecto.GrupoId);
        var activos = grupo.EstudiantesActivos.Select(x => x.Id).ToHashSet();
        ValidarEntradas(entrada.Entregas, activos);
        var actividad = ActividadProyecto.Crear(proyecto.Id, proyecto.GrupoId, entrada.Titulo,
            entrada.Descripcion, entrada.FechaRealizacion, entrada.ObservacionesGenerales,
            proyecto.FechaInicio, proyecto.FechaTermino, activos);
        actividad.ActualizarEntregas(Mapear(entrada.Entregas));
        _actividades.Guardar(actividad, null);
        return ObtenerActividad(actividad.Id);
    }

    public ActividadProyectoDetalle ObtenerActividad(ActividadId id)
    {
        var actividad = CargarActividad(id); return ProyectarActividad(actividad, CargarGrupo(actividad.GrupoId));
    }

    public IReadOnlyList<ActividadProyectoResumen> ListarActividadesDelProyecto(ProyectoId proyectoId)
    {
        CargarProyecto(proyectoId);
        return _actividades.ListarPorProyecto(proyectoId).OrderBy(x => x.FechaRealizacion)
            .ThenBy(x => x.Titulo, StringComparer.Ordinal).ThenBy(x => x.Id.Valor)
            .Select(ProyectarResumen).ToArray();
    }

    public ActividadProyectoDetalle ActualizarActividad(ActividadId id, int version, EntradaActividad entrada)
    {
        var actividad = CargarActividad(id); var proyecto = CargarProyectoEditable(actividad.ProyectoId);
        VerificarVersion(actividad.Version, version);
        actividad.Actualizar(entrada.Titulo, entrada.Descripcion, entrada.FechaRealizacion,
            entrada.ObservacionesGenerales, proyecto.FechaInicio, proyecto.FechaTermino);
        actividad.ActualizarEntregas(MapearValidandoHistorico(actividad, entrada.Entregas));
        _actividades.Guardar(actividad, version); return ObtenerActividad(id);
    }

    public ActividadProyectoDetalle GuardarEntregasActividad(ActividadId id, int version,
        IReadOnlyCollection<EntradaEntregaActividad> entregas)
    {
        var actividad = CargarActividad(id); CargarProyectoEditable(actividad.ProyectoId);
        VerificarVersion(actividad.Version, version);
        actividad.ActualizarEntregas(MapearValidandoHistorico(actividad, entregas));
        _actividades.Guardar(actividad, version); return ObtenerActividad(id);
    }

    public ActividadProyectoDetalle AnularActividad(ActividadId id, int version)
    {
        var actividad = CargarActividad(id); VerificarVersion(actividad.Version, version); actividad.Anular();
        _actividades.Guardar(actividad, version); return ObtenerActividad(id);
    }

    public void EliminarActividadSinSeguimiento(ActividadId id, int version)
    {
        var actividad = CargarActividad(id); VerificarVersion(actividad.Version, version);
        if (actividad.Entregas.Any(x =>
                x.EstadoEntrega != EstadoEntregaActividad.Pendiente || x.NivelLogro != NivelLogro.Pendiente))
            throw new DomainConflictException("La actividad con seguimiento debe conservarse o anularse.");
        _actividades.Eliminar(id, version);
    }

    private ProyectoDetalle ProyectarProyecto(ProyectoDidactico p) => new(p.Id, p.GrupoId, p.Nombre,
        p.Descripcion, p.FechaInicio, p.FechaTermino, p.Estado, p.Observaciones,
        _actividades.ListarPorProyecto(p.Id).Count, p.Version,
        p.FechaTermino.DayNumber - p.FechaInicio.DayNumber + 1 is < 14 or > 31);

    private static ActividadProyectoDetalle ProyectarActividad(ActividadProyecto a, Grupo grupo)
    {
        var estudiantes = grupo.Estudiantes.ToDictionary(x => x.Id);
        var entregas = a.Entregas.Select(x =>
        {
            if (!estudiantes.TryGetValue(x.EstudianteId, out var estudiante))
                throw new DomainConflictException("Un estudiante histórico ya no pertenece al grupo.");
            return new EntregaActividadDetalle(x.EstudianteId, estudiante.NumeroLista,
                estudiante.NombreVisible, estudiante.EstaActivo, x.EstadoEntrega, x.NivelLogro, x.Observacion);
        }).OrderBy(x => x.NumeroLista).ThenBy(x => x.NombreVisible, StringComparer.Ordinal)
            .ThenBy(x => x.EstudianteId.Valor).ToArray();
        return new(a.Id, a.ProyectoId, a.GrupoId, a.Titulo, a.Descripcion, a.FechaRealizacion,
            a.ObservacionesGenerales, a.Estado, entregas, entregas.Length,
            entregas.Count(x => x.EstadoEntrega != EstadoEntregaActividad.NoEntregada && x.NivelLogro == NivelLogro.Pendiente),
            entregas.Count(x => x.NivelLogro == NivelLogro.Domina),
            entregas.Count(x => x.NivelLogro == NivelLogro.Suficiente),
            entregas.Count(x => x.NivelLogro == NivelLogro.EnProceso),
            entregas.Count(x => x.NivelLogro == NivelLogro.RequiereApoyo),
            entregas.Count(x => x.EstadoEntrega == EstadoEntregaActividad.NoEntregada), a.Version);
    }

    private static ActividadProyectoResumen ProyectarResumen(ActividadProyecto a) => new(a.Id, a.ProyectoId,
        a.Titulo, a.FechaRealizacion, a.Estado, a.Entregas.Count,
        a.Entregas.Count(x => x.EstadoEntrega != EstadoEntregaActividad.NoEntregada && x.NivelLogro == NivelLogro.Pendiente),
        a.Entregas.Count(x => x.NivelLogro == NivelLogro.Domina),
        a.Entregas.Count(x => x.NivelLogro == NivelLogro.Suficiente),
        a.Entregas.Count(x => x.NivelLogro == NivelLogro.EnProceso),
        a.Entregas.Count(x => x.NivelLogro == NivelLogro.RequiereApoyo),
        a.Entregas.Count(x => x.EstadoEntrega == EstadoEntregaActividad.NoEntregada), a.Version);

    private static DatosEntregaActividadRehidratada[] Mapear(IEnumerable<EntradaEntregaActividad> entregas) =>
        entregas.Select(x => new DatosEntregaActividadRehidratada(
            x.EstudianteId, x.EstadoEntrega, x.NivelLogro, x.Observacion)).ToArray();

    private static DatosEntregaActividadRehidratada[] MapearValidandoHistorico(ActividadProyecto actividad,
        IReadOnlyCollection<EntradaEntregaActividad> entregas)
    {
        ValidarEntradas(entregas, actividad.Entregas.Select(x => x.EstudianteId).ToHashSet());
        var existentes = actividad.Entregas.ToDictionary(x => x.EstudianteId);
        return entregas.Select(x =>
        {
            var estado = x.EstadoEntrega;
            if (!x.EstadoEntregaEsExplicito
                && estado == EstadoEntregaActividad.Pendiente
                && x.NivelLogro == NivelLogro.Pendiente
                && existentes.TryGetValue(x.EstudianteId, out var existente))
            {
                estado = existente.EstadoEntrega;
            }

            return new DatosEntregaActividadRehidratada(
                x.EstudianteId,
                estado,
                x.NivelLogro,
                x.Observacion);
        }).ToArray();
    }

    private static void ValidarEntradas(IReadOnlyCollection<EntradaEntregaActividad> entradas, HashSet<EstudianteId> esperados)
    {
        ArgumentNullException.ThrowIfNull(entradas);
        var ids = entradas.Select(x => x.EstudianteId).ToArray();
        if (ids.Length != esperados.Count || ids.Distinct().Count() != ids.Length || !esperados.SetEquals(ids))
            throw new DomainValidationException("Debe proporcionarse exactamente el padrón completo.");
    }

    private ProyectoDidactico CargarProyecto(ProyectoId id) => _proyectos.Cargar(id)
        ?? throw new DomainConflictException("El proyecto no existe.");
    private ProyectoDidactico CargarProyectoEditable(ProyectoId id)
    { var p = CargarProyecto(id); if (p.Estado == EstadoProyecto.Finalizado) throw new DomainConflictException("El proyecto Finalizado es de sólo lectura."); return p; }
    private ActividadProyecto CargarActividad(ActividadId id) => _actividades.Cargar(id)
        ?? throw new DomainConflictException("La actividad no existe.");
    private Grupo CargarGrupo(GrupoId id) => _grupos.Cargar(id)
        ?? throw new GrupoNoEncontradoException("El grupo no existe.");
    private static void VerificarVersion(int actual, int esperada)
    { if (actual != esperada) throw new ConflictoConcurrenciaException("Los datos cambiaron desde la última lectura."); }
}