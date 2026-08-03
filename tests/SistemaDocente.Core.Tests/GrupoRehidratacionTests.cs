namespace SistemaDocente.Core.Tests;

public sealed class GrupoRehidratacionTests
{
    [Fact]
    public void IdentificadoresConservanGuidExistente()
    {
        var grupoGuid = Guid.NewGuid();
        var estudianteGuid = Guid.NewGuid();

        var grupoId = GrupoId.DesdeGuid(grupoGuid);
        var estudianteId = EstudianteId.DesdeGuid(estudianteGuid);

        Assert.Equal(grupoGuid, grupoId.Valor);
        Assert.Equal(estudianteGuid, estudianteId.Valor);
    }

    [Fact]
    public void IdentificadoresRechazanGuidVacio()
    {
        Assert.Throws<DomainValidationException>(() => GrupoId.DesdeGuid(Guid.Empty));
        Assert.Throws<DomainValidationException>(() => EstudianteId.DesdeGuid(Guid.Empty));
    }

    [Fact]
    public void RehidratarConservaIdentidadesDatosYEstados()
    {
        var grupoId = GrupoId.DesdeGuid(Guid.NewGuid());
        var activoId = EstudianteId.DesdeGuid(Guid.NewGuid());
        var inactivoId = EstudianteId.DesdeGuid(Guid.NewGuid());
        DatosEstudianteRehidratado[] estudiantes =
        [
            new(activoId, "Ángel O'Connor-López", 1, true),
            new(inactivoId, "María José", 1, false),
        ];

        var grupo = Grupo.Rehidratar(grupoId, "Quinto “A”", estudiantes);

        Assert.Equal(grupoId, grupo.Id);
        Assert.Equal("Quinto “A”", grupo.NombreVisible);
        Assert.Collection(
            grupo.Estudiantes,
            estudiante =>
            {
                Assert.Equal(activoId, estudiante.Id);
                Assert.True(estudiante.EstaActivo);
            },
            estudiante =>
            {
                Assert.Equal(inactivoId, estudiante.Id);
                Assert.False(estudiante.EstaActivo);
            });
    }

    [Theory]
    [InlineData(" Grupo")]
    [InlineData("Grupo ")]
    [InlineData("Grupo  A")]
    public void RehidratarRechazaNombreDeGrupoNoNormalizado(string nombre)
    {
        Assert.Throws<DomainValidationException>(
            () => Grupo.Rehidratar(GrupoId.DesdeGuid(Guid.NewGuid()), nombre, []));
    }

    [Fact]
    public void RehidratarRechazaNombreDeEstudianteNoNormalizado()
    {
        DatosEstudianteRehidratado[] estudiantes =
        [
            new(EstudianteId.DesdeGuid(Guid.NewGuid()), "  Ana", 1, true),
        ];

        Assert.Throws<DomainValidationException>(
            () => Grupo.Rehidratar(
                GrupoId.DesdeGuid(Guid.NewGuid()),
                "Primero A",
                estudiantes));
    }

    [Fact]
    public void RehidratarRechazaIdentidadesDeEstudianteRepetidas()
    {
        var id = EstudianteId.DesdeGuid(Guid.NewGuid());
        DatosEstudianteRehidratado[] estudiantes =
        [
            new(id, "Ana", 1, true),
            new(id, "Luis", 2, true),
        ];

        Assert.Throws<DomainValidationException>(
            () => Grupo.Rehidratar(
                GrupoId.DesdeGuid(Guid.NewGuid()),
                "Primero A",
                estudiantes));
    }

    [Fact]
    public void RehidratarRechazaNumeroNoPositivo()
    {
        DatosEstudianteRehidratado[] estudiantes =
        [
            new(EstudianteId.DesdeGuid(Guid.NewGuid()), "Ana", 0, true),
        ];

        Assert.Throws<DomainValidationException>(
            () => Grupo.Rehidratar(
                GrupoId.DesdeGuid(Guid.NewGuid()),
                "Primero A",
                estudiantes));
    }

    [Fact]
    public void RehidratarRechazaDuplicadosActivosPeroPermiteInactivos()
    {
        var grupoId = GrupoId.DesdeGuid(Guid.NewGuid());
        DatosEstudianteRehidratado[] conflicto =
        [
            new(EstudianteId.DesdeGuid(Guid.NewGuid()), "Ana", 1, true),
            new(EstudianteId.DesdeGuid(Guid.NewGuid()), "Luis", 1, true),
        ];
        DatosEstudianteRehidratado[] valido =
        [
            new(EstudianteId.DesdeGuid(Guid.NewGuid()), "Ana", 1, true),
            new(EstudianteId.DesdeGuid(Guid.NewGuid()), "Luis", 1, false),
        ];

        Assert.Throws<DomainConflictException>(
            () => Grupo.Rehidratar(grupoId, "Primero A", conflicto));
        Assert.Equal(2, Grupo.Rehidratar(grupoId, "Primero A", valido).Estudiantes.Count);
    }

    [Fact]
    public void RutasNormalesSiguenGenerandoIdentidades()
    {
        var grupo = Grupo.Crear("Primero A");
        var estudiante = grupo.AgregarEstudiante("Ana", 1);

        Assert.NotEqual(default, grupo.Id);
        Assert.NotEqual(default, estudiante.Id);
    }
}