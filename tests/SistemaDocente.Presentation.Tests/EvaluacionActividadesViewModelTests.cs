using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Presentation.Tests;

public sealed class EvaluacionActividadesViewModelTests
{
    [Fact]
    public void InicializarConstruyeMatrizConIdentificadoresYPadronesHistoricos()
    {
        var vm = Crear(out var gestion);

        vm.Inicializar(gestion.GrupoId);

        Assert.Single(vm.Proyectos);
        Assert.Equal(3, vm.Actividades.Count);
        Assert.Equal(new[] { "A01", "A02", "A03" }, vm.ColumnasActividades.Select(x => x.Codigo));
        Assert.Equal(4, vm.Filas.Count);
        Assert.NotNull(vm.ActividadSeleccionada);
        Assert.Equal("A01", vm.ActividadColumnaSeleccionada?.Codigo);

        var estudianteTardio = vm.Filas.Single(x => x.NumeroLista == 4);
        Assert.False(estudianteTardio.Celdas[0].EsAplicable);
        Assert.False(estudianteTardio.Celdas[1].EsAplicable);
        Assert.True(estudianteTardio.Celdas[2].EsAplicable);
        Assert.Equal("—", estudianteTardio.Celdas[0].EtiquetaNivel);
    }

    [Fact]
    public void AtajoModificaSoloCeldaSeleccionada()
    {
        var vm = Crear(out var gestion);
        vm.Inicializar(gestion.GrupoId);
        var fila = vm.Filas[0];

        vm.SeleccionarCelda(fila, 1);
        vm.MarcarDominaCommand.Execute(null);

        Assert.Equal(NivelLogro.Pendiente, fila.Celdas[0].NivelLogro);
        Assert.Equal(NivelLogro.Domina, fila.Celdas[1].NivelLogro);
        Assert.Equal(NivelLogro.Pendiente, fila.Celdas[2].NivelLogro);
        Assert.True(vm.TieneCambios);
        Assert.True(vm.GuardarCambiosCommand.CanExecute(null));
    }

    [Fact]
    public void AccionMasivaAfectaSoloActividadSeleccionadaYRespetaNoAplicables()
    {
        var vm = Crear(out var gestion);
        vm.Inicializar(gestion.GrupoId);

        vm.SeleccionarCelda(vm.Filas[0], 0);
        vm.MarcarTodosSuficienteCommand.Execute(null);

        Assert.All(vm.Filas.Take(3), fila => Assert.Equal(NivelLogro.Suficiente, fila.Celdas[0].NivelLogro));
        Assert.False(vm.Filas[3].Celdas[0].EsAplicable);
        Assert.All(vm.Filas.Take(3), fila => Assert.Equal(NivelLogro.Pendiente, fila.Celdas[1].NivelLogro));
    }

    [Fact]
    public void GuardarCambiosPersisteCadaActividadModificadaPorSeparado()
    {
        var vm = Crear(out var gestion);
        vm.Inicializar(gestion.GrupoId);

        vm.SeleccionarCelda(vm.Filas[0], 0);
        vm.MarcarDominaCommand.Execute(null);
        vm.SeleccionarCelda(vm.Filas[1], 1);
        vm.MarcarSuficienteCommand.Execute(null);

        vm.GuardarCambiosCommand.Execute(null);

        Assert.Equal(2, gestion.EntregasGuardadas);
        Assert.False(vm.TieneCambios);
        Assert.False(vm.GuardarCambiosCommand.CanExecute(null));
    }

    [Fact]
    public void FiltroDeNivelUsaLaActividadSeleccionada()
    {
        var vm = Crear(out var gestion);
        vm.Inicializar(gestion.GrupoId);

        vm.SeleccionarCelda(vm.Filas[1], 0);
        vm.MarcarRequiereApoyoCommand.Execute(null);
        vm.FiltroEntrega = FiltroEntrega.RequiereApoyo;

        var visible = Assert.Single(vm.FilasVisibles);
        Assert.Equal(2, visible.NumeroLista);

        vm.SeleccionarActividad(1);
        Assert.Empty(vm.FilasVisibles);
    }

    [Fact]
    public void DescartarRestauraTodasLasActividadesModificadas()
    {
        var vm = Crear(out var gestion);
        vm.Inicializar(gestion.GrupoId);
        vm.SeleccionarCelda(vm.Filas[0], 0);
        vm.MarcarDominaCommand.Execute(null);
        vm.SeleccionarCelda(vm.Filas[1], 2);
        vm.MarcarNoEntregoCommand.Execute(null);

        vm.DescartarCambiosCommand.Execute(null);

        Assert.False(vm.TieneCambios);
        Assert.Equal(NivelLogro.Pendiente, vm.Filas[0].Celdas[0].NivelLogro);
        Assert.Equal(NivelLogro.Pendiente, vm.Filas[1].Celdas[2].NivelLogro);
    }

    private static EvaluacionActividadesViewModel Crear(
        out GestionDoble gestion,
        IDialogoCambiosPendientes? dialogo = null)
    {
        gestion = new GestionDoble();
        return new EvaluacionActividadesViewModel(
            gestion,
            dialogo ?? new Dialogo(DecisionCambiosPendientes.Cancelar),
            new MensajesDoble());
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
        private readonly EstudianteId[] _estudiantes = Enumerable.Range(1, 4)
            .Select(n => EstudianteId.DesdeGuid(GuidUtility(n))).ToArray();

        internal GestionDoble()
        {
            GrupoId = GrupoId.DesdeGuid(Guid.NewGuid());
            _proyectos.Add(new ProyectoDetalle(
                ProyectoId.DesdeGuid(_proyecto1), GrupoId, "Proyecto 1", "",
                new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31),
                EstadoProyecto.EnCurso, "", 3, 1, false));

            _actividades.Add(CrearActividad(1, new DateOnly(2026, 1, 8), [1, 2, 3]));
            _actividades.Add(CrearActividad(2, new DateOnly(2026, 1, 15), [1, 2, 3]));
            _actividades.Add(CrearActividad(3, new DateOnly(2026, 1, 22), [1, 2, 3, 4]));
        }

        internal GrupoId GrupoId { get; }
        internal int EntregasGuardadas { get; private set; }

        public IReadOnlyList<ProyectoResumen> ListarProyectos(GrupoId grupoId) =>
            _proyectos.Where(x => x.GrupoId == grupoId)
                .Select(x => new ProyectoResumen(
                    x.ProyectoId, x.Nombre, x.FechaInicio, x.FechaTermino,
                    x.Estado, x.NumeroActividades, x.Version)).ToArray();

        public ProyectoDetalle ObtenerProyecto(ProyectoId id) => _proyectos.First(x => x.ProyectoId == id);
        public ProyectoDetalle CrearProyecto(GrupoId grupoId, EntradaProyecto datos) => throw new NotImplementedException();
        public ProyectoDetalle ActualizarProyecto(ProyectoId id, int version, EntradaProyecto entrada) => throw new NotImplementedException();
        public ProyectoDetalle CambiarEstado(ProyectoId id, int version, EstadoProyecto estado) => throw new NotImplementedException();
        public ProyectoDetalle Reabrir(ProyectoId id, int version) => throw new NotImplementedException();
        public void EliminarProyecto(ProyectoId id, int version) => throw new NotImplementedException();

        public IReadOnlyList<ActividadProyectoResumen> ListarActividades(ProyectoId proyectoId) =>
            _actividades.Where(x => x.ProyectoId == proyectoId)
                .OrderBy(x => x.FechaRealizacion)
                .Select(Resumir).ToArray();

        public ActividadProyectoDetalle PrepararActividad(
            ProyectoId proyectoId, string titulo, string descripcion, DateOnly fecha, string observaciones) =>
            throw new NotImplementedException();
        public ActividadProyectoDetalle CrearActividad(ProyectoId proyectoId, EntradaActividad datos) => throw new NotImplementedException();
        public ActividadProyectoDetalle ObtenerActividad(ActividadId id) => _actividades.First(x => x.ActividadId == id);
        public ActividadProyectoDetalle ActualizarActividad(ActividadId id, int version, EntradaActividad entrada) => throw new NotImplementedException();

        public ActividadProyectoDetalle GuardarEntregas(
            ActividadId id,
            int version,
            IReadOnlyCollection<EntradaEntregaActividad> entregas)
        {
            EntregasGuardadas++;
            var actividad = _actividades.First(x => x.ActividadId == id);
            Assert.Equal(actividad.Version, version);
            var nuevas = actividad.Entregas.Select(x =>
            {
                var entrada = entregas.Single(e => e.EstudianteId == x.EstudianteId);
                return new EntregaActividadDetalle(
                    x.EstudianteId, x.NumeroLista, x.NombreVisible, x.EstaActivoActualmente,
                    entrada.NivelLogro, entrada.Observacion);
            }).ToArray();

            var actualizada = new ActividadProyectoDetalle(
                actividad.ActividadId, actividad.ProyectoId, actividad.GrupoId,
                actividad.Titulo, actividad.Descripcion, actividad.FechaRealizacion,
                actividad.ObservacionesGenerales, actividad.Estado, nuevas,
                nuevas.Length,
                nuevas.Count(x => x.NivelLogro == NivelLogro.Pendiente),
                nuevas.Count(x => x.NivelLogro == NivelLogro.Domina),
                nuevas.Count(x => x.NivelLogro == NivelLogro.Suficiente),
                nuevas.Count(x => x.NivelLogro == NivelLogro.EnProceso),
                nuevas.Count(x => x.NivelLogro == NivelLogro.RequiereApoyo),
                nuevas.Count(x => x.NivelLogro == NivelLogro.NoEntrego),
                actividad.Version + 1);
            _actividades[_actividades.IndexOf(actividad)] = actualizada;
            return actualizada;
        }

        public ActividadProyectoDetalle AnularActividad(ActividadId id, int version) => throw new NotImplementedException();
        public void EliminarActividad(ActividadId id, int version) => throw new NotImplementedException();

        private ActividadProyectoDetalle CrearActividad(int numero, DateOnly fecha, int[] numerosLista)
        {
            var actividadId = ActividadId.DesdeGuid(GuidUtility(100 + numero));
            var entregas = numerosLista.Select(n => new EntregaActividadDetalle(
                _estudiantes[n - 1], n, $"Estudiante {n}", true,
                NivelLogro.Pendiente, string.Empty)).ToArray();
            return new ActividadProyectoDetalle(
                actividadId, ProyectoId.DesdeGuid(_proyecto1), GrupoId,
                $"Actividad {numero}", "", fecha, "", EstadoActividad.Activa,
                entregas, entregas.Length, entregas.Length, 0, 0, 0, 0, 0, 1);
        }

        private static ActividadProyectoResumen Resumir(ActividadProyectoDetalle actividad) => new(
            actividad.ActividadId, actividad.ProyectoId, actividad.Titulo, actividad.FechaRealizacion,
            actividad.Estado, actividad.Total, actividad.Pendientes, actividad.Domina,
            actividad.Suficiente, actividad.EnProceso, actividad.RequiereApoyo,
            actividad.NoEntrego, actividad.Version);

        private static Guid GuidUtility(int numero)
        {
            var bytes = new byte[16];
            BitConverter.GetBytes(numero).CopyTo(bytes, 0);
            return new Guid(bytes);
        }
    }
}
