using System.IO;
using System.Text.Json;

using SistemaDocente.Core;
using SistemaDocente.Presentation;

namespace SistemaDocente.App.Wpf;

public sealed class AlmacenamientoEstadoJson : IAlmacenamientoEstadoAplicacion
{
    private readonly string _ruta;

    public AlmacenamientoEstadoJson(string ruta)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ruta);
        _ruta = Path.GetFullPath(ruta);
    }

    public ResultadoLecturaReferencia Cargar()
    {
        if (!File.Exists(_ruta)) return new(EstadoLecturaReferencia.Ausente);
        try
        {
            var contenido = File.ReadAllText(_ruta);
            if (string.IsNullOrWhiteSpace(contenido)) return new(EstadoLecturaReferencia.Invalida);
            using var documento = JsonDocument.Parse(contenido);
            if (documento.RootElement.ValueKind != JsonValueKind.Object) return new(EstadoLecturaReferencia.Invalida);
            var propiedades = documento.RootElement.EnumerateObject().ToArray();
            if (propiedades.Length != 1 || propiedades[0].Name != "GrupoId"
                || propiedades[0].Value.ValueKind != JsonValueKind.String
                || !Guid.TryParse(propiedades[0].Value.GetString(), out var valor) || valor == Guid.Empty)
            {
                return new(EstadoLecturaReferencia.Invalida);
            }
            return new(EstadoLecturaReferencia.Valida, GrupoId.DesdeGuid(valor));
        }
        catch (JsonException)
        {
            return new(EstadoLecturaReferencia.Invalida);
        }
    }

    public void Guardar(GrupoId grupoId)
    {
        if (grupoId == default) throw new ArgumentException("La identidad no puede estar vacía.", nameof(grupoId));
        var directorio = Path.GetDirectoryName(_ruta)!;
        Directory.CreateDirectory(directorio);
        var temporal = Path.Combine(directorio, $".{Path.GetFileName(_ruta)}.{Guid.NewGuid():N}.tmp");
        try
        {
            var json = JsonSerializer.Serialize(new EstadoJson(grupoId.Valor));
            using (var flujo = new FileStream(temporal, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough))
            using (var escritor = new StreamWriter(flujo))
            {
                escritor.Write(json);
                escritor.Flush();
                flujo.Flush(true);
            }
            if (File.Exists(_ruta)) File.Replace(temporal, _ruta, null);
            else File.Move(temporal, _ruta);
        }
        finally
        {
            if (File.Exists(temporal)) File.Delete(temporal);
        }
    }

    public void Olvidar()
    {
        if (File.Exists(_ruta)) File.Delete(_ruta);
    }

    private sealed record EstadoJson(Guid GrupoId);
}