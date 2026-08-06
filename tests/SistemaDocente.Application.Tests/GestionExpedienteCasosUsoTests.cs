using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Application.Tests;

public sealed class GestionExpedienteCasosUsoTests
{
    private readonly StubAlmacenamientoGrupos _grupos = new();
    private readonly StubAlmacenamientoAsistencias _asistencias = new();
    private readonly StubAlmacenamientoProyectos _proyectos = new();
    private readonly StubAlmacenamientoActividadesProyecto _actividades = new();
    private readonly StubAlmacenamientoExpedientes _expedientes = new();

    [Fact]
    public void ConsultarExpedienteConsolidaAsistenciaProyectosYAlertasFormativas()
    {
        var grupo = Grupo.Crear("3ro A");
        var estudiante = grupo.AgregarEstudiante("Juan Pérez", 1);
        _grupos.Guardar(grupo);

        var casosUso = new GestionExpedienteCasosUso(_grupos, _asistencias, _proyectos, _actividades, _expedientes);

        var detalle = casosUso.ConsultarExpediente(grupo.Id, estudiante.Id);

        Assert.Equal("Juan Pérez", detalle.NombreEstudiante);
        Assert.Equal(1, detalle.NumeroLista);
        Assert.True(detalle.EstaActivo);
        Assert.NotNull(detalle.Asistencia);
    }

    [Fact]
    public void RegistrarNotaPedagogicaRechazaTerminosClinicos()
    {
        var grupo = Grupo.Crear("3ro A");
        var estudiante = grupo.AgregarEstudiante("Juan Pérez", 1);
        _grupos.Guardar(grupo);

        var casosUso = new GestionExpedienteCasosUso(_grupos, _asistencias, _proyectos, _actividades, _expedientes);

        Assert.Throws<DomainValidationException>(() =>
            casosUso.RegistrarNotaPedagogica(grupo.Id, estudiante.Id, TipoNotaPedagogica.Dificultad, "Diagnóstico de trastorno"));
    }

    private sealed class StubAlmacenamientoGrupos : IAlmacenamientoGrupos
    {
        private readonly Dictionary<GrupoId, Grupo> _map = [];
        public Grupo? Cargar(GrupoId grupoId) => _map.GetValueOrDefault(grupoId);
        public bool Existe(GrupoId grupoId) => _map.ContainsKey(grupoId);
        public void Guardar(Grupo grupo) => _map[grupo.Id] = grupo;
    }

    private sealed class StubAlmacenamientoAsistencias : IAlmacenamientoAsistencias
    {
        public AsistenciaDiaria? Cargar(GrupoId grupoId, DateOnly fecha) => null;
        public bool Existe(GrupoId grupoId, DateOnly fecha) => false;
        public IReadOnlyList<AsistenciaDiaria> CargarIntervalo(GrupoId grupoId, DateOnly desde, DateOnly hasta) => [];
        public void Guardar(AsistenciaDiaria asistencia) { }
    }

    private sealed class StubAlmacenamientoProyectos : IAlmacenamientoProyectos
    {
        public ProyectoDidactico? Cargar(ProyectoId proyectoId) => null;
        public IReadOnlyList<ProyectoDidactico> ListarPorGrupo(GrupoId grupoId) => [];
        public IReadOnlyList<DateOnly> FechasActividadesFueraDeRango(ProyectoId id, DateOnly inicio, DateOnly termino) => [];
        public bool TieneActividades(ProyectoId id) => false;
        public void Guardar(ProyectoDidactico proyecto, int? versionEsperada) { }
        public void Eliminar(ProyectoId proyectoId, int versionEsperada) { }
    }

    private sealed class StubAlmacenamientoActividadesProyecto : IAlmacenamientoActividadesProyecto
    {
        public ActividadProyecto? Cargar(ActividadId actividadId) => null;
        public IReadOnlyList<ActividadProyecto> ListarPorProyecto(ProyectoId proyectoId) => [];
        public void Guardar(ActividadProyecto actividad, int? versionEsperada) { }
        public void Eliminar(ActividadId actividadId, int versionEsperada) { }
    }

    private sealed class StubAlmacenamientoExpedientes : IAlmacenamientoExpedientes
    {
        private readonly List<NotaPedagogica> _notas = [];
        private readonly List<AcuerdoTutor> _acuerdos = [];

        public ExpedienteEstudiante ObtenerExpediente(EstudianteId estudianteId, GrupoId grupoId)
            => new(estudianteId, grupoId, _notas, _acuerdos);

        public void RegistrarNotaPedagogica(EstudianteId estudianteId, GrupoId grupoId, TipoNotaPedagogica tipo, string contenido, DateTime fechaHora)
            => _notas.Add(new NotaPedagogica(Guid.NewGuid(), tipo, contenido, fechaHora));

        public void RegistrarAcuerdoTutor(EstudianteId estudianteId, GrupoId grupoId, string motivo, string acuerdo, DateOnly fechaReunion, DateOnly? fechaSeguimiento)
            => _acuerdos.Add(new AcuerdoTutor(Guid.NewGuid(), motivo, acuerdo, fechaReunion, fechaSeguimiento));
    }
}
