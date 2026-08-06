using SistemaDocente.Core;

namespace SistemaDocente.Core.Tests;

public sealed class ProyectosActividadesTests
{
    private static readonly GrupoId GrupoId = Grupo.Crear("Grupo").Id;

    [Fact]
    public void ProyectoNormalizaPeriodoFlexibleYTransiciones()
    {
        var proyecto = ProyectoDidactico.Crear(GrupoId, "  Proyecto   uno ", null,
            new DateOnly(2026, 1, 1), new DateOnly(2026, 3, 1), null);

        Assert.Equal("Proyecto uno", proyecto.Nombre);
        Assert.Equal(EstadoProyecto.Borrador, proyecto.Estado);
        proyecto.Iniciar();
        proyecto.Finalizar();
        proyecto.Reabrir();
        Assert.Equal(EstadoProyecto.EnCurso, proyecto.Estado);
        Assert.Equal(GrupoId, proyecto.GrupoId);
    }

    [Fact]
    public void ProyectoRechazaNombrePeriodoTransicionYRehidratacionInvalidos()
    {
        Assert.Throws<DomainValidationException>(() => ProyectoDidactico.Crear(GrupoId, " ", null,
            new DateOnly(2026, 2, 1), new DateOnly(2026, 1, 1), null));
        var proyecto = ProyectoDidactico.Crear(GrupoId, "P", null, DateOnly.MinValue, DateOnly.MaxValue, null);
        Assert.Throws<DomainConflictException>(proyecto.Finalizar);
        Assert.Throws<DomainValidationException>(() => ProyectoDidactico.Rehidratar(default, GrupoId,
            "P", "", DateOnly.MinValue, DateOnly.MaxValue, EstadoProyecto.Borrador, "", 1));
        Assert.Throws<DomainValidationException>(() => ProyectoDidactico.Rehidratar(proyecto.Id, GrupoId,
            "P", "", DateOnly.MinValue, DateOnly.MaxValue, (EstadoProyecto)99, "", 1));
    }

    [Fact]
    public void ActualizacionInvalidaDeProyectoEsAtomica()
    {
        var proyecto = ProyectoDidactico.Crear(GrupoId, "Original", "Descripción",
            new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), "Observación");

        Assert.Throws<DomainValidationException>(() => proyecto.Actualizar("Nuevo", "Cambio",
            new DateOnly(2026, 2, 1), new DateOnly(2026, 1, 1), "Cambio"));

        Assert.Equal("Original", proyecto.Nombre);
        Assert.Equal("Descripción", proyecto.Descripcion);
    }

    [Fact]
    public void ActividadCreaPadronPendienteYActualizaCompleto()
    {
        var estudiantes = new[] { EstudianteId.DesdeGuid(Guid.NewGuid()), EstudianteId.DesdeGuid(Guid.NewGuid()) };
        var actividad = ActividadProyecto.Crear(ProyectoId.DesdeGuid(Guid.NewGuid()), GrupoId,
            " Actividad  uno ", null, new DateOnly(2026, 1, 15), null,
            new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), estudiantes);

        Assert.Equal("Actividad uno", actividad.Titulo);
        Assert.All(actividad.Entregas, x => Assert.Equal(NivelLogro.Pendiente, x.NivelLogro));
        actividad.ActualizarEntregas([
            new(estudiantes[0], NivelLogro.Domina, "Bien"),
            new(estudiantes[1], NivelLogro.NoEntrego, "Sin entrega")]);
        Assert.Equal(NivelLogro.Domina, actividad.Entregas[0].NivelLogro);
    }

    [Fact]
    public void ActividadRechazaFechaDuplicadosYPadronParcialAtomicamente()
    {
        var estudiante = EstudianteId.DesdeGuid(Guid.NewGuid());
        Assert.Throws<DomainValidationException>(() => ActividadProyecto.Crear(
            ProyectoId.DesdeGuid(Guid.NewGuid()), GrupoId, "A", null,
            new DateOnly(2026, 2, 1), null, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), [estudiante]));
        Assert.Throws<DomainValidationException>(() => ActividadProyecto.Crear(
            ProyectoId.DesdeGuid(Guid.NewGuid()), GrupoId, "A", null,
            new DateOnly(2026, 1, 1), null, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), [estudiante, estudiante]));

        var actividad = ActividadProyecto.Crear(ProyectoId.DesdeGuid(Guid.NewGuid()), GrupoId, "A", null,
            new DateOnly(2026, 1, 1), null, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), [estudiante]);
        Assert.Throws<DomainValidationException>(() => actividad.ActualizarEntregas([]));
        Assert.Equal(NivelLogro.Pendiente, actividad.Entregas[0].NivelLogro);
    }

    [Fact]
    public void ActividadAnuladaConservaVistaYBloqueaEdicion()
    {
        var actividad = ActividadProyecto.Crear(ProyectoId.DesdeGuid(Guid.NewGuid()), GrupoId, "A", null,
            new DateOnly(2026, 1, 1), null, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), []);
        actividad.Anular();

        Assert.Equal(EstadoActividad.Anulada, actividad.Estado);
        Assert.Throws<DomainConflictException>(() => actividad.Actualizar("B", null,
            new DateOnly(2026, 1, 2), null, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31)));
        Assert.Throws<NotSupportedException>(() => ((IList<EntregaActividad>)actividad.Entregas).Add(null!));
    }
}