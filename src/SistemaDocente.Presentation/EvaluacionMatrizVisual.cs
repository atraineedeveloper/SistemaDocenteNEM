using SistemaDocente.Core;

namespace SistemaDocente.Presentation;

public sealed record OpcionNivelLogroVisual(NivelLogro Nivel, string Texto);
public sealed record OpcionEstadoEntregaVisual(EstadoEntregaActividad Estado, string Texto);
public sealed record OpcionResultadoEvaluacionVisual(ResultadoEvaluacionVisual Resultado, string Texto);

public enum ResultadoEvaluacionVisual
{
    Pendiente = 0,
    EntregadaSinEvaluar = 1,
    Domina = 2,
    Suficiente = 3,
    EnProceso = 4,
    RequiereApoyo = 5,
    NoEntregada = 6,
}

public sealed class ActividadEvaluacionColumnaVisual : ViewModelBase
{
    internal ActividadEvaluacionColumnaVisual(
        ActividadId actividadId,
        string _,
        string titulo,
        DateOnly fechaRealizacion,
        EstadoActividad estado,
        int version)
    {
        ActividadId = actividadId;
        Codigo = CrearCodigoEstable(actividadId);
        Titulo = titulo;
        FechaRealizacion = fechaRealizacion;
        Estado = estado;
        Version = version;
    }

    internal ActividadId ActividadId { get; }
    public string Codigo { get; }
    public string Titulo { get; }
    public DateOnly FechaRealizacion { get; }
    public EstadoActividad Estado { get; }
    public int Version { get; private set; }
    public bool EstaActiva => Estado == EstadoActividad.Activa;
    public string FechaTexto => FechaRealizacion.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.CurrentCulture);
    public string DescripcionAccesible => $"{Codigo} · {Titulo} · {FechaTexto}";

    /// <summary>
    /// Código compacto, estable y puramente visual derivado de la identidad inmutable de la
    /// actividad. Usa ocho dígitos hexadecimales del GUID para reducir de forma importante
    /// el riesgo de colisión sin convertir la identidad técnica completa en ruido visual.
    /// No depende de posición, fecha o título, por lo que no se renumera.
    /// </summary>
    internal static string CrearCodigoEstable(ActividadId actividadId)
    {
        if (actividadId == default) throw new ArgumentException("La actividad debe tener identidad.", nameof(actividadId));
        var hexadecimal = actividadId.Valor.ToString("N", System.Globalization.CultureInfo.InvariantCulture).ToUpperInvariant();
        return "A" + hexadecimal[..8];
    }

    internal void ActualizarVersion(int version)
    {
        if (version == Version) return;
        Version = version;
        OnPropertyChanged(nameof(Version));
    }
}

public sealed class EvaluacionCeldaVisual : ViewModelBase
{
    private static readonly IReadOnlyList<OpcionNivelLogroVisual> OpcionesNivelDisponibles =
    [
        new(NivelLogro.Pendiente, "Pendiente de evaluación"),
        new(NivelLogro.Domina, "Domina"),
        new(NivelLogro.Suficiente, "Suficiente"),
        new(NivelLogro.EnProceso, "En proceso"),
        new(NivelLogro.RequiereApoyo, "Requiere apoyo"),
    ];

    private static readonly IReadOnlyList<OpcionEstadoEntregaVisual> OpcionesEstadoDisponibles =
    [
        new(EstadoEntregaActividad.Pendiente, "Pendiente de entrega"),
        new(EstadoEntregaActividad.Entregada, "Entregada"),
        new(EstadoEntregaActividad.NoEntregada, "No entregada"),
    ];

    private static readonly IReadOnlyList<OpcionResultadoEvaluacionVisual> OpcionesResultadoDisponibles =
    [
        new(ResultadoEvaluacionVisual.Pendiente, "Pendiente"),
        new(ResultadoEvaluacionVisual.EntregadaSinEvaluar, "Entregada · evaluar después"),
        new(ResultadoEvaluacionVisual.Domina, "Domina"),
        new(ResultadoEvaluacionVisual.Suficiente, "Suficiente"),
        new(ResultadoEvaluacionVisual.EnProceso, "En proceso"),
        new(ResultadoEvaluacionVisual.RequiereApoyo, "Requiere apoyo"),
        new(ResultadoEvaluacionVisual.NoEntregada, "No entregó"),
    ];

    private EstadoEntregaActividad _estadoEntrega;
    private NivelLogro _nivelLogro;
    private string _observacion;
    private EstadoEntregaActividad _estadoConfirmado;
    private NivelLogro _nivelConfirmado;
    private string _observacionConfirmada;

    internal EvaluacionCeldaVisual(
        ActividadId actividadId,
        EstudianteId estudianteId,
        bool esAplicable,
        bool esEditable,
        EstadoEntregaActividad estadoEntrega,
        NivelLogro nivelLogro,
        string observacion = "")
    {
        ActividadId = actividadId;
        EstudianteId = estudianteId;
        EsAplicable = esAplicable;
        EsEditable = esAplicable && esEditable;
        (_estadoEntrega, _nivelLogro) = Normalizar(estadoEntrega, nivelLogro);
        _observacion = observacion ?? string.Empty;
        _estadoConfirmado = _estadoEntrega;
        _nivelConfirmado = _nivelLogro;
        _observacionConfirmada = _observacion;
    }

    internal EvaluacionCeldaVisual(
        ActividadId actividadId,
        EstudianteId estudianteId,
        bool esAplicable,
        bool esEditable,
        NivelLogro nivelLogro = NivelLogro.Pendiente,
        string observacion = "")
        : this(
            actividadId,
            estudianteId,
            esAplicable,
            esEditable,
            InferirEstadoLegado(nivelLogro),
            NormalizarNivelLegado(nivelLogro),
            observacion)
    {
    }

    internal ActividadId ActividadId { get; }
    internal EstudianteId EstudianteId { get; }
    public bool EsAplicable { get; }
    public bool EsEditable { get; }
    public IReadOnlyList<OpcionNivelLogroVisual> OpcionesNivel { get; } = OpcionesNivelDisponibles;
    public IReadOnlyList<OpcionEstadoEntregaVisual> OpcionesEstadoEntrega { get; } = OpcionesEstadoDisponibles;
    public IReadOnlyList<OpcionResultadoEvaluacionVisual> OpcionesResultado { get; } = OpcionesResultadoDisponibles;

    public EstadoEntregaActividad EstadoEntrega
    {
        get => _estadoEntrega;
        set
        {
            if (!EsEditable || !EsAplicable || !Enum.IsDefined(value)) return;

            var nivelAnterior = _nivelLogro;
            if (value is EstadoEntregaActividad.Pendiente or EstadoEntregaActividad.NoEntregada)
            {
                _nivelLogro = NivelLogro.Pendiente;
            }

            if (SetProperty(ref _estadoEntrega, value))
            {
                NotificarEstadoVisual();
            }

            if (nivelAnterior != _nivelLogro)
            {
                OnPropertyChanged(nameof(NivelLogro));
                NotificarEstadoVisual();
            }
        }
    }

    public NivelLogro NivelLogro
    {
        get => _nivelLogro;
        set
        {
            if (!EsEditable || !EsAplicable || !Enum.IsDefined(value)) return;

            var estadoAnterior = _estadoEntrega;
            var nivelNormalizado = value;
            if (value == NivelLogro.NoEntrego)
            {
                _estadoEntrega = EstadoEntregaActividad.NoEntregada;
                nivelNormalizado = NivelLogro.Pendiente;
            }
            else if (value != NivelLogro.Pendiente)
            {
                _estadoEntrega = EstadoEntregaActividad.Entregada;
            }

            if (SetProperty(ref _nivelLogro, nivelNormalizado))
            {
                NotificarEstadoVisual();
            }

            if (estadoAnterior != _estadoEntrega)
            {
                OnPropertyChanged(nameof(EstadoEntrega));
                NotificarEstadoVisual();
            }
        }
    }

    public ResultadoEvaluacionVisual Resultado
    {
        get => ObtenerResultado(_estadoEntrega, _nivelLogro);
        set
        {
            if (!EsEditable || !EsAplicable || !Enum.IsDefined(value)) return;

            var (estado, nivel) = ConvertirResultado(value);
            var cambioEstado = _estadoEntrega != estado;
            var cambioNivel = _nivelLogro != nivel;
            if (!cambioEstado && !cambioNivel) return;

            _estadoEntrega = estado;
            _nivelLogro = nivel;
            if (cambioEstado) OnPropertyChanged(nameof(EstadoEntrega));
            if (cambioNivel) OnPropertyChanged(nameof(NivelLogro));
            NotificarEstadoVisual();
        }
    }

    public bool PuedeEvaluarLogro => EsEditable && EsAplicable && _estadoEntrega == EstadoEntregaActividad.Entregada;

    public string Observacion
    {
        get => _observacion;
        set
        {
            if (!EsEditable || !EsAplicable) return;
            var texto = value ?? string.Empty;
            if (texto.Length > 500) texto = texto[..500];
            if (SetProperty(ref _observacion, texto))
            {
                OnPropertyChanged(nameof(TieneCambios));
                OnPropertyChanged(nameof(TieneObservacion));
                OnPropertyChanged(nameof(DescripcionAccesible));
            }
        }
    }

    public bool TieneCambios => EsAplicable
        && (_estadoEntrega != _estadoConfirmado
            || _nivelLogro != _nivelConfirmado
            || !string.Equals(_observacion, _observacionConfirmada, StringComparison.Ordinal));

    public bool TieneObservacion => !string.IsNullOrWhiteSpace(_observacion);

    public string EtiquetaNivel
    {
        get
        {
            if (!EsAplicable) return "—";
            if (_estadoEntrega == EstadoEntregaActividad.NoEntregada) return "N";
            if (_estadoEntrega == EstadoEntregaActividad.Pendiente) return "P";
            return _nivelLogro switch
            {
                NivelLogro.Pendiente => "✓",
                NivelLogro.Domina => "D",
                NivelLogro.Suficiente => "S",
                NivelLogro.EnProceso => "E",
                NivelLogro.RequiereApoyo => "R",
                _ => "?",
            };
        }
    }

    public string NombreNivel
    {
        get
        {
            if (!EsAplicable) return "No aplicable";
            if (_estadoEntrega == EstadoEntregaActividad.NoEntregada) return "No entregada";
            if (_estadoEntrega == EstadoEntregaActividad.Pendiente) return "Pendiente de entrega";
            return _nivelLogro switch
            {
                NivelLogro.Pendiente => "Entregada · pendiente de evaluación",
                NivelLogro.Domina => "Entregada · domina",
                NivelLogro.Suficiente => "Entregada · suficiente",
                NivelLogro.EnProceso => "Entregada · en proceso",
                NivelLogro.RequiereApoyo => "Entregada · requiere apoyo",
                _ => "Estado desconocido",
            };
        }
    }

    public string DescripcionAccesible => TieneObservacion
        ? $"{NombreNivel}. Tiene observación."
        : NombreNivel;

    internal void Confirmar(EstadoEntregaActividad estadoEntrega, NivelLogro nivel, string observacion)
    {
        (_estadoEntrega, _nivelLogro) = Normalizar(estadoEntrega, nivel);
        _observacion = observacion ?? string.Empty;
        _estadoConfirmado = _estadoEntrega;
        _nivelConfirmado = _nivelLogro;
        _observacionConfirmada = _observacion;
        NotificarTodo();
    }

    internal void Confirmar(NivelLogro nivel, string observacion) =>
        Confirmar(InferirEstadoLegado(nivel), NormalizarNivelLegado(nivel), observacion);

    internal void Restaurar()
    {
        _estadoEntrega = _estadoConfirmado;
        _nivelLogro = _nivelConfirmado;
        _observacion = _observacionConfirmada;
        NotificarTodo();
    }

    private void NotificarEstadoVisual()
    {
        OnPropertyChanged(nameof(Resultado));
        OnPropertyChanged(nameof(PuedeEvaluarLogro));
        OnPropertyChanged(nameof(EtiquetaNivel));
        OnPropertyChanged(nameof(NombreNivel));
        OnPropertyChanged(nameof(TieneCambios));
        OnPropertyChanged(nameof(DescripcionAccesible));
    }

    private void NotificarTodo()
    {
        OnPropertyChanged(nameof(EstadoEntrega));
        OnPropertyChanged(nameof(NivelLogro));
        OnPropertyChanged(nameof(Resultado));
        OnPropertyChanged(nameof(Observacion));
        OnPropertyChanged(nameof(PuedeEvaluarLogro));
        OnPropertyChanged(nameof(EtiquetaNivel));
        OnPropertyChanged(nameof(NombreNivel));
        OnPropertyChanged(nameof(TieneCambios));
        OnPropertyChanged(nameof(TieneObservacion));
        OnPropertyChanged(nameof(DescripcionAccesible));
    }

    private static ResultadoEvaluacionVisual ObtenerResultado(
        EstadoEntregaActividad estadoEntrega,
        NivelLogro nivelLogro)
    {
        if (estadoEntrega == EstadoEntregaActividad.Pendiente) return ResultadoEvaluacionVisual.Pendiente;
        if (estadoEntrega == EstadoEntregaActividad.NoEntregada) return ResultadoEvaluacionVisual.NoEntregada;
        return nivelLogro switch
        {
            NivelLogro.Domina => ResultadoEvaluacionVisual.Domina,
            NivelLogro.Suficiente => ResultadoEvaluacionVisual.Suficiente,
            NivelLogro.EnProceso => ResultadoEvaluacionVisual.EnProceso,
            NivelLogro.RequiereApoyo => ResultadoEvaluacionVisual.RequiereApoyo,
            _ => ResultadoEvaluacionVisual.EntregadaSinEvaluar,
        };
    }

    private static (EstadoEntregaActividad Estado, NivelLogro Nivel) ConvertirResultado(ResultadoEvaluacionVisual resultado) =>
        resultado switch
        {
            ResultadoEvaluacionVisual.Pendiente => (EstadoEntregaActividad.Pendiente, NivelLogro.Pendiente),
            ResultadoEvaluacionVisual.EntregadaSinEvaluar => (EstadoEntregaActividad.Entregada, NivelLogro.Pendiente),
            ResultadoEvaluacionVisual.Domina => (EstadoEntregaActividad.Entregada, NivelLogro.Domina),
            ResultadoEvaluacionVisual.Suficiente => (EstadoEntregaActividad.Entregada, NivelLogro.Suficiente),
            ResultadoEvaluacionVisual.EnProceso => (EstadoEntregaActividad.Entregada, NivelLogro.EnProceso),
            ResultadoEvaluacionVisual.RequiereApoyo => (EstadoEntregaActividad.Entregada, NivelLogro.RequiereApoyo),
            ResultadoEvaluacionVisual.NoEntregada => (EstadoEntregaActividad.NoEntregada, NivelLogro.Pendiente),
            _ => (EstadoEntregaActividad.Pendiente, NivelLogro.Pendiente),
        };

    private static (EstadoEntregaActividad EstadoEntrega, NivelLogro NivelLogro) Normalizar(
        EstadoEntregaActividad estadoEntrega,
        NivelLogro nivelLogro)
    {
        if (!Enum.IsDefined(estadoEntrega)) estadoEntrega = EstadoEntregaActividad.Pendiente;
        if (!Enum.IsDefined(nivelLogro)) nivelLogro = NivelLogro.Pendiente;
        if (nivelLogro == NivelLogro.NoEntrego || estadoEntrega == EstadoEntregaActividad.NoEntregada)
            return (EstadoEntregaActividad.NoEntregada, NivelLogro.Pendiente);
        if (estadoEntrega == EstadoEntregaActividad.Pendiente)
            return (EstadoEntregaActividad.Pendiente, NivelLogro.Pendiente);
        if (nivelLogro != NivelLogro.Pendiente)
            return (EstadoEntregaActividad.Entregada, nivelLogro);
        return (EstadoEntregaActividad.Entregada, NivelLogro.Pendiente);
    }

    private static EstadoEntregaActividad InferirEstadoLegado(NivelLogro nivelLogro) => nivelLogro switch
    {
        NivelLogro.NoEntrego => EstadoEntregaActividad.NoEntregada,
        NivelLogro.Pendiente => EstadoEntregaActividad.Pendiente,
        _ => EstadoEntregaActividad.Entregada,
    };

    private static NivelLogro NormalizarNivelLegado(NivelLogro nivelLogro) =>
        nivelLogro == NivelLogro.NoEntrego ? NivelLogro.Pendiente : nivelLogro;
}

public sealed class EvaluacionEstudianteFilaVisual
{
    internal EvaluacionEstudianteFilaVisual(
        EstudianteId estudianteId,
        int numeroLista,
        string nombre,
        bool estaActivoActualmente,
        IReadOnlyList<EvaluacionCeldaVisual> celdas)
    {
        EstudianteId = estudianteId;
        NumeroLista = numeroLista;
        Nombre = nombre;
        EstaActivoActualmente = estaActivoActualmente;
        Celdas = celdas;
    }

    internal EstudianteId EstudianteId { get; }
    public int NumeroLista { get; }
    public string Nombre { get; }
    public bool EstaActivoActualmente { get; }
    public IReadOnlyList<EvaluacionCeldaVisual> Celdas { get; }
    public string SituacionActual => EstaActivoActualmente ? "Activo" : "Inactivo histórico";

    public string Iniciales
    {
        get
        {
            var partes = Nombre.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (partes.Length == 0) return "?";
            if (partes.Length == 1) return char.ToUpper(partes[0][0], System.Globalization.CultureInfo.CurrentCulture).ToString();
            return string.Concat(
                char.ToUpper(partes[0][0], System.Globalization.CultureInfo.CurrentCulture),
                char.ToUpper(partes[1][0], System.Globalization.CultureInfo.CurrentCulture));
        }
    }
}
