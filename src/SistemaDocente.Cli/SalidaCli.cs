using System.Text.Json;
using System.Text.Json.Serialization;

namespace SistemaDocente.Cli;

public sealed record PrivacidadSalidaCli(
    string Classification,
    bool IncludesPersonalData,
    bool IncludesFreeText,
    bool NetworkAccess);

public sealed record ErrorSalidaCli(string Code, string Message);

public sealed record RespuestaCli(
    string SchemaVersion,
    string Command,
    string Mode,
    bool Success,
    object? Data,
    PrivacidadSalidaCli Privacy,
    IReadOnlyList<string> Warnings,
    ErrorSalidaCli? Error = null);

public static class SerializadorCli
{
    private static readonly JsonSerializerOptions Opciones = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    public static string Json(RespuestaCli respuesta) => JsonSerializer.Serialize(respuesta, Opciones);
}