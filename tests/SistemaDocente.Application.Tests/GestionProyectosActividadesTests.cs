using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Application.Tests;

public sealed class GestionProyectosActividadesTests
{
    [Fact]
    public void CreaListaOrdenaYBloqueaPeriodoIncompatible()
    {
        var contexto = new Contexto();
        var uno = contexto.Casos.CrearProyecto(contexto.Grupo.Id,
            new("Borrador", "", new DateOnly(2026, 2, 1), new DateOnly(2026, 2, 28), ""));
        var dos = contexto.Casos.CrearProyecto(contexto.Grupo.Id,
            new("En curso", "", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), ""));
        contexto.Casos.CambiarEstadoProyecto(dos.ProyectoId, dos.Version, EstadoProyecto.EnCurso);

        Assert.Equal("En curso", contexto.Casos.ListarProyectosDelGrupo(contexto.Grupo.Id)[0].Nombre);
        contexto.Proyectos.FechasFuera = [new DateOnly(2026, 2, 20)];
        Assert.Throws<PeriodoProyectoIncompatibleException>(() => contexto.Casos.ActualizarProyecto(
            uno.ProyectoId, uno.Version, new("Cambio", "", new DateOnly(2026, 2, 1), new DateOnly(2026, 2, 10), "")));
    }

    [Fact]
    public void PreparaSinGuardarYGuardaActividadCompletaConConteos()
    {
        var contexto = new Contexto();
        contexto.Grupo.AgregarEstudiante("Ana", 1);
        contexto.Grupo.AgregarEstudiante("Beto", 2);
        var proyecto = contexto.Casos.CrearProyecto(contexto.Grupo.Id,
            new("P", "", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), ""));

        var preparada = contexto.Casos.PrepararNuevaActividad(proyecto.ProyectoId, "A", "",
            new DateOnly(2026, 1, 10), "");

        Assert.Equal(2, preparada.Pendientes);
        Assert.Equal(0, contexto.Actividades.Guardados);
        var entradas = preparada.Entregas.Select((x, i) => new EntradaEntregaActividad(
            x.EstudianteId, i == 0 ? NivelLogro.Domina : NivelLogro.NoEntrego, "")).ToArray();
        var guardada = contexto.Casos.CrearActividad(proyecto.ProyectoId,
            new("A", "", new DateOnly(2026, 1, 10), "", entradas));
        Assert.Equal(1, guardada.Domina);
        Assert.Equal(1, guardada.NoEntrego);
        Assert.Equal(1, contexto.Actividades.Guardados);
    }

    [Fact]
    public void ActividadHistoricaConservaInactivoYNoAgregaAltaPosterior()
    {
        var contexto = new Contexto();
        var historico = contexto.Grupo.AgregarEstudiante("Histórico", 1);
        var proyecto = contexto.Casos.CrearProyecto(contexto.Grupo.Id,
            new("P", "", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), ""));
        var preparada = contexto.Casos.PrepararNuevaActividad(proyecto.ProyectoId, "A", "", new DateOnly(2026, 1, 2), "");
        var guardada = contexto.Casos.CrearActividad(proyecto.ProyectoId,
            new("A", "", new DateOnly(2026, 1, 2), "", preparada.Entregas.Select(x => new EntradaEntregaActividad(x.EstudianteId, x.NivelLogro, "")).ToArray()));
        contexto.Grupo.DesactivarEstudiante(historico.Id);
        contexto.Grupo.AgregarEstudiante("Nuevo", 2);

        var recargada = contexto.Casos.ObtenerActividad(guardada.ActividadId);

        Assert.Single(recargada.Entregas);
        Assert.False(recargada.Entregas[0].EstaActivoActualmente);
    }

    private sealed class Contexto
    {
        internal Contexto()
        {
            Grupo = Grupo.Crear("Grupo"); Grupos = new(Grupo); Proyectos = new(); Actividades = new();
            Casos = new(Grupos, Proyectos, Actividades);
        }
        internal Grupo Grupo { get; }
        internal GruposDoble Grupos { get; }
        internal ProyectosDoble Proyectos { get; }
        internal ActividadesDoble Actividades { get; }
        internal GestionProyectosActividadesCasosUso Casos { get; }
    }

    private sealed class GruposDoble(Grupo grupo) : IAlmacenamientoGrupos
    {
        public Grupo? Cargar(GrupoId grupoId) => grupoId == grupo.Id ? grupo : null;
        public bool Existe(GrupoId grupoId) => grupoId == grupo.Id;
        public void Guardar(Grupo valor) { }
        public IReadOnlyList<Grupo> ListarTodos() => [grupo];
    }

    private sealed class ProyectosDoble : IAlmacenamientoProyectos
    {
        private readonly Dictionary<ProyectoId, ProyectoDidactico> _datos = [];
        public IReadOnlyList<DateOnly> FechasFuera { get; set; } = [];
        public ProyectoDidactico? Cargar(ProyectoId id) => _datos.GetValueOrDefault(id);
        public IReadOnlyList<ProyectoDidactico> ListarPorGrupo(GrupoId id) => _datos.Values.Where(x => x.GrupoId == id).ToArray();
        public void Guardar(ProyectoDidactico p, int? version) => _datos[p.Id] = ProyectoDidactico.Rehidratar(p.Id, p.GrupoId, p.Nombre, p.Descripcion, p.FechaInicio, p.FechaTermino, p.Estado, p.Observaciones, (version ?? 0) + 1);
        public IReadOnlyList<DateOnly> FechasActividadesFueraDeRango(ProyectoId id, DateOnly i, DateOnly t) => FechasFuera;
        public bool TieneActividades(ProyectoId id) => false;
        public void Eliminar(ProyectoId id, int version) => _datos.Remove(id);
    }

    private sealed class ActividadesDoble : IAlmacenamientoActividadesProyecto
    {
        private readonly Dictionary<ActividadId, ActividadProyecto> _datos = [];
        public int Guardados { get; private set; }
        public ActividadProyecto? Cargar(ActividadId id) => _datos.GetValueOrDefault(id);
        public IReadOnlyList<ActividadProyecto> ListarPorProyecto(ProyectoId id) => _datos.Values.Where(x => x.ProyectoId == id).ToArray();
        public void Guardar(ActividadProyecto a, int? version) { Guardados++; _datos[a.Id] = ActividadProyecto.Rehidratar(a.Id, a.ProyectoId, a.GrupoId, a.Titulo, a.Descripcion, a.FechaRealizacion, a.ObservacionesGenerales, a.Estado, (version ?? 0) + 1, a.Entregas.Select(x => new DatosEntregaActividadRehidratada(x.EstudianteId, x.EstadoEntrega, x.NivelLogro, x.Observacion)).ToArray()); }
        public void Eliminar(ActividadId id, int version) => _datos.Remove(id);
    }
}