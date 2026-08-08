using System.IO;

namespace SistemaDocente.App.Wpf;

public sealed record RutasAplicacion(
    string BaseSqlite,
    string EstadoAplicacion,
    bool EsDemostracion = false)
{
    public string DirectorioRespaldosSeguridad
    {
        get
        {
            var directorioDatos = Path.GetDirectoryName(Path.GetFullPath(BaseSqlite))
                ?? throw new InvalidOperationException("La ruta de la base SQLite no tiene directorio contenedor.");
            var directorioAplicacion = Directory.GetParent(directorioDatos)?.FullName
                ?? throw new InvalidOperationException("No fue posible determinar el directorio de la aplicación.");
            return Path.Combine(directorioAplicacion, "backups", "safety");
        }
    }

    public static RutasAplicacion DesdeLocalApplicationData(
        string localApplicationData,
        bool modoDemostracion = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(localApplicationData);
        var carpetaAplicacion = modoDemostracion ? "SistemaDocenteNEM-Demo" : "SistemaDocenteNEM";
        var directorio = Path.Combine(localApplicationData, carpetaAplicacion, "data");
        return new(
            Path.Combine(directorio, "sistema-docente.db"),
            Path.Combine(directorio, "app-state.json"),
            modoDemostracion);
    }

    /// <summary>
    /// Elimina exclusivamente el almacenamiento aislado de demostración. Esta operación
    /// se niega a ejecutarse para rutas de producción para evitar pérdidas accidentales.
    /// </summary>
    public void ReiniciarDemostracion()
    {
        if (!EsDemostracion)
            throw new InvalidOperationException("Sólo pueden reiniciarse rutas de demostración.");

        foreach (var archivo in new[]
        {
            BaseSqlite,
            BaseSqlite + "-wal",
            BaseSqlite + "-shm",
            EstadoAplicacion,
        })
        {
            if (File.Exists(archivo)) File.Delete(archivo);
        }
    }
}
