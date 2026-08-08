using SistemaDocente.Core;
using SistemaDocente.Reporting;

namespace SistemaDocente.Application.Tests;

public sealed class AnalizadorPracticaDocenteTests
{
    [Fact]
    public void RecomendacionesExplicanEvidenciaSinDiagnosticarCausas()
    {
        var reporte = CrearReporte(
            cumplimiento: new ResumenCumplimientoReporte(10, 6, 2, 2, 75),
            logro: new DistribucionLogroReporte(0, 1, 2, 2, 1),
            asistencia: [new MesAsistenciaReporte(2026, 9, 20, 15, 3, 1, 1, 80)]);

        var recomendaciones = AnalizadorPracticaDocente.AnalizarGrupo(reporte);

        Assert.Contains(recomendaciones, x => x.Codigo == "evaluation.complete-pending-evidence");
        Assert.Contains(recomendaciones, x => x.Codigo == "delivery.review-barriers");
        Assert.Contains(recomendaciones, x => x.Codigo == "achievement.targeted-support");
        Assert.Contains(recomendaciones, x => x.Codigo == "achievement.feedback-loop");
        Assert.Contains(recomendaciones, x => x.Codigo == "attendance.review-access-patterns");
        Assert.All(recomendaciones, recomendacion =>
        {
            Assert.False(string.IsNullOrWhiteSpace(recomendacion.Evidencia));
            Assert.False(string.IsNullOrWhiteSpace(recomendacion.Cobertura));
            Assert.DoesNotContain("diagnóstico", recomendacion.Evidencia, StringComparison.OrdinalIgnoreCase);
        });
    }

    [Fact]
    public void SinEvidenciaNoInventaUnaInterpretacionPedagogica()
    {
        var reporte = CrearReporte(
            cumplimiento: new ResumenCumplimientoReporte(0, 0, 0, 0, null),
            logro: new DistribucionLogroReporte(0, 0, 0, 0, 0),
            asistencia: []);

        var recomendacion = Assert.Single(AnalizadorPracticaDocente.AnalizarGrupo(reporte));

        Assert.Equal("evidence.insufficient", recomendacion.Codigo);
        Assert.Equal(PrioridadRecomendacion.Informativa, recomendacion.Prioridad);
    }

    private static ReporteGrupal CrearReporte(
        ResumenCumplimientoReporte cumplimiento,
        DistribucionLogroReporte logro,
        IReadOnlyList<MesAsistenciaReporte> asistencia)
    {
        var grupoId = GrupoId.DesdeGuid(Guid.Parse("3f36d11f-d365-44a7-9acb-fae171e7bc2e"));
        return new ReporteGrupal(
            ContextoGrupo.Crear(grupoId),
            "Grupo ficticio",
            1,
            1,
            asistencia.Count == 0 ? null : 80,
            cumplimiento,
            logro,
            asistencia,
            []);
    }
}