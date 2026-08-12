namespace SistemaDocente.Core.Tests;

public sealed class GrupoCicloVidaTests
{
    [Fact]
    public void GrupoNuevoIniciaActivoYArchivoEsReversibleEIdempotente()
    {
        var grupo = Grupo.Crear("Quinto A");
        var estudiante = grupo.AgregarEstudiante("Ana López", 1);

        Assert.False(grupo.EstaArchivado);

        grupo.Archivar();
        grupo.Archivar();

        Assert.True(grupo.EstaArchivado);
        Assert.Equal(estudiante.Id, Assert.Single(grupo.Estudiantes).Id);

        grupo.Restaurar();
        grupo.Restaurar();

        Assert.False(grupo.EstaArchivado);
        Assert.Equal(estudiante.Id, Assert.Single(grupo.Estudiantes).Id);
    }

    [Fact]
    public void RehidratarConservaEstadoArchivadoEIdentidades()
    {
        var grupoId = GrupoId.DesdeGuid(Guid.NewGuid());
        var estudianteId = EstudianteId.DesdeGuid(Guid.NewGuid());
        DatosEstudianteRehidratado[] estudiantes =
        [
            new(estudianteId, "María José", 4, true),
        ];

        var grupo = Grupo.Rehidratar(grupoId, "Sexto B", estudiantes, estaArchivado: true);

        Assert.True(grupo.EstaArchivado);
        Assert.Equal(grupoId, grupo.Id);
        Assert.Equal(estudianteId, Assert.Single(grupo.Estudiantes).Id);
    }

    [Fact]
    public void GrupoArchivadoRechazaMutacionesOrdinariasSinCambiarDatos()
    {
        var grupo = Grupo.Crear("Cuarto A");
        var estudiante = grupo.AgregarEstudiante("Luis Pérez", 2);
        grupo.Archivar();

        Assert.Throws<DomainConflictException>(() => grupo.Renombrar("Cuarto B"));
        Assert.Throws<DomainConflictException>(() => grupo.AgregarEstudiante("Ana", 3));
        Assert.Throws<DomainConflictException>(() => grupo.CambiarNumeroLista(estudiante.Id, 9));
        Assert.Throws<DomainConflictException>(() => grupo.DesactivarEstudiante(estudiante.Id));

        Assert.True(grupo.EstaArchivado);
        Assert.Equal("Cuarto A", grupo.NombreVisible);
        Assert.Equal(2, estudiante.NumeroLista);
        Assert.True(estudiante.EstaActivo);
        Assert.Single(grupo.Estudiantes);
    }
}
