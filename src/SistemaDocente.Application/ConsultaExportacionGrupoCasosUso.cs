using SistemaDocente.Core;

namespace SistemaDocente.Application;

public sealed class ConsultaExportacionGrupoCasosUso
{
    private readonly IAlmacenamientoProyectos _proyectos;

    public ConsultaExportacionGrupoCasosUso(IAlmacenamientoProyectos proyectos)
    {
        _proyectos = proyectos ?? throw new ArgumentNullException(nameof(proyectos));
    }

    public IReadOnlyList<OpcionProyectoExportacion> ListarProyectos(GrupoId grupoId)
    {
        if (grupoId == default)
        {
            throw new DomainValidationException("Selecciona un grupo para consultar sus proyectos.");
        }

        return _proyectos.ListarPorGrupo(grupoId)
            .OrderByDescending(proyecto => proyecto.FechaInicio)
            .ThenBy(proyecto => proyecto.Nombre)
            .Select(proyecto => new OpcionProyectoExportacion(
                proyecto.Id,
                proyecto.Nombre,
                proyecto.FechaInicio,
                proyecto.FechaTermino))
            .ToArray();
    }
}
