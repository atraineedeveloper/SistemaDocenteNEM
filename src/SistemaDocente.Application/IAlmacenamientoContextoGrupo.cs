using SistemaDocente.Core;

namespace SistemaDocente.Application;

public interface IAlmacenamientoContextoGrupo
{
    ContextoGrupo? Cargar(GrupoId grupoId);

    void Guardar(ContextoGrupo contexto);
}
