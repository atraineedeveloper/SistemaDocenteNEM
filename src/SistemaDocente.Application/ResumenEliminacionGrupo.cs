namespace SistemaDocente.Application;

public sealed record ResumenEliminacionGrupo(
    int Estudiantes,
    int DiasAsistencia,
    int Proyectos,
    int Actividades,
    int Entregas,
    int ConfiguracionSignificativa)
{
    public bool TieneDatos =>
        Estudiantes > 0
        || DiasAsistencia > 0
        || Proyectos > 0
        || Actividades > 0
        || Entregas > 0
        || ConfiguracionSignificativa > 0;
}
