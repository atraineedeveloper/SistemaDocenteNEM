using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Presentation.Tests;

public sealed class ExportacionGrupoViewModelTests
{
    [Fact]
    public void InicializarUsaXlsxCompletoYSensibleDesactivado()
    {
        var entorno = CrearEntorno();
        var viewModel = entorno.ViewModel;

        viewModel.Inicializar(entorno.Grupo.Id, new DateOnly(2026, 8, 8));

        Assert.Equal(PasoExportacionGrupo.Contenido, viewModel.PasoActual);
        Assert.True(viewModel.EsXlsx);
        Assert.True(viewModel.IncluirContexto);
        Assert.True(viewModel.IncluirAlumnos);
        Assert.True(viewModel.IncluirAsistencia);
        Assert.True(viewModel.IncluirProyectos);
        Assert.True(viewModel.IncluirActividades);
        Assert.True(viewModel.IncluirEvaluacion);
        Assert.False(viewModel.IncluirSeguimiento);
        Assert.False(viewModel.IncluirObservacionesEstudiante);
        Assert.False(viewModel.IncluirObservacionesEvaluacion);
        Assert.False(viewModel.ContieneSeleccionSensible);
        Assert.Equal(new DateTime(2026, 8, 1), viewModel.AsistenciaDesde);
        Assert.Equal(new DateTime(2026, 8, 31), viewModel.AsistenciaHasta);
        Assert.True(viewModel.MostrarPeriodoAsistencia);
        Assert.True(viewModel.MostrarAlcanceProyecto);
        Assert.Equal(2, viewModel.Proyectos.Count);
        Assert.Equal("Todos los proyectos", viewModel.ProyectoSeleccionado?.Etiqueta);
        Assert.True(viewModel.SiguienteCommand.CanExecute(null));
    }

    [Fact]
    public void CsvSeguimientoEsUnicoYSensibleYNoExigePeriodoAsistencia()
    {
        var entorno = CrearEntorno();
        var viewModel = entorno.ViewModel;
        viewModel.Inicializar(entorno.Grupo.Id, new DateOnly(2026, 8, 8));

        viewModel.Formato = FormatoExportacionTabular.Csv;
        viewModel.ConjuntoCsv = ConjuntoExportacionGrupo.Seguimiento;

        Assert.True(viewModel.EsCsv);
        Assert.True(viewModel.ContieneSeleccionSensible);
        Assert.NotEmpty(viewModel.AdvertenciaPrivacidad);
        Assert.False(viewModel.MostrarPeriodoAsistencia);
        Assert.False(viewModel.MostrarAlcanceProyecto);

        viewModel.SiguienteCommand.Execute(null);
        Assert.Equal(PasoExportacionGrupo.Alcance, viewModel.PasoActual);
        viewModel.SiguienteCommand.Execute(null);

        Assert.Equal(PasoExportacionGrupo.Archivo, viewModel.PasoActual);
        Assert.StartsWith("Seguimiento_4A_2026-2027_2026-08-08", viewModel.NombreArchivoSugerido, StringComparison.Ordinal);
        Assert.Contains("Seguimiento", viewModel.ResumenPlan, StringComparison.Ordinal);
    }

    [Fact]
    public void XlsxSinConjuntosNoPermiteAvanzar()
    {
        var entorno = CrearEntorno();
        var viewModel = entorno.ViewModel;
        viewModel.Inicializar(entorno.Grupo.Id, new DateOnly(2026, 8, 8));

        viewModel.IncluirContexto = false;
        viewModel.IncluirAlumnos = false;
        viewModel.IncluirAsistencia = false;
        viewModel.IncluirProyectos = false;
        viewModel.IncluirActividades = false;
        viewModel.IncluirEvaluacion = false;
        viewModel.IncluirSeguimiento = false;

        Assert.False(viewModel.SiguienteCommand.CanExecute(null));
    }

    [Fact]
    public void ResultadoApareceSoloDespuesDePublicacionExitosa()
    {
        var entorno = CrearEntorno();
        var viewModel = entorno.ViewModel;
        viewModel.Inicializar(entorno.Grupo.Id, new DateOnly(2026, 8, 8));
        viewModel.Formato = FormatoExportacionTabular.Csv;
        viewModel.ConjuntoCsv = ConjuntoExportacionGrupo.Alumnos;
        viewModel.SiguienteCommand.Execute(null);
        viewModel.SiguienteCommand.Execute(null);
        var ruta = Path.Combine(Path.GetTempPath(), "exportacion-presentacion.csv");

        var exportado = viewModel.ExportarA(ruta);

        Assert.True(exportado);
        Assert.Equal(PasoExportacionGrupo.Resultado, viewModel.PasoActual);
        Assert.Equal(Path.GetFullPath(ruta), viewModel.RutaResultado);
        Assert.Equal(1, entorno.Exportador.Llamadas);
    }

    private static Entorno CrearEntorno()
    {
        var grupo = Grupo.Crear("4.º A");
        grupo.AgregarEstudiante(
            "Ana López Ruiz",
            1,
            "López",
            "Ruiz",
            "Ana",
            new DateOnly(2016, 4, 15),
            GeneroEstudiante.Mujer,
            new DateOnly(2026, 7, 1),
            "",
            GradoPrimaria.Cuarto);
        var contexto = ContextoGrupo.Crear(
            grupo.Id,
            cicloEscolar: "2026-2027",
            grupo: "A",
            gradosAtendidos: [GradoPrimaria.Cuarto]);
        var proyecto = ProyectoDidactico.Crear(
            grupo.Id,
            "Proyecto de prueba",
            "",
            new DateOnly(2026, 8, 3),
            new DateOnly(2026, 8, 28),
            "",
            MetodologiaProyectoNem.ProyectosComunitarios,
            [GradoPrimaria.Cuarto]);
        var exportador = new ExportadorPrueba();
        var proyectos = new ProyectosPrueba([proyecto]);
        var casosUso = new ExportacionGrupoCasosUso(
            new GruposPrueba(grupo),
            new AsistenciasPrueba(),
            proyectos,
            new ActividadesPrueba(),
            new ExpedientesPrueba(),
            new ContextoPrueba(contexto),
            exportador);
        var consulta = new ConsultaExportacionGrupoCasosUso(proyectos);
        return new Entorno(grupo, exportador, new ExportacionGrupoViewModel(casosUso, consulta));
    }

    private sealed record Entorno(
        Grupo Grupo,
        ExportadorPrueba Exportador,
        ExportacionGrupoViewModel ViewModel);

    private sealed class ExportadorPrueba : IExportadorTabular
    {
        public int Llamadas { get; private set; }

        public void Exportar(DocumentoTabularSalida documento, string rutaArchivo, FormatoExportacionTabular formato)
        {
            Llamadas++;
        }
    }

    private sealed class GruposPrueba(Grupo grupo) : IAlmacenamientoGrupos
    {
        public Grupo? Cargar(GrupoId grupoId) => grupoId == grupo.Id ? grupo : null;
        public bool Existe(GrupoId grupoId) => grupoId == grupo.Id;
        public void Guardar(Grupo grupoAGuardar) => throw new InvalidOperationException();
        public IReadOnlyList<Grupo> ListarTodos() => [grupo];
    }

    private sealed class AsistenciasPrueba : IAlmacenamientoAsistencias
    {
        public AsistenciaDiaria? Cargar(GrupoId grupoId, DateOnly fecha) => null;
        public bool Existe(GrupoId grupoId, DateOnly fecha) => false;
        public IReadOnlyList<AsistenciaDiaria> CargarIntervalo(GrupoId grupoId, DateOnly desde, DateOnly hasta) => [];
        public void Guardar(AsistenciaDiaria asistencia) => throw new InvalidOperationException();
    }

    private sealed class ProyectosPrueba(IReadOnlyList<ProyectoDidactico> proyectos) : IAlmacenamientoProyectos
    {
        public ProyectoDidactico? Cargar(ProyectoId proyectoId) => proyectos.FirstOrDefault(x => x.Id == proyectoId);
        public IReadOnlyList<ProyectoDidactico> ListarPorGrupo(GrupoId grupoId) => proyectos.Where(x => x.GrupoId == grupoId).ToArray();
        public void Guardar(ProyectoDidactico proyecto, int? versionEsperada) => throw new InvalidOperationException();
        public IReadOnlyList<DateOnly> FechasActividadesFueraDeRango(ProyectoId proyectoId, DateOnly inicio, DateOnly termino) => [];
        public bool TieneActividades(ProyectoId proyectoId) => false;
        public void Eliminar(ProyectoId proyectoId, int versionEsperada) => throw new InvalidOperationException();
    }

    private sealed class ActividadesPrueba : IAlmacenamientoActividadesProyecto
    {
        public ActividadProyecto? Cargar(ActividadId actividadId) => null;
        public IReadOnlyList<ActividadProyecto> ListarPorProyecto(ProyectoId proyectoId) => [];
        public void Guardar(ActividadProyecto actividad, int? versionEsperada) => throw new InvalidOperationException();
        public void Eliminar(ActividadId actividadId, int versionEsperada) => throw new InvalidOperationException();
    }

    private sealed class ExpedientesPrueba : IAlmacenamientoExpedientes
    {
        public ExpedienteEstudiante ObtenerExpediente(EstudianteId estudianteId, GrupoId grupoId) =>
            new(estudianteId, grupoId, [], []);

        public void RegistrarNotaPedagogica(EstudianteId estudianteId, GrupoId grupoId, TipoNotaPedagogica tipo, string contenido, DateTime fechaHora) =>
            throw new InvalidOperationException();

        public void RegistrarAcuerdoTutor(EstudianteId estudianteId, GrupoId grupoId, string motivo, string acuerdo, DateOnly fechaReunion, DateOnly? fechaSeguimiento) =>
            throw new InvalidOperationException();
    }

    private sealed class ContextoPrueba(ContextoGrupo contexto) : IAlmacenamientoContextoGrupo
    {
        public ContextoGrupo? Cargar(GrupoId grupoId) => grupoId == contexto.GrupoId ? contexto : null;
        public void Guardar(ContextoGrupo contextoAGuardar) => throw new InvalidOperationException();
    }
}