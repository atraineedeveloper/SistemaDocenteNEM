using SistemaDocente.Core;

namespace SistemaDocente.Application;

public sealed class GestionContextoGrupoCasosUso
{
    private readonly IAlmacenamientoGrupos _grupos;
    private readonly IAlmacenamientoContextoGrupo _contextos;

    public GestionContextoGrupoCasosUso(
        IAlmacenamientoGrupos grupos,
        IAlmacenamientoContextoGrupo contextos)
    {
        _grupos = grupos ?? throw new ArgumentNullException(nameof(grupos));
        _contextos = contextos ?? throw new ArgumentNullException(nameof(contextos));
    }

    public ContextoGrupo Obtener(GrupoId grupoId)
    {
        AsegurarGrupo(grupoId);
        return _contextos.Cargar(grupoId) ?? ContextoGrupo.Crear(grupoId);
    }

    public ContextoGrupo Guardar(ContextoGrupo contexto)
    {
        ArgumentNullException.ThrowIfNull(contexto);
        AsegurarGrupo(contexto.GrupoId);
        _contextos.Guardar(contexto);
        return _contextos.Cargar(contexto.GrupoId) ?? contexto;
    }

    private void AsegurarGrupo(GrupoId grupoId)
    {
        if (!_grupos.Existe(grupoId))
        {
            throw new GrupoNoEncontradoException("El grupo configurado no existe.");
        }
    }
}
