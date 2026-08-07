using System.Globalization;
using System.Text;

using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

using SistemaDocente.Application;
using SistemaDocente.Interchange;

namespace SistemaDocente.Application.Tests;

public sealed class ImportacionTabularLectoresTests
{
    [Fact]
    public void Csv_LeeUtf8ComillasYDelimitadorDetectado()
    {
        var ruta = CrearRutaTemporal(".csv");

        try
        {
            File.WriteAllText(
                ruta,
                "No.;Nombre;Observaciones\r\n1;\"Ana, María\";\"Dijo \"\"hola\"\"\"\r\n2;Luis;\"Línea 1\nLínea 2\"",
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

            var documento = new LectorCsvTabular().Leer(ruta);

            var hoja = Assert.Single(documento.Hojas);
            Assert.Equal(3, hoja.Encabezados.Count);
            Assert.Equal(2, hoja.Filas.Count);
            Assert.Equal(2, hoja.Filas[0].NumeroOrigen);
            Assert.Equal("Ana, María", hoja.Filas[0].Celdas[1].Texto);
            Assert.Equal("Dijo \"hola\"", hoja.Filas[0].Celdas[2].Texto);
            Assert.Equal("Línea 1\nLínea 2", hoja.Filas[1].Celdas[2].Texto);
        }
        finally
        {
            EliminarSiExiste(ruta);
        }
    }

    [Fact]
    public void Csv_DelimitadorAmbiguoRequiereResolucionExplicita()
    {
        var ruta = CrearRutaTemporal(".csv");

        try
        {
            File.WriteAllText(ruta, "A,B;C\n1,2;3", new UTF8Encoding(false));

            var lector = new LectorCsvTabular();
            Assert.Throws<ImportacionTabularException>(() => lector.Leer(ruta));

            var documento = LectorCsvTabular.Leer(ruta, ',');
            var hoja = Assert.Single(documento.Hojas);
            Assert.Equal(2, hoja.Encabezados.Count);
            Assert.Equal("B;C", hoja.Encabezados[1].Texto);
        }
        finally
        {
            EliminarSiExiste(ruta);
        }
    }

    [Fact]
    public void Xlsx_LeeHojasUtilesSharedStringsYTiposNativos()
    {
        var ruta = CrearRutaTemporal(".xlsx");

        try
        {
            CrearLibroXlsx(ruta);

            var documento = new LectorXlsxTabular().Leer(ruta);

            Assert.Equal(2, documento.Hojas.Count);
            var alumnos = Assert.Single(documento.Hojas, hoja => hoja.Nombre == "Alumnos");
            var fila = Assert.Single(alumnos.Filas);

            Assert.Equal(2, fila.NumeroOrigen);
            Assert.Equal(TipoCeldaTabular.Numero, fila.Celdas[0].Tipo);
            Assert.Equal(7m, fila.Celdas[0].Numero);
            Assert.Equal("Ana López", fila.Celdas[1].Texto);
            Assert.Equal(TipoCeldaTabular.Fecha, fila.Celdas[2].Tipo);
            Assert.Equal(new DateOnly(2017, 8, 15), fila.Celdas[2].Fecha);
            Assert.Equal(TipoCeldaTabular.Booleano, fila.Celdas[3].Tipo);
            Assert.True(fila.Celdas[3].Booleano);
        }
        finally
        {
            EliminarSiExiste(ruta);
        }
    }

    [Fact]
    public void LectorGeneral_RechazaExtensionNoCompatible()
    {
        var exception = Assert.Throws<ImportacionTabularException>(
            () => new LectorImportacionTabular().Leer("alumnos.xls"));

        Assert.Contains(".xlsx", exception.Message, StringComparison.Ordinal);
        Assert.Contains(".csv", exception.Message, StringComparison.Ordinal);
    }

    private static void CrearLibroXlsx(string ruta)
    {
        using var documento = SpreadsheetDocument.Create(ruta, SpreadsheetDocumentType.Workbook);
        var libro = documento.AddWorkbookPart();
        libro.Workbook = new Workbook();

        var estilos = libro.AddNewPart<WorkbookStylesPart>();
        estilos.Stylesheet = new Stylesheet(
            new Fonts(new Font()),
            new Fills(new Fill()),
            new Borders(new Border()),
            new CellStyleFormats(new CellFormat()),
            new CellFormats(
                new CellFormat(),
                new CellFormat
                {
                    NumberFormatId = 14U,
                    ApplyNumberFormat = true,
                }));
        estilos.Stylesheet.Save();

        var compartidas = libro.AddNewPart<SharedStringTablePart>();
        compartidas.SharedStringTable = new SharedStringTable(
            new SharedStringItem(new Text("Ana López")));
        compartidas.SharedStringTable.Save();

        var hojas = libro.Workbook.AppendChild(new Sheets());

        var parteAlumnos = libro.AddNewPart<WorksheetPart>();
        var datosAlumnos = new SheetData();
        parteAlumnos.Worksheet = new Worksheet(datosAlumnos);

        var encabezado = new Row { RowIndex = 1U };
        encabezado.Append(
            CeldaTexto("A1", "No."),
            CeldaTexto("B1", "Nombre"),
            CeldaTexto("C1", "Fecha"),
            CeldaTexto("D1", "Activo"));
        datosAlumnos.Append(encabezado);

        var fila = new Row { RowIndex = 2U };
        fila.Append(
            new Cell
            {
                CellReference = "A2",
                DataType = CellValues.Number,
                CellValue = new CellValue("7"),
            },
            new Cell
            {
                CellReference = "B2",
                DataType = CellValues.SharedString,
                CellValue = new CellValue("0"),
            },
            new Cell
            {
                CellReference = "C2",
                DataType = CellValues.Number,
                StyleIndex = 1U,
                CellValue = new CellValue(
                    new DateTime(2017, 8, 15).ToOADate().ToString(CultureInfo.InvariantCulture)),
            },
            new Cell
            {
                CellReference = "D2",
                DataType = CellValues.Boolean,
                CellValue = new CellValue("1"),
            });
        datosAlumnos.Append(fila);
        parteAlumnos.Worksheet.Save();

        hojas.Append(new Sheet
        {
            Id = libro.GetIdOfPart(parteAlumnos),
            SheetId = 1U,
            Name = "Alumnos",
        });

        var parteVacia = libro.AddNewPart<WorksheetPart>();
        parteVacia.Worksheet = new Worksheet(new SheetData());
        parteVacia.Worksheet.Save();
        hojas.Append(new Sheet
        {
            Id = libro.GetIdOfPart(parteVacia),
            SheetId = 2U,
            Name = "Vacía",
        });

        var parteResumen = libro.AddNewPart<WorksheetPart>();
        var datosResumen = new SheetData();
        parteResumen.Worksheet = new Worksheet(datosResumen);
        var encabezadoResumen = new Row { RowIndex = 1U };
        encabezadoResumen.Append(CeldaTexto("A1", "Nombre"));
        datosResumen.Append(encabezadoResumen);
        var filaResumen = new Row { RowIndex = 2U };
        filaResumen.Append(CeldaTexto("A2", "Otro alumno"));
        datosResumen.Append(filaResumen);
        parteResumen.Worksheet.Save();
        hojas.Append(new Sheet
        {
            Id = libro.GetIdOfPart(parteResumen),
            SheetId = 3U,
            Name = "Resumen",
        });

        libro.Workbook.Save();
    }

    private static Cell CeldaTexto(string referencia, string texto) =>
        new(new InlineString(new Text(texto)))
        {
            CellReference = referencia,
            DataType = CellValues.InlineString,
        };

    private static string CrearRutaTemporal(string extension) =>
        Path.Combine(Path.GetTempPath(), $"sistema-docente-{Guid.NewGuid():N}{extension}");

    private static void EliminarSiExiste(string ruta)
    {
        if (File.Exists(ruta))
        {
            File.Delete(ruta);
        }
    }
}