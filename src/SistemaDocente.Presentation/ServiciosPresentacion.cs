using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Presentation;

public enum EstadoLecturaReferencia
{
    Ausente,
    Valida,
    Invalida,
}

public sealed record ResultadoLecturaReferencia(
    EstadoLecturaReferencia Estado,
    GrupoId? GrupoId = null);

public interface IAlmacenamientoEstadoAplicacion
{
    ResultadoLecturaReferencia Cargar();

    void Guardar(GrupoId grupoId);

    void Olvidar();
}

public interface IServicioMensajes
{
    void MostrarError(string mensaje);
}

public interface IServicioConfirmacion
{
    bool ConfirmarDesactivacion(string nombreEstudiante);
}

public interface IGestionGrupoPresentacion
{
    GrupoDetalle CrearGrupo(string nombreVisible);
    GrupoDetalle CargarGrupo(GrupoId grupoId);
    IReadOnlyList<GrupoDetalle> ListarGrupos();
    GrupoDetalle CambiarNombreGrupo(GrupoId grupoId, string nombreVisible);
    EstudianteDetalle AgregarEstudiante(
        GrupoId grupoId,
        string nombreVisible,
        int numeroLista,
        string primerApellido = "",
        string segundoApellido = "",
        string nombres = "",
        DateOnly? fechaNacimiento = null,
        GeneroEstudiante genero = GeneroEstudiante.NoEspecificado,
        DateOnly? fechaIngreso = null,
        string observaciones = "");
    EstudianteDetalle RenombrarEstudiante(GrupoId grupoId, EstudianteId estudianteId, string nombreVisible);
    EstudianteDetalle CambiarNumeroLista(GrupoId grupoId, EstudianteId estudianteId, int numeroLista);
    EstudianteDetalle EditarEstudiante(
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
        string observaciones = "");
    EstudianteDetalle DesactivarEstudiante(GrupoId grupoId, EstudianteId estudianteId);
    EstudianteDetalle ReactivarEstudiante(GrupoId grupoId, EstudianteId estudianteId);
    IReadOnlyList<EstudianteDetalle> ObtenerTodosLosEstudiantes(GrupoId grupoId);
}

public sealed class GestionGrupoPresentacion : IGestionGrupoPresentacion
{
    private readonly GestionGrupoCasosUso _casosUso;

    public GestionGrupoPresentacion(GestionGrupoCasosUso casosUso)
    {
        ArgumentNullException.ThrowIfNull(casosUso);
        _casosUso = casosUso;
    }

    public GrupoDetalle CrearGrupo(string nombreVisible) => _casosUso.CrearGrupo(nombreVisible);
    public GrupoDetalle CargarGrupo(GrupoId grupoId) => _casosUso.CargarGrupo(grupoId);
    public IReadOnlyList<GrupoDetalle> ListarGrupos() => _casosUso.ListarGrupos();
    public GrupoDetalle CambiarNombreGrupo(GrupoId grupoId, string nombreVisible) =>
        _casosUso.CambiarNombreGrupo(grupoId, nombreVisible);
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
        string observaciones = "") =>
        _casosUso.AgregarEstudiante(grupoId, nombreVisible, numeroLista, primerApellido, segundoApellido, nombres, fechaNacimiento, genero, fechaIngreso, observaciones);
    public EstudianteDetalle RenombrarEstudiante(GrupoId grupoId, EstudianteId estudianteId, string nombreVisible) =>
        _casosUso.RenombrarEstudiante(grupoId, estudianteId, nombreVisible);
    public EstudianteDetalle CambiarNumeroLista(GrupoId grupoId, EstudianteId estudianteId, int numeroLista) =>
        _casosUso.CambiarNumeroLista(grupoId, estudianteId, numeroLista);
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
        string observaciones = "") =>
        _casosUso.EditarEstudiante(grupoId, estudianteId, nombreVisible, numeroLista, primerApellido, segundoApellido, nombres, fechaNacimiento, genero, fechaIngreso, observaciones);
    public EstudianteDetalle DesactivarEstudiante(GrupoId grupoId, EstudianteId estudianteId) =>
        _casosUso.DesactivarEstudiante(grupoId, estudianteId);
    public EstudianteDetalle ReactivarEstudiante(GrupoId grupoId, EstudianteId estudianteId) =>
        _casosUso.ReactivarEstudiante(grupoId, estudianteId);
    public IReadOnlyList<EstudianteDetalle> ObtenerTodosLosEstudiantes(GrupoId grupoId) =>
        _casosUso.ObtenerTodosLosEstudiantes(grupoId);
}