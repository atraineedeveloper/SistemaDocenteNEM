using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Presentation;

public sealed class GestionExpedienteViewModel : ViewModelBase
{
    private readonly GestionExpedienteCasosUso _casosUso;
    private readonly IServicioMensajes _mensajes;

    private ExpedienteEstudianteDetalle? _expediente;
    private string _nuevaFortaleza = "";
    private string _nuevaDificultad = "";
    private string _nuevoApoyo = "";
    private string _nuevaObservacion = "";
    private string _motivoAcuerdo = "";
    private string _acuerdoConvenido = "";

    public GestionExpedienteViewModel(
        GestionExpedienteCasosUso casosUso,
        IServicioMensajes mensajes)
    {
        ArgumentNullException.ThrowIfNull(casosUso);
        ArgumentNullException.ThrowIfNull(mensajes);

        _casosUso = casosUso;
        _mensajes = mensajes;

        AgregarFortalezaCommand = new RelayCommand(AgregarFortaleza, () => !string.IsNullOrWhiteSpace(NuevaFortaleza));
        AgregarDificultadCommand = new RelayCommand(AgregarDificultad, () => !string.IsNullOrWhiteSpace(NuevaDificultad));
        AgregarApoyoCommand = new RelayCommand(AgregarApoyo, () => !string.IsNullOrWhiteSpace(NuevoApoyo));
        AgregarObservacionCommand = new RelayCommand(AgregarObservacion, () => !string.IsNullOrWhiteSpace(NuevaObservacion));
        AgregarAcuerdoCommand = new RelayCommand(AgregarAcuerdo, () => !string.IsNullOrWhiteSpace(MotivoAcuerdo) && !string.IsNullOrWhiteSpace(AcuerdoConvenido));
    }

    public ExpedienteEstudianteDetalle? Expediente
    {
        get => _expediente;
        private set => SetProperty(ref _expediente, value);
    }

    public string NuevaFortaleza
    {
        get => _nuevaFortaleza;
        set { if (SetProperty(ref _nuevaFortaleza, value)) AgregarFortalezaCommand.NotifyCanExecuteChanged(); }
    }

    public string NuevaDificultad
    {
        get => _nuevaDificultad;
        set { if (SetProperty(ref _nuevaDificultad, value)) AgregarDificultadCommand.NotifyCanExecuteChanged(); }
    }

    public string NuevoApoyo
    {
        get => _nuevoApoyo;
        set { if (SetProperty(ref _nuevoApoyo, value)) AgregarApoyoCommand.NotifyCanExecuteChanged(); }
    }

    public string NuevaObservacion
    {
        get => _nuevaObservacion;
        set { if (SetProperty(ref _nuevaObservacion, value)) AgregarObservacionCommand.NotifyCanExecuteChanged(); }
    }

    public string MotivoAcuerdo
    {
        get => _motivoAcuerdo;
        set { if (SetProperty(ref _motivoAcuerdo, value)) AgregarAcuerdoCommand.NotifyCanExecuteChanged(); }
    }

    public string AcuerdoConvenido
    {
        get => _acuerdoConvenido;
        set { if (SetProperty(ref _acuerdoConvenido, value)) AgregarAcuerdoCommand.NotifyCanExecuteChanged(); }
    }

    private DateTime? _fechaSeguimientoAcuerdo;

    public DateTime? FechaSeguimientoAcuerdo
    {
        get => _fechaSeguimientoAcuerdo;
        set => SetProperty(ref _fechaSeguimientoAcuerdo, value);
    }

    public RelayCommand AgregarFortalezaCommand { get; }
    public RelayCommand AgregarDificultadCommand { get; }
    public RelayCommand AgregarApoyoCommand { get; }
    public RelayCommand AgregarObservacionCommand { get; }
    public RelayCommand AgregarAcuerdoCommand { get; }

    public void Cargar(GrupoId grupoId, EstudianteId estudianteId)
    {
        try
        {
            Expediente = _casosUso.ConsultarExpediente(grupoId, estudianteId);
        }
        catch (Exception ex) when (ex is DomainConflictException or DomainValidationException or ErrorPersistenciaAplicacionException)
        {
            _mensajes.MostrarError(ex.Message);
        }
    }

    private void AgregarFortaleza()
    {
        if (Expediente is null || string.IsNullOrWhiteSpace(NuevaFortaleza)) return;
        try
        {
            _casosUso.RegistrarNotaPedagogica(Expediente.GrupoId, Expediente.EstudianteId, TipoNotaPedagogica.Fortaleza, NuevaFortaleza);
            NuevaFortaleza = "";
            Cargar(Expediente.GrupoId, Expediente.EstudianteId);
        }
        catch (Exception ex) when (ex is DomainValidationException or DomainConflictException or ErrorPersistenciaAplicacionException)
        {
            _mensajes.MostrarError(ex.Message);
        }
    }

    private void AgregarDificultad()
    {
        if (Expediente is null || string.IsNullOrWhiteSpace(NuevaDificultad)) return;
        try
        {
            _casosUso.RegistrarNotaPedagogica(Expediente.GrupoId, Expediente.EstudianteId, TipoNotaPedagogica.Dificultad, NuevaDificultad);
            NuevaDificultad = "";
            Cargar(Expediente.GrupoId, Expediente.EstudianteId);
        }
        catch (Exception ex) when (ex is DomainValidationException or DomainConflictException or ErrorPersistenciaAplicacionException)
        {
            _mensajes.MostrarError(ex.Message);
        }
    }

    private void AgregarApoyo()
    {
        if (Expediente is null || string.IsNullOrWhiteSpace(NuevoApoyo)) return;
        try
        {
            _casosUso.RegistrarNotaPedagogica(Expediente.GrupoId, Expediente.EstudianteId, TipoNotaPedagogica.ApoyoAplicado, NuevoApoyo);
            NuevoApoyo = "";
            Cargar(Expediente.GrupoId, Expediente.EstudianteId);
        }
        catch (Exception ex) when (ex is DomainValidationException or DomainConflictException or ErrorPersistenciaAplicacionException)
        {
            _mensajes.MostrarError(ex.Message);
        }
    }

    private void AgregarObservacion()
    {
        if (Expediente is null || string.IsNullOrWhiteSpace(NuevaObservacion)) return;
        try
        {
            _casosUso.RegistrarNotaPedagogica(Expediente.GrupoId, Expediente.EstudianteId, TipoNotaPedagogica.ObservacionCronologica, NuevaObservacion);
            NuevaObservacion = "";
            Cargar(Expediente.GrupoId, Expediente.EstudianteId);
        }
        catch (Exception ex) when (ex is DomainValidationException or DomainConflictException or ErrorPersistenciaAplicacionException)
        {
            _mensajes.MostrarError(ex.Message);
        }
    }

    private void AgregarAcuerdo()
    {
        if (Expediente is null || string.IsNullOrWhiteSpace(MotivoAcuerdo) || string.IsNullOrWhiteSpace(AcuerdoConvenido)) return;
        try
        {
            DateOnly? fechaSeg = FechaSeguimientoAcuerdo.HasValue ? DateOnly.FromDateTime(FechaSeguimientoAcuerdo.Value) : null;
            _casosUso.RegistrarAcuerdoTutor(Expediente.GrupoId, Expediente.EstudianteId, MotivoAcuerdo, AcuerdoConvenido, DateOnly.FromDateTime(DateTime.Today), fechaSeg);
            MotivoAcuerdo = "";
            AcuerdoConvenido = "";
            FechaSeguimientoAcuerdo = null;
            Cargar(Expediente.GrupoId, Expediente.EstudianteId);
        }
        catch (Exception ex) when (ex is DomainValidationException or DomainConflictException or ErrorPersistenciaAplicacionException)
        {
            _mensajes.MostrarError(ex.Message);
        }
    }
}