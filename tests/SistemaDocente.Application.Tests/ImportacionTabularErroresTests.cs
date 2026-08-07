using System.Text;

using SistemaDocente.Application;
using SistemaDocente.Interchange;

namespace SistemaDocente.Application.Tests;

public sealed class ImportacionTabularErroresTests
{
    [Fact]
    public void CsvOmiteFilasCompletamenteVacias()
    {
        var ruta = CrearRutaTemporal(".csv");

        try
        {
            File.WriteAllText(
                ruta,
                "No.,Nombre\n\n1,Ana López\n,\n",
                new UTF8Encoding(false));

            var documento = new LectorCsvTabular().Leer(ruta);

            var hoja = Assert.Single(documento.Hojas);
            var fila = Assert.Single(hoja.Filas);
            Assert.Equal(3, fila.NumeroOrigen);
            Assert.Equal("Ana López", fila.Celdas[1].Texto);
        }
        finally
        {
            EliminarSiExiste(ruta);
        }
    }

    [Fact]
    public void CsvConComillasSinCierreSeRechaza()
    {
        var ruta = CrearRutaTemporal(".csv");

        try
        {
            File.WriteAllText(
                ruta,
                "No.,Nombre\n1,\"Ana López",
                new UTF8Encoding(false));

            var exception = Assert.Throws<ImportacionTabularException>(
                () => new LectorCsvTabular().Leer(ruta));

            Assert.Contains("sin cierre", exception.Message, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Ana López", exception.Message, StringComparison.Ordinal);
        }
        finally
        {
            EliminarSiExiste(ruta);
        }
    }

    [Fact]
    public void XlsxMalformadoSeTraduceAErrorDeImportacion()
    {
        var ruta = CrearRutaTemporal(".xlsx");

        try
        {
            File.WriteAllBytes(ruta, [0x53, 0x69, 0x73, 0x74, 0x65, 0x6D, 0x61]);

            var exception = Assert.Throws<ImportacionTabularException>(
                () => new LectorXlsxTabular().Leer(ruta));

            Assert.Contains("XLSX", exception.Message, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(ruta, exception.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            EliminarSiExiste(ruta);
        }
    }

    [Fact]
    public void CsvVacioSeRechazaSinCrearDocumentoParcial()
    {
        var ruta = CrearRutaTemporal(".csv");

        try
        {
            File.WriteAllText(ruta, string.Empty, new UTF8Encoding(false));

            Assert.Throws<ImportacionTabularException>(
                () => new LectorCsvTabular().Leer(ruta));
        }
        finally
        {
            EliminarSiExiste(ruta);
        }
    }

    private static string CrearRutaTemporal(string extension) =>
        Path.Combine(Path.GetTempPath(), $"sistema-docente-import-error-{Guid.NewGuid():N}{extension}");

    private static void EliminarSiExiste(string ruta)
    {
        if (File.Exists(ruta))
        {
            File.Delete(ruta);
        }
    }
}