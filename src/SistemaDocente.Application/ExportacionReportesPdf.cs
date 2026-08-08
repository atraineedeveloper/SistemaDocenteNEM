using SistemaDocente.Reporting;

namespace SistemaDocente.Application;

public interface IExportadorReportesPdf
{
    void Exportar(ReporteIndividualAlumno reporte, string rutaArchivo);

    void Exportar(ReporteGrupal reporte, string rutaArchivo);
}

public sealed class ExportacionReportePdfException : Exception
{
    public ExportacionReportePdfException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}