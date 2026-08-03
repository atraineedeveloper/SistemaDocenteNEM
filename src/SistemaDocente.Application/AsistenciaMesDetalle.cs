using SistemaDocente.Core;

namespace SistemaDocente.Application;

public enum TipoCeldaAsistencia
{
    NoAplicable,
    Borrador,
    Confirmada,
}

public sealed record AsistenciaCeldaDetalle(
    DateOnly Fecha,
    EstadoAsistencia? Estado,
    TipoCeldaAsistencia Tipo);

public sealed record AsistenciaDiaColumnaDetalle(
    DateOnly Fecha,
    int NumeroDia,
    string AbreviaturaDiaSemana,
    bool EsLaborable,
    bool ExisteRegistroPersistido,
    bool EsCierreSemana = false);

public sealed record AsistenciaEstudianteMesDetalle(
    EstudianteId EstudianteId,
    int NumeroLista,
    string NombreVisible,
    bool EstaActivoActualmente,
    IReadOnlyList<AsistenciaCeldaDetalle> Estados,
    int Presentes,
    int Faltas,
    int Retardos,
    int FaltasJustificadas,
    double? PorcentajeConfirmado);

public sealed record AsistenciaMesDetalle(
    GrupoId GrupoId,
    int Anio,
    int Mes,
    IReadOnlyList<AsistenciaDiaColumnaDetalle> Dias,
    IReadOnlyList<AsistenciaEstudianteMesDetalle> Estudiantes);

public sealed record EntradaDiaAsistencia(
    DateOnly Fecha,
    IReadOnlyCollection<EntradaEstadoAsistencia> Entradas);

public sealed record ResultadoGuardadoMes(
    IReadOnlyList<DateOnly> FechasGuardadas,
    IReadOnlyList<DateOnly> FechasPendientes);