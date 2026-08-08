using System.Text.Json;

using SistemaDocente.Application;

namespace SistemaDocente.Data.Tests;

public sealed class RegistroDiagnosticoSeguroArchivoTests
{
    [Fact]
    public void RegistrarNoPersisteMensajeRutaNiStackTraceDeLaExcepcion()
    {
        var directorio = Path.Combine(Path.GetTempPath(), $"aularaiz-diagnostics-{Guid.NewGuid():N}");
        var archivo = Path.Combine(directorio, "events.jsonl");

        try
        {
            var registro = new RegistroDiagnosticoSeguroArchivo(
                archivo,
                ModoDiagnosticoLocal.Produccion);
            var exception = CapturarExcepcionConDatoSensible();

            registro.Registrar(exception, CategoriaEventoDiagnostico.FalloNoControlado);

            var contenido = File.ReadAllText(archivo);
            Assert.DoesNotContain("ALUMNA_SECRETA", contenido, StringComparison.Ordinal);
            Assert.DoesNotContain("C:\\Users\\Docente", contenido, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(nameof(CapturarExcepcionConDatoSensible), contenido, StringComparison.Ordinal);

            using var json = JsonDocument.Parse(contenido);
            var raiz = json.RootElement;
            Assert.Equal("FalloNoControlado", raiz.GetProperty("categoria").GetString());
            Assert.Equal(typeof(InvalidOperationException).FullName, raiz.GetProperty("tipoExcepcion").GetString());
            Assert.Equal("Produccion", raiz.GetProperty("modo").GetString());
            Assert.Equal(64, raiz.GetProperty("huellaTecnica").GetString()!.Length);
            Assert.False(raiz.TryGetProperty("message", out _));
            Assert.False(raiz.TryGetProperty("stackTrace", out _));
        }
        finally
        {
            if (Directory.Exists(directorio)) Directory.Delete(directorio, true);
        }
    }

    private static Exception CapturarExcepcionConDatoSensible()
    {
        try
        {
            throw new InvalidOperationException(
                "ALUMNA_SECRETA apareció al procesar C:\\Users\\Docente\\datos.db");
        }
        catch (Exception exception)
        {
            return exception;
        }
    }
}