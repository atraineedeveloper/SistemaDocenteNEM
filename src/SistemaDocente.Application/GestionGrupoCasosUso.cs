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

    public IReadOnlyList<GrupoDetalle> ListarGrupos() =>
        _almacenamiento.ListarTodos().Select(Proyectar).ToList();

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
        int numeroLista,
        string primerApellido = "",
        string segundoApellido = "",
        string nombres = "",
        DateOnly? fechaNacimiento = null,
        GeneroEstudiante genero = GeneroEstudiante.NoEspecificado,
        DateOnly? fechaIngreso = null,
        string observaciones = "",
        GradoPrimaria grado = GradoPrimaria.NoEspecificado)
    {
        var grupo = CargarRequerido(grupoId);
        var estudiante = grupo.AgregarEstudiante(
            nombreVisible,
            numeroLista,
            primerApellido,
            segundoApellido,
            nombres,
            fechaNacimiento,
            genero,
            fechaIngreso,
            observaciones,
            grado);
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

    public EstudianteDetalle CambiarGradoEstudiante(
        GrupoId grupoId,
        EstudianteId estudianteId,
        GradoPrimaria grado)
    {
        var grupo = CargarRequerido(grupoId);
        grupo.CambiarGradoEstudiante(estudianteId, grado);
        _almacenamiento.Guardar(grupo);
        return Proyectar(ObtenerEstudiante(grupo, estudianteId));
    }

    public EstudianteDetalle EditarEstudiante(
        GrupoId grupoId,
        EstudianteId estudianteId,
        string nombreVisible,
        int numeroLista,
        string primerApellido = "",
        string segundoApellido = "",
        string nombres = "",
        DateOnly? fechaNacimiento = null,
        GeneroEstudiante genero = GeneroEstudiante.NoEspecificado,
        DateOnly? fechaIngreso = null,
        string observaciones = "",
        GradoPrimaria? grado = null)
    {
        var grupo = CargarRequerido(grupoId);
        grupo.ActualizarDatosEstudiante(
            estudianteId,
            nombreVisible,
            primerApellido,
            segundoApellido,
            nombres,
            fechaNacimiento,
            genero,
            fechaIngreso,
            observaciones,
            grado);
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
            estudiante.PrimerApellido,
            estudiante.SegundoApellido,
            estudiante.Nombres,
            estudiante.FechaNacimiento,
            estudiante.Edad,
            estudiante.Genero,
            estudiante.FechaIngreso,
            estudiante.Observaciones,
            estudiante.NumeroLista,
            estudiante.EstaActivo,
            estudiante.Grado);

    private static EstudianteDetalle[] ProyectarEstudiantes(IEnumerable<Estudiante> estudiantes) =>
        estudiantes
            .OrderBy(estudiante => estudiante.NumeroLista)
            .ThenBy(estudiante => estudiante.NombreVisible, StringComparer.Ordinal)
            .ThenBy(estudiante => estudiante.Id.Valor)
            .Select(Proyectar)
            .ToArray();
}