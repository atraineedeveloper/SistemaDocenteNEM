using System.Text;

using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

using SistemaDocente.Interchange;

namespace SistemaDocente.Application.Tests;

public sealed class ExportacionTabularEscritoresTests
{
    [Fact]
    public void XlsxEscribeHojasValoresYFechasSinFormulas()
    {
        var directorio = CrearDirectorioTemporal();
        var ruta = Path.Combine(directorio, "grupo.xlsx");
        var documento = DocumentoTabularSalida.Crear(
            new HojaTabularSalida(
                "Alumnos",
                [
                    new ColumnaTabularSalida("Nombre"),
                    new ColumnaTabularSalida("Fecha"),
                    new ColumnaTabularSalida("Texto seguro"),
                ],
                [
                    FilaTabularSalida.Crear(
                        CeldaTabularSalida.DesdeTexto("María López"),
                        CeldaTabularSalida.DesdeFecha(new DateOnly(2016, 4, 15)),
                        CeldaTabularSalida.DesdeTexto("=2+2")),
                ]),
            new HojaTabularSalida(
                "Contexto:grupo",
                [new ColumnaTabularSalida("Valor")],
                [FilaTabularSalida.Crear(CeldaTabularSalida.DesdeTexto("Fase 4"))]));

        new ExportadorTabularArchivo().Exportar(
            documento,
            ruta,
            FormatoExportacionTabular.Xlsx);

        Assert.True(File.Exists(ruta));
        using var spreadsheet = SpreadsheetDocument.Open(ruta, false);
        var workbookPart = Assert.IsType<WorkbookPart>(spreadsheet.WorkbookPart);
        var sheets = workbookPart.Workbook.Sheets!.Elements<Sheet>().ToArray();
        Assert.Equal(2, sheets.Length);
        Assert.Equal("Alumnos", sheets[0].Name!.Value);
        Assert.Equal("Contexto_grupo", sheets[1].Name!.Value);

        var formulas = workbookPart.WorksheetParts
            .SelectMany(part => part.Worksheet.Descendants<CellFormula>())
            .ToArray();
        Assert.Empty(formulas);

        var textos = workbookPart.WorksheetParts
            .SelectMany(part => part.Worksheet.Descendants<InlineString>())
            .Select(inline => inline.InnerText)
            .ToArray();
        Assert.Contains("María López", textos);
        Assert.Contains("=2+2", textos);

        var celdasFecha = workbookPart.WorksheetParts
            .SelectMany(part => part.Worksheet.Descendants<Cell>())
            .Where(cell => cell.StyleIndex?.Value == 2U)
            .ToArray();
        Assert.Single(celdasFecha);
        Assert.Equal(CellValues.Number, celdasFecha[0].DataType?.Value);
    }

    [Fact]
    public void CsvUsaUtf8BomComillasYNeutralizaFormula()
    {
        var directorio = CrearDirectorioTemporal();
        var ruta = Path.Combine(directorio, "alumnos.csv");
        var documento = DocumentoTabularSalida.Crear(
            new HojaTabularSalida(
                "Alumnos",
                [
                    new ColumnaTabularSalida("Nombre"),
                    new ColumnaTabularSalida("Fecha"),
                    new ColumnaTabularSalida("Observaciones"),
                    new ColumnaTabularSalida("Texto externo"),
                ],
                [
                    FilaTabularSalida.Crear(
                        CeldaTabularSalida.DesdeTexto("José Pérez"),
                        CeldaTabularSalida.DesdeFecha(new DateOnly(2026, 8, 8)),
                        CeldaTabularSalida.DesdeTexto("Primera línea, con coma\nSegunda línea"),
                        CeldaTabularSalida.DesdeTexto("=HYPERLINK(\"https://example.invalid\")")),
                ]));

        new ExportadorTabularArchivo().Exportar(
            documento,
            ruta,
            FormatoExportacionTabular.Csv);

        var bytes = File.ReadAllBytes(ruta);
        Assert.True(bytes.Length > 3);
        Assert.Equal(0xEF, bytes[0]);
        Assert.Equal(0xBB, bytes[1]);
        Assert.Equal(0xBF, bytes[2]);

        var contenido = File.ReadAllText(ruta, Encoding.UTF8);
        Assert.Contains("2026-08-08", contenido, StringComparison.Ordinal);
        Assert.Contains("\"Primera línea, con coma", contenido, StringComparison.Ordinal);
        Assert.Contains("Segunda línea\"", contenido, StringComparison.Ordinal);
        Assert.Contains("'=HYPERLINK", contenido, StringComparison.Ordinal);

        var releido = new LectorCsvTabular().Leer(ruta);
        var fila = Assert.Single(Assert.Single(releido.Hojas).Filas);
        Assert.Equal("José Pérez", fila.Celdas[0].Texto);
        Assert.Equal("2026-08-08", fila.Celdas[1].Texto);
        Assert.Equal("Primera línea, con coma\nSegunda línea", fila.Celdas[2].Texto);
        Assert.StartsWith("'=HYPERLINK", fila.Celdas[3].Texto, StringComparison.Ordinal);
    }

    [Fact]
    public void CsvRechazaMultiplesHojasSinPublicarDestino()
    {
        var directorio = CrearDirectorioTemporal();
        var ruta = Path.Combine(directorio, "invalido.csv");
        var hoja = new HojaTabularSalida(
            "Datos",
            [new ColumnaTabularSalida("Valor")],
            [FilaTabularSalida.Crear(CeldaTabularSalida.DesdeTexto("Uno"))]);
        var documento = DocumentoTabularSalida.Crear(hoja, hoja with { Nombre = "Otros" });

        Assert.Throws<ExportacionTabularException>(() =>
            new ExportadorTabularArchivo().Exportar(
                documento,
                ruta,
                FormatoExportacionTabular.Csv));

        Assert.False(File.Exists(ruta));
        Assert.Empty(Directory.GetFiles(directorio, "*.tmp"));
    }

    [Fact]
    public void XlsxFallaDuranteSerializacionSinReemplazarDestinoExistente()
    {
        var directorio = CrearDirectorioTemporal();
        var ruta = Path.Combine(directorio, "existente.xlsx");
        File.WriteAllText(ruta, "ORIGINAL", Encoding.UTF8);
        var documento = DocumentoTabularSalida.Crear(
            new HojaTabularSalida(
                "Datos",
                [new ColumnaTabularSalida("Valor")],
                [FilaTabularSalida.Crear(new CeldaTabularSalida((TipoCeldaTabularSalida)999))]));

        Assert.Throws<ExportacionTabularException>(() =>
            new ExportadorTabularArchivo().Exportar(
                documento,
                ruta,
                FormatoExportacionTabular.Xlsx));

        Assert.Equal("ORIGINAL", File.ReadAllText(ruta, Encoding.UTF8));
        Assert.Empty(Directory.GetFiles(directorio, "*.tmp"));
    }

    private static string CrearDirectorioTemporal()
    {
        var directorio = Path.Combine(
            Path.GetTempPath(),
            "SistemaDocenteNEM-Export-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directorio);
        return directorio;
    }
}