using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Presentation;

public sealed record OpcionEtapaCognoscitiva(
    EtapaDesarrolloCognoscitivo Valor,
    string Texto);

public sealed class ConfiguracionGrupoViewModel : ViewModelBase
{
    private readonly GestionContextoGrupoCasosUso _casosUso;
    private GrupoId? _grupoId;
    private string _cicloEscolar = string.Empty;
    private string _nombreEscuela = string.Empty;
    private string _cct = string.Empty;
    private string _entidadFederativa = string.Empty;
    private string _municipio = string.Empty;
    private string _localidad = string.Empty;
    private string _grado = string.Empty;
    private string _grupo = string.Empty;
    private string _turno = string.Empty;
    private EtapaDesarrolloCognoscitivo _etapaCognoscitiva;
    private string _docenteResponsable = string.Empty;
    private DateOnly? _responsableDesde;
    private DateOnly? _responsableHasta;
    private string _horaEntrada = string.Empty;
    private string _horaSalida = string.Empty;
    private string _mensaje = string.Empty;

    public ConfiguracionGrupoViewModel(GestionContextoGrupoCasosUso casosUso)
    {
        _casosUso = casosUso ?? throw new ArgumentNullException(nameof(casosUso));
        GuardarCommand = new RelayCommand(Guardar, () => _grupoId is not null);
    }

    public RelayCommand GuardarCommand { get; }

    public IReadOnlyList<OpcionEtapaCognoscitiva> EtapasCognoscitivas { get; } =
    [
        new(EtapaDesarrolloCognoscitivo.NoEspecificada, "No especificada"),
        new(EtapaDesarrolloCognoscitivo.Sensoriomotora, "Sensoriomotora"),
        new(EtapaDesarrolloCognoscitivo.Preoperacional, "Preoperacional"),
        new(EtapaDesarrolloCognoscitivo.OperacionesConcretas, "Operaciones concretas"),
        new(EtapaDesarrolloCognoscitivo.OperacionesFormales, "Operaciones formales"),
    ];

    public string CicloEscolar { get => _cicloEscolar; set => SetProperty(ref _cicloEscolar, value); }
    public string NombreEscuela { get => _nombreEscuela; set => SetProperty(ref _nombreEscuela, value); }
    public string Cct { get => _cct; set => SetProperty(ref _cct, value); }
    public string EntidadFederativa { get => _entidadFederativa; set => SetProperty(ref _entidadFederativa, value); }
    public string Municipio { get => _municipio; set => SetProperty(ref _municipio, value); }
    public string Localidad { get => _localidad; set => SetProperty(ref _localidad, value); }
    public string Grado { get => _grado; set => SetProperty(ref _grado, value); }
    public string Grupo { get => _grupo; set => SetProperty(ref _grupo, value); }
    public string Turno { get => _turno; set => SetProperty(ref _turno, value); }
    public EtapaDesarrolloCognoscitivo EtapaCognoscitiva { get => _etapaCognoscitiva; set => SetProperty(ref _etapaCognoscitiva, value); }
    public string DocenteResponsable { get => _docenteResponsable; set => SetProperty(ref _docenteResponsable, value); }
    public DateOnly? ResponsableDesde { get => _responsableDesde; set { if (SetProperty(ref _responsableDesde, value)) OnPropertyChanged(nameof(ResponsableDesdeFecha)); } }
    public DateOnly? ResponsableHasta { get => _responsableHasta; set { if (SetProperty(ref _responsableHasta, value)) OnPropertyChanged(nameof(ResponsableHastaFecha)); } }
    public string HoraEntrada { get => _horaEntrada; set => SetProperty(ref _horaEntrada, value); }
    public string HoraSalida { get => _horaSalida; set => SetProperty(ref _horaSalida, value); }
    public string Mensaje { get => _mensaje; private set => SetProperty(ref _mensaje, value); }
    public bool GuardadoCorrectamente { get; private set; }

    public DateTime? ResponsableDesdeFecha
    {
        get => ResponsableDesde is { } fecha ? fecha.ToDateTime(TimeOnly.MinValue) : null;
        set => ResponsableDesde = value is { } fecha ? DateOnly.FromDateTime(fecha) : null;
    }

    public DateTime? ResponsableHastaFecha
    {
        get => ResponsableHasta is { } fecha ? fecha.ToDateTime(TimeOnly.MinValue) : null;
        set => ResponsableHasta = value is { } fecha ? DateOnly.FromDateTime(fecha) : null;
    }

    public void Inicializar(GrupoId grupoId)
    {
        _grupoId = grupoId;
        var contexto = _casosUso.Obtener(grupoId);
        CicloEscolar = contexto.CicloEscolar;
        NombreEscuela = contexto.NombreEscuela;
        Cct = contexto.Cct;
        EntidadFederativa = contexto.EntidadFederativa;
        Municipio = contexto.Municipio;
        Localidad = contexto.Localidad;
        Grado = contexto.Grado;
        Grupo = contexto.Grupo;
        Turno = contexto.Turno;
        EtapaCognoscitiva = contexto.EtapaCognoscitiva;
        DocenteResponsable = contexto.DocenteResponsable;
        ResponsableDesde = contexto.ResponsableDesde;
        ResponsableHasta = contexto.ResponsableHasta;
        HoraEntrada = contexto.HoraEntrada?.ToString("HH:mm") ?? string.Empty;
        HoraSalida = contexto.HoraSalida?.ToString("HH:mm") ?? string.Empty;
        Mensaje = string.Empty;
        GuardadoCorrectamente = false;
        OnPropertyChanged(nameof(ResponsableDesdeFecha));
        OnPropertyChanged(nameof(ResponsableHastaFecha));
        GuardarCommand.NotifyCanExecuteChanged();
    }

    public void Guardar()
    {
        if (_grupoId is not { } grupoId) return;
        try
        {
            var contexto = ContextoGrupo.Crear(
                grupoId,
                CicloEscolar,
                NombreEscuela,
                Cct,
                EntidadFederativa,
                Municipio,
                Localidad,
                Grado,
                Grupo,
                Turno,
                EtapaCognoscitiva,
                DocenteResponsable,
                ResponsableDesde,
                ResponsableHasta,
                ParseHora(HoraEntrada, "La hora de entrada"),
                ParseHora(HoraSalida, "La hora de salida"));
            _casosUso.Guardar(contexto);
            Mensaje = "Configuración guardada.";
            GuardadoCorrectamente = true;
            OnPropertyChanged(nameof(GuardadoCorrectamente));
        }
        catch (Exception ex) when (ex is DomainValidationException or DomainConflictException or ErrorPersistenciaAplicacionException)
        {
            Mensaje = ex is DomainValidationException ? ex.Message : "No fue posible guardar la configuración del grupo.";
            GuardadoCorrectamente = false;
            OnPropertyChanged(nameof(GuardadoCorrectamente));
        }
    }

    private static TimeOnly? ParseHora(string valor, string campo)
    {
        if (string.IsNullOrWhiteSpace(valor)) return null;
        return TimeOnly.TryParseExact(valor.Trim(), "HH:mm", out var hora)
            ? hora
            : throw new DomainValidationException($"{campo} debe escribirse en formato HH:mm.");
    }
}
