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
        bool estaActivo)
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
    }

    internal EstudianteId Id { get; }

    public string Nombre { get; }
    public string PrimerApellido { get; }
    public string SegundoApellido { get; }
    public string Nombres { get; }
    public DateOnly? FechaNacimiento { get; }
    public int? Edad { get; }
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
}