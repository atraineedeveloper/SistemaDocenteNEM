using System.Collections.ObjectModel;

namespace SistemaDocente.Core;

public sealed class Grupo
{
    private const int LongitudMaximaNombre = 100;
    private const int LongitudMaximaNombreEstudiante = 150;
    private readonly List<Estudiante> _estudiantes = [];
    private readonly ReadOnlyCollection<Estudiante> _vistaEstudiantes;

    private Grupo(string nombreVisible)
    {
        Id = GrupoId.Crear();
        NombreVisible = nombreVisible;
        _vistaEstudiantes = _estudiantes.AsReadOnly();
    }

    public GrupoId Id { get; }

    public string NombreVisible { get; private set; }

    public IReadOnlyList<Estudiante> Estudiantes => _vistaEstudiantes;

    public IReadOnlyList<Estudiante> EstudiantesActivos => Array.AsReadOnly(
        _estudiantes
            .Where(estudiante => estudiante.EstaActivo)
            .OrderBy(estudiante => estudiante.NumeroLista)
            .ThenBy(estudiante => estudiante.NombreVisible, StringComparer.Ordinal)
            .ToArray());

    public static Grupo Crear(string nombreVisible)
    {
        var nombreNormalizado = NormalizadorNombreVisible.NormalizarYValidar(
            nombreVisible,
            LongitudMaximaNombre,
            "El nombre del grupo");

        return new Grupo(nombreNormalizado);
    }

    public void Renombrar(string nombreVisible)
    {
        var nombreNormalizado = NormalizadorNombreVisible.NormalizarYValidar(
            nombreVisible,
            LongitudMaximaNombre,
            "El nombre del grupo");

        NombreVisible = nombreNormalizado;
    }

    public Estudiante AgregarEstudiante(string nombreVisible, int numeroLista)
    {
        var nombreNormalizado = ValidarNombreEstudiante(nombreVisible);
        ValidarNumeroLista(numeroLista);
        ValidarNumeroDisponible(numeroLista);

        var estudiante = new Estudiante(nombreNormalizado, numeroLista);
        _estudiantes.Add(estudiante);

        return estudiante;
    }

    public void RenombrarEstudiante(EstudianteId estudianteId, string nombreVisible)
    {
        var estudiante = ObtenerEstudiante(estudianteId);
        var nombreNormalizado = ValidarNombreEstudiante(nombreVisible);

        estudiante.Renombrar(nombreNormalizado);
    }

    public void CambiarNumeroLista(EstudianteId estudianteId, int numeroLista)
    {
        var estudiante = ObtenerEstudiante(estudianteId);
        ValidarNumeroLista(numeroLista);

        if (estudiante.EstaActivo)
        {
            ValidarNumeroDisponible(numeroLista, estudiante.Id);
        }

        estudiante.CambiarNumeroLista(numeroLista);
    }

    public void DesactivarEstudiante(EstudianteId estudianteId)
    {
        var estudiante = ObtenerEstudiante(estudianteId);

        if (!estudiante.EstaActivo)
        {
            return;
        }

        estudiante.Desactivar();
    }

    public void ReactivarEstudiante(EstudianteId estudianteId)
    {
        var estudiante = ObtenerEstudiante(estudianteId);

        if (estudiante.EstaActivo)
        {
            return;
        }

        ValidarNumeroDisponible(estudiante.NumeroLista, estudiante.Id);
        estudiante.Reactivar();
    }

    private static string ValidarNombreEstudiante(string nombreVisible) =>
        NormalizadorNombreVisible.NormalizarYValidar(
            nombreVisible,
            LongitudMaximaNombreEstudiante,
            "El nombre del estudiante");

    private static void ValidarNumeroLista(int numeroLista)
    {
        if (numeroLista <= 0)
        {
            throw new DomainValidationException("El número de lista debe ser mayor que cero.");
        }
    }

    private Estudiante ObtenerEstudiante(EstudianteId estudianteId)
    {
        var estudiante = _estudiantes.Find(candidato => candidato.Id == estudianteId);

        return estudiante ?? throw new DomainConflictException(
            "El estudiante no pertenece al grupo.");
    }

    private void ValidarNumeroDisponible(int numeroLista, EstudianteId? estudianteExcluido = null)
    {
        var hayConflicto = _estudiantes.Any(
            estudiante => estudiante.EstaActivo
                && estudiante.NumeroLista == numeroLista
                && estudiante.Id != estudianteExcluido);

        if (hayConflicto)
        {
            throw new DomainConflictException(
                $"El número de lista {numeroLista} ya pertenece a un estudiante activo del grupo.");
        }
    }
}