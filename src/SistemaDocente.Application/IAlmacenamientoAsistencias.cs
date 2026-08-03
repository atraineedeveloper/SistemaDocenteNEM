using SistemaDocente.Core;

namespace SistemaDocente.Application;

public interface IAlmacenamientoAsistencias
{
    AsistenciaDiaria? Cargar(GrupoId grupoId, DateOnly fecha);

    bool Existe(GrupoId grupoId, DateOnly fecha);

    IReadOnlyList<AsistenciaDiaria> CargarIntervalo(
        GrupoId grupoId,
        DateOnly desde,
        DateOnly hasta);

    void Guardar(AsistenciaDiaria asistencia);
}