namespace SistemaDocente.Core.Tests;

public sealed class EstadoEntregaActividadTests
{
    [Fact]
    public void ActividadNuevaIniciaConEntregaYNivelPendientes()
    {
        var estudianteId = EstudianteId.DesdeGuid(Guid.NewGuid());
        var actividad = CrearActividad(estudianteId);

        var entrega = Assert.Single(actividad.Entregas);
        Assert.Equal(EstadoEntregaActividad.Pendiente, entrega.EstadoEntrega);
        Assert.Equal(NivelLogro.Pendiente, entrega.NivelLogro);
    }

    [Fact]
    public void EntregadaPuedeQuedarPendienteDeEvaluacion()
    {
        var estudianteId = EstudianteId.DesdeGuid(Guid.NewGuid());
        var actividad = CrearActividad(estudianteId);

        actividad.ActualizarEntregas([
            new DatosEntregaActividadRehidratada(
                estudianteId,
                EstadoEntregaActividad.Entregada,
                NivelLogro.Pendiente,
                "Trabajo recibido")]);

        var entrega = Assert.Single(actividad.Entregas);
        Assert.Equal(EstadoEntregaActividad.Entregada, entrega.EstadoEntrega);
        Assert.Equal(NivelLogro.Pendiente, entrega.NivelLogro);
    }

    [Fact]
    public void NivelEvaluadoFuerzaEstadoEntregada()
    {
        var estudianteId = EstudianteId.DesdeGuid(Guid.NewGuid());
        var actividad = CrearActividad(estudianteId);

        actividad.ActualizarEntregas([
            new DatosEntregaActividadRehidratada(
                estudianteId,
                EstadoEntregaActividad.Pendiente,
                NivelLogro.Domina,
                "")]);

        var entrega = Assert.Single(actividad.Entregas);
        Assert.Equal(EstadoEntregaActividad.Entregada, entrega.EstadoEntrega);
        Assert.Equal(NivelLogro.Domina, entrega.NivelLogro);
    }

    [Fact]
    public void NoEntregadaFuerzaNivelPendiente()
    {
        var estudianteId = EstudianteId.DesdeGuid(Guid.NewGuid());
        var actividad = CrearActividad(estudianteId);

        actividad.ActualizarEntregas([
            new DatosEntregaActividadRehidratada(
                estudianteId,
                EstadoEntregaActividad.NoEntregada,
                NivelLogro.Suficiente,
                "")]);

        var entrega = Assert.Single(actividad.Entregas);
        Assert.Equal(EstadoEntregaActividad.NoEntregada, entrega.EstadoEntrega);
        Assert.Equal(NivelLogro.Pendiente, entrega.NivelLogro);
    }

    [Fact]
    public void RehidratarRechazaNoEntregoComoNivelPersistido()
    {
        var estudianteId = EstudianteId.DesdeGuid(Guid.NewGuid());

        Assert.Throws<DomainValidationException>(() => ActividadProyecto.Rehidratar(
            ActividadId.DesdeGuid(Guid.NewGuid()),
            ProyectoId.DesdeGuid(Guid.NewGuid()),
            GrupoId.DesdeGuid(Guid.NewGuid()),
            "Actividad",
            "",
            new DateOnly(2026, 8, 7),
            "",
            EstadoActividad.Activa,
            1,
            [new DatosEntregaActividadRehidratada(
                estudianteId,
                EstadoEntregaActividad.NoEntregada,
                NivelLogro.NoEntrego,
                "")]));
    }

    private static ActividadProyecto CrearActividad(EstudianteId estudianteId) =>
        ActividadProyecto.Crear(
            ProyectoId.DesdeGuid(Guid.NewGuid()),
            GrupoId.DesdeGuid(Guid.NewGuid()),
            "Actividad",
            "",
            new DateOnly(2026, 8, 7),
            "",
            new DateOnly(2026, 8, 1),
            new DateOnly(2026, 8, 31),
            [estudianteId]);
}