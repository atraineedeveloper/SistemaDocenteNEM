using SistemaDocente.Application;
using SistemaDocente.Core;
using SistemaDocente.Presentation;

namespace SistemaDocente.Presentation.Tests;

public sealed class GestionProyectosViewModelTests
{
    [Fact]
    public void ProyectoExistenteSinCambiosNoTieneCambios()
    {
        var vm = Crear(out var gestion);
        SeleccionarPrimero(vm, gestion);

        Assert.False(vm.TieneCambiosProyecto);
        Assert.False(vm.GuardarProyectoCommand.CanExecute(null));
    }

    [Fact]
    public void NombreDescripcionFechasYObservacionesDetectanCambios()
    {
        VerificarCambio(vm => vm.NombreProyecto = "Nombre editado");
        VerificarCambio(vm => vm.DescripcionProyecto = "Descripción editada");
        VerificarCambio(vm => vm.FechaInicio = vm.FechaInicio.AddDays(1));
        VerificarCambio(vm => vm.FechaTermino = vm.FechaTermino.AddDays(1));
        VerificarCambio(vm => vm.ObservacionesProyecto = "Observación editada");
    }

    [Fact]
    public void ProyectoNuevoNoGuardadoTieneCambios()
    {
        var vm = Crear(out var gestion);
        vm.Inicializar(gestion.GrupoId);
        vm.ConfigurarGradosDisponibles([GradoPrimaria.Cuarto]);

        vm.NuevoProyectoCommand.Execute(null);

        Assert.True(vm.TieneCambiosProyecto);
    }

    [Fact]
    public void ProyectoNuevoUnigradoPreseleccionaGradoYRequiereMetodologia()
    {
        var vm = Crear(out var gestion);
        vm.Inicializar(gestion.GrupoId);
        vm.ConfigurarGradosDisponibles([GradoPrimaria.Cuarto]);

        vm.NuevoProyectoCommand.Execute(null);
        vm.NombreProyecto = "Proyecto NEM";

        Assert.Equal([GradoPrimaria.Cuarto], vm.GradosProyectoSeleccionados);
        Assert.False(vm.GuardarProyectoCommand.CanExecute(null));

        vm.MetodologiaProyecto = MetodologiaProyectoNem.ProyectosComunitarios;

        Assert.True(vm.GuardarProyectoCommand.CanExecute(null));
    }

    [Fact]
    public void GuardarProyectoConfirmaSnapshot()
    {
        var vm = Crear(out var gestion);
        SeleccionarPrimero(vm, gestion);
        vm.NombreProyecto = "Nombre guardado";

        vm.GuardarProyectoCommand.Execute(null);

        Assert.Equal("Nombre guardado", vm.NombreProyecto);
        Assert.False(vm.TieneCambiosProyecto);
        Assert.False(vm.GuardarProyectoCommand.CanExecute(null));
        Assert.Equal(1, gestion.ProyectosGuardados);
    }

    [Fact]
    public void DescartarProyectoRestauraSnapshotConfirmado()
    {
        var dialogo = new Dialogo(DecisionCambiosPendientes.Descartar);
        var vm = Crear(out var gestion, dialogo);
        SeleccionarPrimero(vm, gestion);
        var nombreConfirmado = vm.NombreProyecto;
        vm.NombreProyecto = "Cambio descartado";

        var puedeSalir = vm.SolicitarSalir();

        Assert.True(puedeSalir);
        Assert.Equal(nombreConfirmado, vm.NombreProyecto);
        Assert.False(vm.TieneCambiosProyecto);
    }

    [Fact]
    public void CancelarConservaEdicionYSeleccionAlCambiarProyecto()
    {
        var dialogo = new Dialogo(DecisionCambiosPendientes.Cancelar);
        var vm = Crear(out var gestion, dialogo);
        SeleccionarPrimero(vm, gestion);
        var seleccionado = vm.ProyectoSeleccionado;
        vm.NombreProyecto = "Cambio local";

        vm.ProyectoSeleccionado = vm.ProyectosVisibles[1];

        Assert.Equal(seleccionado, vm.ProyectoSeleccionado);
        Assert.Equal("Cambio local", vm.NombreProyecto);
        Assert.True(vm.TieneCambiosProyecto);
    }

    [Theory]
    [InlineData(DecisionCambiosPendientes.Guardar, 1)]
    [InlineData(DecisionCambiosPendientes.Descartar, 0)]
    public void CambiarProyectoContinuaTrasGuardarODescartar(
        DecisionCambiosPendientes decision,
        int guardados)
    {
        var dialogo = new Dialogo(decision);
        var vm = Crear(out var gestion, dialogo);
        SeleccionarPrimero(vm, gestion);
        vm.NombreProyecto = "Cambio local";
        var destino = vm.ProyectosVisibles[1];

        vm.ProyectoSeleccionado = destino;

        Assert.Equal(destino.ProyectoId, vm.ProyectoSeleccionado?.ProyectoId);
        Assert.Equal(guardados, gestion.ProyectosGuardados);
        Assert.False(vm.TieneCambiosProyecto);
    }

    [Fact]
    public void NuevoProyectoRespetaCancelarYCierraConLaMismaPolitica()
    {
        var dialogo = new Dialogo(
            DecisionCambiosPendientes.Cancelar,
            DecisionCambiosPendientes.Cancelar);
        var vm = Crear(out var gestion, dialogo);
        SeleccionarPrimero(vm, gestion);
        var seleccionado = vm.ProyectoSeleccionado;
        vm.DescripcionProyecto = "Pendiente";

        vm.NuevoProyectoCommand.Execute(null);
        var puedeCerrar = vm.SolicitarSalir();

        Assert.Equal(seleccionado, vm.ProyectoSeleccionado);
        Assert.Equal("Pendiente", vm.DescripcionProyecto);
        Assert.False(puedeCerrar);
        Assert.Equal(["el proyecto", "el proyecto"], dialogo.Contextos);
    }

    [Fact]
    public void FalloAlGuardarProyectoConservaEdicionYNoCambiaSeleccion()
    {
        var dialogo = new Dialogo(DecisionCambiosPendientes.Guardar);
        var vm = Crear(out var gestion, dialogo);
        SeleccionarPrimero(vm, gestion);
        var seleccionado = vm.ProyectoSeleccionado;
        gestion.FallarProyecto = true;
        vm.NombreProyecto = "No persistido";

        vm.ProyectoSeleccionado = vm.ProyectosVisibles[1];

        Assert.Equal(seleccionado, vm.ProyectoSeleccionado);
        Assert.Equal("No persistido", vm.NombreProyecto);
        Assert.True(vm.TieneCambiosProyecto);
        Assert.True(vm.GuardarProyectoCommand.CanExecute(null));
    }

    [Fact]
    public void CambiosSimultaneosResuelvenActividadAntesQueProyectoSinPerdidas()
    {
        var dialogo = new Dialogo(
            DecisionCambiosPendientes.Guardar,
            DecisionCambiosPendientes.Cancelar);
        var vm = Crear(out var gestion, dialogo);
        SeleccionarPrimero(vm, gestion);
        vm.ActividadSeleccionada = Assert.Single(vm.Actividades);
        vm.TituloActividad = "Actividad guardada";
        vm.NombreProyecto = "Proyecto aún local";
        var proyectoOriginal = vm.ProyectoSeleccionado;

        vm.ProyectoSeleccionado = vm.ProyectosVisibles[1];

        Assert.Equal(["la actividad", "el proyecto"], dialogo.Contextos);
        Assert.Equal(1, gestion.ActividadesGuardadas);
        Assert.False(vm.TieneCambiosActividad);
        Assert.True(vm.TieneCambiosProyecto);
        Assert.Equal("Proyecto aún local", vm.NombreProyecto);
        Assert.Equal(proyectoOriginal, vm.ProyectoSeleccionado);
    }

    [Fact]
    public void ActividadPermiteENPConteosFiltrosYGuardar()
    {
        var vm = Crear(out var gestion);
        SeleccionarPrimero(vm, gestion);
        vm.NuevaActividadCommand.Execute(null);
        vm.CampoFormativoActividad = CampoFormativoNem.Lenguajes;
        vm.Entregas[0].Seleccionada = true;

        vm.MarcarDominaCommand.Execute(null);

        Assert.Equal(1, vm.Domina);
        Assert.True(vm.TieneCambiosActividad);
        Assert.True(vm.GuardarActividadCommand.CanExecute(null));
        vm.GuardarActividadCommand.Execute(null);
        Assert.Equal(1, gestion.ActividadesGuardadas);
    }

    [Fact]
    public void FalloAlGuardarActividadConservaEdicionYCanExecute()
    {
        var vm = Crear(out var gestion);
        gestion.FallarActividad = true;
        SeleccionarPrimero(vm, gestion);
        vm.NuevaActividadCommand.Execute(null);
        vm.CampoFormativoActividad = CampoFormativoNem.Lenguajes;
        vm.TituloActividad = "Editada";

        vm.GuardarActividadCommand.Execute(null);

        Assert.Equal("Editada", vm.TituloActividad);
        Assert.True(vm.TieneCambiosActividad);
        Assert.True(vm.GuardarActividadCommand.CanExecute(null));
    }

    [Fact]
    public void ActividadLegacyPuedeEditarCampoSinInventarGrados()
    {
        var vm = Crear(out var gestion);
        gestion.UsarActividadLegacy = true;
        SeleccionarPrimero(vm, gestion);
        vm.ActividadSeleccionada = Assert.Single(vm.Actividades);

        Assert.Empty(vm.GradosActividadSeleccionados);
        Assert.False(vm.PuedeEditarGradosActividad);

        vm.CampoFormativoActividad = CampoFormativoNem.Lenguajes;

        Assert.True(vm.GuardarActividadCommand.CanExecute(null));
        vm.GuardarActividadCommand.Execute(null);

        Assert.NotNull(gestion.UltimaEntradaActividad);
        Assert.Empty(gestion.UltimaEntradaActividad.GradosObjetivo ?? []);
    }

    [Fact]
    public void GrillaRepresentaCuarentaEstudiantesSinPerdida()
    {
        var vm = Crear(out var gestion);
        gestion.CantidadEstudiantes = 40;
        SeleccionarPrimero(vm, gestion);
        vm.ActividadSeleccionada = Assert.Single(vm.Actividades);

        Assert.Equal(40, vm.Entregas.Count);
        Assert.Equal(40, vm.Entregas.Select(x => x.NumeroLista).Distinct().Count());
        Assert.Equal(40, vm.Total);
    }

    private static void VerificarCambio(Action<GestionProyectosViewModel> cambio)
    {
        var vm = Crear(out var gestion);
        SeleccionarPrimero(vm, gestion);

        cambio(vm);

        Assert.True(vm.TieneCambiosProyecto);
        Assert.True(vm.GuardarProyectoCommand.CanExecute(null));
    }

    private static void SeleccionarPrimero(GestionProyectosViewModel vm, GestionDoble gestion)
    {
        vm.Inicializar(gestion.GrupoId);
        vm.ConfigurarGradosDisponibles([GradoPrimaria.Cuarto]);
        vm.ProyectoSeleccionado = vm.ProyectosVisibles[0];
    }

    private static GestionProyectosViewModel Crear(
        out GestionDoble gestion,
        Dialogo? dialogo = null)
    {
        gestion = new();
        return new(gestion, dialogo ?? new Dialogo(), new Confirmacion(), new Mensajes());
    }

    private sealed class GestionDoble : IGestionProyectosPresentacion
    {
        private readonly ProyectoId _proyecto1 = ProyectoId.DesdeGuid(Guid.NewGuid());
        private readonly ProyectoId _proyecto2 = ProyectoId.DesdeGuid(Guid.NewGuid());
        private readonly ActividadId _actividadId = ActividadId.DesdeGuid(Guid.NewGuid());
        private readonly Dictionary<ProyectoId, ProyectoDetalle> _proyectos;

        public GestionDoble()
        {
            GrupoId = Grupo.Crear("Grupo").Id;
            _proyectos = new()
            {
                [_proyecto1] = Proyecto(_proyecto1, "Proyecto uno"),
                [_proyecto2] = Proyecto(_proyecto2, "Proyecto dos"),
            };
        }

        internal GrupoId GrupoId { get; }
        internal bool FallarProyecto { get; set; }
        internal bool FallarActividad { get; set; }
        internal bool UsarActividadLegacy { get; set; }
        internal int CantidadEstudiantes { get; set; } = 1;
        internal int ProyectosGuardados { get; private set; }
        internal int ActividadesGuardadas { get; private set; }
        internal EntradaActividad? UltimaEntradaActividad { get; private set; }

        public IReadOnlyList<ProyectoResumen> ListarProyectos(GrupoId grupoId) =>
            _proyectos.Values.Select(Resumir).OrderBy(x => x.Nombre).ToArray();

        public ProyectoDetalle ObtenerProyecto(ProyectoId id) => _proyectos[id];

        public ProyectoDetalle CrearProyecto(GrupoId grupoId, EntradaProyecto entrada)
        {
            FallarSiCorresponde(FallarProyecto);
            var id = ProyectoId.DesdeGuid(Guid.NewGuid());
            var proyecto = CrearDetalle(id, entrada, 1);
            _proyectos[id] = proyecto;
            ProyectosGuardados++;
            return proyecto;
        }

        public ProyectoDetalle ActualizarProyecto(ProyectoId id, int version, EntradaProyecto entrada)
        {
            FallarSiCorresponde(FallarProyecto);
            var proyecto = CrearDetalle(id, entrada, version + 1);
            _proyectos[id] = proyecto;
            ProyectosGuardados++;
            return proyecto;
        }

        public ProyectoDetalle CambiarEstado(ProyectoId id, int version, EstadoProyecto estado)
        {
            var actualizado = _proyectos[id] with { Estado = estado, Version = version + 1 };
            _proyectos[id] = actualizado;
            return actualizado;
        }

        public ProyectoDetalle Reabrir(ProyectoId id, int version) =>
            CambiarEstado(id, version, EstadoProyecto.EnCurso);

        public void EliminarProyecto(ProyectoId id, int version) => _proyectos.Remove(id);

        public IReadOnlyList<ActividadProyectoResumen> ListarActividades(ProyectoId proyectoId) =>
            [Resumir(Actividad("Actividad", 1, legacy: UsarActividadLegacy))];

        public ActividadProyectoDetalle PrepararActividad(
            ProyectoId proyectoId,
            string titulo,
            string descripcion,
            DateOnly fecha,
            string observaciones) => Actividad(
                titulo,
                1,
                CampoFormativoNem.NoEspecificado,
                [],
                legacy: true);

        public ActividadProyectoDetalle CrearActividad(ProyectoId proyectoId, EntradaActividad entrada)
        {
            FallarSiCorresponde(FallarActividad);
            UltimaEntradaActividad = entrada;
            ActividadesGuardadas++;
            return Actividad(
                entrada.Titulo,
                2,
                entrada.CampoFormativo,
                entrada.GradosObjetivo?.ToArray() ?? [],
                legacy: false);
        }

        public ActividadProyectoDetalle ObtenerActividad(ActividadId id) =>
            Actividad("Actividad", 1, legacy: UsarActividadLegacy);

        public ActividadProyectoDetalle ActualizarActividad(
            ActividadId id,
            int version,
            EntradaActividad entrada) => CrearActividad(_proyecto1, entrada);

        public ActividadProyectoDetalle GuardarEntregas(
            ActividadId id,
            int version,
            IReadOnlyCollection<EntradaEntregaActividad> entradas) => Actividad("Actividad", version + 1);

        public ActividadProyectoDetalle AnularActividad(ActividadId id, int version) =>
            Actividad("Actividad", version + 1) with { Estado = EstadoActividad.Anulada };

        public void EliminarActividad(ActividadId id, int version) { }

        private ProyectoDetalle Proyecto(ProyectoId id, string nombre) => new(
            id,
            GrupoId,
            nombre,
            "Descripción",
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 1, 31),
            EstadoProyecto.Borrador,
            "Observaciones",
            1,
            1,
            false,
            MetodologiaProyectoNem.ProyectosComunitarios,
            [GradoPrimaria.Cuarto]);

        private ProyectoDetalle CrearDetalle(ProyectoId id, EntradaProyecto entrada, int version) => new(
            id,
            GrupoId,
            entrada.Nombre,
            entrada.Descripcion,
            entrada.FechaInicio,
            entrada.FechaTermino,
            EstadoProyecto.Borrador,
            entrada.Observaciones,
            1,
            version,
            false,
            entrada.Metodologia,
            entrada.GradosObjetivo?.ToArray() ?? []);

        private ActividadProyectoDetalle Actividad(
            string titulo,
            int version,
            CampoFormativoNem campo = CampoFormativoNem.Lenguajes,
            IReadOnlyList<GradoPrimaria>? grados = null,
            bool legacy = false)
        {
            var entregas = Enumerable.Range(1, CantidadEstudiantes)
                .Select(numero => new EntregaActividadDetalle(
                    EstudianteId.DesdeGuid(GuidUtility(numero)),
                    numero,
                    $"Estudiante {numero}",
                    true,
                    EstadoEntregaActividad.Pendiente,
                    NivelLogro.Pendiente,
                    "",
                    GradoPrimaria.Cuarto))
                .ToArray();
            var campoReal = legacy ? CampoFormativoNem.NoEspecificado : campo;
            var gradosReales = legacy ? Array.Empty<GradoPrimaria>() : grados ?? [GradoPrimaria.Cuarto];
            return new(
                _actividadId,
                _proyecto1,
                GrupoId,
                titulo,
                "",
                new DateOnly(2026, 1, 2),
                "",
                EstadoActividad.Activa,
                entregas,
                entregas.Length,
                entregas.Length,
                0,
                0,
                0,
                0,
                0,
                version,
                campoReal,
                gradosReales);
        }

        private static Guid GuidUtility(int numero)
        {
            var bytes = new byte[16];
            BitConverter.GetBytes(numero).CopyTo(bytes, 0);
            return new Guid(bytes);
        }

        private static ProyectoResumen Resumir(ProyectoDetalle proyecto) => new(
            proyecto.ProyectoId,
            proyecto.Nombre,
            proyecto.FechaInicio,
            proyecto.FechaTermino,
            proyecto.Estado,
            proyecto.NumeroActividades,
            proyecto.Version,
            proyecto.Metodologia,
            proyecto.GradosObjetivo);

        private static ActividadProyectoResumen Resumir(ActividadProyectoDetalle actividad) => new(
            actividad.ActividadId,
            actividad.ProyectoId,
            actividad.Titulo,
            actividad.FechaRealizacion,
            actividad.Estado,
            actividad.Total,
            actividad.Pendientes,
            actividad.Domina,
            actividad.Suficiente,
            actividad.EnProceso,
            actividad.RequiereApoyo,
            actividad.NoEntrego,
            actividad.Version,
            actividad.CampoFormativo,
            actividad.GradosObjetivo);

        private static void FallarSiCorresponde(bool fallar)
        {
            if (fallar)
            {
                throw new ErrorPersistenciaAplicacionException("Fallo", new InvalidOperationException());
            }
        }
    }

    private sealed class Dialogo(params DecisionCambiosPendientes[] decisiones) : IDialogoCambiosPendientes
    {
        private readonly Queue<DecisionCambiosPendientes> _decisiones = new(decisiones);

        internal List<string> Contextos { get; } = [];

        public DecisionCambiosPendientes ConfirmarCambiosPendientes() =>
            ConfirmarCambiosPendientes("cambios");

        public DecisionCambiosPendientes ConfirmarCambiosPendientes(string contexto)
        {
            Contextos.Add(contexto);
            return _decisiones.Count == 0 ? DecisionCambiosPendientes.Cancelar : _decisiones.Dequeue();
        }
    }

    private sealed class Confirmacion : IConfirmacionProyectos
    {
        public bool Confirmar(string mensaje) => true;
    }

    private sealed class Mensajes : IServicioMensajes
    {
        public void MostrarError(string mensaje) { }
    }
}