using System.IO;

namespace SistemaDocente.App.Wpf;

public sealed record RutasAplicacion(
    string BaseSqlite,
    string EstadoAplicacion,
    bool EsDemostracion = false)
{
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