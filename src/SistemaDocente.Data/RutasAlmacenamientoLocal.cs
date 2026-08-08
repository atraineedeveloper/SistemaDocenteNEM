using SistemaDocente.Application;

namespace SistemaDocente.Data;

public sealed record RutasAlmacenamientoLocal(
    string DirectorioPerfil,
    string DirectorioDatos,
    string BaseSqlite,
    string EstadoAplicacion,
    string DirectorioRespaldosSeguridad,
    string DirectorioDiagnosticos,
    bool EsDemostracion)
{
    public static RutasAlmacenamientoLocal DesdeLocalApplicationData(
        string localApplicationData,
        bool modoDemostracion = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(localApplicationData);

        var carpetaAplicacion = modoDemostracion
            ? $"{IdentidadProducto.IdentificadorTecnicoLegado}-Demo"
            : IdentidadProducto.IdentificadorTecnicoLegado;
        var perfil = Path.Combine(Path.GetFullPath(localApplicationData), carpetaAplicacion);
        var datos = Path.Combine(perfil, "data");

        return new RutasAlmacenamientoLocal(
            perfil,
            datos,
            Path.Combine(datos, "sistema-docente.db"),
            Path.Combine(datos, "app-state.json"),
            Path.Combine(perfil, "backups", "safety"),
            Path.Combine(perfil, "diagnostics"),
            modoDemostracion);
    }
}