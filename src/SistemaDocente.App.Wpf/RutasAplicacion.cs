using System.IO;

namespace SistemaDocente.App.Wpf;

public sealed record RutasAplicacion(string BaseSqlite, string EstadoAplicacion)
{
    public static RutasAplicacion DesdeLocalApplicationData(string localApplicationData)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(localApplicationData);
        var directorio = Path.Combine(localApplicationData, "SistemaDocenteNEM", "data");
        return new(
            Path.Combine(directorio, "sistema-docente.db"),
            Path.Combine(directorio, "app-state.json"));
    }
}