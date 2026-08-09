namespace SistemaDocente.Reporting;

public enum PrioridadRecomendacion
{
    Informativa = 0,
    Revisar = 1,
    Atender = 2,
}

public sealed record RecomendacionPracticaDocente(
    string Codigo,
    PrioridadRecomendacion Prioridad,
    string Titulo,
    string Recomendacion,
    string Evidencia,
    string Cobertura);

public static class AnalizadorPracticaDocente
{
    public static IReadOnlyList<RecomendacionPracticaDocente> AnalizarGrupo(ReporteGrupal reporte)
    {
        ArgumentNullException.ThrowIfNull(reporte);

        var recomendaciones = new List<RecomendacionPracticaDocente>();
        var registrosAsistencia = reporte.AsistenciaMensual.Sum(x => x.Dias);
        var actividades = reporte.Cumplimiento.ActividadesAplicables;
        var evaluadas = reporte.Logro.Domina
            + reporte.Logro.Suficiente
            + reporte.Logro.EnProceso
            + reporte.Logro.RequiereApoyo;

        if (registrosAsistencia == 0 && actividades == 0)
        {
            recomendaciones.Add(new(
                "evidence.insufficient",
                PrioridadRecomendacion.Informativa,
                "Reunir evidencia antes de ajustar la intervención",
                "Todavía no hay suficiente asistencia o actividad registrada para sostener una recomendación pedagógica más específica.",
                "0 registros de asistencia y 0 actividades aplicables.",
                "La recomendación se limita a la cobertura de datos; no interpreta desempeño ni causas."));
            return recomendaciones;
        }

        if (reporte.Cumplimiento.Pendientes > 0)
        {
            recomendaciones.Add(new(
                "evaluation.complete-pending-evidence",
                PrioridadRecomendacion.Revisar,
                "Cerrar evidencia pendiente antes de interpretar el avance",
                "Revisa las actividades que siguen pendientes de entrega o decisión para evitar conclusiones con evidencia incompleta.",
                $"{reporte.Cumplimiento.Pendientes} de {actividades} registros de actividad están pendientes.",
                "Describe cobertura de evaluación; no atribuye el pendiente a esfuerzo, capacidad o contexto familiar."));
        }

        if (reporte.Cumplimiento.NoEntregadas > 0)
        {
            recomendaciones.Add(new(
                "delivery.review-barriers",
                PrioridadRecomendacion.Revisar,
                "Revisar barreras de participación o entrega",
                "Antes de aumentar exigencia o calificar la falta de entrega como desinterés, identifica condiciones de acceso, comprensión de consignas, tiempo y apoyos disponibles.",
                $"{reporte.Cumplimiento.NoEntregadas} de {actividades} registros aplicables figuran como no entregados.",
                "La evidencia sólo confirma no entrega; no identifica su causa."));
        }

        if (reporte.Logro.RequiereApoyo > 0)
        {
            recomendaciones.Add(new(
                "achievement.targeted-support",
                PrioridadRecomendacion.Atender,
                "Planear apoyo focalizado con criterios visibles",
                "Selecciona los aprendizajes o criterios con mayor necesidad de apoyo, ofrece andamiaje y genera una nueva oportunidad de evidencia antes de cerrar conclusiones.",
                $"{reporte.Logro.RequiereApoyo} evidencias evaluadas están en Requiere apoyo, de {evaluadas} con nivel de logro registrado.",
                "El nivel de logro es evidencia pedagógica, no un diagnóstico del estudiante."));
        }

        if (reporte.Logro.EnProceso > 0)
        {
            recomendaciones.Add(new(
                "achievement.feedback-loop",
                PrioridadRecomendacion.Revisar,
                "Fortalecer el ciclo de retroalimentación y reintento",
                "Usa retroalimentación concreta sobre el criterio observado y ofrece una oportunidad breve de revisión, práctica o reentrega.",
                $"{reporte.Logro.EnProceso} evidencias evaluadas están En proceso, de {evaluadas} con nivel de logro registrado.",
                "No supone que todos los casos requieran la misma estrategia; orienta una revisión docente."));
        }

        var faltas = reporte.AsistenciaMensual.Sum(x => x.Faltas);
        var justificadas = reporte.AsistenciaMensual.Sum(x => x.Justificadas);
        if (faltas + justificadas > 0)
        {
            recomendaciones.Add(new(
                "attendance.review-access-patterns",
                PrioridadRecomendacion.Revisar,
                "Considerar la asistencia al planear apoyos y recuperación",
                "Cruza las ausencias observadas con las oportunidades de aprendizaje perdidas y planea recuperación de evidencia sin inferir automáticamente una causa personal o familiar.",
                $"Se registran {faltas} faltas y {justificadas} ausencias justificadas en {registrosAsistencia} registros de asistencia.",
                "La asistencia muestra presencia registrada; por sí sola no explica aprendizaje ni motivos de ausencia."));
        }

        if (recomendaciones.Count == 0)
        {
            recomendaciones.Add(new(
                "practice.maintain-and-document",
                PrioridadRecomendacion.Informativa,
                "Mantener y documentar las estrategias que están funcionando",
                "Conserva evidencia de las estrategias actuales y sigue observando si el patrón se sostiene antes de introducir cambios innecesarios.",
                $"Hay {registrosAsistencia} registros de asistencia y {actividades} registros de actividad sin pendientes, no entregas, En proceso o Requiere apoyo en el resumen actual.",
                "Esta señal describe el conjunto de datos disponible y no garantiza por sí sola dominio de todos los aprendizajes."));
        }

        return recomendaciones;
    }
}