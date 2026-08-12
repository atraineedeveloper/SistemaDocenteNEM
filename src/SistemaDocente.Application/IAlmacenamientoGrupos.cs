using SistemaDocente.Core;

namespace SistemaDocente.Application;

public interface IAlmacenamientoGrupos
{
    Grupo? Cargar(GrupoId grupoId);

    bool Existe(GrupoId grupoId);

    void Guardar(Grupo grupo);

    IReadOnlyList<Grupo> ListarTodos();

    ResumenEliminacionGrupo ObtenerResumenEliminacion(GrupoId grupoId) =>
        new(0, 0, 0, 0, 0, 0);

    void Eliminar(GrupoId grupoId) =>
        throw new NotSupportedException("Este almacenamiento no admite eliminación permanente de grupos.");
}
