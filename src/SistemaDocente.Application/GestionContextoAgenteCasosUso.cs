using SistemaDocente.Core;
using SistemaDocente.Reporting;

namespace SistemaDocente.Application;

public sealed record SeguimientoAgenteAlumno(
    EstudianteId EstudianteId,
    int NumeroLista,
    string? Nombre,
    bool EstaActivo,
    double? PorcentajeAsistencia,
    ResumenCumplimientoReporte Cumplimiento,
    int RequiereApoyo);

public sealed record ContextoAgenteGrupo(
    GrupoId GrupoId,
    string ModalidadGrupo,
    IReadOnlyList<GradoPrimaria> GradosAtendidos,
    IReadOnlyList<FaseNem> FasesNem,
    OrganizacionEscolar OrganizacionEscolar,
    int AlumnosHistoricos,
    int AlumnosActivos,
    double? PorcentajeAsistencia,
    ResumenCumplimientoReporte Cumplimiento,
    DistribucionLogroReporte Logro,
    IReadOnlyList<MesAsistenciaReporte> AsistenciaMensual,
    IReadOnlyList<SeguimientoAgenteAlumno> Seguimiento,
    bool IncluyeDatosPersonales);

public sealed class GestionContextoAgenteCasosUso
{
    private readonly GestionReportesCasosUso _reportes;

    public GestionContextoAgenteCasosUso(GestionReportesCasosUso reportes)
    {
        _reportes = reportes ?? throw new ArgumentNullException(nameof(reportes));
    }

    public ContextoAgenteGrupo GenerarGrupo(GrupoId grupoId, bool incluirDatosPersonales = false)
    {
        var reporte = _reportes.GenerarGrupal(grupoId);
        var seguimiento = reporte.Seguimiento
            .Select(x => new SeguimientoAgenteAlumno(
                x.EstudianteId,
                x.NumeroLista,
                incluirDatosPersonales ? x.Nombre : null,
                x.EstaActivo,
                x.PorcentajeAsistencia,
                x.Cumplimiento,
                x.RequiereApoyo))
            .ToArray();

        return new ContextoAgenteGrupo(
            grupoId,
            reporte.Contexto.ModalidadGrupo,
            reporte.Contexto.GradosAtendidos,
            reporte.Contexto.FasesNem,
            reporte.Contexto.OrganizacionEscolar,
            reporte.AlumnosHistoricos,
            reporte.AlumnosActivos,
            reporte.PorcentajeAsistencia,
            reporte.Cumplimiento,
            reporte.Logro,
            reporte.AsistenciaMensual,
            seguimiento,
            incluirDatosPersonales);
    }
}