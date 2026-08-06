using System.Collections.ObjectModel;

namespace SistemaDocente.Core;

public sealed class Grupo
{
    private const int LongitudMaximaNombre = 100;
    private const int LongitudMaximaNombreEstudiante = 150;
    private readonly List<Estudiante> _estudiantes;
    private readonly ReadOnlyCollection<Estudiante> _vistaEstudiantes;

    private Grupo(string nombreVisible)
        : this(GrupoId.Crear(), nombreVisible, [])
    {
    }

    private Grupo(GrupoId id, string nombreVisible, List<Estudiante> estudiantes)
    {
        Id = id;
        NombreVisible = nombreVisible;
        _estudiantes = estudiantes;
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

    public static Grupo Rehidratar(
        GrupoId id,
        string nombreVisible,
        IReadOnlyCollection<DatosEstudianteRehidratado> estudiantes)
    {
        if (id == default)
        {
            throw new DomainValidationException("La identidad del grupo no puede estar vacía.");
        }

        ArgumentNullException.ThrowIfNull(estudiantes);

        var nombreValidado = ValidarNombreNormalizado(
            nombreVisible,
            LongitudMaximaNombre,
            "El nombre del grupo");
        var snapshot = estudiantes.ToArray();
        var identidades = new HashSet<EstudianteId>();
        var numerosActivos = new HashSet<int>();
        var estudiantesValidados = new List<Estudiante>(snapshot.Length);

        foreach (var datos in snapshot)
        {
            if (datos.Id == default || !identidades.Add(datos.Id))
            {
                throw new DomainValidationException(
                    "Las identidades de estudiantes deben ser válidas y únicas.");
            }

            var nombreEstudiante = ValidarNombreNormalizado(
                datos.NombreVisible,
                LongitudMaximaNombreEstudiante,
                "El nombre del estudiante");
            ValidarNumeroLista(datos.NumeroLista);

            if (datos.EstaActivo && !numerosActivos.Add(datos.NumeroLista))
            {
                throw new DomainConflictException(
                    $"El número de lista {datos.NumeroLista} está repetido entre estudiantes activos.");
            }

            estudiantesValidados.Add(
                new Estudiante(
                    datos.Id,
                    nombreEstudiante,
                    datos.PrimerApellido,
                    datos.SegundoApellido,
                    datos.Nombres,
                    datos.FechaNacimiento,
                    datos.Genero,
                    datos.FechaIngreso,
                    datos.Observaciones,
                    datos.NumeroLista,
                    datos.EstaActivo));
        }

        return new Grupo(id, nombreValidado, estudiantesValidados);
    }

    public void Renombrar(string nombreVisible)
    {
        var nombreNormalizado = NormalizadorNombreVisible.NormalizarYValidar(
            nombreVisible,
            LongitudMaximaNombre,
            "El nombre del grupo");

        NombreVisible = nombreNormalizado;
    }

    public Estudiante AgregarEstudiante(
        string nombreVisible,
        int numeroLista,
        string primerApellido = "",
        string segundoApellido = "",
        string nombres = "",
        DateOnly? fechaNacimiento = null,
        GeneroEstudiante genero = GeneroEstudiante.NoEspecificado,
        DateOnly? fechaIngreso = null,
        string observaciones = "")
    {
        var nombreNormalizado = ValidarNombreEstudiante(nombreVisible);
        ValidarNumeroLista(numeroLista);
        ValidarNumeroDisponible(numeroLista);

        var estudiante = new Estudiante(
            EstudianteId.Crear(),
            nombreNormalizado,
            primerApellido,
            segundoApellido,
            nombres,
            fechaNacimiento,
            genero,
            fechaIngreso,
            observaciones,
            numeroLista,
            true);

        _estudiantes.Add(estudiante);

        return estudiante;
    }

    public void ActualizarDatosEstudiante(
        EstudianteId estudianteId,
        string nombreVisible,
        string primerApellido,
        string segundoApellido,
        string nombres,
        DateOnly? fechaNacimiento,
        GeneroEstudiante genero,
        DateOnly? fechaIngreso,
        string observaciones)
    {
        var estudiante = ObtenerEstudiante(estudianteId);
        var nombreNormalizado = ValidarNombreEstudiante(nombreVisible);

        estudiante.Renombrar(nombreNormalizado);
        estudiante.ActualizarDatos(primerApellido, segundoApellido, nombres, fechaNacimiento, genero, fechaIngreso, observaciones);
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

    private static string ValidarNombreNormalizado(
        string nombreVisible,
        int longitudMaxima,
        string campo)
    {
        var normalizado = NormalizadorNombreVisible.NormalizarYValidar(
            nombreVisible,
            longitudMaxima,
            campo);

        if (!string.Equals(nombreVisible, normalizado, StringComparison.Ordinal))
        {
            throw new DomainValidationException($"{campo} debe estar normalizado.");
        }

        return normalizado;
    }

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