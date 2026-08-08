using System.Globalization;
using System.Text;

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

using SistemaDocente.Application;

namespace SistemaDocente.Interchange;

public sealed class ExportadorTabularArchivo : IExportadorTabular
{
    private const uint EstiloEncabezado = 1U;
    private const uint EstiloFecha = 2U;

    public void Exportar(
        DocumentoTabularSalida documento,
        string rutaArchivo,
        FormatoExportacionTabular formato)
    {
        ArgumentNullException.ThrowIfNull(documento);
        ArgumentException.ThrowIfNullOrWhiteSpace(rutaArchivo);

        ValidarDocumento(documento, formato);

        var rutaCompleta = Path.GetFullPath(rutaArchivo);
        var directorio = Path.GetDirectoryName(rutaCompleta)
            ?? throw new ExportacionTabularException("No fue posible determinar la carpeta de destino.");

        if (!Directory.Exists(directorio))
        {
            throw new ExportacionTabularException("La carpeta de destino no existe.");
        }

        var temporal = Path.Combine(
            directorio,
            $".{Path.GetFileName(rutaCompleta)}.{Guid.NewGuid():N}.tmp");

        try
        {
            if (formato == FormatoExportacionTabular.Xlsx)
            {
                EscribirXlsx(documento, temporal);
            }
            else
            {
                EscribirCsv(documento.Hojas[0], temporal);
            }

            File.Move(temporal, rutaCompleta, overwrite: true);
        }
        catch (ExportacionTabularException)
        {
            EliminarTemporal(temporal);
            throw;
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            EliminarTemporal(temporal);
            throw new ExportacionTabularException(
                "No se pudo crear el archivo de exportación solicitado.",
                exception);
        }
    }

    private static void ValidarDocumento(
        DocumentoTabularSalida documento,
        FormatoExportacionTabular formato)
    {
        if (documento.Hojas.Count == 0)
        {
            throw new ExportacionTabularException("La exportación no contiene conjuntos de datos.");
        }

        if (formato == FormatoExportacionTabular.Csv && documento.Hojas.Count != 1)
        {
            throw new ExportacionTabularException(
                "CSV admite exactamente un conjunto de datos por archivo.");
        }

        foreach (var hoja in documento.Hojas)
        {
            if (hoja.Columnas.Count == 0)
            {
                throw new ExportacionTabularException(
                    $"El conjunto '{hoja.Nombre}' no contiene columnas exportables.");
            }

            if (hoja.Filas.Any(fila => fila.Celdas.Count != hoja.Columnas.Count))
            {
                throw new ExportacionTabularException(
                    $"El conjunto '{hoja.Nombre}' contiene filas con un número de celdas incompatible.");
            }
        }
    }

    private static void EscribirXlsx(
        DocumentoTabularSalida documento,
        string rutaTemporal)
    {
        using var spreadsheet = SpreadsheetDocument.Create(
            rutaTemporal,
            SpreadsheetDocumentType.Workbook);

        var workbookPart = spreadsheet.AddWorkbookPart();
        workbookPart.Workbook = new Workbook();
        AgregarEstilos(workbookPart);

        var sheets = workbookPart.Workbook.AppendChild(new Sheets());
        var nombresUsados = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        uint sheetId = 1;

        foreach (var hoja in documento.Hojas)
        {
            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = CrearWorksheet(hoja);

            var nombreSeguro = CrearNombreHojaSeguro(hoja.Nombre, nombresUsados);
            sheets.Append(new Sheet
            {
                Id = workbookPart.GetIdOfPart(worksheetPart),
                SheetId = sheetId++,
                Name = nombreSeguro,
            });
        }

        workbookPart.Workbook.Save();
    }

    private static Worksheet CrearWorksheet(HojaTabularSalida hoja)
    {
        var sheetView = new SheetView
        {
            WorkbookViewId = 0U,
        };
        sheetView.Append(new Pane
        {
            VerticalSplit = 1D,
            TopLeftCell = "A2",
            ActivePane = PaneValues.BottomLeft,
            State = PaneStateValues.Frozen,
        });

        var worksheet = new Worksheet();
        worksheet.Append(new SheetViews(sheetView));
        worksheet.Append(CrearColumnas(hoja));

        var sheetData = new SheetData();
        sheetData.Append(CrearFilaEncabezado(hoja.Columnas));

        foreach (var fila in hoja.Filas)
        {
            var row = new Row();
            foreach (var celda in fila.Celdas)
            {
                row.Append(CrearCeldaXlsx(celda));
            }

            sheetData.Append(row);
        }

        worksheet.Append(sheetData);
        return worksheet;
    }

    private static Columns CrearColumnas(HojaTabularSalida hoja)
    {
        var columns = new Columns();
        for (var indice = 0; indice < hoja.Columnas.Count; indice++)
        {
            var longitud = hoja.Filas
                .Select(fila => TextoVisible(fila.Celdas[indice]).Length)
                .Prepend(hoja.Columnas[indice].Encabezado.Length)
                .DefaultIfEmpty(10)
                .Max();

            var ancho = Math.Clamp(longitud + 2D, 10D, 50D);
            columns.Append(new Column
            {
                Min = (uint)(indice + 1),
                Max = (uint)(indice + 1),
                Width = ancho,
                CustomWidth = true,
            });
        }

        return columns;
    }

    private static Row CrearFilaEncabezado(IReadOnlyList<ColumnaTabularSalida> columnas)
    {
        var row = new Row();
        foreach (var columna in columnas)
        {
            var cell = CrearCeldaTexto(columna.Encabezado);
            cell.StyleIndex = EstiloEncabezado;
            row.Append(cell);
        }

        return row;
    }

    private static Cell CrearCeldaXlsx(CeldaTabularSalida celda) => celda.Tipo switch
    {
        TipoCeldaTabularSalida.Vacia => new Cell(),
        TipoCeldaTabularSalida.Texto => CrearCeldaTexto(celda.Texto),
        TipoCeldaTabularSalida.Numero => new Cell
        {
            DataType = CellValues.Number,
            CellValue = new CellValue(
                (celda.Numero ?? 0M).ToString(CultureInfo.InvariantCulture)),
        },
        TipoCeldaTabularSalida.Fecha => new Cell
        {
            DataType = CellValues.Number,
            CellValue = new CellValue(
                (celda.Fecha ?? DateOnly.MinValue)
                    .ToDateTime(TimeOnly.MinValue)
                    .ToOADate()
                    .ToString(CultureInfo.InvariantCulture)),
            StyleIndex = EstiloFecha,
        },
        TipoCeldaTabularSalida.Booleano => new Cell
        {
            DataType = CellValues.Boolean,
            CellValue = new CellValue((celda.Booleano ?? false) ? "1" : "0"),
        },
        _ => throw new ExportacionTabularException("Se encontró un tipo de celda no compatible."),
    };

    private static Cell CrearCeldaTexto(string texto) => new()
    {
        DataType = CellValues.InlineString,
        InlineString = new InlineString(
            new Text(texto)
            {
                Space = SpaceProcessingModeValues.Preserve,
            }),
    };

    private static void AgregarEstilos(WorkbookPart workbookPart)
    {
        var stylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();
        var stylesheet = new Stylesheet();

        stylesheet.Append(new NumberingFormats(
            new NumberingFormat
            {
                NumberFormatId = 164U,
                FormatCode = "dd/mm/yyyy",
            })
        {
            Count = 1U,
        });

        stylesheet.Append(new Fonts(
            new Font(),
            new Font(new Bold()))
        {
            Count = 2U,
        });

        stylesheet.Append(new Fills(
            new Fill(new PatternFill { PatternType = PatternValues.None }),
            new Fill(new PatternFill { PatternType = PatternValues.Gray125 }))
        {
            Count = 2U,
        });

        stylesheet.Append(new Borders(new Border())
        {
            Count = 1U,
        });

        stylesheet.Append(new CellFormats(
            new CellFormat(),
            new CellFormat
            {
                FontId = 1U,
                ApplyFont = true,
            },
            new CellFormat
            {
                NumberFormatId = 164U,
                ApplyNumberFormat = true,
            })
        {
            Count = 3U,
        });

        stylesPart.Stylesheet = stylesheet;
        stylesPart.Stylesheet.Save();
    }

    private static void EscribirCsv(
        HojaTabularSalida hoja,
        string rutaTemporal)
    {
        using var writer = new StreamWriter(
            rutaTemporal,
            append: false,
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

        writer.WriteLine(string.Join(',', hoja.Columnas.Select(c => EscaparCsv(c.Encabezado))));

        foreach (var fila in hoja.Filas)
        {
            writer.WriteLine(string.Join(',', fila.Celdas.Select(SerializarCeldaCsv)));
        }
    }

    private static string SerializarCeldaCsv(CeldaTabularSalida celda)
    {
        var valor = celda.Tipo switch
        {
            TipoCeldaTabularSalida.Vacia => string.Empty,
            TipoCeldaTabularSalida.Texto => NeutralizarFormulaCsv(celda.Texto),
            TipoCeldaTabularSalida.Numero =>
                (celda.Numero ?? 0M).ToString(CultureInfo.InvariantCulture),
            TipoCeldaTabularSalida.Fecha =>
                (celda.Fecha ?? DateOnly.MinValue).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            TipoCeldaTabularSalida.Booleano => (celda.Booleano ?? false) ? "TRUE" : "FALSE",
            _ => throw new ExportacionTabularException("Se encontró un tipo de celda no compatible."),
        };

        return EscaparCsv(valor);
    }

    private static string NeutralizarFormulaCsv(string valor)
    {
        if (valor.Length == 0)
        {
            return valor;
        }

        var sinEspacios = valor.TrimStart(' ', '\t');
        if (sinEspacios.Length > 0 && sinEspacios[0] is '=' or '+' or '-' or '@')
        {
            return "'" + valor;
        }

        if (valor[0] is '\t' or '\r')
        {
            return "'" + valor;
        }

        return valor;
    }

    private static string EscaparCsv(string valor)
    {
        if (!valor.Contains(',') &&
            !valor.Contains('"') &&
            !valor.Contains('\r') &&
            !valor.Contains('\n'))
        {
            return valor;
        }

        return $"\"{valor.Replace("\"", "\"\"")}\"";
    }

    private static string CrearNombreHojaSeguro(
        string nombre,
        ISet<string> nombresUsados)
    {
        var invalido = new HashSet<char>(['[', ']', ':', '*', '?', '/', '\\']);
        var baseNombre = new string(
            (string.IsNullOrWhiteSpace(nombre) ? "Datos" : nombre.Trim())
                .Select(caracter => invalido.Contains(caracter) ? '_' : caracter)
                .ToArray());

        if (baseNombre.Length > 31)
        {
            baseNombre = baseNombre[..31];
        }

        var candidato = baseNombre;
        var sufijo = 2;
        while (!nombresUsados.Add(candidato))
        {
            var textoSufijo = $" ({sufijo++})";
            var limiteBase = Math.Max(1, 31 - textoSufijo.Length);
            candidato = baseNombre[..Math.Min(baseNombre.Length, limiteBase)] + textoSufijo;
        }

        return candidato;
    }

    private static string TextoVisible(CeldaTabularSalida celda) => celda.Tipo switch
    {
        TipoCeldaTabularSalida.Vacia => string.Empty,
        TipoCeldaTabularSalida.Texto => celda.Texto,
        TipoCeldaTabularSalida.Numero =>
            (celda.Numero ?? 0M).ToString(CultureInfo.InvariantCulture),
        TipoCeldaTabularSalida.Fecha =>
            (celda.Fecha ?? DateOnly.MinValue).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
        TipoCeldaTabularSalida.Booleano => (celda.Booleano ?? false) ? "TRUE" : "FALSE",
        _ => string.Empty,
    };

    private static void EliminarTemporal(string rutaTemporal)
    {
        try
        {
            if (File.Exists(rutaTemporal))
            {
                File.Delete(rutaTemporal);
            }
        }
        catch
        {
            // Best-effort cleanup only. Do not hide the original export failure.
        }
    }
}
