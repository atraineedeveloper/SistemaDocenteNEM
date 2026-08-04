using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Presentation;

public interface IGestionAsistenciaPresentacion
{
    AsistenciaDiaDetalle Preparar(GrupoId grupoId, DateOnly fecha);

    AsistenciaMesDetalle CargarMes(GrupoId grupoId, int anio, int mes);

    AsistenciaDiaDetalle Guardar(
        GrupoId grupoId,
        DateOnly fecha,
        IReadOnlyCollection<EntradaEstadoAsistencia> entradas);

    ResultadoGuardadoMes GuardarMes(
        GrupoId grupoId,
        IReadOnlyCollection<EntradaDiaAsistencia> dias);
}

public sealed class GestionAsistenciaPresentacion : IGestionAsistenciaPresentacion
{
    private readonly IGestionAsistenciaCasosUso _casosUso;

    public GestionAsistenciaPresentacion(IGestionAsistenciaCasosUso casosUso)
    {
        ArgumentNullException.ThrowIfNull(casosUso);
        _casosUso = casosUso;
    }

    public AsistenciaDiaDetalle Preparar(GrupoId grupoId, DateOnly fecha) =>
        _casosUso.Preparar(grupoId, fecha);

    public AsistenciaMesDetalle CargarMes(GrupoId grupoId, int anio, int mes) =>
        _casosUso.CargarMes(grupoId, anio, mes);

    public AsistenciaDiaDetalle Guardar(
        GrupoId grupoId,
        DateOnly fecha,
        IReadOnlyCollection<EntradaEstadoAsistencia> entradas) =>
        _casosUso.Guardar(grupoId, fecha, entradas);

    public ResultadoGuardadoMes GuardarMes(
        GrupoId grupoId,
        IReadOnlyCollection<EntradaDiaAsistencia> dias) =>
        _casosUso.GuardarMes(grupoId, dias);
}

public interface IRelojLocal
{
    DateOnly Hoy { get; }
}

public enum DecisionCambiosPendientes
{
    Guardar,
    Descartar,
    Cancelar,
}

public interface IDialogoCambiosPendientes
{
    DecisionCambiosPendientes ConfirmarCambiosPendientes();

    DecisionCambiosPendientes ConfirmarCambiosPendientes(string contexto) =>
        ConfirmarCambiosPendientes();
}
