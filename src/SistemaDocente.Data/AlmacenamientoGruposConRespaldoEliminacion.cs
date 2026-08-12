using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Data;

public sealed class AlmacenamientoGruposConRespaldoEliminacion : IAlmacenamientoGrupos
{
    private readonly IAlmacenamientoGrupos _inner;
    private readonly IServicioRecuperacionLocal _recuperacion;
    private readonly string _directorioRespaldos;

    public AlmacenamientoGruposConRespaldoEliminacion(
        IAlmacenamientoGrupos inner,
        IServicioRecuperacionLocal recuperacion,
        string directorioRespaldos)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _recuperacion = recuperacion ?? throw new ArgumentNullException(nameof(recuperacion));
        ArgumentException.ThrowIfNullOrWhiteSpace(directorioRespaldos);
        _directorioRespaldos = Path.GetFullPath(directorioRespaldos);
    }

    public Grupo? Cargar(GrupoId grupoId) => _inner.Cargar(grupoId);

    public bool Existe(GrupoId grupoId) => _inner.Existe(grupoId);

    public void Guardar(Grupo grupo) => _inner.Guardar(grupo);

    public IReadOnlyList<Grupo> ListarTodos() => _inner.ListarTodos();

    public ResumenEliminacionGrupo ObtenerResumenEliminacion(GrupoId grupoId) =>
        _inner.ObtenerResumenEliminacion(grupoId);

    public void Eliminar(GrupoId grupoId)
    {
        var resumen = _inner.ObtenerResumenEliminacion(grupoId);
        if (resumen.TieneDatos)
        {
            Directory.CreateDirectory(_directorioRespaldos);
            var ahoraUtc = DateTimeOffset.UtcNow;
            var nombre = $"{IdentidadProducto.NombreSeguroArchivo}_PreEliminacionGrupo_{ahoraUtc:yyyy-MM-dd_HHmmss}_{grupoId.Valor:N}.sdocbackup";
            var ruta = Path.Combine(_directorioRespaldos, nombre);
            _recuperacion.CrearRespaldo(ruta, ahoraUtc, IdentidadProducto.Version);
        }

        _inner.Eliminar(grupoId);
    }
}
