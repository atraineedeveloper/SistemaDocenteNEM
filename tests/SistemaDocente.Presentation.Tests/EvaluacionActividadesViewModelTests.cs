using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Presentation.Tests;

public sealed class EvaluacionActividadesViewModelTests
{
    [Fact]
    public void InicializarCargaProyectosYActividades()
    {
        var vm = Crear(out var gestion);
        vm.Inicializar(gestion.GrupoId);

        Assert.Single(vm.Proyectos);
        Assert.Single(vm.Actividades);
        Assert.NotNull(vm.ProyectoSeleccionado);
        Assert.NotNull(vm.ActividadSeleccionada);
        Assert.Equal(3, vm.Entregas.Count);
    }

    [Fact]
    public void MarcarNivelAsignaYDetectaCambios()
    {
        var vm = Crear(out var gestion);
        vm.Inicializar(gestion.GrupoId);

        vm.MarcarDominaCommand.Execute(null);

        Assert.Equal(3, vm.Domina);
        Assert.True(vm.TieneCambios);
        Assert.True(vm.GuardarActividadCommand.CanExecute(null));

        vm.GuardarActividadCommand.Execute(null);

        Assert.False(vm.TieneCambios);
        Assert.Equal(1, gestion.EntregasGuardadas);
    }

    [Fact]
    public void FiltrosEntregaFiltranPorNivelCorrectamente()
    {
        var vm = Crear(out var gestion);
        vm.Inicializar(gestion.GrupoId);

        vm.Entregas[0].NivelLogro = NivelLogro.Domina;
        vm.Entregas[1].NivelLogro = NivelLogro.RequiereApoyo;
        vm.Entregas[2].NivelLogro = NivelLogro.NoEntrego;

        vm.FiltroEntrega = FiltroEntrega.Domina;
        Assert.Single(vm.EntregasVisibles);
        Assert.Equal(NivelLogro.Domina, vm.EntregasVisibles[0].NivelLogro);

        vm.FiltroEntrega = FiltroEntrega.SoloIncidencias;
        Assert.Equal(2, vm.EntregasVisibles.Count);
    }

    private static EvaluacionActividadesViewModel Crear(out GestionDoble gestion, IDialogoCambiosPendientes? dialogo = null)
    {
        gestion = new GestionDoble();
        return new EvaluacionActividadesViewModel(gestion, dialogo ?? new Dialogo(DecisionCambiosPendientes.Cancelar), new MensajesDoble());
    }

    private sealed class MensajesDoble : IServicioMensajes
    {
        public void MostrarError(string mensaje) { }
    }

    private sealed class Dialogo(params DecisionCambiosPendientes[] decisiones) : IDialogoCambiosPendientes
    {
        private readonly Queue<DecisionCambiosPendientes> _decisiones = new(decisiones);
        public DecisionCambiosPendientes ConfirmarCambiosPendientes() =>
            _decisiones.Count > 0 ? _decisiones.Dequeue() : DecisionCambiosPendientes.Cancelar;
    }

    private sealed class GestionDoble : IGestionProyectosPresentacion
    {
        private readonly List<ProyectoDetalle> _proyectos = [];
        private readonly List<ActividadProyectoDetalle> _actividades = [];
        private readonly Guid _proyecto1 = Guid.NewGuid();
        private readonly Guid _actividad1 = Guid.NewGuid();

        internal GestionDoble()
        {
            GrupoId = GrupoId.DesdeGuid(Guid.NewGuid());
            var entregas = Enumerable.Range(1, 3)
                .Select(n => new EntregaActividadDetalle(EstudianteId.DesdeGuid(GuidUtility(n)), n, $"Estudiante {n}", true, NivelLogro.Pendiente, ""))
                .ToArray();

            _proyectos.Add(new ProyectoDetalle(ProyectoId.DesdeGuid(_proyecto1), GrupoId, "Proyecto 1", "", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), EstadoProyecto.EnCurso, "", 1, 1, false));
            _actividades.Add(new ActividadProyectoDetalle(ActividadId.DesdeGuid(_actividad1), ProyectoId.DesdeGuid(_proyecto1), GrupoId, "Actividad 1", "", new DateOnly(2026, 1, 10), "", EstadoActividad.Activa, entregas, 3, 3, 0, 0, 0, 0, 0, 1));
        }

        internal GrupoId GrupoId { get; }
        internal int EntregasGuardadas { get; private set; }

        public IReadOnlyList<ProyectoResumen> ListarProyectos(GrupoId grupoId) =>
            _proyectos.Where(x => x.GrupoId == grupoId).Select(x => new ProyectoResumen(x.ProyectoId, x.Nombre, x.FechaInicio, x.FechaTermino, x.Estado, x.NumeroActividades, x.Version)).ToArray();

        public ProyectoDetalle ObtenerProyecto(ProyectoId id) => _proyectos.First(x => x.ProyectoId == id);
        public ProyectoDetalle CrearProyecto(GrupoId grupoId, EntradaProyecto datos) => throw new NotImplementedException();
        public ProyectoDetalle ActualizarProyecto(ProyectoId id, int version, EntradaProyecto entrada) => throw new NotImplementedException();
        public ProyectoDetalle CambiarEstado(ProyectoId id, int version, EstadoProyecto estado) => throw new NotImplementedException();
        public ProyectoDetalle Reabrir(ProyectoId id, int version) => throw new NotImplementedException();
        public void EliminarProyecto(ProyectoId id, int version) => throw new NotImplementedException();

        public IReadOnlyList<ActividadProyectoResumen> ListarActividades(ProyectoId proyectoId) =>
            _actividades.Where(x => x.ProyectoId == proyectoId).Select(x => new ActividadProyectoResumen(x.ActividadId, x.ProyectoId, x.Titulo, x.FechaRealizacion, x.Estado, x.Total, x.Pendientes, x.Domina, x.Suficiente, x.EnProceso, x.RequiereApoyo, x.NoEntrego, x.Version)).ToArray();

        public ActividadProyectoDetalle PrepararActividad(ProyectoId proyectoId, string titulo, string descripcion, DateOnly fecha, string observaciones) => throw new NotImplementedException();
        public ActividadProyectoDetalle CrearActividad(ProyectoId proyectoId, EntradaActividad datos) => throw new NotImplementedException();
        public ActividadProyectoDetalle ObtenerActividad(ActividadId id) => _actividades.First(x => x.ActividadId == id);
        public ActividadProyectoDetalle ActualizarActividad(ActividadId id, int version, EntradaActividad entrada) => throw new NotImplementedException();

        public ActividadProyectoDetalle GuardarEntregas(ActividadId id, int version, IReadOnlyCollection<EntradaEntregaActividad> entregas)
        {
            EntregasGuardadas++;
            var act = _actividades.First(x => x.ActividadId == id);
            var nuevasEntregas = act.Entregas.Select(x =>
            {
                var nueva = entregas.First(e => e.EstudianteId == x.EstudianteId);
                return new EntregaActividadDetalle(x.EstudianteId, x.NumeroLista, x.NombreVisible, x.EstaActivoActualmente, nueva.NivelLogro, nueva.Observacion);
            }).ToArray();
            var domina = nuevasEntregas.Count(x => x.NivelLogro == NivelLogro.Domina);
            var pend = nuevasEntregas.Count(x => x.NivelLogro == NivelLogro.Pendiente);
            var actualizada = new ActividadProyectoDetalle(act.ActividadId, act.ProyectoId, act.GrupoId, act.Titulo, act.Descripcion, act.FechaRealizacion, act.ObservacionesGenerales, act.Estado, nuevasEntregas, nuevasEntregas.Length, pend, domina, 0, 0, 0, 0, act.Version + 1);
            _actividades[_actividades.IndexOf(act)] = actualizada;
            return actualizada;
        }

        public ActividadProyectoDetalle AnularActividad(ActividadId id, int version) => throw new NotImplementedException();
        public void EliminarActividad(ActividadId id, int version) => throw new NotImplementedException();

        private static Guid GuidUtility(int numero)
        {
            var bytes = new byte[16];
            BitConverter.GetBytes(numero).CopyTo(bytes, 0);
            return new Guid(bytes);
        }
    }
}
