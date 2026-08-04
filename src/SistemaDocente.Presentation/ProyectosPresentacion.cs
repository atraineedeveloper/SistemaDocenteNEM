using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Presentation;

public interface IGestionProyectosPresentacion
{
    IReadOnlyList<ProyectoResumen> ListarProyectos(GrupoId grupoId);
    ProyectoDetalle ObtenerProyecto(ProyectoId id);
    ProyectoDetalle CrearProyecto(GrupoId grupoId, EntradaProyecto entrada);
    ProyectoDetalle ActualizarProyecto(ProyectoId id, int version, EntradaProyecto entrada);
    ProyectoDetalle CambiarEstado(ProyectoId id, int version, EstadoProyecto estado);
    ProyectoDetalle Reabrir(ProyectoId id, int version);
    void EliminarProyecto(ProyectoId id, int version);
    IReadOnlyList<ActividadProyectoResumen> ListarActividades(ProyectoId proyectoId);
    ActividadProyectoDetalle PrepararActividad(ProyectoId proyectoId, string titulo, string descripcion, DateOnly fecha, string observaciones);
    ActividadProyectoDetalle CrearActividad(ProyectoId proyectoId, EntradaActividad entrada);
    ActividadProyectoDetalle ObtenerActividad(ActividadId id);
    ActividadProyectoDetalle ActualizarActividad(ActividadId id, int version, EntradaActividad entrada);
    ActividadProyectoDetalle GuardarEntregas(ActividadId id, int version, IReadOnlyCollection<EntradaEntregaActividad> entradas);
    ActividadProyectoDetalle AnularActividad(ActividadId id, int version);
    void EliminarActividad(ActividadId id, int version);
}

public sealed class GestionProyectosPresentacion(GestionProyectosActividadesCasosUso casos) : IGestionProyectosPresentacion
{
    public IReadOnlyList<ProyectoResumen> ListarProyectos(GrupoId grupoId) => casos.ListarProyectosDelGrupo(grupoId);
    public ProyectoDetalle ObtenerProyecto(ProyectoId id) => casos.ObtenerProyecto(id);
    public ProyectoDetalle CrearProyecto(GrupoId grupoId, EntradaProyecto entrada) => casos.CrearProyecto(grupoId, entrada);
    public ProyectoDetalle ActualizarProyecto(ProyectoId id, int version, EntradaProyecto entrada) => casos.ActualizarProyecto(id, version, entrada);
    public ProyectoDetalle CambiarEstado(ProyectoId id, int version, EstadoProyecto estado) => casos.CambiarEstadoProyecto(id, version, estado);
    public ProyectoDetalle Reabrir(ProyectoId id, int version) => casos.ReabrirProyecto(id, version);
    public void EliminarProyecto(ProyectoId id, int version) => casos.EliminarProyectoBorradorSinActividades(id, version);
    public IReadOnlyList<ActividadProyectoResumen> ListarActividades(ProyectoId proyectoId) => casos.ListarActividadesDelProyecto(proyectoId);
    public ActividadProyectoDetalle PrepararActividad(ProyectoId proyectoId, string titulo, string descripcion, DateOnly fecha, string observaciones) => casos.PrepararNuevaActividad(proyectoId, titulo, descripcion, fecha, observaciones);
    public ActividadProyectoDetalle CrearActividad(ProyectoId proyectoId, EntradaActividad entrada) => casos.CrearActividad(proyectoId, entrada);
    public ActividadProyectoDetalle ObtenerActividad(ActividadId id) => casos.ObtenerActividad(id);
    public ActividadProyectoDetalle ActualizarActividad(ActividadId id, int version, EntradaActividad entrada) => casos.ActualizarActividad(id, version, entrada);
    public ActividadProyectoDetalle GuardarEntregas(ActividadId id, int version, IReadOnlyCollection<EntradaEntregaActividad> entradas) => casos.GuardarEntregasActividad(id, version, entradas);
    public ActividadProyectoDetalle AnularActividad(ActividadId id, int version) => casos.AnularActividad(id, version);
    public void EliminarActividad(ActividadId id, int version) => casos.EliminarActividadSinSeguimiento(id, version);
}

public interface IConfirmacionProyectos
{
    bool Confirmar(string mensaje);
}