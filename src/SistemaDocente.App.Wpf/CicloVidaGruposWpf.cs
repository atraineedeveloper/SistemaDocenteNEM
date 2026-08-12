using SistemaDocente.Application;
using SistemaDocente.Core;
using SistemaDocente.Presentation;

namespace SistemaDocente.App.Wpf;

internal static class CicloVidaGruposWpf
{
    private static GestionGrupoCasosUso? _gestion;
    private static IAlmacenamientoEstadoAplicacion? _estado;

    internal static void Configurar(
        GestionGrupoCasosUso gestion,
        IAlmacenamientoEstadoAplicacion estado)
    {
        _gestion = gestion ?? throw new ArgumentNullException(nameof(gestion));
        _estado = estado ?? throw new ArgumentNullException(nameof(estado));
    }

    internal static IReadOnlyList<GrupoDetalle> ListarArchivados() =>
        ObtenerGestion().ListarGruposArchivados();

    internal static void Archivar(GrupoId grupoId)
    {
        ObtenerGestion().ArchivarGrupo(grupoId);
        LimpiarReferenciaSiActual(grupoId);
    }

    internal static void Restaurar(GrupoId grupoId) =>
        ObtenerGestion().RestaurarGrupo(grupoId);

    internal static ResumenEliminacionGrupo ObtenerResumenEliminacion(GrupoId grupoId) =>
        ObtenerGestion().ObtenerResumenEliminacion(grupoId);

    internal static void Eliminar(GrupoId grupoId)
    {
        ObtenerGestion().EliminarGrupo(grupoId);
        LimpiarReferenciaSiActual(grupoId);
    }

    private static GestionGrupoCasosUso ObtenerGestion() =>
        _gestion ?? throw new InvalidOperationException(
            "El ciclo de vida de grupos todavía no fue configurado.");

    private static void LimpiarReferenciaSiActual(GrupoId grupoId)
    {
        var estado = _estado ?? throw new InvalidOperationException(
            "El estado local todavía no fue configurado.");
        var referencia = estado.Cargar();
        if (referencia.Estado == EstadoLecturaReferencia.Valida
            && referencia.GrupoId == grupoId)
        {
            estado.Olvidar();
        }
    }
}
