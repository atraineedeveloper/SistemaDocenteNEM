using System.IO;

using SistemaDocente.Data;

namespace SistemaDocente.App.Wpf;

public sealed record RutasAplicacion(
    string BaseSqlite,
    string EstadoAplicacion,
    string DirectorioRespaldosSeguridad,
    bool EsDemostracion = false)
{
    public static RutasAplicacion DesdeLocalApplicationData(
        string localApplicationData,
        bool modoDemostracion = false)
    {
        var rutas = RutasAlmacenamientoLocal.DesdeLocalApplicationData(
            localApplicationData,
            modoDemostracion);
        return new(
            rutas.BaseSqlite,
            rutas.EstadoAplicacion,
            rutas.DirectorioRespaldosSeguridad,
            rutas.EsDemostracion);
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