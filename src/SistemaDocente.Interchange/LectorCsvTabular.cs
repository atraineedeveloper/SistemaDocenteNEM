using System.Text;

using SistemaDocente.Application;

namespace SistemaDocente.Interchange;

public sealed class LectorCsvTabular : ILectorImportacionTabular
{
    private static readonly char[] DelimitadoresSoportados = [',', ';', '\t'];

    public DocumentoTabular Leer(string rutaArchivo) => Leer(rutaArchivo, null);

    public static DocumentoTabular Leer(string rutaArchivo, char? delimitador)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rutaArchivo);

        try
        {
            var contenido = File.ReadAllText(
                rutaArchivo,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true));

            var delimitadorReal = delimitador ?? DetectarDelimitador(contenido);
            if (!DelimitadoresSoportados.Contains(delimitadorReal))
            {
                throw new ImportacionTabularException(
                    "El delimitador CSV seleccionado no es compatible.",
                    "csv-delimiter-unsupported");
            }

            var filas = Parsear(contenido, delimitadorReal)
                .Where(fila => fila.Valores.Any(valor => !string.IsNullOrWhiteSpace(valor)))
                .ToArray();

            if (filas.Length == 0)
            {
                throw new ImportacionTabularException("El archivo CSV no contiene filas utilizables.");
            }

            var ancho = filas.Max(fila => fila.Valores.Count);
            var encabezados = ConvertirCeldas(filas[0].Valores, ancho);
            var datos = filas
                .Skip(1)
                .Select(fila => new FilaTabular(
                    fila.NumeroLineaOrigen,
                    ConvertirCeldas(fila.Valores, ancho)))
                .ToArray();

            var hoja = new HojaTabular(
                Path.GetFileNameWithoutExtension(rutaArchivo),
                encabezados,
                datos);

            return new DocumentoTabular(Path.GetFileName(rutaArchivo), [hoja]);
        }
        catch (ImportacionTabularException)
        {
            throw;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or DecoderFallbackException)
        {
            throw new ImportacionTabularException("No se pudo leer el archivo CSV seleccionado.", exception);
        }
    }

    private static char DetectarDelimitador(string contenido)
    {
        var conteos = DelimitadoresSoportados
            .Select(delimitador => new
            {
                Delimitador = delimitador,
                Conteo = ContarSeparadoresPrimerRegistro(contenido, delimitador),
            })
            .OrderByDescending(item => item.Conteo)
            .ToArray();

        if (conteos[0].Conteo <= 0 || (conteos.Length > 1 && conteos[0].Conteo == conteos[1].Conteo))
        {
            throw new ImportacionTabularException(
                "No fue posible detectar de forma unívoca el delimitador CSV. Selecciónalo explícitamente.",
                "csv-delimiter-ambiguous");
        }

        return conteos[0].Delimitador;
    }

    private static int ContarSeparadoresPrimerRegistro(string contenido, char delimitador)
    {
        var entreComillas = false;
        var conteo = 0;

        for (var indice = 0; indice < contenido.Length; indice++)
        {
            var caracter = contenido[indice];
            if (caracter == '"')
            {
                if (entreComillas && indice + 1 < contenido.Length && contenido[indice + 1] == '"')
                {
                    indice++;
                    continue;
                }

                entreComillas = !entreComillas;
                continue;
            }

            if (!entreComillas && caracter == delimitador)
            {
                conteo++;
                continue;
            }

            if (!entreComillas && caracter is '\r' or '\n')
            {
                break;
            }
        }

        return conteo;
    }

    private static List<FilaCsv> Parsear(string contenido, char delimitador)
    {
        var filas = new List<FilaCsv>();
        var valores = new List<string>();
        var campo = new StringBuilder();
        var entreComillas = false;
        var numeroLinea = 1;
        var lineaInicioRegistro = 1;

        for (var indice = 0; indice < contenido.Length; indice++)
        {
            var caracter = contenido[indice];

            if (caracter == '"')
            {
                if (entreComillas && indice + 1 < contenido.Length && contenido[indice + 1] == '"')
                {
                    campo.Append('"');
                    indice++;
                    continue;
                }

                entreComillas = !entreComillas;
                continue;
            }

            if (!entreComillas && caracter == delimitador)
            {
                valores.Add(campo.ToString());
                campo.Clear();
                continue;
            }

            if (caracter is '\r' or '\n')
            {
                if (entreComillas)
                {
                    campo.Append('\n');
                    if (caracter == '\r' && indice + 1 < contenido.Length && contenido[indice + 1] == '\n')
                    {
                        indice++;
                    }

                    numeroLinea++;
                    continue;
                }

                valores.Add(campo.ToString());
                campo.Clear();
                filas.Add(new FilaCsv(lineaInicioRegistro, [.. valores]));
                valores.Clear();

                if (caracter == '\r' && indice + 1 < contenido.Length && contenido[indice + 1] == '\n')
                {
                    indice++;
                }

                numeroLinea++;
                lineaInicioRegistro = numeroLinea;
                continue;
            }

            campo.Append(caracter);
        }

        if (entreComillas)
        {
            throw new ImportacionTabularException("El archivo CSV contiene un campo entre comillas sin cierre.");
        }

        if (campo.Length > 0 || valores.Count > 0)
        {
            valores.Add(campo.ToString());
            filas.Add(new FilaCsv(lineaInicioRegistro, [.. valores]));
        }

        return filas;
    }

    private static CeldaTabular[] ConvertirCeldas(IReadOnlyList<string> valores, int ancho)
    {
        var celdas = new CeldaTabular[ancho];
        for (var indice = 0; indice < ancho; indice++)
        {
            celdas[indice] = indice < valores.Count
                ? CeldaTabular.DesdeTexto(valores[indice])
                : CeldaTabular.Vacia;
        }

        return celdas;
    }

    private sealed record FilaCsv(int NumeroLineaOrigen, IReadOnlyList<string> Valores);
}
