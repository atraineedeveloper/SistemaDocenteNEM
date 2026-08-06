using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Presentation;

public sealed class EvaluacionActividadesViewModel : ViewModelBase
{
    private readonly IGestionProyectosPresentacion _gestion;
    private readonly IDialogoCambiosPendientes _dialogo;
    private readonly IServicioMensajes _mensajes;
    private GrupoId? _grupoId;
    private ProyectoResumen? _proyecto;
    private ActividadProyectoResumen? _actividad;
    private ActividadProyectoDetalle? _actividadConfirmada;
    private IReadOnlyList<ProyectoResumen> _proyectos = [];
    private IReadOnlyList<ActividadProyectoResumen> _actividades = [];
    private IReadOnlyList<EntregaActividadVisual> _entregas = [];
    private IReadOnlyList<EntregaActividadVisual> _entregasVisibles = [];
    private EntregaActividadVisual? _entregaSeleccionada;
    private bool _estaOcupado;
    private FiltroEntrega _filtroEntrega;
    private string _busquedaEstudiante = string.Empty;

    public EvaluacionActividadesViewModel(
        IGestionProyectosPresentacion gestion,
        IDialogoCambiosPendientes dialogo,
        IServicioMensajes mensajes)
    {
        _gestion = gestion ?? throw new ArgumentNullException(nameof(gestion));
        _dialogo = dialogo ?? throw new ArgumentNullException(nameof(dialogo));
        _mensajes = mensajes ?? throw new ArgumentNullException(nameof(mensajes));

        GuardarActividadCommand = new RelayCommand(
            GuardarActividad,
            () => PuedeEditar && TieneCambios);
        DescartarActividadCommand = new RelayCommand(
            DescartarActividad,
            () => TieneCambios && !EstaOcupado);
        MarcarDominaCommand = new RelayCommand(
            () => Marcar(NivelLogro.Domina), () => PuedeEditar);
        MarcarSuficienteCommand = new RelayCommand(
            () => Marcar(NivelLogro.Suficiente), () => PuedeEditar);
        MarcarEnProcesoCommand = new RelayCommand(
            () => Marcar(NivelLogro.EnProceso), () => PuedeEditar);
        MarcarRequiereApoyoCommand = new RelayCommand(
            () => Marcar(NivelLogro.RequiereApoyo), () => PuedeEditar);
        MarcarNoEntregoCommand = new RelayCommand(
            () => Marcar(NivelLogro.NoEntrego), () => PuedeEditar);
        MarcarPendienteCommand = new RelayCommand(
            () => Marcar(NivelLogro.Pendiente), () => PuedeEditar);

        MarcarTodosDominaCommand = new RelayCommand(
            () => MarcarTodos(NivelLogro.Domina), () => PuedeEditar);
        MarcarTodosSuficienteCommand = new RelayCommand(
            () => MarcarTodos(NivelLogro.Suficiente), () => PuedeEditar);
        MarcarTodosEnProcesoCommand = new RelayCommand(
            () => MarcarTodos(NivelLogro.EnProceso), () => PuedeEditar);
        MarcarTodosRequiereApoyoCommand = new RelayCommand(
            () => MarcarTodos(NivelLogro.RequiereApoyo), () => PuedeEditar);
        MarcarTodosNoEntregoCommand = new RelayCommand(
            () => MarcarTodos(NivelLogro.NoEntrego), () => PuedeEditar);
    }

    public RelayCommand GuardarActividadCommand { get; }
    public RelayCommand DescartarActividadCommand { get; }
    public RelayCommand MarcarDominaCommand { get; }
    public RelayCommand MarcarSuficienteCommand { get; }
    public RelayCommand MarcarEnProcesoCommand { get; }
    public RelayCommand MarcarRequiereApoyoCommand { get; }
    public RelayCommand MarcarNoEntregoCommand { get; }
    public RelayCommand MarcarPendienteCommand { get; }
    public RelayCommand MarcarTodosDominaCommand { get; }
    public RelayCommand MarcarTodosSuficienteCommand { get; }
    public RelayCommand MarcarTodosEnProcesoCommand { get; }
    public RelayCommand MarcarTodosRequiereApoyoCommand { get; }
    public RelayCommand MarcarTodosNoEntregoCommand { get; }

    public EntregaActividadVisual? EntregaSeleccionada
    {
        get => _entregaSeleccionada;
        set => SetProperty(ref _entregaSeleccionada, value);
    }

    public IReadOnlyList<FiltroEntrega> FiltrosEntrega { get; } = Enum.GetValues<FiltroEntrega>();

    public IReadOnlyList<ProyectoResumen> Proyectos
    {
        get => _proyectos;
        private set => SetProperty(ref _proyectos, value);
    }

    public IReadOnlyList<ActividadProyectoResumen> Actividades
    {
        get => _actividades;
        private set => SetProperty(ref _actividades, value);
    }

    public IReadOnlyList<EntregaActividadVisual> Entregas
    {
        get => _entregas;
        private set => SetProperty(ref _entregas, value);
    }

    public IReadOnlyList<EntregaActividadVisual> EntregasVisibles
    {
        get => _entregasVisibles;
        private set => SetProperty(ref _entregasVisibles, value);
    }

    public ProyectoResumen? ProyectoSeleccionado
    {
        get => _proyecto;
        set
        {
            if (value == _proyecto) return;
            if (!ConfirmarPendientes())
            {
                OnPropertyChanged();
                return;
            }

            _proyecto = value;
            OnPropertyChanged();
            LimpiarActividad();
            CargarActividades();
        }
    }

    public ActividadProyectoResumen? ActividadSeleccionada
    {
        get => _actividad;
        set
        {
            if (value == _actividad) return;
            if (!ConfirmarPendientes())
            {
                OnPropertyChanged();
                return;
            }

            _actividad = value;
            OnPropertyChanged();
            CargarActividad();
        }
    }

    public FiltroEntrega FiltroEntrega
    {
        get => _filtroEntrega;
        set { if (SetProperty(ref _filtroEntrega, value)) AplicarFiltros(); }
    }

    public string BusquedaEstudiante
    {
        get => _busquedaEstudiante;
        set { if (SetProperty(ref _busquedaEstudiante, value)) AplicarFiltros(); }
    }

    public bool EstaOcupado
    {
        get => _estaOcupado;
        private set { if (SetProperty(ref _estaOcupado, value)) NotificarComandos(); }
    }

    public bool PuedeEditar => !EstaOcupado
        && ProyectoSeleccionado?.Estado != EstadoProyecto.Finalizado
        && ActividadSeleccionada?.Estado == EstadoActividad.Activa;

    public bool TieneCambios => _actividadConfirmada is not null
        && Entregas.Any(fila =>
        {
            var confirmada = _actividadConfirmada.Entregas.Single(x => x.EstudianteId == fila.EstudianteId);
            return confirmada.NivelLogro != fila.NivelLogro || confirmada.Observacion != fila.Observacion;
        });

    public int Total => Entregas.Count;
    public int Pendientes => Entregas.Count(x => x.NivelLogro == NivelLogro.Pendiente);
    public int Domina => Entregas.Count(x => x.NivelLogro == NivelLogro.Domina);
    public int Suficiente => Entregas.Count(x => x.NivelLogro == NivelLogro.Suficiente);
    public int EnProceso => Entregas.Count(x => x.NivelLogro == NivelLogro.EnProceso);
    public int RequiereApoyo => Entregas.Count(x => x.NivelLogro == NivelLogro.RequiereApoyo);
    public int NoEntrego => Entregas.Count(x => x.NivelLogro == NivelLogro.NoEntrego);

    public void Inicializar(GrupoId grupoId)
    {
        if (_grupoId is not null && _grupoId != grupoId && !ConfirmarPendientes()) return;
        _grupoId = grupoId;
        RecargarProyectos();
    }

    public bool SolicitarSalir() => ConfirmarPendientes();

    private void RecargarProyectos()
    {
        if (_grupoId is null) return;
        var proyectos = EjecutarResultado(() => _gestion.ListarProyectos(_grupoId.Value));
        if (proyectos is null) return;
        Proyectos = proyectos;
        if (_proyecto is not null)
        {
            _proyecto = proyectos.FirstOrDefault(x => x.ProyectoId == _proyecto.ProyectoId);
            OnPropertyChanged(nameof(ProyectoSeleccionado));
        }
        if (_proyecto is null && Proyectos.Count > 0)
        {
            ProyectoSeleccionado = Proyectos[0];
        }
    }

    private void CargarActividades()
    {
        if (_proyecto is null)
        {
            Actividades = [];
            return;
        }

        var actividades = EjecutarResultado(() => _gestion.ListarActividades(_proyecto.ProyectoId));
        if (actividades is null) return;
        Actividades = actividades;
        if (Actividades.Count > 0)
        {
            ActividadSeleccionada = Actividades[0];
        }
    }

    private void CargarActividad()
    {
        if (_actividad is null)
        {
            _actividadConfirmada = null;
            Entregas = [];
            AplicarFiltros();
            NotificarEdicion();
            return;
        }

        var detalle = EjecutarResultado(() => _gestion.ObtenerActividad(_actividad.ActividadId));
        if (detalle is not null)
        {
            AplicarActividad(detalle);
        }
    }

    private void AplicarActividad(ActividadProyectoDetalle detalle)
    {
        _actividadConfirmada = detalle;
        Entregas = detalle.Entregas.Select(x => new EntregaActividadVisual(x)).ToArray();
        foreach (var fila in Entregas)
        {
            fila.PropertyChanged += (_, _) =>
            {
                AplicarFiltros();
                NotificarEdicion();
            };
        }

        AplicarFiltros();
        NotificarEdicion();
    }

    private void GuardarActividad() => IntentarGuardarActividad();

    private bool IntentarGuardarActividad()
    {
        if (_actividad is null || _actividadConfirmada is null) return false;
        ActividadProyectoDetalle? guardada = null;
        var entradas = Entregas.Select(x =>
            new EntradaEntregaActividad(x.EstudianteId, x.NivelLogro, x.Observacion)).ToArray();
        var entrada = new EntradaActividad(
            _actividadConfirmada.Titulo, _actividadConfirmada.Descripcion,
            _actividadConfirmada.FechaRealizacion, _actividadConfirmada.ObservacionesGenerales, entradas);

        var correcto = Ejecutar(() => guardada = _gestion.GuardarEntregas(
            _actividad.ActividadId, _actividad.Version, entradas));

        if (!correcto || guardada is null) return false;

        _actividad = Resumir(guardada);
        OnPropertyChanged(nameof(ActividadSeleccionada));
        AplicarActividad(guardada);
        if (_proyecto is not null)
        {
            _actividades = _gestion.ListarActividades(_proyecto.ProyectoId);
            OnPropertyChanged(nameof(Actividades));
        }

        return true;
    }

    private void DescartarActividad()
    {
        if (_actividadConfirmada is not null) AplicarActividad(_actividadConfirmada);
    }

    private void Marcar(NivelLogro nivel)
    {
        var marcados = Entregas.Where(x => x.Seleccionada).ToArray();
        if (marcados.Length > 0)
        {
            foreach (var fila in marcados) fila.NivelLogro = nivel;
        }
        else if (EntregaSeleccionada is not null)
        {
            EntregaSeleccionada.NivelLogro = nivel;
        }
        else
        {
            foreach (var fila in EntregasVisibles) fila.NivelLogro = nivel;
        }

        AplicarFiltros();
        NotificarEdicion();
    }

    private void MarcarTodos(NivelLogro nivel)
    {
        foreach (var fila in EntregasVisibles) fila.NivelLogro = nivel;
        AplicarFiltros();
        NotificarEdicion();
    }

    private bool ConfirmarPendientes()
    {
        if (!TieneCambios) return true;
        return _dialogo.ConfirmarCambiosPendientes("la evaluación de la actividad") switch
        {
            DecisionCambiosPendientes.Guardar => IntentarGuardarActividad(),
            DecisionCambiosPendientes.Descartar => DescartarActividadPendiente(),
            _ => false,
        };
    }

    private bool DescartarActividadPendiente()
    {
        DescartarActividad();
        return true;
    }

    private void LimpiarActividad()
    {
        _actividad = null;
        _actividadConfirmada = null;
        _actividades = [];
        Entregas = [];
        OnPropertyChanged(nameof(ActividadSeleccionada));
        OnPropertyChanged(nameof(Actividades));
        AplicarFiltros();
        NotificarEdicion();
    }

    private void AplicarFiltros()
    {
        EntregasVisibles = Entregas.Where(x =>
        {
            var coincideBusqueda = string.IsNullOrWhiteSpace(BusquedaEstudiante)
                || x.Nombre.Contains(BusquedaEstudiante, StringComparison.CurrentCultureIgnoreCase)
                || x.NumeroLista.ToString(System.Globalization.CultureInfo.InvariantCulture).Contains(BusquedaEstudiante, StringComparison.Ordinal);

            if (!coincideBusqueda) return false;

            return FiltroEntrega switch
            {
                FiltroEntrega.Pendientes => x.NivelLogro == NivelLogro.Pendiente,
                FiltroEntrega.Domina => x.NivelLogro == NivelLogro.Domina,
                FiltroEntrega.Suficiente => x.NivelLogro == NivelLogro.Suficiente,
                FiltroEntrega.EnProceso => x.NivelLogro == NivelLogro.EnProceso,
                FiltroEntrega.RequiereApoyo => x.NivelLogro == NivelLogro.RequiereApoyo,
                FiltroEntrega.NoEntrego => x.NivelLogro == NivelLogro.NoEntrego,
                FiltroEntrega.SoloIncidencias => x.NivelLogro == NivelLogro.Pendiente
                    || x.NivelLogro == NivelLogro.RequiereApoyo || x.NivelLogro == NivelLogro.NoEntrego,
                FiltroEntrega.SoloActivos => x.EstaActivoActualmente,
                _ => true,
            };
        }).ToArray();

        OnPropertyChanged(nameof(Total));
        OnPropertyChanged(nameof(Pendientes));
        OnPropertyChanged(nameof(Domina));
        OnPropertyChanged(nameof(Suficiente));
        OnPropertyChanged(nameof(EnProceso));
        OnPropertyChanged(nameof(RequiereApoyo));
        OnPropertyChanged(nameof(NoEntrego));
    }

    private bool Ejecutar(Action accion)
    {
        if (EstaOcupado) return false;
        EstaOcupado = true;
        try
        {
            accion();
            return true;
        }
        catch (ConflictoConcurrenciaException)
        {
            _mensajes.MostrarError("Los datos cambiaron. Recarga antes de guardar.");
        }
        catch (DomainValidationException exception)
        {
            _mensajes.MostrarError(exception.Message);
        }
        catch (DomainConflictException exception)
        {
            _mensajes.MostrarError(exception.Message);
        }
        catch (ErrorPersistenciaAplicacionException)
        {
            _mensajes.MostrarError("No fue posible guardar la evaluación. Intenta nuevamente.");
        }
        finally
        {
            EstaOcupado = false;
        }

        return false;
    }

    private T? EjecutarResultado<T>(Func<T> accion)
    {
        T? valor = default;
        Ejecutar(() => valor = accion());
        return valor;
    }

    private void NotificarEdicion()
    {
        OnPropertyChanged(nameof(TieneCambios));
        OnPropertyChanged(nameof(PuedeEditar));
        NotificarComandos();
    }

    private void NotificarComandos()
    {
        foreach (var comando in new[]
        {
            GuardarActividadCommand, DescartarActividadCommand, MarcarDominaCommand,
            MarcarSuficienteCommand, MarcarEnProcesoCommand, MarcarRequiereApoyoCommand,
            MarcarNoEntregoCommand, MarcarPendienteCommand,
        })
        {
            comando.NotifyCanExecuteChanged();
        }
    }

    private static ActividadProyectoResumen Resumir(ActividadProyectoDetalle actividad) => new(
        actividad.ActividadId, actividad.ProyectoId, actividad.Titulo, actividad.FechaRealizacion,
        actividad.Estado, actividad.Total, actividad.Pendientes, actividad.Domina,
        actividad.Suficiente, actividad.EnProceso, actividad.RequiereApoyo,
        actividad.NoEntrego, actividad.Version);
}
