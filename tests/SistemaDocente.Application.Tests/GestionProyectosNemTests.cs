using SistemaDocente.Core;

namespace SistemaDocente.Application.Tests;

public sealed class GestionProyectosNemTests
{
    [Fact]
    public void PrepararActividadFiltraActivosPorGradoObjetivo()
    {
        var contexto = new ContextoPrueba();
        contexto.Grupo.AgregarEstudiante("Ana", 1, grado: GradoPrimaria.Primero);
        contexto.Grupo.AgregarEstudiante("Beto", 2, grado: GradoPrimaria.Segundo);
        var inactiva = contexto.Grupo.AgregarEstudiante("Carla", 3, grado: GradoPrimaria.Segundo);
        contexto.Grupo.AgregarEstudiante("Diego", 4, grado: GradoPrimaria.Tercero);
        contexto.Grupo.DesactivarEstudiante(inactiva.Id);

        var proyecto = contexto.CasosUso.CrearProyecto(
            contexto.Grupo.Id,
            new(
                "Comunidad",
                "",
                new DateOnly(2026, 9, 1),
                new DateOnly(2026, 9, 30),
                "",
                MetodologiaProyectoNem.ProyectosComunitarios,
                [GradoPrimaria.Primero, GradoPrimaria.Segundo]));

        var actividad = contexto.CasosUso.PrepararNuevaActividad(
            proyecto.ProyectoId,
            "Texto colectivo",
            "",
            new DateOnly(2026, 9, 10),
            "",
            CampoFormativoNem.Lenguajes,
            [GradoPrimaria.Segundo]);

        var entrega = Assert.Single(actividad.Entregas);
        Assert.Equal("Beto", entrega.NombreVisible);
        Assert.Equal(CampoFormativoNem.Lenguajes, actividad.CampoFormativo);
        Assert.Equal([GradoPrimaria.Segundo], actividad.GradosObjetivo);
    }

    [Fact]
    public void RechazaGradoDeActividadFueraDelProyecto()
    {
        var contexto = new ContextoPrueba();
        contexto.Grupo.AgregarEstudiante("Ana", 1, grado: GradoPrimaria.Primero);
        contexto.Grupo.AgregarEstudiante("Beto", 2, grado: GradoPrimaria.Tercero);
        var proyecto = contexto.CasosUso.CrearProyecto(
            contexto.Grupo.Id,
            new(
                "Proyecto",
                "",
                new DateOnly(2026, 9, 1),
                new DateOnly(2026, 9, 30),
                "",
                MetodologiaProyectoNem.AprendizajeBasadoEnProblemas,
                [GradoPrimaria.Primero]));

        Assert.Throws<DomainValidationException>(() =>
            contexto.CasosUso.PrepararNuevaActividad(
                proyecto.ProyectoId,
                "Actividad",
                "",
                new DateOnly(2026, 9, 10),
                "",
                CampoFormativoNem.EticaNaturalezaSociedades,
                [GradoPrimaria.Tercero]));
    }

    [Fact]
    public void FlujoLegacySinGradosSigueUsandoTodosLosActivos()
    {
        var contexto = new ContextoPrueba();
        contexto.Grupo.AgregarEstudiante("Ana", 1, grado: GradoPrimaria.Primero);
        contexto.Grupo.AgregarEstudiante("Beto", 2, grado: GradoPrimaria.Sexto);
        var proyecto = contexto.CasosUso.CrearProyecto(
            contexto.Grupo.Id,
            new(
                "Legacy",
                "",
                new DateOnly(2026, 9, 1),
                new DateOnly(2026, 9, 30),
                ""));

        var actividad = contexto.CasosUso.PrepararNuevaActividad(
            proyecto.ProyectoId,
            "Actividad legacy",
            "",
            new DateOnly(2026, 9, 8),
            "");

        Assert.Equal(2, actividad.Entregas.Count);
        Assert.Equal(CampoFormativoNem.NoEspecificado, actividad.CampoFormativo);
        Assert.Empty(actividad.GradosObjetivo ?? []);
    }

    private sealed class ContextoPrueba
    {
        internal ContextoPrueba()
        {
            Grupo = Grupo.Crear("Multigrado");
            Grupos.Grupo = Grupo;
            CasosUso = new(Grupos, Proyectos, Actividades);
        }

        internal Grupo Grupo { get; }
        internal AlmacenamientoGruposDoble Grupos { get; } = new();
        internal AlmacenamientoProyectosDoble Proyectos { get; } = new();
        internal AlmacenamientoActividadesDoble Actividades { get; } = new();
        internal GestionProyectosActividadesCasosUso CasosUso { get; }
    }

    private sealed class AlmacenamientoGruposDoble : IAlmacenamientoGrupos
    {
        internal Grupo? Grupo { get; set; }
        public Grupo? Cargar(GrupoId grupoId) => Grupo?.Id == grupoId ? Grupo : null;
        public bool Existe(GrupoId grupoId) => Grupo?.Id == grupoId;
        public void Guardar(Grupo grupo) => Grupo = grupo;
        public IReadOnlyList<Grupo> ListarTodos() => Grupo is null ? [] : [Grupo];
    }

    private sealed class AlmacenamientoProyectosDoble : IAlmacenamientoProyectos
    {
        private readonly Dictionary<ProyectoId, ProyectoDidactico> _proyectos = [];

        public ProyectoDidactico? Cargar(ProyectoId proyectoId) =>
            _proyectos.GetValueOrDefault(proyectoId);

        public IReadOnlyList<ProyectoDidactico> ListarPorGrupo(GrupoId grupoId) =>
            _proyectos.Values.Where(x => x.GrupoId == grupoId).ToArray();

        public void Guardar(ProyectoDidactico proyecto, int? versionEsperada) =>
            _proyectos[proyecto.Id] = proyecto;

        public IReadOnlyList<DateOnly> FechasActividadesFueraDeRango(
            ProyectoId proyectoId,
            DateOnly inicio,
            DateOnly termino) => [];

        public bool TieneActividades(ProyectoId proyectoId) => false;

        public void Eliminar(ProyectoId proyectoId, int versionEsperada) =>
            _proyectos.Remove(proyectoId);
    }

    private sealed class AlmacenamientoActividadesDoble : IAlmacenamientoActividadesProyecto
    {
        private readonly Dictionary<ActividadId, ActividadProyecto> _actividades = [];

        public ActividadProyecto? Cargar(ActividadId actividadId) =>
            _actividades.GetValueOrDefault(actividadId);

        public IReadOnlyList<ActividadProyecto> ListarPorProyecto(ProyectoId proyectoId) =>
            _actividades.Values.Where(x => x.ProyectoId == proyectoId).ToArray();

        public void Guardar(ActividadProyecto actividad, int? versionEsperada) =>
            _actividades[actividad.Id] = actividad;

        public void Eliminar(ActividadId actividadId, int versionEsperada) =>
            _actividades.Remove(actividadId);
    }
}