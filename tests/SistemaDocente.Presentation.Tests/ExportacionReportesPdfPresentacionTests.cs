using SistemaDocente.Application;
using SistemaDocente.Core;
using SistemaDocente.Reporting;

namespace SistemaDocente.Presentation.Tests;

public sealed class ExportacionReportesPdfPresentacionTests
{
    [Fact]
    public void IndividualUsaNombreSeguroYDespachaModeloIndividual()
    {
        var exportador = new ExportadorFalso();
        var presentacion = new ExportacionReportesPdfPresentacion(exportador);
        var reporte = CrearIndividual("Ana: Pérez/Prueba");

        var nombre = ExportacionReportesPdfPresentacion.CrearNombreArchivo(
            reporte,
            new DateOnly(2026, 8, 8));
        presentacion.Exportar(reporte, "individual.pdf");

        Assert.StartsWith("AulaRaiz_Reporte_Individual_1_", nombre, StringComparison.Ordinal);
        Assert.EndsWith("_2026-08-08.pdf", nombre, StringComparison.Ordinal);
        Assert.DoesNotContain(':', nombre);
        Assert.DoesNotContain('/', nombre);
        Assert.Same(reporte, exportador.Individual);
        Assert.Null(exportador.Grupal);
        Assert.Equal("individual.pdf", exportador.Ruta);
    }

    [Fact]
    public void GrupalDespachaModeloGrupalYNoConvierteElTipo()
    {
        var exportador = new ExportadorFalso();
        var presentacion = new ExportacionReportesPdfPresentacion(exportador);
        var reporte = CrearGrupal("Multigrado A/B");

        var nombre = ExportacionReportesPdfPresentacion.CrearNombreArchivo(
            reporte,
            new DateOnly(2026, 8, 8));
        presentacion.Exportar(reporte, "grupal.pdf");

        Assert.StartsWith("AulaRaiz_Reporte_Grupal_", nombre, StringComparison.Ordinal);
        Assert.DoesNotContain('/', nombre);
        Assert.Same(reporte, exportador.Grupal);
        Assert.Null(exportador.Individual);
        Assert.Equal("grupal.pdf", exportador.Ruta);
    }

    private static ReporteIndividualAlumno CrearIndividual(string nombre) => new(
        CrearContexto(),
        "4.º A",
        EstudianteId.DesdeGuid(Guid.Parse("11111111-1111-1111-1111-111111111111")),
        1,
        nombre,
        GeneroEstudiante.Mujer,
        9,
        true,
        null,
        [],
        new ResumenCumplimientoReporte(0, 0, 0, 0, null),
        new DistribucionLogroReporte(0, 0, 0, 0, 0),
        [],
        [],
        [],
        [],
        [],
        []);

    private static ReporteGrupal CrearGrupal(string nombreGrupo) => new(
        CrearContexto(),
        nombreGrupo,
        0,
        0,
        null,
        new ResumenCumplimientoReporte(0, 0, 0, 0, null),
        new DistribucionLogroReporte(0, 0, 0, 0, 0),
        [],
        []);

    private static ContextoGrupo CrearContexto() => ContextoGrupo.Crear(
        GrupoId.DesdeGuid(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")),
        cicloEscolar: "2026-2027",
        nombreEscuela: "Primaria Demo",
        gradosAtendidos: [GradoPrimaria.Cuarto]);

    private sealed class ExportadorFalso : IExportadorReportesPdf
    {
        public ReporteIndividualAlumno? Individual { get; private set; }
        public ReporteGrupal? Grupal { get; private set; }
        public string? Ruta { get; private set; }

        public void Exportar(ReporteIndividualAlumno reporte, string rutaArchivo)
        {
            Individual = reporte;
            Ruta = rutaArchivo;
        }

        public void Exportar(ReporteGrupal reporte, string rutaArchivo)
        {
            Grupal = reporte;
            Ruta = rutaArchivo;
        }
    }
}