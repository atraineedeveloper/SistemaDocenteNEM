using SistemaDocente.Core;

namespace SistemaDocente.Application.Tests;

public sealed class GestionGrupoCicloVidaTests
{
    [Fact]
    public void ListadoNormalExcluyeArchivadosYListadoArchivadoLosSepara()
    {
        var activo = Grupo.Crear("Activo");
        var archivado = Grupo.Crear("Archivado");
        archivado.Archivar();
        var almacenamiento = new AlmacenamientoEnMemoria(activo, archivado);
        var gestion = new GestionGrupoCasosUso(almacenamiento);

        var activos = gestion.ListarGrupos();
        var archivados = gestion.ListarGruposArchivados();

        Assert.Equal(activo.Id, Assert.Single(activos).GrupoId);
        var detalleArchivado = Assert.Single(archivados);
        Assert.Equal(archivado.Id, detalleArchivado.GrupoId);
        Assert.True(detalleArchivado.EstaArchivado);
    }

    [Fact]
    public void GrupoArchivadoNoPuedeCargarseComoContextoActivoHastaRestaurarlo()
    {
        var grupo = Grupo.Crear("Quinto A");
        var almacenamiento = new AlmacenamientoEnMemoria(grupo);
        var gestion = new GestionGrupoCasosUso(almacenamiento);

        gestion.ArchivarGrupo(grupo.Id);

        Assert.Throws<GrupoArchivadoException>(() => gestion.CargarGrupo(grupo.Id));
        Assert.Empty(gestion.ListarGrupos());

        var restaurado = gestion.RestaurarGrupo(grupo.Id);

        Assert.False(restaurado.EstaArchivado);
        Assert.Equal(grupo.Id, gestion.CargarGrupo(grupo.Id).GrupoId);
    }

    [Fact]
    public void EliminarDelegaResumenYEliminacionAlAlmacenamiento()
    {
        var grupo = Grupo.Crear("Error");
        var almacenamiento = new AlmacenamientoEnMemoria(grupo)
        {
            Resumen = new ResumenEliminacionGrupo(2, 1, 0, 0, 0, 0),
        };
        var gestion = new GestionGrupoCasosUso(almacenamiento);

        var resumen = gestion.ObtenerResumenEliminacion(grupo.Id);
        gestion.EliminarGrupo(grupo.Id);

        Assert.True(resumen.TieneDatos);
        Assert.Equal(2, resumen.Estudiantes);
        Assert.True(almacenamiento.EliminacionSolicitada);
        Assert.False(gestion.Existe(grupo.Id));
    }

    private sealed class AlmacenamientoEnMemoria : IAlmacenamientoGrupos
    {
        private readonly List<Grupo> _grupos;

        internal AlmacenamientoEnMemoria(params Grupo[] grupos)
        {
            _grupos = [.. grupos];
        }

        internal ResumenEliminacionGrupo Resumen { get; init; } = new(0, 0, 0, 0, 0, 0);
        internal bool EliminacionSolicitada { get; private set; }

        public Grupo? Cargar(GrupoId grupoId) => _grupos.SingleOrDefault(x => x.Id == grupoId);
        public bool Existe(GrupoId grupoId) => _grupos.Any(x => x.Id == grupoId);

        public void Guardar(Grupo grupo)
        {
            var indice = _grupos.FindIndex(x => x.Id == grupo.Id);
            if (indice >= 0) _grupos[indice] = grupo;
            else _grupos.Add(grupo);
        }

        public IReadOnlyList<Grupo> ListarTodos() => _grupos.ToArray();
        public ResumenEliminacionGrupo ObtenerResumenEliminacion(GrupoId grupoId) => Resumen;

        public void Eliminar(GrupoId grupoId)
        {
            EliminacionSolicitada = true;
            _grupos.RemoveAll(x => x.Id == grupoId);
        }
    }
}
