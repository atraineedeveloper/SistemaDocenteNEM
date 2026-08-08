using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Presentation;

public enum PasoExportacionGrupo
{
    Contenido = 0,
    Alcance = 1,
    Archivo = 2,
    Resultado = 3,
}

public sealed record OpcionFormatoExportacion(
    FormatoExportacionTabular Formato,
    string Etiqueta,
    string Descripcion);

public sealed record OpcionConjuntoExportacion(
    ConjuntoExportacionGrupo Conjunto,
    string Etiqueta);

public sealed record OpcionProyectoExportacionVisual(
    ProyectoId? ProyectoId,
    string Etiqueta);

public sealed class ExportacionGrupoViewModel : ViewModelBase
{
    private readonly ExportacionGrupoCasosUso _exportacion;
    private readonly ConsultaExportacionGrupoCasosUso _consulta;
    private GrupoId? _grupoId;
    private DateOnly _fechaReferencia;
    private PasoExportacionGrupo _pasoActual;
    private FormatoExportacionTabular _formato = FormatoExportacionTabular.Xlsx;
    private bool _incluirContexto = true;
    private bool _incluirAlumnos = true;
    private bool _incluirAsistencia = true;
    private bool _incluirProyectos = true;
    private bool _incluirActividades = true;
    private bool _incluirEvaluacion = true;
    private bool _incluirSeguimiento;
    private bool _incluirObservacionesEstudiante;
    private bool _incluirObservacionesEvaluacion;
    private DateTime? _asistenciaDesde;
    private DateTime? _asistenciaHasta;
    private ConjuntoExportacionGrupo _conjuntoCsv = ConjuntoExportacionGrupo.Alumnos;
    private IReadOnlyList<OpcionProyectoExportacionVisual> _proyectos = [];
    private OpcionProyectoExportacionVisual? _proyectoSeleccionado;
    private PlanExportacionGrupo? _plan;
    private ResultadoExportacionGrupo? _resultado;
    private string _mensaje = string.Empty;

    public ExportacionGrupoViewModel(
        ExportacionGrupoCasosUso exportacion,
        ConsultaExportacionGrupoCasosUso consulta)
    {
        _exportacion = exportacion ?? throw new ArgumentNullException(nameof(exportacion));
        _consulta = consulta ?? throw new ArgumentNullException(nameof(consulta));
        SiguienteCommand = new RelayCommand(Siguiente, PuedeAvanzar);
        VolverCommand = new RelayCommand(Volver, () => PasoActual is PasoExportacionGrupo.Alcance or PasoExportacionGrupo.Archivo);
    }

    public IReadOnlyList<OpcionFormatoExportacion> Formatos { get; } =
    [
        new(FormatoExportacionTabular.Xlsx, "Excel (.xlsx)", "Un libro con una hoja por conjunto seleccionado."),
        new(FormatoExportacionTabular.Csv, "CSV (.csv)", "Un solo conjunto tabular en UTF-8 para intercambio."),
    ];

    public IReadOnlyList<OpcionConjuntoExportacion> ConjuntosCsv { get; } =
    [
        new(ConjuntoExportacionGrupo.Alumnos, "Alumnos"),
        new(ConjuntoExportacionGrupo.Asistencia, "Asistencia"),
        new(ConjuntoExportacionGrupo.Proyectos, "Proyectos"),
        new(ConjuntoExportacionGrupo.Actividades, "Actividades"),
        new(ConjuntoExportacionGrupo.Evaluacion, "Evaluación"),
        new(ConjuntoExportacionGrupo.Seguimiento, "Seguimiento (sensible)"),
    ];

    public RelayCommand SiguienteCommand { get; }
    public RelayCommand VolverCommand { get; }

    public PasoExportacionGrupo PasoActual
    {
        get => _pasoActual;
        private set
        {
            if (!SetProperty(ref _pasoActual, value)) return;
            NotificarPaso();
        }
    }

    public string TituloPaso => PasoActual switch
    {
        PasoExportacionGrupo.Contenido => "Selecciona el contenido",
        PasoExportacionGrupo.Alcance => "Define el alcance",
        PasoExportacionGrupo.Archivo => "Elige dónde guardar",
        PasoExportacionGrupo.Resultado => "Exportación completada",
        _ => string.Empty,
    };

    public bool MostrarContenido => PasoActual == PasoExportacionGrupo.Contenido;
    public bool MostrarAlcance => PasoActual == PasoExportacionGrupo.Alcance;
    public bool MostrarArchivo => PasoActual == PasoExportacionGrupo.Archivo;
    public bool MostrarResultado => PasoActual == PasoExportacionGrupo.Resultado;

    public FormatoExportacionTabular Formato
    {
        get => _formato;
        set
        {
            if (!SetProperty(ref _formato, value)) return;
            OnPropertyChanged(nameof(EsXlsx));
            OnPropertyChanged(nameof(EsCsv));
            NotificarSeleccion();
        }
    }

    public bool EsXlsx => Formato == FormatoExportacionTabular.Xlsx;
    public bool EsCsv => Formato == FormatoExportacionTabular.Csv;

    public ConjuntoExportacionGrupo ConjuntoCsv
    {
        get => _conjuntoCsv;
        set
        {
            if (SetProperty(ref _conjuntoCsv, value))
            {
                NotificarSeleccion();
            }
        }
    }

    public bool IncluirContexto
    {
        get => _incluirContexto;
        set { if (SetProperty(ref _incluirContexto, value)) NotificarSeleccion(); }
    }

    public bool IncluirAlumnos
    {
        get => _incluirAlumnos;
        set { if (SetProperty(ref _incluirAlumnos, value)) NotificarSeleccion(); }
    }

    public bool IncluirAsistencia
    {
        get => _incluirAsistencia;
        set { if (SetProperty(ref _incluirAsistencia, value)) NotificarSeleccion(); }
    }

    public bool IncluirProyectos
    {
        get => _incluirProyectos;
        set { if (SetProperty(ref _incluirProyectos, value)) NotificarSeleccion(); }
    }

    public bool IncluirActividades
    {
        get => _incluirActividades;
        set { if (SetProperty(ref _incluirActividades, value)) NotificarSeleccion(); }
    }

    public bool IncluirEvaluacion
    {
        get => _incluirEvaluacion;
        set { if (SetProperty(ref _incluirEvaluacion, value)) NotificarSeleccion(); }
    }

    public bool IncluirSeguimiento
    {
        get => _incluirSeguimiento;
        set { if (SetProperty(ref _incluirSeguimiento, value)) NotificarSeleccion(); }
    }

    public bool IncluirObservacionesEstudiante
    {
        get => _incluirObservacionesEstudiante;
        set { if (SetProperty(ref _incluirObservacionesEstudiante, value)) NotificarSeleccion(); }
    }

    public bool IncluirObservacionesEvaluacion
    {
        get => _incluirObservacionesEvaluacion;
        set { if (SetProperty(ref _incluirObservacionesEvaluacion, value)) NotificarSeleccion(); }
    }

    public DateTime? AsistenciaDesde
    {
        get => _asistenciaDesde;
        set
        {
            if (SetProperty(ref _asistenciaDesde, value))
            {
                SiguienteCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public DateTime? AsistenciaHasta
    {
        get => _asistenciaHasta;
        set
        {
            if (SetProperty(ref _asistenciaHasta, value))
            {
                SiguienteCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public IReadOnlyList<OpcionProyectoExportacionVisual> Proyectos
    {
        get => _proyectos;
        private set => SetProperty(ref _proyectos, value);
    }

    public OpcionProyectoExportacionVisual? ProyectoSeleccionado
    {
        get => _proyectoSeleccionado;
        set => SetProperty(ref _proyectoSeleccionado, value);
    }

    public bool MostrarPeriodoAsistencia => ConjuntosSeleccionados().Contains(ConjuntoExportacionGrupo.Asistencia);

    public bool MostrarAlcanceProyecto => ConjuntosSeleccionados().Any(conjunto =>
        conjunto is ConjuntoExportacionGrupo.Proyectos or ConjuntoExportacionGrupo.Actividades or ConjuntoExportacionGrupo.Evaluacion);

    public bool ContieneSeleccionSensible =>
        ConjuntosSeleccionados().Contains(ConjuntoExportacionGrupo.Seguimiento)
        || (ConjuntosSeleccionados().Contains(ConjuntoExportacionGrupo.Alumnos) && IncluirObservacionesEstudiante)
        || (ConjuntosSeleccionados().Contains(ConjuntoExportacionGrupo.Evaluacion) && IncluirObservacionesEvaluacion);

    public string AdvertenciaPrivacidad => ContieneSeleccionSensible
        ? "Esta exportación incluirá información pedagógica o de seguimiento sensible. Guarda y comparte el archivo sólo donde corresponda."
        : string.Empty;

    public string NombreArchivoSugerido => _plan?.NombreArchivoSugerido ?? string.Empty;

    public string ResumenPlan => _plan is null
        ? string.Empty
        : string.Join(" · ", _plan.Conjuntos.Select(conjunto => $"{conjunto.Nombre}: {conjunto.Filas}"));

    public string RutaResultado => _resultado?.RutaArchivo ?? string.Empty;

    public string ResumenResultado => _resultado is null
        ? string.Empty
        : string.Join(" · ", _resultado.Conjuntos.Select(conjunto => $"{conjunto.Nombre}: {conjunto.Filas}"));

    public string Mensaje
    {
        get => _mensaje;
        private set => SetProperty(ref _mensaje, value);
    }

    public void Inicializar(GrupoId grupoId, DateOnly fechaReferencia)
    {
        if (grupoId == default) throw new ArgumentException("La identidad del grupo es obligatoria.", nameof(grupoId));
        _grupoId = grupoId;
        _fechaReferencia = fechaReferencia;
        _plan = null;
        _resultado = null;
        Mensaje = string.Empty;
        Formato = FormatoExportacionTabular.Xlsx;
        IncluirContexto = true;
        IncluirAlumnos = true;
        IncluirAsistencia = true;
        IncluirProyectos = true;
        IncluirActividades = true;
        IncluirEvaluacion = true;
        IncluirSeguimiento = false;
        IncluirObservacionesEstudiante = false;
        IncluirObservacionesEvaluacion = false;
        ConjuntoCsv = ConjuntoExportacionGrupo.Alumnos;
        var primerDia = new DateOnly(fechaReferencia.Year, fechaReferencia.Month, 1);
        AsistenciaDesde = primerDia.ToDateTime(TimeOnly.MinValue);
        AsistenciaHasta = primerDia.AddMonths(1).AddDays(-1).ToDateTime(TimeOnly.MinValue);
        Proyectos =
        [
            new OpcionProyectoExportacionVisual(null, "Todos los proyectos"),
            .. _consulta.ListarProyectos(grupoId)
                .Select(proyecto => new OpcionProyectoExportacionVisual(
                    proyecto.ProyectoId,
                    $"{proyecto.Nombre} · {proyecto.FechaInicio:dd/MM/yyyy}")),
        ];
        ProyectoSeleccionado = Proyectos[0];
        PasoActual = PasoExportacionGrupo.Contenido;
        NotificarSeleccion();
        OnPropertyChanged(nameof(NombreArchivoSugerido));
        OnPropertyChanged(nameof(ResumenPlan));
        OnPropertyChanged(nameof(RutaResultado));
        OnPropertyChanged(nameof(ResumenResultado));
    }

    public bool ExportarA(string rutaArchivo)
    {
        if (_plan is null || PasoActual != PasoExportacionGrupo.Archivo)
        {
            Mensaje = "Prepara la exportación antes de elegir el archivo de destino.";
            return false;
        }

        try
        {
            _resultado = _exportacion.Exportar(_plan, rutaArchivo);
            Mensaje = string.Empty;
            PasoActual = PasoExportacionGrupo.Resultado;
            OnPropertyChanged(nameof(RutaResultado));
            OnPropertyChanged(nameof(ResumenResultado));
            return true;
        }
        catch (Exception exception) when (
            exception is DomainValidationException
                or DomainConflictException
                or GrupoNoEncontradoException
                or ExportacionTabularException
                or IOException
                or UnauthorizedAccessException)
        {
            Mensaje = exception.Message;
            return false;
        }
    }

    private void Siguiente()
    {
        Mensaje = string.Empty;
        if (PasoActual == PasoExportacionGrupo.Contenido)
        {
            PasoActual = PasoExportacionGrupo.Alcance;
            return;
        }

        if (PasoActual != PasoExportacionGrupo.Alcance || _grupoId is not { } grupoId)
        {
            return;
        }

        try
        {
            _plan = _exportacion.Preparar(CrearSolicitud(grupoId), _fechaReferencia);
            PasoActual = PasoExportacionGrupo.Archivo;
            OnPropertyChanged(nameof(NombreArchivoSugerido));
            OnPropertyChanged(nameof(ResumenPlan));
        }
        catch (Exception exception) when (
            exception is DomainValidationException or DomainConflictException or GrupoNoEncontradoException)
        {
            Mensaje = exception.Message;
        }
    }

    private void Volver()
    {
        Mensaje = string.Empty;
        _plan = null;
        if (PasoActual == PasoExportacionGrupo.Archivo)
        {
            PasoActual = PasoExportacionGrupo.Alcance;
        }
        else if (PasoActual == PasoExportacionGrupo.Alcance)
        {
            PasoActual = PasoExportacionGrupo.Contenido;
        }

        OnPropertyChanged(nameof(NombreArchivoSugerido));
        OnPropertyChanged(nameof(ResumenPlan));
    }

    private bool PuedeAvanzar()
    {
        if (_grupoId is null || PasoActual is PasoExportacionGrupo.Archivo or PasoExportacionGrupo.Resultado)
        {
            return false;
        }

        var conjuntos = ConjuntosSeleccionados();
        if (conjuntos.Count == 0)
        {
            return false;
        }

        if (PasoActual == PasoExportacionGrupo.Alcance && conjuntos.Contains(ConjuntoExportacionGrupo.Asistencia))
        {
            if (!AsistenciaDesde.HasValue || !AsistenciaHasta.HasValue || AsistenciaDesde > AsistenciaHasta)
            {
                return false;
            }
        }

        return true;
    }

    private SolicitudExportacionGrupo CrearSolicitud(GrupoId grupoId) => new(
        grupoId,
        Formato,
        ConjuntosSeleccionados(),
        MostrarPeriodoAsistencia && AsistenciaDesde.HasValue
            ? DateOnly.FromDateTime(AsistenciaDesde.Value)
            : null,
        MostrarPeriodoAsistencia && AsistenciaHasta.HasValue
            ? DateOnly.FromDateTime(AsistenciaHasta.Value)
            : null,
        MostrarAlcanceProyecto ? ProyectoSeleccionado?.ProyectoId : null,
        IncluirObservacionesEstudiante,
        IncluirObservacionesEvaluacion);

    private IReadOnlyList<ConjuntoExportacionGrupo> ConjuntosSeleccionados()
    {
        if (EsCsv)
        {
            return [ConjuntoCsv];
        }

        var conjuntos = new List<ConjuntoExportacionGrupo>();
        if (IncluirContexto) conjuntos.Add(ConjuntoExportacionGrupo.Contexto);
        if (IncluirAlumnos) conjuntos.Add(ConjuntoExportacionGrupo.Alumnos);
        if (IncluirAsistencia) conjuntos.Add(ConjuntoExportacionGrupo.Asistencia);
        if (IncluirProyectos) conjuntos.Add(ConjuntoExportacionGrupo.Proyectos);
        if (IncluirActividades) conjuntos.Add(ConjuntoExportacionGrupo.Actividades);
        if (IncluirEvaluacion) conjuntos.Add(ConjuntoExportacionGrupo.Evaluacion);
        if (IncluirSeguimiento) conjuntos.Add(ConjuntoExportacionGrupo.Seguimiento);
        return conjuntos;
    }

    private void NotificarSeleccion()
    {
        OnPropertyChanged(nameof(MostrarPeriodoAsistencia));
        OnPropertyChanged(nameof(MostrarAlcanceProyecto));
        OnPropertyChanged(nameof(ContieneSeleccionSensible));
        OnPropertyChanged(nameof(AdvertenciaPrivacidad));
        SiguienteCommand.NotifyCanExecuteChanged();
    }

    private void NotificarPaso()
    {
        OnPropertyChanged(nameof(TituloPaso));
        OnPropertyChanged(nameof(MostrarContenido));
        OnPropertyChanged(nameof(MostrarAlcance));
        OnPropertyChanged(nameof(MostrarArchivo));
        OnPropertyChanged(nameof(MostrarResultado));
        SiguienteCommand.NotifyCanExecuteChanged();
        VolverCommand.NotifyCanExecuteChanged();
    }
}
