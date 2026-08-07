using System.Runtime.CompilerServices;

using SistemaDocente.Core;

[assembly: InternalsVisibleTo("SistemaDocente.App.Wpf")]

namespace SistemaDocente.Presentation;

public sealed class EstudianteVisual
{
    internal EstudianteVisual(
        EstudianteId id,
        string nombre,
        string primerApellido,
        string segundoApellido,
        string nombres,
        DateOnly? fechaNacimiento,
        int? edad,
        GeneroEstudiante genero,
        DateOnly? fechaIngreso,
        string observaciones,
        int numeroLista,
        bool estaActivo,
        GradoPrimaria grado = GradoPrimaria.NoEspecificado)
    {
        Id = id;
        Nombre = nombre;
        PrimerApellido = primerApellido;
        SegundoApellido = segundoApellido;
        Nombres = nombres;
        FechaNacimiento = fechaNacimiento;
        Edad = edad;
        Genero = genero;
        FechaIngreso = fechaIngreso;
        Observaciones = observaciones;
        NumeroLista = numeroLista;
        EstaActivo = estaActivo;
        Grado = grado;
    }

    internal EstudianteId Id { get; }

    public string Nombre { get; }
    public string PrimerApellido { get; }
    public string SegundoApellido { get; }
    public string Nombres { get; }
    public DateOnly? FechaNacimiento { get; }
    public int? Edad { get; }
    public string EdadTexto => Edad?.ToString(System.Globalization.CultureInfo.CurrentCulture) ?? "—";
    public GeneroEstudiante Genero { get; }
    public string GeneroTexto => Genero switch
    {
        GeneroEstudiante.Hombre => "Hombre",
        GeneroEstudiante.Mujer => "Mujer",
        _ => "No especificado"
    };
    public DateOnly? FechaIngreso { get; }
    public string Observaciones { get; }
    public int NumeroLista { get; }
    public bool EstaActivo { get; }
    public string Estado => EstaActivo ? "Activo" : "Inactivo";
    public GradoPrimaria Grado { get; }
    public string GradoTexto => CatalogoNemPrimaria.EsGradoReal(Grado)
        ? CatalogoNemPrimaria.FormatearGrado(Grado)
        : "—";

    /// <summary>
    /// Iniciales puramente visuales para identificar rápidamente una fila sin introducir
    /// archivos de avatar ni datos personales adicionales.
    /// </summary>
    public string Iniciales
    {
        get
        {
            var partes = Nombre.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (partes.Length == 0) return "?";
            if (partes.Length == 1) return char.ToUpper(partes[0][0], System.Globalization.CultureInfo.CurrentCulture).ToString();
            return string.Concat(
                char.ToUpper(partes[0][0], System.Globalization.CultureInfo.CurrentCulture),
                char.ToUpper(partes[1][0], System.Globalization.CultureInfo.CurrentCulture));
        }
    }
}