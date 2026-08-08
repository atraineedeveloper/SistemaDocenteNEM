using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using SistemaDocente.Application;

namespace SistemaDocente.Data;

public sealed class RegistroDiagnosticoSeguroArchivo : IRegistroDiagnosticoSeguro
{
    private static readonly UTF8Encoding Utf8SinBom = new(false);
    private static readonly JsonSerializerOptions OpcionesJson = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly string _rutaArchivo;
    private readonly ModoDiagnosticoLocal _modo;

    public RegistroDiagnosticoSeguroArchivo(string rutaArchivo, ModoDiagnosticoLocal modo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rutaArchivo);
        if (!Enum.IsDefined(modo))
            throw new ArgumentOutOfRangeException(nameof(modo));

        _rutaArchivo = Path.GetFullPath(rutaArchivo);
        _modo = modo;
    }

    public static RegistroDiagnosticoSeguroArchivo DesdeLocalApplicationData(
        string localApplicationData,
        bool modoDemostracion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(localApplicationData);
        var carpeta = modoDemostracion ? "SistemaDocenteNEM-Demo" : IdentidadProducto.IdentificadorTecnicoLegado;
        var ruta = Path.Combine(localApplicationData, carpeta, "diagnostics", "events.jsonl");
        return new RegistroDiagnosticoSeguroArchivo(
            ruta,
            modoDemostracion ? ModoDiagnosticoLocal.Demostracion : ModoDiagnosticoLocal.Produccion);
    }

    public void Registrar(Exception exception, CategoriaEventoDiagnostico categoria)
    {
        ArgumentNullException.ThrowIfNull(exception);

        try
        {
            var evento = DiagnosticoSeguro.CrearEvento(exception, categoria, _modo);
            var directorio = Path.GetDirectoryName(_rutaArchivo)
                ?? throw new InvalidOperationException("La ruta de diagnóstico no tiene directorio contenedor.");
            Directory.CreateDirectory(directorio);
            var linea = JsonSerializer.Serialize(evento, OpcionesJson) + Environment.NewLine;
            File.AppendAllText(_rutaArchivo, linea, Utf8SinBom);
        }
        catch
        {
            // El diagnóstico nunca debe interrumpir el trabajo ni provocar un segundo fallo.
        }
    }
}