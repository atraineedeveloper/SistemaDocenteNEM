using SistemaDocente.Application;
using SistemaDocente.Core;
using SistemaDocente.Presentation;

namespace SistemaDocente.Presentation.Tests;

public sealed class GestionExpedienteViewModelTests
{
    [Fact]
    public void CargarYAgregarCompromisosActualizaFichaYSanitizaExcepciones()
    {
        var stubCasosUso = new StubGestionExpedienteCasosUso();
        var stubMensajes = new StubServicioMensajes();
        var vm = new GestionExpedienteViewModel(stubCasosUso, stubMensajes);

        var grupoId = GrupoId.DesdeGuid(Guid.NewGuid());
        var grupo = Grupo.Crear("3ro A");
        var est = grupo.AgregarEstudiante("Estudiante Prueba", 1);
        stubCasosUso.Grupo = grupo;

        vm.Cargar(grupoId, est.Id);

        Assert.NotNull(vm.Expediente);
        Assert.Equal("Estudiante Prueba", vm.Expediente.NombreEstudiante);

        vm.NuevaFortaleza = "Participación constante";
        vm.AgregarFortalezaCommand.Execute(null);

        Assert.Equal("", vm.NuevaFortaleza);
        Assert.Single(vm.Expediente.Fortalezas);
    }

    [Fact]
    public void IntentarAgregarTerminoClinicoMuestraErrorYConservaEstado()
    {
        var stubCasosUso = new StubGestionExpedienteCasosUso();
        var stubMensajes = new StubServicioMensajes();
        var vm = new GestionExpedienteViewModel(stubCasosUso, stubMensajes);

        var grupoId = GrupoId.DesdeGuid(Guid.NewGuid());
        var grupo = Grupo.Crear("3ro A");
        var est = grupo.AgregarEstudiante("Estudiante Prueba", 1);
        stubCasosUso.Grupo = grupo;

        vm.Cargar(grupoId, est.Id);

        vm.NuevaDificultad = "Diagnóstico de TDAH";
        vm.AgregarDificultadCommand.Execute(null);

        Assert.NotNull(stubMensajes.UltimoMensajeError);
        Assert.Contains("términos de carácter médico o clínico", stubMensajes.UltimoMensajeError);
    }

    private sealed class StubGestionExpedienteCasosUso
    {
        private readonly List<NotaPedagogica> _notas = [];
        private readonly List<AcuerdoTutor> _acuerdos = [];
        public Grupo? Grupo { get; set; }

        public static implicit operator GestionExpedienteCasosUso(StubGestionExpedienteCasosUso s)
        {
            return new GestionExpedienteCasosUso(
                new StubGrupos(s), new StubAsistencias(), new StubProyectos(), new StubActividades(), new StubExpedientes(s._notas, s._acuerdos));
        }
    }

    private sealed class StubServicioMensajes : IServicioMensajes
    {
        public string? UltimoMensajeError { get; private set; }
        public string? UltimoMensajeInformacion { get; private set; }
        public void MostrarInformacion(string mensaje) => UltimoMensajeInformacion = mensaje;
        public void MostrarError(string mensaje) => UltimoMensajeError = mensaje;
    }

    private sealed class StubGrupos(StubGestionExpedienteCasosUso parent) : IAlmacenamientoGrupos
    {
        public Grupo? Cargar(GrupoId grupoId) => parent.Grupo;
        public bool Existe(GrupoId grupoId) => true;
        public void Guardar(Grupo grupo) { }
    }

    private sealed class StubAsistencias : IAlmacenamientoAsistencias
    {
        public AsistenciaDiaria? Cargar(GrupoId grupoId, DateOnly fecha) => null;
        public bool Existe(GrupoId grupoId, DateOnly fecha) => false;
        public IReadOnlyList<AsistenciaDiaria> CargarIntervalo(GrupoId grupoId, DateOnly desde, DateOnly hasta) => [];
        public void Guardar(AsistenciaDiaria asistencia) { }
    }

    private sealed class StubProyectos : IAlmacenamientoProyectos
    {
        public ProyectoDidactico? Cargar(ProyectoId proyectoId) => null;
        public IReadOnlyList<ProyectoDidactico> ListarPorGrupo(GrupoId grupoId) => [];
        public IReadOnlyList<DateOnly> FechasActividadesFueraDeRango(ProyectoId id, DateOnly inicio, DateOnly termino) => [];
        public bool TieneActividades(ProyectoId id) => false;
        public void Guardar(ProyectoDidactico proyecto, int? versionEsperada) { }
        public void Eliminar(ProyectoId proyectoId, int versionEsperada) { }
    }

    private sealed class StubActividades : IAlmacenamientoActividadesProyecto
    {
        public ActividadProyecto? Cargar(ActividadId actividadId) => null;
        public IReadOnlyList<ActividadProyecto> ListarPorProyecto(ProyectoId proyectoId) => [];
        public void Guardar(ActividadProyecto actividad, int? versionEsperada) { }
        public void Eliminar(ActividadId actividadId, int versionEsperada) { }
    }

    private sealed class StubExpedientes(List<NotaPedagogica> notas, List<AcuerdoTutor> acuerdos) : IAlmacenamientoExpedientes
    {
        public ExpedienteEstudiante ObtenerExpediente(EstudianteId estudianteId, GrupoId grupoId)
            => new(estudianteId, grupoId, notas, acuerdos);

        public void RegistrarNotaPedagogica(EstudianteId estudianteId, GrupoId grupoId, TipoNotaPedagogica tipo, string contenido, DateTime fechaHora)
            => notas.Add(new NotaPedagogica(Guid.NewGuid(), tipo, contenido, fechaHora));

        public void RegistrarAcuerdoTutor(EstudianteId estudianteId, GrupoId grupoId, string motivo, string acuerdo, DateOnly fechaReunion, DateOnly? fechaSeguimiento)
            => acuerdos.Add(new AcuerdoTutor(Guid.NewGuid(), motivo, acuerdo, fechaReunion, fechaSeguimiento));
    }
}
