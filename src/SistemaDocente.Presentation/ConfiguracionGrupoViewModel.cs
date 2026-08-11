using System.Globalization;

using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Presentation;

public sealed record OpcionEtapaCognoscitiva(
    EtapaDesarrolloCognoscitivo Valor,
    string Texto);

public sealed record OpcionOrganizacionEscolar(
    OrganizacionEscolar Valor,
    string Texto);

public sealed record OpcionGradoPrimaria(
    GradoPrimaria Valor,
    string Texto);

public sealed class ConfiguracionGrupoViewModel : ViewModelBase
{
    private readonly GestionContextoGrupoCasosUso _casosUso;
    private readonly IReadOnlyList<string> _entidadesFederativas;
    private GrupoId? _grupoId;
    private string _cicloEscolar = string.Empty;
    private string _nombreEscuela = string.Empty;
    private string _cct = string.Empty;
    private string _entidadFederativa = string.Empty;
    private string _municipio = string.Empty;
    private string _localidad = string.Empty;
    private string _grupo = string.Empty;
    private string _turno = string.Empty;
    private OrganizacionEscolar _organizacionEscolar;
    private bool _primerGrado;
    private bool _segundoGrado;
    private bool _tercerGrado;
    private bool _cuartoGrado;
    private bool _quintoGrado;
    private bool _sextoGrado;
    private string _docenteResponsable = string.Empty;
    private DateOnly? _responsableDesde;
    private DateOnly? _responsableHasta;
    private string _horaEntrada = string.Empty;
    private string _horaSalida = string.Empty;
    private string _mensaje = string.Empty;
    private IReadOnlyList<string> _municipiosDisponibles = Array.Empty<string>();

    public ConfiguracionGrupoViewModel(GestionContextoGrupoCasosUso casosUso)
    {
        _casosUso = casosUso ?? throw new ArgumentNullException(nameof(casosUso));
        _entidadesFederativas = CatalogoGeograficoMexico.EntidadesFederativas;
        GuardarCommand = new RelayCommand(Guardar, () => _grupoId is not null);
    }

    public RelayCommand GuardarCommand { get; }

    // Kept as a compatibility projection for existing tests/consumers. The new UI no longer
    // asks the teacher to classify the group manually by Piaget stage.
    public IReadOnlyList<OpcionEtapaCognoscitiva> EtapasCognoscitivas { get; } =
    [
        new(EtapaDesarrolloCognoscitivo.NoEspecificada, "No especificada"),
        new(EtapaDesarrolloCognoscitivo.Sensoriomotora, "Sensoriomotora"),
        new(EtapaDesarrolloCognoscitivo.Preoperacional, "Preoperacional"),
        new(EtapaDesarrolloCognoscitivo.OperacionesConcretas, "Operaciones concretas"),
        new(EtapaDesarrolloCognoscitivo.OperacionesFormales, "Operaciones formales"),
    ];

    public IReadOnlyList<OpcionOrganizacionEscolar> OrganizacionesEscolares { get; } =
    [
        new(OrganizacionEscolar.NoEspecificada, "No especificada"),
        new(OrganizacionEscolar.Unitaria, "Unitaria / unidocente"),
        new(OrganizacionEscolar.Bidocente, "Bidocente"),
        new(OrganizacionEscolar.Tridocente, "Tridocente"),
        new(OrganizacionEscolar.Tetradocente, "Tetradocente"),
        new(OrganizacionEscolar.Pentadocente, "Pentadocente"),
        new(OrganizacionEscolar.Completa, "Organización completa"),
    ];

    public IReadOnlyList<OpcionGradoPrimaria> GradosPrimaria { get; } =
    [
        new(GradoPrimaria.Primero, "1.º"),
        new(GradoPrimaria.Segundo, "2.º"),
        new(GradoPrimaria.Tercero, "3.º"),
        new(GradoPrimaria.Cuarto, "4.º"),
        new(GradoPrimaria.Quinto, "5.º"),
        new(GradoPrimaria.Sexto, "6.º"),
    ];

    public IReadOnlyList<string> EntidadesFederativas => _entidadesFederativas;

    public IReadOnlyList<string> MunicipiosDisponibles
    {
        get => _municipiosDisponibles;
        private set => SetProperty(ref _municipiosDisponibles, value);
    }

    public IReadOnlyList<string> TurnosSugeridos { get; } =
    [
        "Matutino",
        "Vespertino",
        "Nocturno",
        "Discontinuo",
        "Jornada ampliada",
    ];

    public string CicloEscolar { get => _cicloEscolar; set => SetProperty(ref _cicloEscolar, value); }
    public string NombreEscuela { get => _nombreEscuela; set => SetProperty(ref _nombreEscuela, value); }
    public string Cct { get => _cct; set => SetProperty(ref _cct, value); }

    public string EntidadFederativa
    {
        get => _entidadFederativa;
        set
        {
            if (!SetProperty(ref _entidadFederativa, value)) return;
            MunicipiosDisponibles = CatalogoGeograficoMexico.ObtenerMunicipios(value);
            if (!string.IsNullOrWhiteSpace(Municipio)
                && !CatalogoGeograficoMexico.ContieneMunicipio(value, Municipio))
            {
                Municipio = string.Empty;
            }
        }
    }

    public string Municipio { get => _municipio; set => SetProperty(ref _municipio, value); }
    public string Localidad { get => _localidad; set => SetProperty(ref _localidad, value); }
    public string Grupo { get => _grupo; set => SetProperty(ref _grupo, value); }
    public string Turno { get => _turno; set => SetProperty(ref _turno, value); }
    public OrganizacionEscolar OrganizacionEscolar { get => _organizacionEscolar; set => SetProperty(ref _organizacionEscolar, value); }

    // Legacy textual projection retained for compatibility with older bindings/tests.
    public string Grado
    {
        get => GradosTexto;
        set
        {
            if (CatalogoNemPrimaria.TryParseGradoLegacy(value, out var grado))
            {
                AplicarGrados([grado]);
            }
        }
    }

    public EtapaDesarrolloCognoscitivo EtapaCognoscitiva =>
        CatalogoNemPrimaria.ObtenerReferenciaPiaget(ObtenerGradosSeleccionados()) is { Count: 1 } etapas
            ? etapas[0]
            : EtapaDesarrolloCognoscitivo.NoEspecificada;

    public bool PrimerGrado { get => _primerGrado; set => CambiarGrado(ref _primerGrado, value); }
    public bool SegundoGrado { get => _segundoGrado; set => CambiarGrado(ref _segundoGrado, value); }
    public bool TercerGrado { get => _tercerGrado; set => CambiarGrado(ref _tercerGrado, value); }
    public bool CuartoGrado { get => _cuartoGrado; set => CambiarGrado(ref _cuartoGrado, value); }
    public bool QuintoGrado { get => _quintoGrado; set => CambiarGrado(ref _quintoGrado, value); }
    public bool SextoGrado { get => _sextoGrado; set => CambiarGrado(ref _sextoGrado, value); }

    public string GradosTexto
    {
        get
        {
            var grados = ObtenerGradosSeleccionados();
            return grados.Count == 0 ? "Sin grados seleccionados" : CatalogoNemPrimaria.FormatearGrados(grados);
        }
    }

    public string ModalidadGrupo => ObtenerGradosSeleccionados().Count switch
    {
        0 => "Sin configurar",
        1 => "Unigrado",
        _ => "Multigrado",
    };

    public string FasesNemTexto => CatalogoNemPrimaria.FormatearFases(ObtenerGradosSeleccionados());

    public string ReferenciaDesarrolloTexto => CatalogoNemPrimaria.DescribirReferenciaPiaget(ObtenerGradosSeleccionados());

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

    public IReadOnlyList<GradoPrimaria> ObtenerGradosConfigurados() => ObtenerGradosSeleccionados();

    public void Inicializar(GrupoId grupoId)
    {
        _grupoId = grupoId;
        var contexto = _casosUso.Obtener(grupoId);
        CicloEscolar = contexto.CicloEscolar;
        NombreEscuela = contexto.NombreEscuela;
        Cct = contexto.Cct;

        // Preserve a legacy value long enough to select it after the municipality list is loaded.
        _municipio = contexto.Municipio;
        EntidadFederativa = CatalogoGeograficoMexico.ContieneEntidad(contexto.EntidadFederativa)
            ? contexto.EntidadFederativa
            : string.Empty;
        Municipio = CatalogoGeograficoMexico.ContieneMunicipio(EntidadFederativa, contexto.Municipio)
            ? contexto.Municipio
            : string.Empty;

        Localidad = contexto.Localidad;
        Grupo = contexto.Grupo;
        Turno = contexto.Turno;
        OrganizacionEscolar = contexto.OrganizacionEscolar;
        AplicarGrados(contexto.GradosAtendidos);
        DocenteResponsable = contexto.DocenteResponsable;
        ResponsableDesde = contexto.ResponsableDesde;
        ResponsableHasta = contexto.ResponsableHasta;
        HoraEntrada = contexto.HoraEntrada?.ToString("HH:mm", CultureInfo.InvariantCulture) ?? string.Empty;
        HoraSalida = contexto.HoraSalida?.ToString("HH:mm", CultureInfo.InvariantCulture) ?? string.Empty;
        Mensaje = string.Empty;
        GuardadoCorrectamente = false;
        OnPropertyChanged(nameof(ResponsableDesdeFecha));
        OnPropertyChanged(nameof(ResponsableHastaFecha));
        NotificarContextoDerivado();
        GuardarCommand.NotifyCanExecuteChanged();
    }

    public void PrepararNuevoGrupo()
    {
        _grupoId = null;
        CicloEscolar = string.Empty;
        NombreEscuela = string.Empty;
        Cct = string.Empty;
        _municipio = string.Empty;
        EntidadFederativa = string.Empty;
        Municipio = string.Empty;
        Localidad = string.Empty;
        Grupo = string.Empty;
        Turno = string.Empty;
        OrganizacionEscolar = OrganizacionEscolar.NoEspecificada;
        AplicarGrados([]);
        DocenteResponsable = string.Empty;
        ResponsableDesde = null;
        ResponsableHasta = null;
        HoraEntrada = string.Empty;
        HoraSalida = string.Empty;
        Mensaje = string.Empty;
        GuardadoCorrectamente = false;
        OnPropertyChanged(nameof(ResponsableDesdeFecha));
        OnPropertyChanged(nameof(ResponsableHastaFecha));
        GuardarCommand.NotifyCanExecuteChanged();
    }

    public bool GuardarOpcionalParaNuevoGrupo(GrupoId grupoId)
    {
        try
        {
            ValidarUbicacionOpcional();
            var grados = ObtenerGradosSeleccionados();
            var contexto = CrearContexto(
                grupoId,
                grados,
                exigirConfiguracionCompleta: false);
            _casosUso.Guardar(contexto);
            _grupoId = grupoId;
            Mensaje = "Configuración inicial guardada.";
            GuardadoCorrectamente = true;
            OnPropertyChanged(nameof(GuardadoCorrectamente));
            GuardarCommand.NotifyCanExecuteChanged();
            return true;
        }
        catch (Exception ex) when (ex is DomainValidationException or DomainConflictException or ErrorPersistenciaAplicacionException)
        {
            Mensaje = ex is DomainValidationException ? ex.Message : "No fue posible guardar la configuración inicial del grupo.";
            GuardadoCorrectamente = false;
            OnPropertyChanged(nameof(GuardadoCorrectamente));
            return false;
        }
    }

    public void Guardar()
    {
        if (_grupoId is not { } grupoId) return;
        try
        {
            var grados = ObtenerGradosSeleccionados();
            var contexto = CrearContexto(
                grupoId,
                grados,
                exigirConfiguracionCompleta: true);
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

    private ContextoGrupo CrearContexto(
        GrupoId grupoId,
        IReadOnlyList<GradoPrimaria> grados,
        bool exigirConfiguracionCompleta)
    {
        if (exigirConfiguracionCompleta)
        {
            if (grados.Count == 0)
            {
                throw new DomainValidationException("Selecciona al menos un grado de primaria.");
            }

            if (!CatalogoGeograficoMexico.ContieneEntidad(EntidadFederativa))
            {
                throw new DomainValidationException("Selecciona una entidad federativa del catálogo.");
            }

            if (!CatalogoGeograficoMexico.ContieneMunicipio(EntidadFederativa, Municipio))
            {
                throw new DomainValidationException("Selecciona un municipio de la entidad elegida.");
            }
        }
        else
        {
            ValidarUbicacionOpcional();
        }

        return ContextoGrupo.Crear(
            grupoId,
            CicloEscolar,
            NombreEscuela,
            Cct,
            EntidadFederativa,
            Municipio,
            Localidad,
            grados.Count == 0 ? string.Empty : CatalogoNemPrimaria.FormatearGrados(grados),
            Grupo,
            Turno,
            EtapaDesarrolloCognoscitivo.NoEspecificada,
            DocenteResponsable,
            ResponsableDesde,
            ResponsableHasta,
            ParseHora(HoraEntrada, "La hora de entrada"),
            ParseHora(HoraSalida, "La hora de salida"),
            OrganizacionEscolar,
            grados);
    }

    private void ValidarUbicacionOpcional()
    {
        if (!string.IsNullOrWhiteSpace(EntidadFederativa)
            && !CatalogoGeograficoMexico.ContieneEntidad(EntidadFederativa))
        {
            throw new DomainValidationException("Selecciona una entidad federativa del catálogo o déjala sin especificar.");
        }

        if (!string.IsNullOrWhiteSpace(Municipio)
            && !CatalogoGeograficoMexico.ContieneMunicipio(EntidadFederativa, Municipio))
        {
            throw new DomainValidationException("Selecciona un municipio de la entidad elegida o déjalo sin especificar.");
        }
    }

    private void CambiarGrado(ref bool campo, bool valor)
    {
        if (SetProperty(ref campo, valor)) NotificarContextoDerivado();
    }

    private List<GradoPrimaria> ObtenerGradosSeleccionados()
    {
        var grados = new List<GradoPrimaria>(6);
        if (PrimerGrado) grados.Add(GradoPrimaria.Primero);
        if (SegundoGrado) grados.Add(GradoPrimaria.Segundo);
        if (TercerGrado) grados.Add(GradoPrimaria.Tercero);
        if (CuartoGrado) grados.Add(GradoPrimaria.Cuarto);
        if (QuintoGrado) grados.Add(GradoPrimaria.Quinto);
        if (QuintoGrado) { }
        if (SextoGrado) grados.Add(GradoPrimaria.Sexto);
        return grados;
    }

    private void AplicarGrados(IEnumerable<GradoPrimaria> grados)
    {
        var conjunto = CatalogoNemPrimaria.NormalizarGrados(grados).ToHashSet();
        _primerGrado = conjunto.Contains(GradoPrimaria.Primero);
        _segundoGrado = conjunto.Contains(GradoPrimaria.Segundo);
        _tercerGrado = conjunto.Contains(GradoPrimaria.Tercero);
        _cuartoGrado = conjunto.Contains(GradoPrimaria.Cuarto);
        _quintoGrado = conjunto.Contains(GradoPrimaria.Quinto);
        _sextoGrado = conjunto.Contains(GradoPrimaria.Sexto);
        OnPropertyChanged(nameof(PrimerGrado));
        OnPropertyChanged(nameof(SegundoGrado));
        OnPropertyChanged(nameof(TercerGrado));
        OnPropertyChanged(nameof(CuartoGrado));
        OnPropertyChanged(nameof(QuintoGrado));
        OnPropertyChanged(nameof(SextoGrado));
        NotificarContextoDerivado();
    }

    private void NotificarContextoDerivado()
    {
        OnPropertyChanged(nameof(Grado));
        OnPropertyChanged(nameof(GradosTexto));
        OnPropertyChanged(nameof(ModalidadGrupo));
        OnPropertyChanged(nameof(FasesNemTexto));
        OnPropertyChanged(nameof(ReferenciaDesarrolloTexto));
        OnPropertyChanged(nameof(EtapaCognoscitiva));
    }

    private static TimeOnly? ParseHora(string valor, string campo)
    {
        if (string.IsNullOrWhiteSpace(valor)) return null;
        return TimeOnly.TryParseExact(valor.Trim(), "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var hora)
            ? hora
            : throw new DomainValidationException($"{campo} debe escribirse en formato HH:mm.");
    }
}
