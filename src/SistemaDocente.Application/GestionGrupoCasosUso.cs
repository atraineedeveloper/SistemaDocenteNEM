using SistemaDocente.Core;

namespace SistemaDocente.Application;

public sealed class GestionGrupoCasosUso
{
    private readonly IAlmacenamientoGrupos _almacenamiento;

    public GestionGrupoCasosUso(IAlmacenamientoGrupos almacenamiento)
    {
        ArgumentNullException.ThrowIfNull(almacenamiento);
        _almacenamiento = almacenamiento;
    }

    public GrupoDetalle CrearGrupo(string nombreVisible)
    {
        var grupo = Grupo.Crear(nombreVisible);
        _almacenamiento.Guardar(grupo);
        return Proyectar(grupo);
    }

    public GrupoDetalle CargarGrupo(GrupoId grupoId) => Proyectar(CargarRequerido(grupoId));

    public bool Existe(GrupoId grupoId) => _almacenamiento.Existe(grupoId);

    public GrupoDetalle CambiarNombreGrupo(GrupoId grupoId, string nombreVisible)
    {
        var grupo = CargarRequerido(grupoId);
        grupo.Renombrar(nombreVisible);
        _almacenamiento.Guardar(grupo);
        return Proyectar(grupo);
    }

    public EstudianteDetalle AgregarEstudiante(
        GrupoId grupoId,
        string nombreVisible,
        int numeroLista)
    {
        var grupo = CargarRequerido(grupoId);
        var estudiante = grupo.AgregarEstudiante(nombreVisible, numeroLista);
        _almacenamiento.Guardar(grupo);
        return Proyectar(estudiante);
    }

    public EstudianteDetalle RenombrarEstudiante(
        GrupoId grupoId,
        EstudianteId estudianteId,
        string nombreVisible)
    {
        var grupo = CargarRequerido(grupoId);
        grupo.RenombrarEstudiante(estudianteId, nombreVisible);
        _almacenamiento.Guardar(grupo);
        return Proyectar(ObtenerEstudiante(grupo, estudianteId));
    }

    public EstudianteDetalle CambiarNumeroLista(
        GrupoId grupoId,
        EstudianteId estudianteId,
        int numeroLista)
    {
        var grupo = CargarRequerido(grupoId);
        grupo.CambiarNumeroLista(estudianteId, numeroLista);
        _almacenamiento.Guardar(grupo);
        return Proyectar(ObtenerEstudiante(grupo, estudianteId));
    }

    public EstudianteDetalle DesactivarEstudiante(
        GrupoId grupoId,
        EstudianteId estudianteId)
    {
        var grupo = CargarRequerido(grupoId);
        grupo.DesactivarEstudiante(estudianteId);
        _almacenamiento.Guardar(grupo);
        return Proyectar(ObtenerEstudiante(grupo, estudianteId));
    }

    public EstudianteDetalle ReactivarEstudiante(
        GrupoId grupoId,
        EstudianteId estudianteId)
    {
        var grupo = CargarRequerido(grupoId);
        grupo.ReactivarEstudiante(estudianteId);
        _almacenamiento.Guardar(grupo);
        return Proyectar(ObtenerEstudiante(grupo, estudianteId));
    }

    public IReadOnlyList<EstudianteDetalle> ObtenerTodosLosEstudiantes(GrupoId grupoId) =>
        ProyectarEstudiantes(CargarRequerido(grupoId).Estudiantes);

    public IReadOnlyList<EstudianteDetalle> ObtenerEstudiantesActivos(GrupoId grupoId) =>
        ProyectarEstudiantes(CargarRequerido(grupoId).Estudiantes.Where(x => x.EstaActivo));

    private Grupo CargarRequerido(GrupoId grupoId) =>
        _almacenamiento.Cargar(grupoId)
        ?? throw new GrupoNoEncontradoException($"No existe el grupo {grupoId}.");

    private static Estudiante ObtenerEstudiante(Grupo grupo, EstudianteId estudianteId) =>
        grupo.Estudiantes.Single(estudiante => estudiante.Id == estudianteId);

    private static GrupoDetalle Proyectar(Grupo grupo) =>
        new(grupo.Id, grupo.NombreVisible, ProyectarEstudiantes(grupo.Estudiantes));

    private static EstudianteDetalle Proyectar(Estudiante estudiante) =>
        new(
            estudiante.Id,
            estudiante.NombreVisible,
            estudiante.NumeroLista,
            estudiante.EstaActivo);

    private static EstudianteDetalle[] ProyectarEstudiantes(IEnumerable<Estudiante> estudiantes) =>
        estudiantes
            .OrderBy(estudiante => estudiante.NumeroLista)
            .ThenBy(estudiante => estudiante.NombreVisible, StringComparer.Ordinal)
            .ThenBy(estudiante => estudiante.Id.Valor)
            .Select(Proyectar)
            .ToArray();
}