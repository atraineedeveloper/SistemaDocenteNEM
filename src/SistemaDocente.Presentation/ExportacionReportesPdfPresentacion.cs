using SistemaDocente.Application;
using SistemaDocente.Reporting;

namespace SistemaDocente.Presentation;

public sealed class ExportacionReportesPdfPresentacion
{
    private readonly IExportadorReportesPdf _exportador;

    public ExportacionReportesPdfPresentacion(IExportadorReportesPdf exportador)
    {
        _exportador = exportador ?? throw new ArgumentNullException(nameof(exportador));
    }

    public static string AdvertenciaPrivacidad =>
        "El PDF puede contener datos personales, pedagógicos y de seguimiento. Guárdalo y compártelo sólo en ubicaciones y canales autorizados para tu contexto escolar.";

    public static string CrearNombreArchivo(ReporteIndividualAlumno reporte, DateOnly fecha)
    {
        ArgumentNullException.ThrowIfNull(reporte);
        return SanitizarNombreArchivo(
            $"{IdentidadProducto.NombreSeguroArchivo}_Reporte_Individual_{reporte.NumeroLista}_{reporte.Nombre}_{fecha:yyyy-MM-dd}.pdf");
    }

    public static string CrearNombreArchivo(ReporteGrupal reporte, DateOnly fecha)
    {
        ArgumentNullException.ThrowIfNull(reporte);
        return SanitizarNombreArchivo(
            $"{IdentidadProducto.NombreSeguroArchivo}_Reporte_Grupal_{reporte.NombreGrupo}_{fecha:yyyy-MM-dd}.pdf");
    }

    public void Exportar(ReporteIndividualAlumno reporte, string rutaArchivo)
    {
        ArgumentNullException.ThrowIfNull(reporte);
        ArgumentException.ThrowIfNullOrWhiteSpace(rutaArchivo);
        _exportador.Exportar(reporte, rutaArchivo);
    }

    public void Exportar(ReporteGrupal reporte, string rutaArchivo)
    {
        ArgumentNullException.ThrowIfNull(reporte);
        ArgumentException.ThrowIfNullOrWhiteSpace(rutaArchivo);
        _exportador.Exportar(reporte, rutaArchivo);
    }

    private static string SanitizarNombreArchivo(string valor)
    {
        var invalidos = Path.GetInvalidFileNameChars().ToHashSet();
        var caracteres = valor.Select(caracter => invalidos.Contains(caracter) ? '_' : caracter).ToArray();
        var limpio = new string(caracteres).Replace(' ', '_');
        while (limpio.Contains("__", StringComparison.Ordinal))
        {
            limpio = limpio.Replace("__", "_", StringComparison.Ordinal);
        }
        return limpio;
    }
}