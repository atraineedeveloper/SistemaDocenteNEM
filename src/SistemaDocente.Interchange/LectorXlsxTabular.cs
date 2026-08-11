using System.Globalization;

using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

using SistemaDocente.Application;

namespace SistemaDocente.Interchange;

public sealed class LectorXlsxTabular : ILectorImportacionTabular
{
    private static readonly HashSet<uint> FormatosFechaIntegrados =
    [
        14, 15, 16, 17, 18, 19, 20, 21, 22,
        27, 28, 29, 30, 31, 32, 33, 34, 35, 36,
        45, 46, 47,
        50, 51, 52, 53, 54, 55, 56, 57, 58,
    ];

    public DocumentoTabular Leer(string rutaArchivo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rutaArchivo);

        try
        {
            using var documento = SpreadsheetDocument.Open(rutaArchivo, false);
            var libro = documento.WorkbookPart
                ?? throw new ImportacionTabularException("El archivo XLSX no contiene un libro válido.");
            var contenidoLibro = libro.Workbook
                ?? throw new ImportacionTabularException("El archivo XLSX no contiene una estructura de libro válida.");

            var cadenasCompartidas = libro.SharedStringTablePart?.SharedStringTable?
                .Elements<SharedStringItem>()
                .Select(item => item.InnerText)
                .ToArray() ?? [];

            var formatosCelda = libro.WorkbookStylesPart?.Stylesheet?.CellFormats?
                .Elements<CellFormat>()
                .ToArray() ?? [];

            var formatosPersonalizados = libro.WorkbookStylesPart?.Stylesheet?.NumberingFormats?
                .Elements<NumberingFormat>()
                .Where(formato => formato.NumberFormatId?.Value is not null)
                .ToDictionary(
                    formato => formato.NumberFormatId!.Value,
                    formato => formato.FormatCode?.Value ?? string.Empty)
                ?? new Dictionary<uint, string>();

            var usaSistema1904 = contenidoLibro.WorkbookProperties?.Date1904?.Value ?? false;
            var hojas = new List<HojaTabular>();

            foreach (var hoja in contenidoLibro.Sheets?.Elements<Sheet>() ?? [])
            {
                if (hoja.Id?.Value is not string relacionId ||
                    libro.GetPartById(relacionId) is not WorksheetPart parteHoja)
                {
                    continue;
                }

                var hojaTabular = LeerHoja(
                    hoja.Name?.Value ?? "Hoja",
                    parteHoja,
                    cadenasCompartidas,
                    formatosCelda,
                    formatosPersonalizados,
                    usaSistema1904);

                if (hojaTabular is not null)
                {
                    hojas.Add(hojaTabular);
                }
            }

            if (hojas.Count == 0)
            {
                throw new ImportacionTabularException("El archivo XLSX no contiene hojas con filas utilizables.");
            }

            return new DocumentoTabular(Path.GetFileName(rutaArchivo), hojas);
        }
        catch (ImportacionTabularException)
        {
            throw;
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException or OpenXmlPackageException or InvalidDataException or FileFormatException)
        {
            throw new ImportacionTabularException("No se pudo leer el archivo XLSX seleccionado.", exception);
        }
    }

    private static HojaTabular? LeerHoja(
        string nombre,
        WorksheetPart parteHoja,
        IReadOnlyList<string> cadenasCompartidas,
        IReadOnlyList<CellFormat> formatosCelda,
        IReadOnlyDictionary<uint, string> formatosPersonalizados,
        bool usaSistema1904)
    {
        var contenidoHoja = parteHoja.Worksheet;
        if (contenidoHoja is null)
        {
            return null;
        }

        var filas = contenidoHoja.GetFirstChild<SheetData>()?
            .Elements<Row>()
            .Select(fila => LeerFila(
                fila,
                cadenasCompartidas,
                formatosCelda,
                formatosPersonalizados,
                usaSistema1904))
            .Where(fila => fila.Celdas.Values.Any(celda => celda.Tipo != TipoCeldaTabular.Vacia))
            .ToArray() ?? [];

        if (filas.Length == 0)
        {
            return null;
        }

        var ancho = filas.Max(fila => fila.Celdas.Count == 0 ? 0 : fila.Celdas.Keys.Max() + 1);
        var encabezados = CompletarFila(filas[0].Celdas, ancho);
        var datos = filas
            .Skip(1)
            .Select(fila => new FilaTabular(fila.NumeroOrigen, CompletarFila(fila.Celdas, ancho)))
            .ToArray();

        return new HojaTabular(nombre, encabezados, datos);
    }

    private static FilaXlsx LeerFila(
        Row fila,
        IReadOnlyList<string> cadenasCompartidas,
        IReadOnlyList<CellFormat> formatosCelda,
        IReadOnlyDictionary<uint, string> formatosPersonalizados,
        bool usaSistema1904)
    {
        var celdas = new Dictionary<int, CeldaTabular>();
        var indiceSecuencial = 0;

        foreach (var celda in fila.Elements<Cell>())
        {
            var indice = ObtenerIndiceColumna(celda.CellReference?.Value) ?? indiceSecuencial;
            celdas[indice] = ConvertirCelda(
                celda,
                cadenasCompartidas,
                formatosCelda,
                formatosPersonalizados,
                usaSistema1904);
            indiceSecuencial = indice + 1;
        }

        var numeroOrigen = checked((int)(fila.RowIndex?.Value ?? 0));
        return new FilaXlsx(numeroOrigen, celdas);
    }

    private static CeldaTabular ConvertirCelda(
        Cell celda,
        IReadOnlyList<string> cadenasCompartidas,
        IReadOnlyList<CellFormat> formatosCelda,
        IReadOnlyDictionary<uint, string> formatosPersonalizados,
        bool usaSistema1904)
    {
        var texto = celda.CellValue?.InnerText ?? celda.InlineString?.InnerText ?? string.Empty;
        var tipo = celda.DataType?.Value;

        if (tipo == CellValues.SharedString)
        {
            return int.TryParse(texto, NumberStyles.Integer, CultureInfo.InvariantCulture, out var indice) &&
                   indice >= 0 && indice < cadenasCompartidas.Count
                ? CeldaTabular.DesdeTexto(cadenasCompartidas[indice])
                : CeldaTabular.Vacia;
        }

        if (tipo == CellValues.InlineString || tipo == CellValues.String)
        {
            return CeldaTabular.DesdeTexto(celda.InlineString?.InnerText ?? texto);
        }

        if (tipo == CellValues.Boolean)
        {
            return texto switch
            {
                "1" => CeldaTabular.DesdeBooleano(true, "TRUE"),
                "0" => CeldaTabular.DesdeBooleano(false, "FALSE"),
                _ => CeldaTabular.DesdeTexto(texto),
            };
        }

        if (tipo == CellValues.Date &&
            DateTime.TryParse(texto, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var fechaIso))
        {
            var fecha = DateOnly.FromDateTime(fechaIso);
            return CeldaTabular.DesdeFecha(fecha, fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        }

        if ((tipo is null || tipo == CellValues.Number) &&
            double.TryParse(texto, NumberStyles.Float, CultureInfo.InvariantCulture, out var numeroDoble) &&
            EsFormatoFecha(celda.StyleIndex?.Value, formatosCelda, formatosPersonalizados))
        {
            var serialOa = usaSistema1904 ? numeroDoble + 1462d : numeroDoble;
            var fecha = DateOnly.FromDateTime(DateTime.FromOADate(serialOa));
            return CeldaTabular.DesdeFecha(fecha, fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        }

        if ((tipo is null || tipo == CellValues.Number) &&
            decimal.TryParse(texto, NumberStyles.Float, CultureInfo.InvariantCulture, out var numero))
        {
            return CeldaTabular.DesdeNumero(numero, texto);
        }

        return CeldaTabular.DesdeTexto(texto);
    }

    private static bool EsFormatoFecha(
        uint? indiceEstilo,
        IReadOnlyList<CellFormat> formatosCelda,
        IReadOnlyDictionary<uint, string> formatosPersonalizados)
    {
        if (indiceEstilo is null || indiceEstilo.Value >= formatosCelda.Count)
        {
            return false;
        }

        var formatoId = formatosCelda[(int)indiceEstilo.Value].NumberFormatId?.Value;
        if (formatoId is null)
        {
            return false;
        }

        if (FormatosFechaIntegrados.Contains(formatoId.Value))
        {
            return true;
        }

        return formatosPersonalizados.TryGetValue(formatoId.Value, out var codigo) &&
               CodigoPareceFecha(codigo);
    }

    private static bool CodigoPareceFecha(string codigo)
    {
        var sinLiterales = new string(codigo
            .ToLowerInvariant()
            .Where(caracter => caracter is not '"' and not '\\')
            .ToArray());

        return sinLiterales.Contains('y', StringComparison.Ordinal) ||
               sinLiterales.Contains('d', StringComparison.Ordinal) ||
               sinLiterales.Contains('h', StringComparison.Ordinal) ||
               sinLiterales.Contains("m/", StringComparison.Ordinal) ||
               sinLiterales.Contains("/m", StringComparison.Ordinal) ||
               sinLiterales.Contains("m-", StringComparison.Ordinal) ||
               sinLiterales.Contains("-m", StringComparison.Ordinal);
    }

    private static int? ObtenerIndiceColumna(string? referencia)
    {
        if (string.IsNullOrWhiteSpace(referencia))
        {
            return null;
        }

        var indice = 0;
        var encontroLetra = false;
        foreach (var caracter in referencia)
        {
            if (!char.IsLetter(caracter))
            {
                break;
            }

            encontroLetra = true;
            indice = checked((indice * 26) + (char.ToUpperInvariant(caracter) - 'A' + 1));
        }

        return encontroLetra ? indice - 1 : null;
    }

    private static CeldaTabular[] CompletarFila(
        IReadOnlyDictionary<int, CeldaTabular> celdas,
        int ancho)
    {
        var resultado = new CeldaTabular[ancho];
        for (var indice = 0; indice < ancho; indice++)
        {
            resultado[indice] = celdas.TryGetValue(indice, out var celda)
                ? celda
                : CeldaTabular.Vacia;
        }

        return resultado;
    }

    private sealed record FilaXlsx(int NumeroOrigen, IReadOnlyDictionary<int, CeldaTabular> Celdas);
}
