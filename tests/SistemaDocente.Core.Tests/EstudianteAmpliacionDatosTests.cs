using SistemaDocente.Core;

namespace SistemaDocente.Core.Tests;

public sealed class EstudianteAmpliacionDatosTests
{
    [Fact]
    public void EstudianteConservaApellidosNombresYCalculaEdad()
    {
        var fechaNacimiento = DateOnly.FromDateTime(DateTime.Today.AddYears(-10));
        var fechaIngreso = new DateOnly(2025, 8, 25);
        var estudianteId = EstudianteId.DesdeGuid(Guid.NewGuid());

        var datos = new DatosEstudianteRehidratado(
            Id: estudianteId,
            NombreVisible: "Pérez Gómez, Juan",
            PrimerApellido: "Pérez",
            SegundoApellido: "Gómez",
            Nombres: "Juan",
            FechaNacimiento: fechaNacimiento,
            Genero: GeneroEstudiante.Hombre,
            FechaIngreso: fechaIngreso,
            Observaciones: "Alumno participativo y ordenado",
            NumeroLista: 1,
            EstaActivo: true);

        var grupo = Grupo.Rehidratar(GrupoId.DesdeGuid(Guid.NewGuid()), "Sexto A", [datos]);
        var estudiante = Assert.Single(grupo.Estudiantes);

        Assert.Equal("Pérez", estudiante.PrimerApellido);
        Assert.Equal("Gómez", estudiante.SegundoApellido);
        Assert.Equal("Juan", estudiante.Nombres);
        Assert.Equal(10, estudiante.Edad);
        Assert.Equal(GeneroEstudiante.Hombre, estudiante.Genero);
        Assert.Equal(fechaIngreso, estudiante.FechaIngreso);
        Assert.Equal("Alumno participativo y ordenado", estudiante.Observaciones);
    }

    [Fact]
    public void ObservacionesConTerminoClinicoLanzaExcepcion()
    {
        var datos = new DatosEstudianteRehidratado(
            Id: EstudianteId.DesdeGuid(Guid.NewGuid()),
            NombreVisible: "González, María",
            PrimerApellido: "González",
            SegundoApellido: "",
            Nombres: "María",
            FechaNacimiento: null,
            Genero: GeneroEstudiante.Mujer,
            FechaIngreso: null,
            Observaciones: "Diagnóstico de TDAH severo",
            NumeroLista: 2,
            EstaActivo: true);

        var ex = Assert.Throws<DomainValidationException>(() =>
            Grupo.Rehidratar(GrupoId.DesdeGuid(Guid.NewGuid()), "Sexto A", [datos]));

        Assert.Contains("observaciones", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}