using SistemaDocente.Core;

namespace SistemaDocente.Application;

public interface IAlmacenamientoGrupos
{
    Grupo? Cargar(GrupoId grupoId);

    bool Existe(GrupoId grupoId);

    void Guardar(Grupo grupo);
}