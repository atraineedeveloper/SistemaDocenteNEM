using SistemaDocente.Core;

namespace SistemaDocente.Application;

public interface IAlmacenamientoProyectos
{
    ProyectoDidactico? Cargar(ProyectoId proyectoId);
    IReadOnlyList<ProyectoDidactico> ListarPorGrupo(GrupoId grupoId);
    void Guardar(ProyectoDidactico proyecto, int? versionEsperada);
    IReadOnlyList<DateOnly> FechasActividadesFueraDeRango(ProyectoId proyectoId, DateOnly inicio, DateOnly termino);
    bool TieneActividades(ProyectoId proyectoId);
    void Eliminar(ProyectoId proyectoId, int versionEsperada);
}