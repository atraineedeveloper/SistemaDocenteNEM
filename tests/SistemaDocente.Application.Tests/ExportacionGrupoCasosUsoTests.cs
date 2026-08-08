using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Application.Tests;

public sealed class ExportacionGrupoCasosUsoTests
{
    [Fact]
    public void PrepararXlsxProyectaContextoAlumnosYAsistenciaSinDatosSensiblesPorDefecto()
    {
        var grupo = CrearGrupoCuarto();
        var estudiante = grupo.Estudiantes.Single();
        var contexto = ContextoGrupo.Crear(
            grupo.Id,
            cicloEscolar: "2026-2027",
            nombreEscuela: "Primaria Demo",
            grupo: "A",
            organizacionEscolar: OrganizacionEscolar.Completa,
            gradosAtendidos: [GradoPrimaria.Cuarto]);
        var asistencia = AsistenciaDiaria.Crear(
            grupo.Id,
            new DateOnly(2026, 8, 8),
            [new EstadoEstudianteAsistencia(estudiante.Id, EstadoAsistencia.Presente)]);
        var exportador = new ExportadorPrueba();
        var casosUso = CrearCasosUso(grupo, contexto, [asistencia], exportador);
        var solicitud = new SolicitudExportacionGrupo(
            grupo.Id,
            FormatoExportacionTabular.Xlsx,
            [
                ConjuntoExportacionGrupo.Contexto,
                ConjuntoExportacionGrupo.Alumnos,
                ConjuntoExportacionGrupo.Asistencia,
            ],
            new DateOnly(2026, 8, 1),
            new DateOnly(2026, 8, 31));

        var plan = casosUso.Preparar(solicitud, new DateOnly(2026, 8, 8));

        Assert.Equal("Grupo_4A_2026-2027_2026-08-08.xlsx", plan.NombreArchivoSugerido);
        Assert.False(plan.ContieneDatosSensibles);
        Assert.Equal(["Contexto", "Alumnos", "Asistencia"], plan.Documento.Hojas.Select(x => x.Nombre));
        Assert.Equal(18, plan.Documento.Hojas[0].Filas.Count);
        Assert.Single(plan.Documento.Hojas[1].Filas);
        Assert.Single(plan.Documento.Hojas[2].Filas);
        Assert.DoesNotContain(
            plan.Documento.Hojas[1].Columnas,
            columna => columna.Encabezado == "Observaciones pedagógicas");
        Assert.Equal("Presente", plan.Documento.Hojas[2].Filas[0].Celdas[4].Texto);
        Assert.Equal(0, exportador.Llamadas);
    }

    [Fact]
    public void CsvExigeUnSoloConjunto()
    {
        var grupo = CrearGrupoCuarto();
        var casosUso = CrearCasosUso(
            grupo,
            ContextoGrupo.Crear(grupo.Id, gradosAtendidos: [GradoPrimaria.Cuarto]),
            [],
            new ExportadorPrueba());
        var solicitud = new SolicitudExportacionGrupo(
            grupo.Id,
            FormatoExportacionTabular.Csv,
            [ConjuntoExportacionGrupo.Alumnos, ConjuntoExportacionGrupo.Contexto]);

        var exception = Assert.Throws<DomainValidationException>(() =>
            casosUso.Preparar(solicitud, new DateOnly(2026, 8, 8)));

        Assert.Contains("exactamente un conjunto", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AsistenciaExigePeriodoVisible()
    {
        var grupo = CrearGrupoCuarto();
        var casosUso = CrearCasosUso(
            grupo,
            ContextoGrupo.Crear(grupo.Id, gradosAtendidos: [GradoPrimaria.Cuarto]),
            [],
            new ExportadorPrueba());
        var solicitud = new SolicitudExportacionGrupo(
            grupo.Id,
            FormatoExportacionTabular.Xlsx,
            [ConjuntoExportacionGrupo.Asistencia]);

        var exception = Assert.Throws<DomainValidationException>(() =>
            casosUso.Preparar(solicitud, new DateOnly(2026, 8, 8)));

        Assert.Contains("periodo", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ObservacionesYSeguimientoSonOptInYSensibles()
    {
        var grupo = CrearGrupoCuarto();
        var contexto = ContextoGrupo.Crear(grupo.Id, gradosAtendidos: [GradoPrimaria.Cuarto]);
        var expedientes = new AlmacenamientoExpedientesPrueba(grupo);
        var estudiante = grupo.Estudiantes.Single();
        expedientes.Nota = new NotaPedagogica(
            Guid.NewGuid(),
            TipoNotaPedagogica.Fortaleza,
            "Explica sus procedimientos con claridad.",
            new DateTime(2026, 8, 8, 10, 0, 0));
        var exportador = new ExportadorPrueba();
        var casosUso = new ExportacionGrupoCasosUso(
            new AlmacenamientoGruposExportacionPrueba(grupo),
            new AlmacenamientoAsistenciasExportacionPrueba([]),
            new AlmacenamientoProyectosExportacionPrueba(),
            new AlmacenamientoActividadesExportacionPrueba(),
            expedientes,
            new AlmacenamientoContextoExportacionPrueba(contexto),
            exportador);
        var solicitud = new SolicitudExportacionGrupo(
            grupo.Id,
            FormatoExportacionTabular.Xlsx,
            [ConjuntoExportacionGrupo.Alumnos, ConjuntoExportacionGrupo.Seguimiento],
            IncluirObservacionesEstudiante: true);

        var plan = casosUso.Preparar(solicitud, new DateOnly(2026, 8, 8));

        Assert.True(plan.ContieneDatosSensibles);
        Assert.Contains(
            plan.Documento.Hojas.Single(x => x.Nombre == "Alumnos").Columnas,
            columna => columna.Encabezado == "Observaciones pedagógicas");
        var seguimiento = plan.Documento.Hojas.Single(x => x.Nombre == "Seguimiento");
        Assert.Single(seguimiento.Filas);
        Assert.Equal("Fortaleza", seguimiento.Filas[0].Celdas[3].Texto);
        Assert.Equal((decimal)estudiante.NumeroLista, seguimiento.Filas[0].Celdas[0].Numero);
    }

    [Fact]
    public void NombreMultigradoUsaGradosEstructurados()
    {
        var grupo = Grupo.Crear("Nombre libre que no debe parsearse");
        var contexto = ContextoGrupo.Crear(
            grupo.Id,
            cicloEscolar: "2026-2027",
            gradosAtendidos: [GradoPrimaria.Primero, GradoPrimaria.Segundo, GradoPrimaria.Tercero]);
        var casosUso = CrearCasosUso(grupo, contexto, [], new ExportadorPrueba());
        var solicitud = new SolicitudExportacionGrupo(
            grupo.Id,
            FormatoExportacionTabular.Xlsx,
            [ConjuntoExportacionGrupo.Contexto]);

        var plan = casosUso.Preparar(solicitud, new DateOnly(2026, 8, 8));

        Assert.Equal("Grupo_Multigrado_1-2-3_2026-2027_2026-08-08.xlsx", plan.NombreArchivoSugerido);
        Assert.Contains(
            plan.Documento.Hojas[0].Filas,
            fila => fila.Celdas[0].Texto == "Modalidad" && fila.Celdas[1].Texto == "Multigrado");
        Assert.Contains(
            plan.Documento.Hojas[0].Filas,
            fila => fila.Celdas[0].Texto == "Fase(s) NEM" && fila.Celdas[1].Texto == "Fase 3 · Fase 4");
    }

    [Fact]
    public void ExportarPublicaSoloPorElPuertoYDevuelveRutaCompleta()
    {
        var grupo = CrearGrupoCuarto();
        var exportador = new ExportadorPrueba();
        var casosUso = CrearCasosUso(
            grupo,
            ContextoGrupo.Crear(grupo.Id, gradosAtendidos: [GradoPrimaria.Cuarto]),
            [],
            exportador);
        var plan = casosUso.Preparar(
            new SolicitudExportacionGrupo(
                grupo.Id,
                FormatoExportacionTabular.Csv,
                [ConjuntoExportacionGrupo.Alumnos]),
            new DateOnly(2026, 8, 8));
        var ruta = Path.Combine(Path.GetTempPath(), "grupo-export.csv");

        var resultado = casosUso.Exportar(plan, ruta);

        Assert.Equal(1, exportador.Llamadas);
        Assert.Same(plan.Documento, exportador.Documento);
        Assert.Equal(FormatoExportacionTabular.Csv, exportador.Formato);
        Assert.Equal(Path.GetFullPath(ruta), resultado.RutaArchivo);
        Assert.Equal(Path.GetFullPath(ruta), exportador.Ruta);
    }

    private static Grupo CrearGrupoCuarto()
    {
        var grupo = Grupo.Crear("4.º A");
        grupo.AgregarEstudiante(
            "López Ruiz Ana María",
            1,
            "López",
            "Ruiz",
            "Ana María",
            new DateOnly(2016, 4, 15),
            GeneroEstudiante.Mujer,
            new DateOnly(2026, 7, 1),
            "Observación sensible de prueba.",
            GradoPrimaria.Cuarto);
        return grupo;
    }

    private static ExportacionGrupoCasosUso CrearCasosUso(
        Grupo grupo,
        ContextoGrupo contexto,
        IReadOnlyList<AsistenciaDiaria> asistencias,
        ExportadorPrueba exportador) =>
        new(
            new AlmacenamientoGruposExportacionPrueba(grupo),
            new AlmacenamientoAsistenciasExportacionPrueba(asistencias),
            new AlmacenamientoProyectosExportacionPrueba(),
            new AlmacenamientoActividadesExportacionPrueba(),
            new AlmacenamientoExpedientesPrueba(grupo),
            new AlmacenamientoContextoExportacionPrueba(contexto),
            exportador);

    private sealed class ExportadorPrueba : IExportadorTabular
    {
        public int Llamadas { get; private set; }
        public DocumentoTabularSalida? Documento { get; private set; }
        public string? Ruta { get; private set; }
        public FormatoExportacionTabular Formato { get; private set; }

        public void Exportar(
            DocumentoTabularSalida documento,
            string rutaArchivo,
            FormatoExportacionTabular formato)
        {
            Llamadas++;
            Documento = documento;
            Ruta = Path.GetFullPath(rutaArchivo);
            Formato = formato;
        }
    }

    private sealed class AlmacenamientoGruposExportacionPrueba(Grupo grupo) : IAlmacenamientoGrupos
    {
        public Grupo? Cargar(GrupoId grupoId) => grupo.Id == grupoId ? grupo : null;
        public bool Existe(GrupoId grupoId) => grupo.Id == grupoId;
        public void Guardar(Grupo grupoAGuardar) => throw new InvalidOperationException("Export must not persist the group.");
        public IReadOnlyList<Grupo> ListarTodos() => [grupo];
    }

    private sealed class AlmacenamientoAsistenciasExportacionPrueba(
        IReadOnlyList<AsistenciaDiaria> asistencias) : IAlmacenamientoAsistencias
    {
        public AsistenciaDiaria? Cargar(GrupoId grupoId, DateOnly fecha) =>
            asistencias.FirstOrDefault(x => x.GrupoId == grupoId && x.Fecha == fecha);

        public bool Existe(GrupoId grupoId, DateOnly fecha) => Cargar(grupoId, fecha) is not null;

        public IReadOnlyList<AsistenciaDiaria> CargarIntervalo(GrupoId grupoId, DateOnly desde, DateOnly hasta) =>
            asistencias.Where(x => x.GrupoId == grupoId && x.Fecha >= desde && x.Fecha <= hasta).ToArray();

        public void Guardar(AsistenciaDiaria asistencia) => throw new InvalidOperationException("Export must not persist attendance.");
    }

    private sealed class AlmacenamientoProyectosExportacionPrueba : IAlmacenamientoProyectos
    {
        public ProyectoDidactico? Cargar(ProyectoId proyectoId) => null;
        public IReadOnlyList<ProyectoDidactico> ListarPorGrupo(GrupoId grupoId) => [];
        public void Guardar(ProyectoDidactico proyecto, int? versionEsperada) => throw new InvalidOperationException();
        public IReadOnlyList<DateOnly> FechasActividadesFueraDeRango(ProyectoId proyectoId, DateOnly inicio, DateOnly termino) => [];
        public bool TieneActividades(ProyectoId proyectoId) => false;
        public void Eliminar(ProyectoId proyectoId, int versionEsperada) => throw new InvalidOperationException();
    }

    private sealed class AlmacenamientoActividadesExportacionPrueba : IAlmacenamientoActividadesProyecto
    {
        public ActividadProyecto? Cargar(ActividadId actividadId) => null;
        public IReadOnlyList<ActividadProyecto> ListarPorProyecto(ProyectoId proyectoId) => [];
        public void Guardar(ActividadProyecto actividad, int? versionEsperada) => throw new InvalidOperationException();
        public void Eliminar(ActividadId actividadId, int versionEsperada) => throw new InvalidOperationException();
    }

    private sealed class AlmacenamientoExpedientesPrueba(Grupo grupo) : IAlmacenamientoExpedientes
    {
        public Grupo GrupoOrigen { get; } = grupo;
        public NotaPedagogica? Nota { get; set; }

        public ExpedienteEstudiante ObtenerExpediente(EstudianteId estudianteId, GrupoId grupoId) =>
            new(
                estudianteId,
                grupoId,
                Nota is null ? [] : [Nota],
                []);

        public void RegistrarNotaPedagogica(EstudianteId estudianteId, GrupoId grupoId, TipoNotaPedagogica tipo, string contenido, DateTime fechaHora) =>
            throw new InvalidOperationException("Export must not persist follow-up data.");

        public void RegistrarAcuerdoTutor(EstudianteId estudianteId, GrupoId grupoId, string motivo, string acuerdo, DateOnly fechaReunion, DateOnly? fechaSeguimiento) =>
            throw new InvalidOperationException("Export must not persist follow-up data.");
    }

    private sealed class AlmacenamientoContextoExportacionPrueba(ContextoGrupo contexto) : IAlmacenamientoContextoGrupo
    {
        public ContextoGrupo? Cargar(GrupoId grupoId) => contexto.GrupoId == grupoId ? contexto : null;
        public void Guardar(ContextoGrupo contextoAGuardar) => throw new InvalidOperationException("Export must not persist context.");
    }
}