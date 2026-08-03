using SistemaDocente.Core;

namespace SistemaDocente.Application;

public interface IGestionAsistenciaCasosUso
{
    AsistenciaDiaDetalle? Cargar(GrupoId grupoId, DateOnly fecha);

    AsistenciaDiaDetalle Preparar(GrupoId grupoId, DateOnly fecha);

    bool Existe(GrupoId grupoId, DateOnly fecha);

    AsistenciaMesDetalle CargarMes(GrupoId grupoId, int anio, int mes);

    AsistenciaDiaDetalle Guardar(
        GrupoId grupoId,
        DateOnly fecha,
        IReadOnlyCollection<EntradaEstadoAsistencia> entradas);

    ResultadoGuardadoMes GuardarMes(
        GrupoId grupoId,
        IReadOnlyCollection<EntradaDiaAsistencia> dias);
}