namespace SistemaDocente.Core;

public sealed class Estudiante
{
    internal Estudiante(string nombreVisible, int numeroLista)
        : this(EstudianteId.Crear(), nombreVisible, "", "", "", null, GeneroEstudiante.NoEspecificado, null, "", numeroLista, true)
    {
    }

    internal Estudiante(
        EstudianteId id,
        string nombreVisible,
        string primerApellido,
        string segundoApellido,
        string nombres,
        DateOnly? fechaNacimiento,
        GeneroEstudiante genero,
        DateOnly? fechaIngreso,
        string observaciones,
        int numeroLista,
        bool estaActivo)
    {
        if (!string.IsNullOrWhiteSpace(observaciones))
        {
            ValidadorContenidoPedagogico.ValidarTextoPedagogico(observaciones, nameof(observaciones));
        }

        Id = id;
        PrimerApellido = primerApellido?.Trim() ?? "";
        SegundoApellido = segundoApellido?.Trim() ?? "";
        Nombres = nombres?.Trim() ?? "";
        FechaNacimiento = fechaNacimiento;
        Genero = genero;
        FechaIngreso = fechaIngreso;
        Observaciones = observaciones?.Trim() ?? "";
        NumeroLista = numeroLista;
        EstaActivo = estaActivo;

        NombreVisible = ConstruirNombreCompleto(nombreVisible, PrimerApellido, SegundoApellido, Nombres);
    }

    public EstudianteId Id { get; }

    public string NombreVisible { get; private set; }

    public string PrimerApellido { get; private set; }

    public string SegundoApellido { get; private set; }

    public string Nombres { get; private set; }

    public DateOnly? FechaNacimiento { get; private set; }

    public GeneroEstudiante Genero { get; private set; }

    public DateOnly? FechaIngreso { get; private set; }

    public string Observaciones { get; private set; }

    public int NumeroLista { get; private set; }

    public bool EstaActivo { get; private set; }

    public int? Edad
    {
        get
        {
            if (!FechaNacimiento.HasValue) return null;
            var hoy = DateOnly.FromDateTime(DateTime.Today);
            var edad = hoy.Year - FechaNacimiento.Value.Year;
            if (FechaNacimiento.Value > hoy.AddYears(-edad)) edad--;
            return edad >= 0 ? edad : null;
        }
    }

    internal void ActualizarDatos(
        string primerApellido,
        string segundoApellido,
        string nombres,
        DateOnly? fechaNacimiento,
        GeneroEstudiante genero,
        DateOnly? fechaIngreso,
        string observaciones)
    {
        if (!string.IsNullOrWhiteSpace(observaciones))
        {
            ValidadorContenidoPedagogico.ValidarTextoPedagogico(observaciones, nameof(observaciones));
        }

        PrimerApellido = primerApellido?.Trim() ?? "";
        SegundoApellido = segundoApellido?.Trim() ?? "";
        Nombres = nombres?.Trim() ?? "";
        FechaNacimiento = fechaNacimiento;
        Genero = genero;
        FechaIngreso = fechaIngreso;
        Observaciones = observaciones?.Trim() ?? "";

        NombreVisible = ConstruirNombreCompleto(NombreVisible, PrimerApellido, SegundoApellido, Nombres);
    }

    internal void Renombrar(string nombreVisible)
    {
        NombreVisible = nombreVisible?.Trim() ?? "";
    }

    internal void CambiarNumeroLista(int numeroLista)
    {
        NumeroLista = numeroLista;
    }

    internal void Desactivar()
    {
        EstaActivo = false;
    }

    internal void Reactivar()
    {
        EstaActivo = true;
    }

    private static string ConstruirNombreCompleto(string nombreGenerico, string primerApellido, string segundoApellido, string nombres)
    {
        var partes = new[] { primerApellido, segundoApellido, nombres }.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
        if (partes.Length > 0)
        {
            return string.Join(" ", partes);
        }
        return nombreGenerico?.Trim() ?? "";
    }
}