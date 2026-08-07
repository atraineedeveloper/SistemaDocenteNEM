using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Presentation;

public sealed class GestionProyectosViewModel : ViewModelBase
{
    private readonly IGestionProyectosPresentacion _gestion;
    private readonly IDialogoCambiosPendientes _dialogo;
    private readonly IConfirmacionProyectos _confirmacion;
    private readonly IServicioMensajes _mensajes;
    private GrupoId? _grupoId;
    private ProyectoResumen? _proyecto;
    private ProyectoDetalle? _proyectoConfirmado;
    private ActividadProyectoResumen? _actividad;
    private ActividadProyectoDetalle? _actividadConfirmada;
    private IReadOnlyList<ProyectoResumen> _proyectos = [];
    private IReadOnlyList<ProyectoResumen> _proyectosVisibles = [];
    private IReadOnlyList<ActividadProyectoResumen> _actividades = [];
    private IReadOnlyList<ActividadProyectoResumen> _todasActividades = [];
    private IReadOnlyList<EntregaActividadVisual> _entregas = [];
    private IReadOnlyList<EntregaActividadVisual> _entregasVisibles = [];
    private IReadOnlyList<GradoPrimaria> _gradosConfigurados = CatalogoNemPrimaria.TodosLosGrados;
    private IReadOnlyList<SeleccionGradoPlaneacion> _gradosProyecto = [];
    private IReadOnlyList<SeleccionGradoPlaneacion> _gradosActividad = [];
    private bool _estaOcupado;
    private FiltroProyecto _filtroProyecto;
    private FiltroEntrega _filtroEntrega;
    private string _busquedaActividad = string.Empty;
    private string _nombreProyecto = string.Empty;
    private string _descripcionProyecto = string.Empty;
    private string _observacionesProyecto = string.Empty;
    private DateOnly _inicio = DateOnly.FromDateTime(DateTime.Today);
    private DateOnly _termino = DateOnly.FromDateTime(DateTime.Today).AddDays(13);
    private MetodologiaProyectoNem _metodologiaProyecto = MetodologiaProyectoNem.NoEspecificada;
    private string _tituloActividad = string.Empty;
    private string _descripcionActividad = string.Empty;
    private string _observacionesActividad = string.Empty;
    private DateOnly _fechaActividad = DateOnly.FromDateTime(DateTime.Today);
    private CampoFormativoNem _campoFormativoActividad = CampoFormativoNem.NoEspecificado;
    private bool _nuevoProyecto;
    private bool _nuevaActividad;

    public GestionProyectosViewModel(
        IGestionProyectosPresentacion gestion,
        IDialogoCambiosPendientes dialogo,
        IConfirmacionProyectos confirmacion,
        IServicioMensajes mensajes)
    {
        _gestion = gestion;
        _dialogo = dialogo;
        _confirmacion = confirmacion;
        _mensajes = mensajes;
        MetodologiasProyecto =
        [
            new(MetodologiaProyectoNem.NoEspecificada, "No especificada"),
            .. CatalogoPlaneacionNem.MetodologiasProyecto.Select(
                valor => new OpcionMetodologiaProyectoNem(
                    valor,
                    CatalogoPlaneacionNem.FormatearMetodologia(valor))),
        ];
        CamposFormativos =
        [
            new(CampoFormativoNem.NoEspecificado, "No especificado"),
            .. CatalogoPlaneacionNem.CamposFormativos.Select(
                valor => new OpcionCampoFormativoNem(
                    valor,
                    CatalogoPlaneacionNem.FormatearCampo(valor))),
        ];
        EstablecerGradosProyecto([]);
        EstablecerGradosActividad([]);

        NuevoProyectoCommand = new RelayCommand(NuevoProyecto, () => !EstaOcupado);
        GuardarProyectoCommand = new RelayCommand(
            GuardarProyecto,
            () => !EstaOcupado && _grupoId is not null
                && !string.IsNullOrWhiteSpace(NombreProyecto)
                && TieneCambiosProyecto
                && CatalogoPlaneacionNem.EsMetodologiaEspecifica(MetodologiaProyecto)
                && GradosProyectoSeleccionados.Count > 0);
        IniciarProyectoCommand = new RelayCommand(
            () => CambiarEstado(EstadoProyecto.EnCurso),
            () => ProyectoSeleccionado?.Estado == EstadoProyecto.Borrador && !EstaOcupado);
        FinalizarProyectoCommand = new RelayCommand(
            () => CambiarEstado(EstadoProyecto.Finalizado),
            () => ProyectoSeleccionado?.Estado == EstadoProyecto.EnCurso && !EstaOcupado);
        ReabrirProyectoCommand = new RelayCommand(
            Reabrir,
            () => ProyectoSeleccionado?.Estado == EstadoProyecto.Finalizado && !EstaOcupado);
        EliminarProyectoCommand = new RelayCommand(
            EliminarProyecto,
            () => ProyectoSeleccionado?.Estado == EstadoProyecto.Borrador && !EstaOcupado);
        NuevaActividadCommand = new RelayCommand(
            NuevaActividad,
            () => ProyectoSeleccionado is not null
                && ProyectoSeleccionado.Estado != EstadoProyecto.Finalizado && !EstaOcupado);
        GuardarActividadCommand = new RelayCommand(
            GuardarActividad,
            () => PuedeEditarActividad
                && !string.IsNullOrWhiteSpace(TituloActividad)
                && TieneCambiosActividad
                && CatalogoPlaneacionNem.EsCampoEspecifico(CampoFormativoActividad)
                && (!_nuevaActividad || GradosActividadSeleccionados.Count > 0));
        DescartarActividadCommand = new RelayCommand(
            DescartarActividad,
            () => TieneCambiosActividad && !EstaOcupado);
        AnularActividadCommand = new RelayCommand(
            AnularActividad,
            () => ActividadSeleccionada?.Estado == EstadoActividad.Activa && !_nuevaActividad && !EstaOcupado);
        EliminarActividadCommand = new RelayCommand(
            EliminarActividad,
            () => ActividadSeleccionada?.Pendientes == ActividadSeleccionada?.Total
                && !_nuevaActividad && !EstaOcupado);
        MarcarDominaCommand = new RelayCommand(
            () => Marcar(NivelLogro.Domina), () => PuedeEditarActividad);
        MarcarSuficienteCommand = new RelayCommand(
            () => Marcar(NivelLogro.Suficiente), () => PuedeEditarActividad);
        MarcarEnProcesoCommand = new RelayCommand(
            () => Marcar(NivelLogro.EnProceso), () => PuedeEditarActividad);
        MarcarRequiereApoyoCommand = new RelayCommand(
            () => Marcar(NivelLogro.RequiereApoyo), () => PuedeEditarActividad);
        MarcarNoEntregoCommand = new RelayCommand(
            () => Marcar(NivelLogro.NoEntrego), () => PuedeEditarActividad);
        MarcarPendienteCommand = new RelayCommand(
            () => Marcar(NivelLogro.Pendiente), () => PuedeEditarActividad);
        MarcarTodosDominaCommand = new RelayCommand(
            () =>
            {
                foreach (var fila in EntregasVisibles) fila.NivelLogro = NivelLogro.Domina;
                NotificarEdicion();
            },
            () => PuedeEditarActividad);
    }

    public RelayCommand NuevoProyectoCommand { get; }
    public RelayCommand GuardarProyectoCommand { get; }
    public RelayCommand IniciarProyectoCommand { get; }
    public RelayCommand FinalizarProyectoCommand { get; }
    public RelayCommand ReabrirProyectoCommand { get; }
    public RelayCommand EliminarProyectoCommand { get; }
    public RelayCommand NuevaActividadCommand { get; }
    public RelayCommand GuardarActividadCommand { get; }
    public RelayCommand DescartarActividadCommand { get; }
    public RelayCommand AnularActividadCommand { get; }
    public RelayCommand EliminarActividadCommand { get; }
    public RelayCommand MarcarDominaCommand { get; }
    public RelayCommand MarcarSuficienteCommand { get; }
    public RelayCommand MarcarEnProcesoCommand { get; }
    public RelayCommand MarcarRequiereApoyoCommand { get; }
    public RelayCommand MarcarNoEntregoCommand { get; }
    public RelayCommand MarcarPendienteCommand { get; }
    public RelayCommand MarcarTodosDominaCommand { get; }
    public IReadOnlyList<FiltroProyecto> FiltrosProyecto { get; } = Enum.GetValues<FiltroProyecto>();
    public IReadOnlyList<FiltroEntrega> FiltrosEntrega { get; } = Enum.GetValues<FiltroEntrega>();
    public IReadOnlyList<OpcionMetodologiaProyectoNem> MetodologiasProyecto { get; }
    public IReadOnlyList<OpcionCampoFormativoNem> CamposFormativos { get; }
    public GrupoId? GrupoIdActual => _grupoId;

    public IReadOnlyList<ProyectoResumen> ProyectosVisibles
    {
        get => _proyectosVisibles;
        private set => SetProperty(ref _proyectosVisibles, value);
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

    public IReadOnlyList<SeleccionGradoPlaneacion> GradosProyecto
    {
        get => _gradosProyecto;
        private set => SetProperty(ref _gradosProyecto, value);
    }

    public IReadOnlyList<SeleccionGradoPlaneacion> GradosActividad
    {
        get => _gradosActividad;
        private set => SetProperty(ref _gradosActividad, value);
    }

    public IReadOnlyList<GradoPrimaria> GradosProyectoSeleccionados => ObtenerSeleccionados(GradosProyecto);
    public IReadOnlyList<GradoPrimaria> GradosActividadSeleccionados => ObtenerSeleccionados(GradosActividad);
    public bool EsGrupoUnigrado => _gradosConfigurados.Count == 1;
    public bool EsGrupoMultigrado => _gradosConfigurados.Count > 1;
    public string GradoUnigradoTexto => EsGrupoUnigrado
        ? CatalogoNemPrimaria.FormatearGrado(_gradosConfigurados[0])
        : string.Empty;
    public bool PuedeEditarGradosActividad => _nuevaActividad && PuedeEditarActividad;

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
            LimpiarActividadActual();
            CargarProyecto();
        }
    }

    public ActividadProyectoResumen? ActividadSeleccionada
    {
        get => _actividad;
        set
        {
            if (value == _actividad) return;
            if (!ConfirmarPendientesActividad())
            {
                OnPropertyChanged();
                return;
            }

            _actividad = value;
            OnPropertyChanged();
            CargarActividad();
        }
    }

    public FiltroProyecto FiltroProyecto
    {
        get => _filtroProyecto;
        set { if (SetProperty(ref _filtroProyecto, value)) AplicarFiltros(); }
    }

    public FiltroEntrega FiltroEntrega
    {
        get => _filtroEntrega;
        set { if (SetProperty(ref _filtroEntrega, value)) AplicarFiltros(); }
    }

    public string BusquedaActividad
    {
        get => _busquedaActividad;
        set { if (SetProperty(ref _busquedaActividad, value)) AplicarFiltros(); }
    }

    public string NombreProyecto
    {
        get => _nombreProyecto;
        set { if (SetProperty(ref _nombreProyecto, value)) NotificarEdicion(); }
    }

    public string DescripcionProyecto
    {
        get => _descripcionProyecto;
        set { if (SetProperty(ref _descripcionProyecto, value)) NotificarEdicion(); }
    }

    public string ObservacionesProyecto
    {
        get => _observacionesProyecto;
        set { if (SetProperty(ref _observacionesProyecto, value)) NotificarEdicion(); }
    }

    public DateOnly FechaInicio
    {
        get => _inicio;
        set { if (SetProperty(ref _inicio, value)) NotificarEdicion(); }
    }

    public DateOnly FechaTermino
    {
        get => _termino;
        set { if (SetProperty(ref _termino, value)) NotificarEdicion(); }
    }

    public MetodologiaProyectoNem MetodologiaProyecto
    {
        get => _metodologiaProyecto;
        set { if (SetProperty(ref _metodologiaProyecto, value)) NotificarEdicion(); }
    }

    public string TituloActividad
    {
        get => _tituloActividad;
        set { if (SetProperty(ref _tituloActividad, value)) NotificarEdicion(); }
    }

    public string DescripcionActividad
    {
        get => _descripcionActividad;
        set { if (SetProperty(ref _descripcionActividad, value)) NotificarEdicion(); }
    }

    public string ObservacionesActividad
    {
        get => _observacionesActividad;
        set { if (SetProperty(ref _observacionesActividad, value)) NotificarEdicion(); }
    }

    public DateOnly FechaActividad
    {
        get => _fechaActividad;
        set { if (SetProperty(ref _fechaActividad, value)) NotificarEdicion(); }
    }

    public CampoFormativoNem CampoFormativoActividad
    {
        get => _campoFormativoActividad;
        set { if (SetProperty(ref _campoFormativoActividad, value)) NotificarEdicion(); }
    }

    public bool EstaOcupado
    {
        get => _estaOcupado;
        private set { if (SetProperty(ref _estaOcupado, value)) NotificarComandos(); }
    }

    public bool DuracionAtipica => FechaTermino.DayNumber - FechaInicio.DayNumber + 1 is < 14 or > 31;
    public bool TieneCambiosProyecto => _nuevoProyecto || _proyectoConfirmado is not null
        && (NombreProyecto != _proyectoConfirmado.Nombre
            || DescripcionProyecto != _proyectoConfirmado.Descripcion
            || FechaInicio != _proyectoConfirmado.FechaInicio
            || FechaTermino != _proyectoConfirmado.FechaTermino
            || ObservacionesProyecto != _proyectoConfirmado.Observaciones
            || MetodologiaProyecto != _proyectoConfirmado.Metodologia
            || !MismosGrados(GradosProyectoSeleccionados, _proyectoConfirmado.GradosObjetivo));
    public bool TieneCambiosActividad => _nuevaActividad || _actividadConfirmada is not null
        && (TituloActividad != _actividadConfirmada.Titulo
            || DescripcionActividad != _actividadConfirmada.Descripcion
            || FechaActividad != _actividadConfirmada.FechaRealizacion
            || ObservacionesActividad != _actividadConfirmada.ObservacionesGenerales
            || CampoFormativoActividad != _actividadConfirmada.CampoFormativo
            || !MismosGrados(GradosActividadSeleccionados, _actividadConfirmada.GradosObjetivo)
            || Entregas.Any(fila =>
            {
                var confirmada = _actividadConfirmada.Entregas.Single(x => x.EstudianteId == fila.EstudianteId);
                return confirmada.NivelLogro != fila.NivelLogro || confirmada.Observacion != fila.Observacion;
            }));
    public bool PuedeEditarActividad => !EstaOcupado
        && ProyectoSeleccionado?.Estado != EstadoProyecto.Finalizado
        && (_nuevaActividad || ActividadSeleccionada?.Estado == EstadoActividad.Activa);
    public int Total => EntregasObjetivo().Count();
    public int Pendientes => EntregasObjetivo().Count(x => x.NivelLogro == NivelLogro.Pendiente);
    public int Domina => EntregasObjetivo().Count(x => x.NivelLogro == NivelLogro.Domina);
    public int Suficiente => EntregasObjetivo().Count(x => x.NivelLogro == NivelLogro.Suficiente);
    public int EnProceso => EntregasObjetivo().Count(x => x.NivelLogro == NivelLogro.EnProceso);
    public int RequiereApoyo => EntregasObjetivo().Count(x => x.NivelLogro == NivelLogro.RequiereApoyo);
    public int NoEntrego => EntregasObjetivo().Count(x => x.NivelLogro == NivelLogro.NoEntrego);

    public void Inicializar(GrupoId grupoId)
    {
        if (_grupoId is not null && _grupoId != grupoId && !ConfirmarPendientes()) return;
        _grupoId = grupoId;
        OnPropertyChanged(nameof(GrupoIdActual));
        RecargarProyectos();
    }

    public void ConfigurarGradosDisponibles(IEnumerable<GradoPrimaria>? gradosConfigurados)
    {
        var grados = CatalogoNemPrimaria.NormalizarGrados(gradosConfigurados);
        _gradosConfigurados = grados.Count == 0 ? CatalogoNemPrimaria.TodosLosGrados : grados;

        var seleccionProyecto = GradosProyectoSeleccionados;
        var seleccionActividad = GradosActividadSeleccionados;
        EstablecerGradosProyecto(seleccionProyecto);
        EstablecerGradosActividad(seleccionActividad);

        if (_nuevoProyecto && GradosProyectoSeleccionados.Count == 0 && EsGrupoUnigrado)
        {
            EstablecerGradosProyecto(_gradosConfigurados);
        }

        if (_nuevaActividad && GradosActividadSeleccionados.Count == 0)
        {
            EstablecerGradosActividad(GradosPredeterminadosActividad());
        }

        OnPropertyChanged(nameof(EsGrupoUnigrado));
        OnPropertyChanged(nameof(EsGrupoMultigrado));
        OnPropertyChanged(nameof(GradoUnigradoTexto));
        NotificarEdicion();
    }

    public bool SolicitarSalir() => ConfirmarPendientes();

    private void NuevoProyecto()
    {
        if (!ConfirmarPendientes()) return;
        _nuevoProyecto = true;
        _proyecto = null;
        _proyectoConfirmado = null;
        OnPropertyChanged(nameof(ProyectoSeleccionado));
        LimpiarActividadActual();
        NombreProyecto = string.Empty;
        DescripcionProyecto = string.Empty;
        ObservacionesProyecto = string.Empty;
        FechaInicio = DateOnly.FromDateTime(DateTime.Today);
        FechaTermino = FechaInicio.AddDays(13);
        _metodologiaProyecto = MetodologiaProyectoNem.NoEspecificada;
        OnPropertyChanged(nameof(MetodologiaProyecto));
        EstablecerGradosProyecto(EsGrupoUnigrado ? _gradosConfigurados : []);
        NotificarEdicion();
    }

    private void CargarProyecto()
    {
        if (_proyecto is null)
        {
            _proyectoConfirmado = null;
            _todasActividades = [];
            AplicarFiltros();
            NotificarEdicion();
            return;
        }

        var detalle = EjecutarResultado(() => _gestion.ObtenerProyecto(_proyecto.ProyectoId));
        if (detalle is null) return;
        _nuevoProyecto = false;
        AplicarProyecto(detalle);
        var actividades = EjecutarResultado(() => _gestion.ListarActividades(_proyecto.ProyectoId));
        if (actividades is not null) _todasActividades = actividades;
        AplicarFiltros();
        NotificarComandos();
    }

    private void AplicarProyecto(ProyectoDetalle detalle)
    {
        _proyectoConfirmado = detalle;
        _nombreProyecto = detalle.Nombre;
        _descripcionProyecto = detalle.Descripcion;
        _observacionesProyecto = detalle.Observaciones;
        _inicio = detalle.FechaInicio;
        _termino = detalle.FechaTermino;
        _metodologiaProyecto = detalle.Metodologia;
        EstablecerGradosProyecto(detalle.GradosObjetivo ?? []);
        OnPropertyChanged(nameof(NombreProyecto));
        OnPropertyChanged(nameof(DescripcionProyecto));
        OnPropertyChanged(nameof(ObservacionesProyecto));
        OnPropertyChanged(nameof(FechaInicio));
        OnPropertyChanged(nameof(FechaTermino));
        OnPropertyChanged(nameof(MetodologiaProyecto));
        NotificarEdicion();
    }

    private void GuardarProyecto() => IntentarGuardarProyecto();

    private bool IntentarGuardarProyecto()
    {
        if (_grupoId is null) return false;
        if (!CatalogoPlaneacionNem.EsMetodologiaEspecifica(MetodologiaProyecto))
        {
            _mensajes.MostrarError("Selecciona una metodología NEM para el proyecto.");
            return false;
        }

        var grados = GradosProyectoSeleccionados;
        if (grados.Count == 0)
        {
            _mensajes.MostrarError("Selecciona al menos un grado objetivo para el proyecto.");
            return false;
        }

        ProyectoDetalle? guardado = null;
        var entrada = new EntradaProyecto(
            NombreProyecto,
            DescripcionProyecto,
            FechaInicio,
            FechaTermino,
            ObservacionesProyecto,
            MetodologiaProyecto,
            grados);
        var correcto = Ejecutar(() =>
        {
            guardado = _nuevoProyecto
                ? _gestion.CrearProyecto(_grupoId.Value, entrada)
                : _proyecto is not null
                    ? _gestion.ActualizarProyecto(_proyecto.ProyectoId, _proyecto.Version, entrada)
                    : null;
        });
        if (!correcto || guardado is null) return false;

        _nuevoProyecto = false;
        _proyecto = Resumir(guardado);
        _proyectoConfirmado = guardado;
        OnPropertyChanged(nameof(ProyectoSeleccionado));
        AplicarProyecto(guardado);
        RecargarProyectos();
        return true;
    }

    private void CambiarEstado(EstadoProyecto estado)
    {
        if (_proyecto is null || !ConfirmarPendientes()) return;
        ProyectoDetalle? actualizado = null;
        if (Ejecutar(() => actualizado = _gestion.CambiarEstado(_proyecto.ProyectoId, _proyecto.Version, estado))
            && actualizado is not null)
        {
            AplicarProyectoActualizado(actualizado);
        }
    }

    private void Reabrir()
    {
        if (_proyecto is null || !ConfirmarPendientes()
            || !_confirmacion.Confirmar("¿Reabrir el proyecto finalizado?")) return;
        ProyectoDetalle? actualizado = null;
        if (Ejecutar(() => actualizado = _gestion.Reabrir(_proyecto.ProyectoId, _proyecto.Version))
            && actualizado is not null)
        {
            AplicarProyectoActualizado(actualizado);
        }
    }

    private void AplicarProyectoActualizado(ProyectoDetalle detalle)
    {
        _proyecto = Resumir(detalle);
        OnPropertyChanged(nameof(ProyectoSeleccionado));
        AplicarProyecto(detalle);
        RecargarProyectos();
    }

    private void EliminarProyecto()
    {
        if (_proyecto is null || !ConfirmarPendientes()
            || !_confirmacion.Confirmar("¿Eliminar el proyecto Borrador vacío?")) return;
        if (Ejecutar(() => _gestion.EliminarProyecto(_proyecto.ProyectoId, _proyecto.Version)))
        {
            _proyecto = null;
            _proyectoConfirmado = null;
            OnPropertyChanged(nameof(ProyectoSeleccionado));
            RecargarProyectos();
            NotificarEdicion();
        }
    }

    private void NuevaActividad()
    {
        if (_proyecto is null || !ConfirmarPendientesActividad() || !ConfirmarPendientesProyecto()) return;
        var detalle = EjecutarResultado(() =>
            _gestion.PrepararActividad(_proyecto.ProyectoId, "Nueva actividad", "", FechaInicio, ""));
        if (detalle is not null)
        {
            _nuevaActividad = true;
            _actividad = null;
            OnPropertyChanged(nameof(ActividadSeleccionada));
            AplicarActividad(detalle);
        }
    }

    private void CargarActividad()
    {
        if (_actividad is null) return;
        var detalle = EjecutarResultado(() => _gestion.ObtenerActividad(_actividad.ActividadId));
        if (detalle is not null)
        {
            _nuevaActividad = false;
            AplicarActividad(detalle);
        }
    }

    private void AplicarActividad(ActividadProyectoDetalle detalle)
    {
        _actividadConfirmada = detalle;
        _tituloActividad = detalle.Titulo;
        _descripcionActividad = detalle.Descripcion;
        _fechaActividad = detalle.FechaRealizacion;
        _observacionesActividad = detalle.ObservacionesGenerales;
        _campoFormativoActividad = detalle.CampoFormativo;
        var gradosDetalle = detalle.GradosObjetivo ?? [];
        EstablecerGradosActividad(
            _nuevaActividad && gradosDetalle.Count == 0
                ? GradosPredeterminadosActividad()
                : gradosDetalle);
        OnPropertyChanged(nameof(TituloActividad));
        OnPropertyChanged(nameof(DescripcionActividad));
        OnPropertyChanged(nameof(FechaActividad));
        OnPropertyChanged(nameof(ObservacionesActividad));
        OnPropertyChanged(nameof(CampoFormativoActividad));
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
        if (_proyecto is null) return false;
        if (!CatalogoPlaneacionNem.EsCampoEspecifico(CampoFormativoActividad))
        {
            _mensajes.MostrarError("Selecciona el campo formativo de la actividad.");
            return false;
        }

        var grados = GradosActividadSeleccionados;
        if (_nuevaActividad && grados.Count == 0)
        {
            _mensajes.MostrarError("Selecciona al menos un grado objetivo para la actividad.");
            return false;
        }

        ActividadProyectoDetalle? guardada = null;
        var gradosSet = grados.ToHashSet();
        var filas = _nuevaActividad
            ? Entregas.Where(x => gradosSet.Contains(x.Grado)).ToArray()
            : Entregas.ToArray();
        var entradas = filas.Select(x =>
            new EntradaEntregaActividad(x.EstudianteId, x.EstadoEntrega, x.NivelLogro, x.Observacion)).ToArray();
        var entrada = new EntradaActividad(
            TituloActividad,
            DescripcionActividad,
            FechaActividad,
            ObservacionesActividad,
            entradas,
            CampoFormativoActividad,
            grados);
        var correcto = Ejecutar(() => guardada = _nuevaActividad
            ? _gestion.CrearActividad(_proyecto.ProyectoId, entrada)
            : _gestion.ActualizarActividad(_actividad!.ActividadId, _actividad.Version, entrada));
        if (!correcto || guardada is null) return false;

        _nuevaActividad = false;
        _actividad = Resumir(guardada);
        OnPropertyChanged(nameof(ActividadSeleccionada));
        AplicarActividad(guardada);
        _todasActividades = _gestion.ListarActividades(_proyecto.ProyectoId);
        AplicarFiltros();
        return true;
    }

    private void DescartarActividad()
    {
        if (_actividadConfirmada is not null) AplicarActividad(_actividadConfirmada);
        if (_nuevaActividad)
        {
            _nuevaActividad = false;
            LimpiarActividadActual();
        }
    }

    private void AnularActividad()
    {
        if (_actividad is null || !_confirmacion.Confirmar("¿Anular la actividad y conservar su historial?")) return;
        Ejecutar(() =>
        {
            _gestion.AnularActividad(_actividad.ActividadId, _actividad.Version);
            RecargarActividades();
        });
    }

    private void EliminarActividad()
    {
        if (_actividad is null || !_confirmacion.Confirmar("¿Eliminar la actividad sin seguimiento?")) return;
        Ejecutar(() =>
        {
            _gestion.EliminarActividad(_actividad.ActividadId, _actividad.Version);
            LimpiarActividadActual();
            RecargarActividades();
        });
    }

    private void Marcar(NivelLogro nivel)
    {
        var filas = Entregas.Where(x => x.Seleccionada).ToArray();
        if (filas.Length == 0) filas = EntregasVisibles.ToArray();
        foreach (var fila in filas) fila.NivelLogro = nivel;
        NotificarEdicion();
    }

    // Deliberate order: resolve activity changes before project changes.
    private bool ConfirmarPendientes() =>
        ConfirmarPendientesActividad() && ConfirmarPendientesProyecto();

    private bool ConfirmarPendientesActividad()
    {
        if (!TieneCambiosActividad) return true;
        return _dialogo.ConfirmarCambiosPendientes("la actividad") switch
        {
            DecisionCambiosPendientes.Guardar => IntentarGuardarActividad(),
            DecisionCambiosPendientes.Descartar => DescartarActividadPendiente(),
            _ => false,
        };
    }

    private bool ConfirmarPendientesProyecto()
    {
        if (!TieneCambiosProyecto) return true;
        return _dialogo.ConfirmarCambiosPendientes("el proyecto") switch
        {
            DecisionCambiosPendientes.Guardar => IntentarGuardarProyecto(),
            DecisionCambiosPendientes.Descartar => DescartarProyectoPendiente(),
            _ => false,
        };
    }

    private bool DescartarActividadPendiente()
    {
        DescartarActividad();
        return true;
    }

    private bool DescartarProyectoPendiente()
    {
        if (_nuevoProyecto)
        {
            _nuevoProyecto = false;
            _proyectoConfirmado = null;
            _nombreProyecto = string.Empty;
            _descripcionProyecto = string.Empty;
            _observacionesProyecto = string.Empty;
            _metodologiaProyecto = MetodologiaProyectoNem.NoEspecificada;
            EstablecerGradosProyecto([]);
            OnPropertyChanged(nameof(MetodologiaProyecto));
        }
        else if (_proyectoConfirmado is not null)
        {
            AplicarProyecto(_proyectoConfirmado);
        }

        NotificarEdicion();
        return true;
    }

    private void LimpiarActividadActual()
    {
        _actividad = null;
        _actividadConfirmada = null;
        _nuevaActividad = false;
        _todasActividades = [];
        _campoFormativoActividad = CampoFormativoNem.NoEspecificado;
        EstablecerGradosActividad([]);
        Entregas = [];
        OnPropertyChanged(nameof(ActividadSeleccionada));
        OnPropertyChanged(nameof(CampoFormativoActividad));
        AplicarFiltros();
        NotificarEdicion();
    }

    private void RecargarProyectos()
    {
        if (_grupoId is null) return;
        var datos = EjecutarResultado(() => _gestion.ListarProyectos(_grupoId.Value));
        if (datos is null) return;
        _proyectos = datos;
        if (_proyecto is not null)
        {
            _proyecto = datos.FirstOrDefault(x => x.ProyectoId == _proyecto.ProyectoId) ?? _proyecto;
            OnPropertyChanged(nameof(ProyectoSeleccionado));
        }

        AplicarFiltros();
    }

    private void RecargarActividades()
    {
        if (_proyecto is null) return;
        var datos = EjecutarResultado(() => _gestion.ListarActividades(_proyecto.ProyectoId));
        if (datos is not null)
        {
            _todasActividades = datos;
            AplicarFiltros();
        }
    }

    private void AplicarFiltros()
    {
        ProyectosVisibles = _proyectos
            .Where(x => FiltroProyecto == FiltroProyecto.Todos
                || x.Estado.ToString() == FiltroProyecto.ToString()).ToArray();
        Actividades = _todasActividades
            .Where(x => string.IsNullOrWhiteSpace(BusquedaActividad)
                || x.Titulo.Contains(BusquedaActividad, StringComparison.CurrentCultureIgnoreCase)
                || x.FechaRealizacion.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture)
                    .Contains(BusquedaActividad, StringComparison.Ordinal)).ToArray();
        EntregasVisibles = EntregasObjetivo().Where(x => FiltroEntrega switch
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
        }).ToArray();
        OnPropertyChanged(nameof(Total));
        OnPropertyChanged(nameof(Pendientes));
        OnPropertyChanged(nameof(Domina));
        OnPropertyChanged(nameof(Suficiente));
        OnPropertyChanged(nameof(EnProceso));
        OnPropertyChanged(nameof(RequiereApoyo));
        OnPropertyChanged(nameof(NoEntrego));
    }

    private IEnumerable<EntregaActividadVisual> EntregasObjetivo()
    {
        if (!_nuevaActividad)
        {
            return Entregas;
        }

        var grados = GradosActividadSeleccionados;
        if (grados.Count == 0)
        {
            return Array.Empty<EntregaActividadVisual>();
        }

        var conjunto = grados.ToHashSet();
        return Entregas.Where(x => conjunto.Contains(x.Grado));
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
        catch (PeriodoProyectoIncompatibleException)
        {
            _mensajes.MostrarError("El periodo deja actividades fuera del rango.");
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
            _mensajes.MostrarError("No fue posible guardar los proyectos. Intenta nuevamente.");
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

    private void EstablecerGradosProyecto(IEnumerable<GradoPrimaria> seleccionados)
    {
        GradosProyecto = CrearSelecciones(seleccionados, NotificarEdicion);
        OnPropertyChanged(nameof(GradosProyectoSeleccionados));
    }

    private void EstablecerGradosActividad(IEnumerable<GradoPrimaria> seleccionados)
    {
        GradosActividad = CrearSelecciones(seleccionados, NotificarEdicion);
        OnPropertyChanged(nameof(GradosActividadSeleccionados));
    }

    private SeleccionGradoPlaneacion[] CrearSelecciones(
        IEnumerable<GradoPrimaria> seleccionados,
        Action alCambiar)
    {
        var seleccion = CatalogoPlaneacionNem.NormalizarGradosObjetivo(seleccionados);
        var disponibles = _gradosConfigurados
            .Concat(seleccion)
            .Where(CatalogoNemPrimaria.EsGradoReal)
            .Distinct()
            .OrderBy(x => (int)x)
            .ToArray();
        var conjunto = seleccion.ToHashSet();
        var opciones = disponibles
            .Select(grado => new SeleccionGradoPlaneacion(grado, conjunto.Contains(grado)))
            .ToArray();
        foreach (var opcion in opciones)
        {
            opcion.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(SeleccionGradoPlaneacion.Seleccionado))
                {
                    OnPropertyChanged(nameof(GradosProyectoSeleccionados));
                    OnPropertyChanged(nameof(GradosActividadSeleccionados));
                    alCambiar();
                }
            };
        }
        return opciones;
    }

    private IReadOnlyList<GradoPrimaria> GradosPredeterminadosActividad()
    {
        var proyecto = GradosProyectoSeleccionados;
        return proyecto.Count > 0 ? proyecto : _gradosConfigurados;
    }

    private static GradoPrimaria[] ObtenerSeleccionados(
        IEnumerable<SeleccionGradoPlaneacion> opciones) =>
        opciones.Where(x => x.Seleccionado).Select(x => x.Grado).OrderBy(x => (int)x).ToArray();

    private static bool MismosGrados(
        IEnumerable<GradoPrimaria> actuales,
        IEnumerable<GradoPrimaria>? confirmados) =>
        CatalogoPlaneacionNem.NormalizarGradosObjetivo(actuales)
            .SequenceEqual(CatalogoPlaneacionNem.NormalizarGradosObjetivo(confirmados));

    private void NotificarEdicion()
    {
        OnPropertyChanged(nameof(DuracionAtipica));
        OnPropertyChanged(nameof(TieneCambiosProyecto));
        OnPropertyChanged(nameof(TieneCambiosActividad));
        OnPropertyChanged(nameof(GradosProyectoSeleccionados));
        OnPropertyChanged(nameof(GradosActividadSeleccionados));
        OnPropertyChanged(nameof(PuedeEditarGradosActividad));
        AplicarFiltros();
        NotificarComandos();
    }

    private void NotificarComandos()
    {
        foreach (var comando in new[]
        {
            NuevoProyectoCommand, GuardarProyectoCommand, IniciarProyectoCommand,
            FinalizarProyectoCommand, ReabrirProyectoCommand, EliminarProyectoCommand,
            NuevaActividadCommand, GuardarActividadCommand, DescartarActividadCommand,
            AnularActividadCommand, EliminarActividadCommand, MarcarDominaCommand,
            MarcarSuficienteCommand, MarcarEnProcesoCommand, MarcarRequiereApoyoCommand,
            MarcarNoEntregoCommand, MarcarPendienteCommand, MarcarTodosDominaCommand,
        })
        {
            comando.NotifyCanExecuteChanged();
        }
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
}