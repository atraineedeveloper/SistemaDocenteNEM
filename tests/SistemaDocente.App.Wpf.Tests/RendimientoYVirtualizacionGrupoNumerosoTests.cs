using SistemaDocente.Core;

namespace SistemaDocente.App.Wpf.Tests;

public sealed class RendimientoYVirtualizacionGrupoNumerosoTests
{
    [Fact]
    public void GrupoConCuarentaEstudiantesSeCreaYConservaIdentidades()
    {
        var grupo = Grupo.Crear("Quinto B");

        for (int i = 1; i <= 40; i++)
        {
            grupo.AgregarEstudiante(
                nombreVisible: $"Estudiante Número {i}",
                numeroLista: i,
                primerApellido: $"ApellidoP{i}",
                segundoApellido: $"ApellidoM{i}",
                nombres: $"Nombre{i}",
                fechaNacimiento: new DateOnly(2015, 1, 1).AddDays(i),
                genero: i % 2 == 0 ? GeneroEstudiante.Hombre : GeneroEstudiante.Mujer,
                fechaIngreso: new DateOnly(2025, 8, 20),
                observaciones: $"Desempeño adecuado en grupo numeroso {i}");
        }

        Assert.Equal(40, grupo.Estudiantes.Count);
        Assert.All(grupo.Estudiantes, e => Assert.True(e.NumeroLista > 0 && e.NumeroLista <= 40));
    }
}