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
    private IReadOnlyList<ProyectoResumen> _proyectos = [];
    private IReadOnlyList<ActividadProyectoResumen> _actividades = [];
    private IReadOnlyList<ActividadEvaluacionColumnaVisual> _columnasActividades = [];
    private IReadOnlyList<EvaluacionEstudianteFilaVisual> _filas = [];
    private IReadOnlyList<EvaluacionEstudianteFilaVisual> _filasVisibles = [];
    private EvaluacionCeldaVisual? _celdaSeleccionada;
    private int _indiceActividadSeleccionada = -1;
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

        GuardarCambiosCommand = new RelayCommand(GuardarCambios, () => TieneCambios && !EstaOcupado);
        DescartarCambiosCommand = new RelayCommand(DescartarCambios, () => TieneCambios && !EstaOcupado);
        GuardarActividadCommand = GuardarCambiosCommand;
        DescartarActividadCommand = DescartarCambiosCommand;

        MarcarDominaCommand = new RelayCommand(() => MarcarNivel(NivelLogro.Domina), () => PuedeEditarCelda);
        MarcarSuficienteCommand = new RelayCommand(() => MarcarNivel(NivelLogro.Suficiente), () => PuedeEditarCelda);
        MarcarEnProcesoCommand = new RelayCommand(() => MarcarNivel(NivelLogro.EnProceso), () => PuedeEditarCelda);
        MarcarRequiereApoyoCommand = new RelayCommand(() => MarcarNivel(NivelLogro.RequiereApoyo), () => PuedeEditarCelda);
        MarcarEntregadaCommand = new RelayCommand(() => MarcarEstado(EstadoEntregaActividad.Entregada), () => PuedeEditarCelda);
        MarcarNoEntregadaCommand = new RelayCommand(() => MarcarEstado(EstadoEntregaActividad.NoEntregada), () => PuedeEditarCelda);
        MarcarPendienteEntregaCommand = new RelayCommand(() => MarcarEstado(EstadoEntregaActividad.Pendiente), () => PuedeEditarCelda);

        MarcarTodosDominaCommand = CrearComandoMasivoNivel(NivelLogro.Domina);
        MarcarTodosSuficienteCommand = CrearComandoMasivoNivel(NivelLogro.Suficiente);
        MarcarTodosEnProcesoCommand = CrearComandoMasivoNivel(NivelLogro.EnProceso);
        MarcarTodosRequiereApoyoCommand = CrearComandoMasivoNivel(NivelLogro.RequiereApoyo);
        MarcarTodosEntregadaCommand = CrearComandoMasivoEstado(EstadoEntregaActividad.Entregada);
        MarcarTodosNoEntregadaCommand = CrearComandoMasivoEstado(EstadoEntregaActividad.NoEntregada);
        MarcarTodosPendienteEntregaCommand = CrearComandoMasivoEstado(EstadoEntregaActividad.Pendiente);
    }

    public RelayCommand GuardarCambiosCommand { get; }
    public RelayCommand DescartarCambiosCommand { get; }
    public RelayCommand GuardarActividadCommand { get; }
    public RelayCommand DescartarActividadCommand { get; }
    public RelayCommand MarcarDominaCommand { get; }
    public RelayCommand MarcarSuficienteCommand { get; }
    public RelayCommand MarcarEnProcesoCommand { get; }
    public RelayCommand MarcarRequiereApoyoCommand { get; }
    public RelayCommand MarcarEntregadaCommand { get; }
    public RelayCommand MarcarNoEntregadaCommand { get; }
    public RelayCommand MarcarPendienteEntregaCommand { get; }
    public RelayCommand MarcarNoEntregoCommand => MarcarNoEntregadaCommand;
    public RelayCommand MarcarPendienteCommand => MarcarPendienteEntregaCommand;
    public RelayCommand MarcarTodosDominaCommand { get; }
    public RelayCommand MarcarTodosSuficienteCommand { get; }
    public RelayCommand MarcarTodosEnProcesoCommand { get; }
    public RelayCommand MarcarTodosRequiereApoyoCommand { get; }
    public RelayCommand MarcarTodosEntregadaCommand { get; }
    public RelayCommand MarcarTodosNoEntregadaCommand { get; }
    public RelayCommand MarcarTodosPendienteEntregaCommand { get; }
    public RelayCommand MarcarTodosNoEntregoCommand => MarcarTodosNoEntregadaCommand;
    public RelayCommand MarcarTodosPendienteCommand => MarcarTodosPendienteEntregaCommand;

    public IReadOnlyList<FiltroEntrega> FiltrosEntrega { get; } =
    [
        FiltroEntrega.Todos,
        FiltroEntrega.Entregadas,
        FiltroEntrega.NoEntregadas,
        FiltroEntrega.Pendientes,
        FiltroEntrega.PendientesEvaluacion,
        FiltroEntrega.Domina,
        FiltroEntrega.Suficiente,
        FiltroEntrega.EnProceso,
        FiltroEntrega.RequiereApoyo,
        FiltroEntrega.SoloIncidencias,
        FiltroEntrega.SoloActivos,
        FiltroEntrega.ActivosEInactivosHistoricos,
    ];

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

    public IReadOnlyList<ActividadEvaluacionColumnaVisual> ColumnasActividades
    {
        get => _columnasActividades;
        private set => SetProperty(ref _columnasActividades, value);
    }

    public IReadOnlyList<EvaluacionEstudianteFilaVisual> Filas
    {
        get => _filas;
        private set => SetProperty(ref _filas, value);
    }

    public IReadOnlyList<EvaluacionEstudianteFilaVisual> FilasVisibles
    {
        get => _filasVisibles;
        private set => SetProperty(ref _filasVisibles, value);
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
            CargarMatriz();
        }
    }

    /// <summary>
    /// Compatibilidad de lectura: la actividad seleccionada se deriva de la columna actual,
    /// ya no de un selector independiente.
    /// </summary>
    public ActividadProyectoResumen? ActividadSeleccionada =>
        _indiceActividadSeleccionada >= 0 && _indiceActividadSeleccionada < Actividades.Count
            ? Actividades[_indiceActividadSeleccionada]
            : null;

    public ActividadEvaluacionColumnaVisual? ActividadColumnaSeleccionada =>
        _indiceActividadSeleccionada >= 0 && _indiceActividadSeleccionada < ColumnasActividades.Count
            ? ColumnasActividades[_indiceActividadSeleccionada]
            : null;

    public string ContextoActividadSeleccionada => ActividadColumnaSeleccionada is { } actividad
        ? $"{actividad.Codigo} · {actividad.Titulo} · {actividad.FechaTexto}"
        : "Selecciona una celda de actividad para comenzar.";

    public EvaluacionCeldaVisual? CeldaSeleccionada
    {
        get => _celdaSeleccionada;
        private set
        {
            if (SetProperty(ref _celdaSeleccionada, value))
            {
                OnPropertyChanged(nameof(PuedeEditarCelda));
                NotificarComandos();
            }
        }
    }

    public FiltroEntrega FiltroEntrega
    {
        get => _filtroEntrega;
        set
        {
            if (SetProperty(ref _filtroEntrega, value)) AplicarFiltros();
        }
    }

    public string BusquedaEstudiante
    {
        get => _busquedaEstudiante;
        set
        {
            if (SetProperty(ref _busquedaEstudiante, value)) AplicarFiltros();
        }
    }

    public bool EstaOcupado
    {
        get => _estaOcupado;
        private set
        {
            if (SetProperty(ref _estaOcupado, value))
            {
                OnPropertyChanged(nameof(PuedeEditarActividadSeleccionada));
                OnPropertyChanged(nameof(PuedeEditarCelda));
                NotificarComandos();
            }
        }
    }

    public bool PuedeEditarActividadSeleccionada => !EstaOcupado
        && ProyectoSeleccionado?.Estado != EstadoProyecto.Finalizado
        && ActividadColumnaSeleccionada?.Estado == EstadoActividad.Activa;

    public bool PuedeEditarCelda => PuedeEditarActividadSeleccionada
        && CeldaSeleccionada?.EsEditable == true;

    public bool TieneCambios => Filas.Any(fila => fila.Celdas.Any(celda => celda.TieneCambios));

    public int Total => CeldasActividadSeleccionada().Length;
    public int Pendientes => ContarPendientesNivel();
    public int PendientesEntrega => ContarEstado(EstadoEntregaActividad.Pendiente);
    public int Entregadas => ContarEstado(EstadoEntregaActividad.Entregada);
    public int NoEntregadas => ContarEstado(EstadoEntregaActividad.NoEntregada);
    public int PendientesEvaluacion => CeldasActividadSeleccionada().Count(x =>
        x.EstadoEntrega == EstadoEntregaActividad.Entregada && x.NivelLogro == NivelLogro.Pendiente);
    public int Domina => ContarNivel(NivelLogro.Domina);
    public int Suficiente => ContarNivel(NivelLogro.Suficiente);
    public int EnProceso => ContarNivel(NivelLogro.EnProceso);
    public int RequiereApoyo => ContarNivel(NivelLogro.RequiereApoyo);
    public int NoEntrego => NoEntregadas;

    public void Inicializar(GrupoId grupoId)
    {
        if (_grupoId is not null && _grupoId != grupoId && !ConfirmarPendientes()) return;
        _grupoId = grupoId;
        RecargarProyectos();
    }

    public bool SolicitarSalir() => ConfirmarPendientes();

    public void SeleccionarCelda(EvaluacionEstudianteFilaVisual fila, int indiceActividad)
    {
        ArgumentNullException.ThrowIfNull(fila);
        if (indiceActividad < 0 || indiceActividad >= ColumnasActividades.Count
            || indiceActividad >= fila.Celdas.Count)
        {
            return;
        }

        if (_indiceActividadSeleccionada != indiceActividad)
        {
            _indiceActividadSeleccionada = indiceActividad;
            NotificarContextoActividad();
            AplicarFiltros();
        }

        CeldaSeleccionada = fila.Celdas[indiceActividad];
    }

    public void SeleccionarActividad(int indiceActividad)
    {
        if (indiceActividad < 0 || indiceActividad >= ColumnasActividades.Count
            || _indiceActividadSeleccionada == indiceActividad)
        {
            return;
        }

        _indiceActividadSeleccionada = indiceActividad;
        CeldaSeleccionada = null;
        NotificarContextoActividad();
        AplicarFiltros();
    }

    private RelayCommand CrearComandoMasivoNivel(NivelLogro nivel) =>
        new(() => MarcarTodosNivel(nivel), () => PuedeEditarActividadSeleccionada);

    private RelayCommand CrearComandoMasivoEstado(EstadoEntregaActividad estado) =>
        new(() => MarcarTodosEstado(estado), () => PuedeEditarActividadSeleccionada);

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
            _proyecto = Proyectos[0];
            OnPropertyChanged(nameof(ProyectoSeleccionado));
        }

        CargarMatriz();
    }

    private void CargarMatriz()
    {
        LimpiarMatriz();
        if (_proyecto is null) return;

        var carga = EjecutarResultado(() =>
        {
            var resumenes = _gestion.ListarActividades(_proyecto.ProyectoId);
            var detalles = resumenes.Select(x => _gestion.ObtenerActividad(x.ActividadId)).ToArray();
            return new CargaMatriz(resumenes, detalles);
        });
        if (carga is null) return;

        Actividades = carga.Resumenes;
        ColumnasActividades = carga.Detalles
            .Select((detalle, indice) => new ActividadEvaluacionColumnaVisual(
                detalle.ActividadId,
                $"A{indice + 1:D2}",
                detalle.Titulo,
                detalle.FechaRealizacion,
                detalle.Estado,
                detalle.Version))
            .ToArray();

        var estudiantes = carga.Detalles
            .SelectMany(x => x.Entregas)
            .GroupBy(x => x.EstudianteId)
            .Select(grupo => grupo.First())
            .OrderBy(x => x.NumeroLista)
            .ThenBy(x => x.NombreVisible, StringComparer.CurrentCulture)
            .ThenBy(x => x.EstudianteId.Valor)
            .ToArray();

        var filas = new List<EvaluacionEstudianteFilaVisual>(estudiantes.Length);
        foreach (var estudiante in estudiantes)
        {
            var celdas = new EvaluacionCeldaVisual[carga.Detalles.Count];
            for (var indice = 0; indice < carga.Detalles.Count; indice++)
            {
                var detalle = carga.Detalles[indice];
                var entrega = detalle.Entregas.FirstOrDefault(x => x.EstudianteId == estudiante.EstudianteId);
                var esAplicable = entrega is not null;
                var esEditable = _proyecto.Estado != EstadoProyecto.Finalizado
                    && detalle.Estado == EstadoActividad.Activa;
                celdas[indice] = new EvaluacionCeldaVisual(
                    detalle.ActividadId,
                    estudiante.EstudianteId,
                    esAplicable,
                    esEditable,
                    entrega?.EstadoEntrega ?? EstadoEntregaActividad.Pendiente,
                    entrega?.NivelLogro ?? NivelLogro.Pendiente,
                    entrega?.Observacion ?? string.Empty);
                celdas[indice].PropertyChanged += (_, _) => OnCeldaModificada();
            }

            filas.Add(new EvaluacionEstudianteFilaVisual(
                estudiante.EstudianteId,
                estudiante.NumeroLista,
                estudiante.NombreVisible,
                estudiante.EstaActivoActualmente,
                celdas));
        }

        Filas = filas;
        _indiceActividadSeleccionada = ColumnasActividades.Count > 0 ? 0 : -1;
        CeldaSeleccionada = null;
        NotificarContextoActividad();
        AplicarFiltros();
        NotificarEdicion();
    }

    private void LimpiarMatriz()
    {
        Actividades = [];
        ColumnasActividades = [];
        Filas = [];
        FilasVisibles = [];
        _indiceActividadSeleccionada = -1;
        CeldaSeleccionada = null;
        NotificarContextoActividad();
        NotificarEdicion();
    }

    private void MarcarNivel(NivelLogro nivel)
    {
        if (CeldaSeleccionada?.EsEditable != true) return;
        CeldaSeleccionada.NivelLogro = nivel;
    }

    private void MarcarEstado(EstadoEntregaActividad estado)
    {
        if (CeldaSeleccionada?.EsEditable != true) return;
        CeldaSeleccionada.EstadoEntrega = estado;
    }

    private void MarcarTodosNivel(NivelLogro nivel)
    {
        var indice = _indiceActividadSeleccionada;
        if (!PuedeEditarActividadSeleccionada || indice < 0) return;

        foreach (var fila in Filas)
        {
            var celda = fila.Celdas[indice];
            if (celda.EsEditable) celda.NivelLogro = nivel;
        }

        NotificarEdicion();
    }

    private void MarcarTodosEstado(EstadoEntregaActividad estado)
    {
        var indice = _indiceActividadSeleccionada;
        if (!PuedeEditarActividadSeleccionada || indice < 0) return;

        foreach (var fila in Filas)
        {
            var celda = fila.Celdas[indice];
            if (celda.EsEditable) celda.EstadoEntrega = estado;
        }

        NotificarEdicion();
    }

    private void GuardarCambios() => IntentarGuardarCambios();

    private bool IntentarGuardarCambios()
    {
        if (!TieneCambios) return true;
        if (EstaOcupado) return false;

        EstaOcupado = true;
        try
        {
            for (var indice = 0; indice < ColumnasActividades.Count; indice++)
            {
                var hayCambios = Filas.Select(x => x.Celdas[indice])
                    .Any(x => x.EsAplicable && x.TieneCambios);
                if (!hayCambios) continue;

                var columna = ColumnasActividades[indice];
                var entradas = Filas
                    .Select(x => x.Celdas[indice])
                    .Where(x => x.EsAplicable)
                    .Select(x => new EntradaEntregaActividad(
                        x.EstudianteId,
                        x.EstadoEntrega,
                        x.NivelLogro,
                        x.Observacion))
                    .ToArray();

                ActividadProyectoDetalle guardada;
                try
                {
                    guardada = _gestion.GuardarEntregas(columna.ActividadId, columna.Version, entradas);
                }
                catch (ConflictoConcurrenciaException)
                {
                    _mensajes.MostrarError($"{columna.Codigo} cambió desde la última lectura. Las actividades guardadas antes de este punto se conservaron.");
                    return false;
                }
                catch (DomainValidationException exception)
                {
                    _mensajes.MostrarError(exception.Message);
                    return false;
                }
                catch (DomainConflictException exception)
                {
                    _mensajes.MostrarError(exception.Message);
                    return false;
                }
                catch (ErrorPersistenciaAplicacionException)
                {
                    _mensajes.MostrarError($"No fue posible guardar {columna.Codigo}. Las actividades guardadas antes de este punto se conservaron.");
                    return false;
                }

                ConfirmarActividadGuardada(indice, guardada);
            }

            return true;
        }
        finally
        {
            EstaOcupado = false;
            AplicarFiltros();
            NotificarEdicion();
        }
    }

    private void ConfirmarActividadGuardada(int indice, ActividadProyectoDetalle guardada)
    {
        ColumnasActividades[indice].ActualizarVersion(guardada.Version);
        var porEstudiante = guardada.Entregas.ToDictionary(x => x.EstudianteId);

        foreach (var fila in Filas)
        {
            var celda = fila.Celdas[indice];
            if (!celda.EsAplicable) continue;
            if (!porEstudiante.TryGetValue(fila.EstudianteId, out var entrega))
                throw new DomainConflictException("El padrón guardado de la actividad no coincide con la matriz.");
            celda.Confirmar(entrega.EstadoEntrega, entrega.NivelLogro, entrega.Observacion);
        }

        var resumenes = Actividades.ToArray();
        if (indice < resumenes.Length)
        {
            resumenes[indice] = Resumir(guardada);
            Actividades = resumenes;
        }
        OnPropertyChanged(nameof(ActividadSeleccionada));
    }

    private void DescartarCambios()
    {
        foreach (var celda in Filas.SelectMany(x => x.Celdas))
        {
            if (celda.TieneCambios) celda.Restaurar();
        }

        AplicarFiltros();
        NotificarEdicion();
    }

    private bool ConfirmarPendientes()
    {
        if (!TieneCambios) return true;
        return _dialogo.ConfirmarCambiosPendientes("las evaluaciones del proyecto") switch
        {
            DecisionCambiosPendientes.Guardar => IntentarGuardarCambios(),
            DecisionCambiosPendientes.Descartar => DescartarPendientes(),
            _ => false,
        };
    }

    private bool DescartarPendientes()
    {
        DescartarCambios();
        return true;
    }

    private void OnCeldaModificada()
    {
        AplicarFiltros();
        NotificarEdicion();
    }

    private void AplicarFiltros()
    {
        var indice = _indiceActividadSeleccionada;
        FilasVisibles = Filas.Where(fila =>
        {
            var coincideBusqueda = string.IsNullOrWhiteSpace(BusquedaEstudiante)
                || fila.Nombre.Contains(BusquedaEstudiante, StringComparison.CurrentCultureIgnoreCase)
                || fila.NumeroLista.ToString(System.Globalization.CultureInfo.InvariantCulture)
                    .Contains(BusquedaEstudiante, StringComparison.Ordinal);
            if (!coincideBusqueda) return false;

            if (FiltroEntrega == FiltroEntrega.SoloActivos) return fila.EstaActivoActualmente;
            if (FiltroEntrega is FiltroEntrega.Todos or FiltroEntrega.ActivosEInactivosHistoricos) return true;
            if (indice < 0 || indice >= fila.Celdas.Count) return false;

            var celda = fila.Celdas[indice];
            if (!celda.EsAplicable) return false;
            return FiltroEntrega switch
            {
                FiltroEntrega.Entregadas => celda.EstadoEntrega == EstadoEntregaActividad.Entregada,
                FiltroEntrega.NoEntregadas or FiltroEntrega.NoEntrego =>
                    celda.EstadoEntrega == EstadoEntregaActividad.NoEntregada,
                FiltroEntrega.Pendientes => celda.EstadoEntrega == EstadoEntregaActividad.Pendiente,
                FiltroEntrega.PendientesEvaluacion =>
                    celda.EstadoEntrega == EstadoEntregaActividad.Entregada
                    && celda.NivelLogro == NivelLogro.Pendiente,
                FiltroEntrega.Domina =>
                    celda.EstadoEntrega == EstadoEntregaActividad.Entregada
                    && celda.NivelLogro == NivelLogro.Domina,
                FiltroEntrega.Suficiente =>
                    celda.EstadoEntrega == EstadoEntregaActividad.Entregada
                    && celda.NivelLogro == NivelLogro.Suficiente,
                FiltroEntrega.EnProceso =>
                    celda.EstadoEntrega == EstadoEntregaActividad.Entregada
                    && celda.NivelLogro == NivelLogro.EnProceso,
                FiltroEntrega.RequiereApoyo =>
                    celda.EstadoEntrega == EstadoEntregaActividad.Entregada
                    && celda.NivelLogro == NivelLogro.RequiereApoyo,
                FiltroEntrega.SoloIncidencias =>
                    celda.EstadoEntrega != EstadoEntregaActividad.Entregada
                    || celda.NivelLogro is NivelLogro.Pendiente or NivelLogro.RequiereApoyo,
                _ => true,
            };
        }).ToArray();

        NotificarEstadisticas();
    }

    private EvaluacionCeldaVisual[] CeldasActividadSeleccionada()
    {
        var indice = _indiceActividadSeleccionada;
        if (indice < 0 || indice >= ColumnasActividades.Count) return [];
        return Filas.Select(x => x.Celdas[indice]).Where(x => x.EsAplicable).ToArray();
    }

    private int ContarEstado(EstadoEntregaActividad estado) =>
        CeldasActividadSeleccionada().Count(x => x.EstadoEntrega == estado);

    private int ContarNivel(NivelLogro nivel) =>
        CeldasActividadSeleccionada().Count(x =>
            x.EstadoEntrega == EstadoEntregaActividad.Entregada && x.NivelLogro == nivel);

    private int ContarPendientesNivel() =>
        CeldasActividadSeleccionada().Count(x =>
            x.EstadoEntrega != EstadoEntregaActividad.NoEntregada && x.NivelLogro == NivelLogro.Pendiente);

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
            _mensajes.MostrarError("Los datos cambiaron. Recarga antes de continuar.");
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
            _mensajes.MostrarError("No fue posible cargar la evaluación. Intenta nuevamente.");
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

    private void NotificarContextoActividad()
    {
        OnPropertyChanged(nameof(ActividadSeleccionada));
        OnPropertyChanged(nameof(ActividadColumnaSeleccionada));
        OnPropertyChanged(nameof(ContextoActividadSeleccionada));
        OnPropertyChanged(nameof(PuedeEditarActividadSeleccionada));
        OnPropertyChanged(nameof(PuedeEditarCelda));
        NotificarEstadisticas();
        NotificarComandos();
    }

    private void NotificarEstadisticas()
    {
        OnPropertyChanged(nameof(Total));
        OnPropertyChanged(nameof(Pendientes));
        OnPropertyChanged(nameof(PendientesEntrega));
        OnPropertyChanged(nameof(Entregadas));
        OnPropertyChanged(nameof(NoEntregadas));
        OnPropertyChanged(nameof(PendientesEvaluacion));
        OnPropertyChanged(nameof(Domina));
        OnPropertyChanged(nameof(Suficiente));
        OnPropertyChanged(nameof(EnProceso));
        OnPropertyChanged(nameof(RequiereApoyo));
        OnPropertyChanged(nameof(NoEntrego));
    }

    private void NotificarEdicion()
    {
        OnPropertyChanged(nameof(TieneCambios));
        OnPropertyChanged(nameof(PuedeEditarActividadSeleccionada));
        OnPropertyChanged(nameof(PuedeEditarCelda));
        NotificarEstadisticas();
        NotificarComandos();
    }

    private void NotificarComandos()
    {
        foreach (var comando in new[]
        {
            GuardarCambiosCommand, DescartarCambiosCommand,
            MarcarDominaCommand, MarcarSuficienteCommand, MarcarEnProcesoCommand,
            MarcarRequiereApoyoCommand, MarcarEntregadaCommand,
            MarcarNoEntregadaCommand, MarcarPendienteEntregaCommand,
            MarcarTodosDominaCommand, MarcarTodosSuficienteCommand, MarcarTodosEnProcesoCommand,
            MarcarTodosRequiereApoyoCommand, MarcarTodosEntregadaCommand,
            MarcarTodosNoEntregadaCommand, MarcarTodosPendienteEntregaCommand,
        })
        {
            comando.NotifyCanExecuteChanged();
        }
    }

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
        actividad.Version);

    private sealed record CargaMatriz(
        IReadOnlyList<ActividadProyectoResumen> Resumenes,
        IReadOnlyList<ActividadProyectoDetalle> Detalles);
}