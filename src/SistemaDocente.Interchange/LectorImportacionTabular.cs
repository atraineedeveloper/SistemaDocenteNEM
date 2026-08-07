using SistemaDocente.Application;

namespace SistemaDocente.Interchange;

public sealed class LectorImportacionTabular : ILectorImportacionTabular
{
    private readonly LectorCsvTabular lectorCsv = new();
    private readonly LectorXlsxTabular lectorXlsx = new();

    public DocumentoTabular Leer(string rutaArchivo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rutaArchivo);

        return Path.GetExtension(rutaArchivo).ToLowerInvariant() switch
        {
            ".csv" => lectorCsv.Leer(rutaArchivo),
            ".xlsx" => lectorXlsx.Leer(rutaArchivo),
            _ => throw new ImportacionTabularException(
                "El formato de archivo no es compatible. Selecciona un archivo .xlsx o .csv."),
        };
    }
}
