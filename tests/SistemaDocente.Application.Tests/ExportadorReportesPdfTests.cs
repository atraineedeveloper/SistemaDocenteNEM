using System.Text;

using SistemaDocente.Core;
using SistemaDocente.Interchange;
using SistemaDocente.Reporting;

namespace SistemaDocente.Application.Tests;

public sealed class ExportadorReportesPdfTests
{
    [Fact]
    public void ExportaReporteIndividualComoPdfReal()
    {
        if (!OperatingSystem.IsWindows()) return;
        var directorio = CrearDirectorioTemporal();
        try
        {
            var ruta = Path.Combine(directorio, "individual.pdf");
            new ExportadorReportesPdf().Exportar(CrearIndividual(), ruta);

            AssertPdfValido(ruta);
        }
        finally
        {
            Directory.Delete(directorio, true);
        }
    }

    [Fact]
    public void ExportaReporteGrupalComoPdfReal()
    {
        if (!OperatingSystem.IsWindows()) return;
        var directorio = CrearDirectorioTemporal();
        try
        {
            var ruta = Path.Combine(directorio, "grupal.pdf");
            new ExportadorReportesPdf().Exportar(CrearGrupal(), ruta);

            AssertPdfValido(ruta);
        }
        finally
        {
            Directory.Delete(directorio, true);
        }
    }

    private static void AssertPdfValido(string ruta)
    {
        Assert.True(File.Exists(ruta));
        var bytes = File.ReadAllBytes(ruta);
        Assert.True(bytes.Length > 1_000);
        Assert.Equal("%PDF-", Encoding.ASCII.GetString(bytes, 0, 5));
    }

    private static ReporteIndividualAlumno CrearIndividual()
    {
        var contexto = CrearContexto();
        return new ReporteIndividualAlumno(
            contexto,
            "4.º A",
            EstudianteId.DesdeGuid(Guid.Parse("11111111-1111-1111-1111-111111111111")),
            1,
            "Ana Pérez López",
            GeneroEstudiante.Mujer,
            9,
            true,
            92.5,
            [new MesAsistenciaReporte(2026, 8, 20, 17, 1, 1, 1, 90)],
            new ResumenCumplimientoReporte(4, 3, 1, 0, 75),
            new DistribucionLogroReporte(0, 1, 1, 1, 0),
            [
                new ActividadReporteFuente(
                    "Cuidemos el agua",
                    "Registro del consumo",
                    new DateOnly(2026, 8, 7),
                    EstadoEntregaActividad.Entregada,
                    NivelLogro.Domina,
                    "Explica los datos con claridad."),
            ],
            ["Explica sus ideas con claridad."],
            ["Fortalecer la argumentación escrita."],
            ["Organizador gráfico y modelado de ejemplo."],
            ["Mostró avances durante el proyecto."],
            ["Familia y docente acuerdan revisar el cuaderno semanalmente."]);
    }

    private static ReporteGrupal CrearGrupal()
    {
        var contexto = CrearContexto();
        return new ReporteGrupal(
            contexto,
            "4.º A",
            3,
            3,
            94.2,
            new ResumenCumplimientoReporte(12, 10, 1, 1, 1000d / 11d),
            new DistribucionLogroReporte(1, 3, 3, 2, 1),
            [new MesAsistenciaReporte(2026, 8, 60, 54, 2, 2, 2, 280d / 3d)],
            [
                new SeguimientoAlumnoReporte(
                    EstudianteId.DesdeGuid(Guid.Parse("11111111-1111-1111-1111-111111111111")),
                    1,
                    "Ana Pérez López",
                    true,
                    95,
                    new ResumenCumplimientoReporte(4, 4, 0, 0, 100),
                    0),
                new SeguimientoAlumnoReporte(
                    EstudianteId.DesdeGuid(Guid.Parse("22222222-2222-2222-2222-222222222222")),
                    2,
                    "Luis García Pérez",
                    true,
                    90,
                    new ResumenCumplimientoReporte(4, 3, 1, 0, 75),
                    1),
            ]);
    }

    private static ContextoGrupo CrearContexto() => ContextoGrupo.Crear(
        GrupoId.DesdeGuid(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")),
        cicloEscolar: "2026-2027",
        nombreEscuela: "Primaria Demo AulaRaíz",
        cct: "27DPR0000Z",
        entidadFederativa: "Tabasco",
        municipio: "Centro",
        localidad: "Villahermosa",
        grado: "4.º",
        grupo: "A",
        turno: "Matutino",
        docenteResponsable: "Docente Demo",
        gradosAtendidos: [GradoPrimaria.Cuarto]);

    private static string CrearDirectorioTemporal()
    {
        var ruta = Path.Combine(Path.GetTempPath(), "AulaRaiz-PdfTests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(ruta);
        return ruta;
    }
}