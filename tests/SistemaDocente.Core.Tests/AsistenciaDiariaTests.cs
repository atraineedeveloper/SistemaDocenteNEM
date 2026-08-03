using SistemaDocente.Core;

namespace SistemaDocente.Core.Tests;

public sealed class AsistenciaDiariaTests
{
    [Theory]
    [InlineData(EstadoAsistencia.Presente)]
    [InlineData(EstadoAsistencia.Falta)]
    [InlineData(EstadoAsistencia.Retardo)]
    [InlineData(EstadoAsistencia.Justificada)]
    public void CrearConservaCadaEstadoAdmitido(EstadoAsistencia estado)
    {
        var grupoId = Grupo.Crear("Primero A").Id;
        var estudianteId = Grupo.Crear("Temporal").AgregarEstudiante("Ana", 1).Id;

        var asistencia = AsistenciaDiaria.Crear(
            grupoId,
            new DateOnly(2026, 8, 3),
            [new(estudianteId, estado)]);

        Assert.Equal(grupoId, asistencia.GrupoId);
        Assert.Equal(new DateOnly(2026, 8, 3), asistencia.Fecha);
        Assert.Equal(estado, Assert.Single(asistencia.Registros).Estado);
    }

    [Fact]
    public void FechasDistintasIdentificanDiasDistintos()
    {
        var grupoId = Grupo.Crear("Primero A").Id;

        var uno = AsistenciaDiaria.Crear(grupoId, new DateOnly(2026, 8, 3), []);
        var otro = AsistenciaDiaria.Crear(grupoId, new DateOnly(2026, 8, 4), []);

        Assert.NotEqual(uno.Fecha, otro.Fecha);
    }

    [Fact]
    public void CrearRechazaEstadoFueraDeRangoSinResultadoParcial()
    {
        var grupoId = Grupo.Crear("Primero A").Id;
        var estudianteId = Grupo.Crear("Temporal").AgregarEstudiante("Ana", 1).Id;

        Assert.Throws<DomainValidationException>(() => AsistenciaDiaria.Crear(
            grupoId,
            new DateOnly(2026, 8, 3),
            [new(estudianteId, (EstadoAsistencia)99)]));
    }

    [Fact]
    public void CrearRechazaIdentidadesDuplicadas()
    {
        var grupoId = Grupo.Crear("Primero A").Id;
        var estudianteId = Grupo.Crear("Temporal").AgregarEstudiante("Ana", 1).Id;

        Assert.Throws<DomainValidationException>(() => AsistenciaDiaria.Crear(
            grupoId,
            new DateOnly(2026, 8, 3),
            [new(estudianteId, EstadoAsistencia.Presente), new(estudianteId, EstadoAsistencia.Falta)]));
    }

    [Fact]
    public void CambiarEstadoModificaUnicamenteElRegistroSolicitado()
    {
        var grupo = Grupo.Crear("Primero A");
        var ana = grupo.AgregarEstudiante("Ana", 1);
        var bea = grupo.AgregarEstudiante("Bea", 2);
        var asistencia = AsistenciaDiaria.Crear(
            grupo.Id,
            new DateOnly(2026, 8, 3),
            [new(ana.Id, EstadoAsistencia.Presente), new(bea.Id, EstadoAsistencia.Presente)]);

        asistencia.CambiarEstado(ana.Id, EstadoAsistencia.Justificada);

        Assert.Equal(EstadoAsistencia.Justificada, asistencia.Registros[0].Estado);
        Assert.Equal(EstadoAsistencia.Presente, asistencia.Registros[1].Estado);
    }

    [Fact]
    public void CambiarEstadoInvalidoOEstudianteAusenteEsAtomico()
    {
        var grupo = Grupo.Crear("Primero A");
        var ana = grupo.AgregarEstudiante("Ana", 1);
        var ajeno = Grupo.Crear("Otro").AgregarEstudiante("Ajeno", 1);
        var asistencia = AsistenciaDiaria.Crear(
            grupo.Id,
            new DateOnly(2026, 8, 3),
            [new(ana.Id, EstadoAsistencia.Presente)]);

        Assert.Throws<DomainValidationException>(
            () => asistencia.CambiarEstado(ana.Id, (EstadoAsistencia)(-1)));
        Assert.Throws<DomainConflictException>(
            () => asistencia.CambiarEstado(ajeno.Id, EstadoAsistencia.Falta));
        Assert.Equal(EstadoAsistencia.Presente, asistencia.Registros[0].Estado);
    }

    [Fact]
    public void RegistrosEsVistaDeSoloLectura()
    {
        var asistencia = AsistenciaDiaria.Crear(
            Grupo.Crear("Primero A").Id,
            new DateOnly(2026, 8, 3),
            []);

        Assert.IsAssignableFrom<IReadOnlyList<RegistroAsistencia>>(asistencia.Registros);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<RegistroAsistencia>)asistencia.Registros).Clear());
    }

    [Fact]
    public void RehidratarConservaIdentidadesYRechazaSnapshotInvalido()
    {
        var grupo = Grupo.Crear("Primero A");
        var estudiante = grupo.AgregarEstudiante("Ana", 1);
        var fecha = new DateOnly(2026, 8, 3);

        var asistencia = AsistenciaDiaria.Rehidratar(
            grupo.Id,
            fecha,
            [new(estudiante.Id, EstadoAsistencia.Retardo)]);

        Assert.Equal(grupo.Id, asistencia.GrupoId);
        Assert.Equal(estudiante.Id, Assert.Single(asistencia.Registros).EstudianteId);
        Assert.Throws<DomainValidationException>(() => AsistenciaDiaria.Rehidratar(
            grupo.Id,
            fecha,
            [new(estudiante.Id, (EstadoAsistencia)4)]));
    }
}