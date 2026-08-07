using SistemaDocente.Core;

namespace SistemaDocente.Reporting;

public static class GeneradorReportes
{
    public static ReporteIndividualAlumno CrearIndividual(
        ContextoGrupo contexto,
        string nombreGrupo,
        EstudianteReporteFuente estudiante)
    {
        ArgumentNullException.ThrowIfNull(contexto);
        ArgumentNullException.ThrowIfNull(estudiante);

        var meses = estudiante.AsistenciaMensual
            .OrderBy(x => x.Anio)
            .ThenBy(x => x.Mes)
            .Select(CalcularMes)
            .ToArray();
        var cumplimiento = CalcularCumplimiento(estudiante.Actividades);
        var logro = CalcularLogro(estudiante.Actividades);
        var porcentajeAsistencia = CalcularPorcentajeAsistencia(estudiante.AsistenciaMensual);

        return new ReporteIndividualAlumno(
            contexto,
            nombreGrupo,
            estudiante.EstudianteId,
            estudiante.NumeroLista,
            estudiante.Nombre,
            estudiante.Genero,
            estudiante.Edad,
            estudiante.EstaActivo,
            porcentajeAsistencia,
            meses,
            cumplimiento,
            logro,
            estudiante.Actividades.OrderBy(x => x.Fecha).ThenBy(x => x.Proyecto, StringComparer.Ordinal).ThenBy(x => x.Actividad, StringComparer.Ordinal).ToArray(),
            estudiante.Fortalezas,
            estudiante.Dificultades,
            estudiante.Apoyos,
            estudiante.Observaciones,
            estudiante.Acuerdos);
    }

    public static ReporteGrupal CrearGrupal(
        ContextoGrupo contexto,
        string nombreGrupo,
        IReadOnlyCollection<EstudianteReporteFuente> estudiantes)
    {
        ArgumentNullException.ThrowIfNull(contexto);
        ArgumentNullException.ThrowIfNull(estudiantes);

        var fuentes = estudiantes.ToArray();
        var actividades = fuentes.SelectMany(x => x.Actividades).ToArray();
        var asistenciaMes = fuentes.SelectMany(x => x.AsistenciaMensual)
            .GroupBy(x => (x.Anio, x.Mes))
            .OrderBy(x => x.Key.Anio)
            .ThenBy(x => x.Key.Mes)
            .Select(grupo => CalcularMes(new AsistenciaMesFuente(
                grupo.Key.Anio,
                grupo.Key.Mes,
                grupo.SelectMany(x => x.Registros).ToArray())))
            .ToArray();

        var seguimiento = fuentes
            .OrderBy(x => x.NumeroLista)
            .ThenBy(x => x.Nombre, StringComparer.Ordinal)
            .Select(x => new SeguimientoAlumnoReporte(
                x.EstudianteId,
                x.NumeroLista,
                x.Nombre,
                x.EstaActivo,
                CalcularPorcentajeAsistencia(x.AsistenciaMensual),
                CalcularCumplimiento(x.Actividades),
                x.Actividades.Count(a =>
                    a.EstadoEntrega == EstadoEntregaActividad.Entregada
                    && a.NivelLogro == NivelLogro.RequiereApoyo)))
            .ToArray();

        return new ReporteGrupal(
            contexto,
            nombreGrupo,
            fuentes.Length,
            fuentes.Count(x => x.EstaActivo),
            CalcularPorcentajeAsistencia(fuentes.SelectMany(x => x.AsistenciaMensual)),
            CalcularCumplimiento(actividades),
            CalcularLogro(actividades),
            asistenciaMes,
            seguimiento);
    }

    public static ResumenCumplimientoReporte CalcularCumplimiento(IEnumerable<ActividadReporteFuente> actividades)
    {
        ArgumentNullException.ThrowIfNull(actividades);
        var snapshot = actividades.ToArray();
        var entregadas = snapshot.Count(x => x.EstadoEntrega == EstadoEntregaActividad.Entregada);
        var noEntregadas = snapshot.Count(x => x.EstadoEntrega == EstadoEntregaActividad.NoEntregada);
        var pendientes = snapshot.Count(x => x.EstadoEntrega == EstadoEntregaActividad.Pendiente);
        var decididas = entregadas + noEntregadas;
        double? porcentaje = decididas == 0 ? null : entregadas * 100d / decididas;
        return new(snapshot.Length, entregadas, noEntregadas, pendientes, porcentaje);
    }

    public static DistribucionLogroReporte CalcularLogro(IEnumerable<ActividadReporteFuente> actividades)
    {
        ArgumentNullException.ThrowIfNull(actividades);
        var entregadas = actividades.Where(x => x.EstadoEntrega == EstadoEntregaActividad.Entregada).ToArray();
        return new DistribucionLogroReporte(
            entregadas.Count(x => x.NivelLogro == NivelLogro.Pendiente),
            entregadas.Count(x => x.NivelLogro == NivelLogro.Domina),
            entregadas.Count(x => x.NivelLogro == NivelLogro.Suficiente),
            entregadas.Count(x => x.NivelLogro == NivelLogro.EnProceso),
            entregadas.Count(x => x.NivelLogro == NivelLogro.RequiereApoyo));
    }

    private static MesAsistenciaReporte CalcularMes(AsistenciaMesFuente fuente)
    {
        var registros = fuente.Registros.ToArray();
        var presentes = registros.Count(x => x == EstadoAsistencia.Presente);
        var faltas = registros.Count(x => x == EstadoAsistencia.Falta);
        var retardos = registros.Count(x => x == EstadoAsistencia.Retardo);
        var justificadas = registros.Count(x => x == EstadoAsistencia.Justificada);
        double? porcentaje = registros.Length == 0
            ? null
            : (presentes + retardos) * 100d / registros.Length;
        return new(fuente.Anio, fuente.Mes, registros.Length, presentes, faltas, retardos, justificadas, porcentaje);
    }

    private static double? CalcularPorcentajeAsistencia(IEnumerable<AsistenciaMesFuente> meses)
    {
        var registros = meses.SelectMany(x => x.Registros).ToArray();
        if (registros.Length == 0) return null;
        var presentes = registros.Count(x => x == EstadoAsistencia.Presente);
        var retardos = registros.Count(x => x == EstadoAsistencia.Retardo);
        return (presentes + retardos) * 100d / registros.Length;
    }
}
