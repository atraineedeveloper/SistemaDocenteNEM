using SistemaDocente.Core;

namespace SistemaDocente.Application;

public interface IAlmacenamientoActividadesProyecto
{
    ActividadProyecto? Cargar(ActividadId actividadId);
    IReadOnlyList<ActividadProyecto> ListarPorProyecto(ProyectoId proyectoId);
    void Guardar(ActividadProyecto actividad, int? versionEsperada);
    void Eliminar(ActividadId actividadId, int versionEsperada);
}