using SistemaDocente.Core;

namespace SistemaDocente.Presentation;

public sealed record OpcionNivelLogroVisual(NivelLogro Nivel, string Texto);
public sealed record OpcionEstadoEntregaVisual(EstadoEntregaActividad Estado, string Texto);

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
        new(NivelLogro.Pendiente, "Pendiente de evaluar"),
        new(NivelLogro.Domina, "Domina"),
        new(NivelLogro.Suficiente, "Suficiente"),
        new(NivelLogro.EnProceso, "En proceso"),
        new(NivelLogro.RequiereApoyo, "Requiere apoyo"),
    ];

    private static readonly IReadOnlyList<OpcionEstadoEntregaVisual> OpcionesEstadoDisponibles =
    [
        new(EstadoEntregaActividad.Pendiente, "Pendiente de registro"),
        new(EstadoEntregaActividad.Entregada, "Entregada"),
        new(EstadoEntregaActividad.NoEntregada, "No entregada"),
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
        NivelLogro nivelLogro = NivelLogro.Pendiente,
        string observacion = "")
    {
        ActividadId = actividadId;
        EstudianteId = estudianteId;
        EsAplicable = esAplicable;
        EsEditable = esAplicable && esEditable;
        Normalizar(ref estadoEntrega, ref nivelLogro);
        _estadoEntrega = estadoEntrega;
        _nivelLogro = nivelLogro;
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
            EstadoDesdeNivelLegado(nivelLogro),
            nivelLogro == NivelLogro.NoEntrego ? NivelLogro.Pendiente : nivelLogro,
            observacion)
    {
    }

    internal ActividadId ActividadId { get; }
    internal EstudianteId EstudianteId { get; }
    public bool EsAplicable { get; }
    public bool EsEditable { get; }
    public IReadOnlyList<OpcionNivelLogroVisual> OpcionesNivel { get; } = OpcionesNivelDisponibles;
    public IReadOnlyList<OpcionEstadoEntregaVisual> OpcionesEstadoEntrega { get; } = OpcionesEstadoDisponibles;

    public EstadoEntregaActividad EstadoEntrega
    {
        get => _estadoEntrega;
        set
        {
            if (!EsEditable || !EsAplicable || !Enum.IsDefined(value)) return;
            var nivelAnterior = _nivelLogro;
            if (value == EstadoEntregaActividad.NoEntregada)
            {
                _nivelLogro = NivelLogro.Pendiente;
            }

            if (!SetProperty(ref _estadoEntrega, value)) return;
            if (nivelAnterior != _nivelLogro) OnPropertyChanged(nameof(NivelLogro));
            NotificarEstadoVisual();
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

            var cambioNivel = SetProperty(ref _nivelLogro, nivelNormalizado);
            if (estadoAnterior != _estadoEntrega) OnPropertyChanged(nameof(EstadoEntrega));
            if (cambioNivel || estadoAnterior != _estadoEntrega) NotificarEstadoVisual();
        }
    }

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

    public string EtiquetaNivel => !EsAplicable ? "—" : _estadoEntrega switch
    {
        EstadoEntregaActividad.NoEntregada => "N",
        _ => _nivelLogro switch
        {
            NivelLogro.Pendiente => "P",
            NivelLogro.Domina => "D",
            NivelLogro.Suficiente => "S",
            NivelLogro.EnProceso => "E",
            NivelLogro.RequiereApoyo => "R",
            _ => "?",
        },
    };

    public string NombreNivel => !EsAplicable ? "No aplicable" : _estadoEntrega switch
    {
        EstadoEntregaActividad.NoEntregada => "No entregada",
        EstadoEntregaActividad.Pendiente => "Entrega pendiente de registro",
        _ => _nivelLogro switch
        {
            NivelLogro.Pendiente => "Entregada, pendiente de evaluar",
            NivelLogro.Domina => "Domina",
            NivelLogro.Suficiente => "Suficiente",
            NivelLogro.EnProceso => "En proceso",
            NivelLogro.RequiereApoyo => "Requiere apoyo",
            _ => "Nivel desconocido",
        },
    };

    public string DescripcionAccesible => TieneObservacion
        ? $"{NombreNivel}. Tiene observación."
        : NombreNivel;

    internal void Confirmar(EstadoEntregaActividad estado, NivelLogro nivel, string observacion)
    {
        Normalizar(ref estado, ref nivel);
        _estadoEntrega = estado;
        _nivelLogro = nivel;
        _observacion = observacion ?? string.Empty;
        _estadoConfirmado = estado;
        _nivelConfirmado = nivel;
        _observacionConfirmada = _observacion;
        NotificarTodo();
    }

    internal void Confirmar(NivelLogro nivel, string observacion) =>
        Confirmar(EstadoDesdeNivelLegado(nivel), nivel == NivelLogro.NoEntrego ? NivelLogro.Pendiente : nivel, observacion);

    internal void Restaurar()
    {
        _estadoEntrega = _estadoConfirmado;
        _nivelLogro = _nivelConfirmado;
        _observacion = _observacionConfirmada;
        NotificarTodo();
    }

    private static EstadoEntregaActividad EstadoDesdeNivelLegado(NivelLogro nivel) => nivel switch
    {
        NivelLogro.NoEntrego => EstadoEntregaActividad.NoEntregada,
        NivelLogro.Pendiente => EstadoEntregaActividad.Pendiente,
        _ => EstadoEntregaActividad.Entregada,
    };

    private static void Normalizar(ref EstadoEntregaActividad estado, ref NivelLogro nivel)
    {
        if (!Enum.IsDefined(estado)) estado = EstadoEntregaActividad.Pendiente;
        if (!Enum.IsDefined(nivel)) nivel = NivelLogro.Pendiente;
        if (nivel == NivelLogro.NoEntrego)
        {
            estado = EstadoEntregaActividad.NoEntregada;
            nivel = NivelLogro.Pendiente;
        }
        else if (estado == EstadoEntregaActividad.NoEntregada)
        {
            nivel = NivelLogro.Pendiente;
        }
        else if (nivel != NivelLogro.Pendiente)
        {
            estado = EstadoEntregaActividad.Entregada;
        }
    }

    private void NotificarEstadoVisual()
    {
        OnPropertyChanged(nameof(EtiquetaNivel));
        OnPropertyChanged(nameof(NombreNivel));
        OnPropertyChanged(nameof(TieneCambios));
        OnPropertyChanged(nameof(DescripcionAccesible));
    }

    private void NotificarTodo()
    {
        OnPropertyChanged(nameof(EstadoEntrega));
        OnPropertyChanged(nameof(NivelLogro));
        OnPropertyChanged(nameof(Observacion));
        OnPropertyChanged(nameof(EtiquetaNivel));
        OnPropertyChanged(nameof(NombreNivel));
        OnPropertyChanged(nameof(TieneCambios));
        OnPropertyChanged(nameof(TieneObservacion));
        OnPropertyChanged(nameof(DescripcionAccesible));
    }
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
