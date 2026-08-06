using SistemaDocente.Core;

namespace SistemaDocente.Data.Tests;

public sealed class MigracionVersionSeisEstudiantesTests : IDisposable
{
    private readonly BaseSqliteTemporal _base = new();

    [Fact]
    public void GuardarYCargarEstudianteConCamposAmpliadosConservaDatos()
    {
        var grupo = Grupo.Crear("Sexto A");
        var fechaNac = new DateOnly(2014, 5, 12);
        var fechaIng = new DateOnly(2025, 8, 20);

        var estudiante = grupo.AgregarEstudiante(
            nombreVisible: "López Hernández, Sofia",
            numeroLista: 5,
            primerApellido: "López",
            segundoApellido: "Hernández",
            nombres: "Sofia",
            fechaNacimiento: fechaNac,
            genero: GeneroEstudiante.Mujer,
            fechaIngreso: fechaIng,
            observaciones: "Excelente en lectura dramatizada");

        _base.Persistencia.Guardar(grupo);

        var cargado = _base.Persistencia.Cargar(grupo.Id)!;
        var estCargado = Assert.Single(cargado.Estudiantes);

        Assert.Equal("López", estCargado.PrimerApellido);
        Assert.Equal("Hernández", estCargado.SegundoApellido);
        Assert.Equal("Sofia", estCargado.Nombres);
        Assert.Equal(fechaNac, estCargado.FechaNacimiento);
        Assert.Equal(GeneroEstudiante.Mujer, estCargado.Genero);
        Assert.Equal(fechaIng, estCargado.FechaIngreso);
        Assert.Equal("Excelente en lectura dramatizada", estCargado.Observaciones);
    }

    public void Dispose() => _base.Dispose();
}
