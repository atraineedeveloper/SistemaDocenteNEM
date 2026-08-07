using SistemaDocente.Core;

namespace SistemaDocente.Presentation;

public sealed record OpcionNivelLogroVisual(NivelLogro Nivel, string Texto);

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
    private static readonly IReadOnlyList<OpcionNivelLogroVisual> Opciones =
    [
        new(NivelLogro.Pendiente, "Pendiente"),
        new(NivelLogro.Domina, "Domina"),
        new(NivelLogro.Suficiente, "Suficiente"),
        new(NivelLogro.EnProceso, "En proceso"),
        new(NivelLogro.RequiereApoyo, "Requiere apoyo"),
        new(NivelLogro.NoEntrego, "No entregó"),
    ];

    private NivelLogro _nivelLogro;
    private string _observacion;
    private NivelLogro _nivelConfirmado;
    private string _observacionConfirmada;

    internal EvaluacionCeldaVisual(
        ActividadId actividadId,
        EstudianteId estudianteId,
        bool esAplicable,
        bool esEditable,
        NivelLogro nivelLogro = NivelLogro.Pendiente,
        string observacion = "")
    {
        ActividadId = actividadId;
        EstudianteId = estudianteId;
        EsAplicable = esAplicable;
        EsEditable = esAplicable && esEditable;
        _nivelLogro = nivelLogro;
        _observacion = observacion ?? string.Empty;
        _nivelConfirmado = _nivelLogro;
        _observacionConfirmada = _observacion;
    }

    internal ActividadId ActividadId { get; }
    internal EstudianteId EstudianteId { get; }
    public bool EsAplicable { get; }
    public bool EsEditable { get; }
    public IReadOnlyList<OpcionNivelLogroVisual> OpcionesNivel { get; } = Opciones;

    public NivelLogro NivelLogro
    {
        get => _nivelLogro;
        set
        {
            if (!EsEditable || !EsAplicable || !Enum.IsDefined(value)) return;
            if (SetProperty(ref _nivelLogro, value))
            {
                OnPropertyChanged(nameof(EtiquetaNivel));
                OnPropertyChanged(nameof(NombreNivel));
                OnPropertyChanged(nameof(TieneCambios));
                OnPropertyChanged(nameof(DescripcionAccesible));
            }
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
        && (_nivelLogro != _nivelConfirmado
            || !string.Equals(_observacion, _observacionConfirmada, StringComparison.Ordinal));

    public bool TieneObservacion => !string.IsNullOrWhiteSpace(_observacion);

    public string EtiquetaNivel => !EsAplicable ? "—" : _nivelLogro switch
    {
        NivelLogro.Pendiente => "P",
        NivelLogro.Domina => "D",
        NivelLogro.Suficiente => "S",
        NivelLogro.EnProceso => "E",
        NivelLogro.RequiereApoyo => "R",
        NivelLogro.NoEntrego => "N",
        _ => "?",
    };

    public string NombreNivel => !EsAplicable ? "No aplicable" : _nivelLogro switch
    {
        NivelLogro.Pendiente => "Pendiente",
        NivelLogro.Domina => "Domina",
        NivelLogro.Suficiente => "Suficiente",
        NivelLogro.EnProceso => "En proceso",
        NivelLogro.RequiereApoyo => "Requiere apoyo",
        NivelLogro.NoEntrego => "No entregó",
        _ => "Nivel desconocido",
    };

    public string DescripcionAccesible => TieneObservacion
        ? $"{NombreNivel}. Tiene observación."
        : NombreNivel;

    internal void Confirmar(NivelLogro nivel, string observacion)
    {
        _nivelLogro = nivel;
        _observacion = observacion ?? string.Empty;
        _nivelConfirmado = nivel;
        _observacionConfirmada = _observacion;
        NotificarTodo();
    }

    internal void Restaurar()
    {
        _nivelLogro = _nivelConfirmado;
        _observacion = _observacionConfirmada;
        NotificarTodo();
    }

    private void NotificarTodo()
    {
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