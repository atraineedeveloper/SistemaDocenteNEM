namespace SistemaDocente.Presentation;

public sealed class MainWindowViewModel : ViewModelBase
{
    private bool _mostrarAsistencia;
    private bool _mostrarProyectos;
    private bool _mostrarEvaluacion;
    private bool _mostrarVistaDiaria;

    public MainWindowViewModel(
        GestionGrupoViewModel grupo,
        GestionAsistenciaViewModel asistencia,
        GestionAsistenciaMensualViewModel asistenciaMensual,
        GestionProyectosViewModel? proyectos = null,
        EvaluacionActividadesViewModel? evaluacion = null,
        GestionExpedienteViewModel? expediente = null)
    {
        ArgumentNullException.ThrowIfNull(grupo);
        ArgumentNullException.ThrowIfNull(asistencia);
        Grupo = grupo;
        Asistencia = asistencia;
        AsistenciaMensual = asistenciaMensual;
        Proyectos = proyectos;
        Evaluacion = evaluacion;
        Expediente = expediente;

        IrAGrupoCommand = new RelayCommand(IrAGrupo, () => (MostrarAsistencia || MostrarProyectos || MostrarEvaluacion) && !Asistencia.EstaOcupado);
        IrAAsistenciaCommand = new RelayCommand(IrAAsistencia, () => !MostrarAsistencia && Grupo.GrupoIdActual is not null);
        IrAProyectosCommand = new RelayCommand(IrAProyectos, () => !MostrarProyectos && Grupo.GrupoIdActual is not null && Proyectos is not null);
        IrAEvaluacionCommand = new RelayCommand(IrAEvaluacion, () => !MostrarEvaluacion && Grupo.GrupoIdActual is not null && Evaluacion is not null);
        MostrarVistaMensualCommand = new RelayCommand(() => MostrarVistaDiaria = false);
        MostrarVistaDiariaCommand = new RelayCommand(() => MostrarVistaDiaria = true);

        Grupo.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(GestionGrupoViewModel.GrupoIdActual))
            {
                IrAAsistenciaCommand.NotifyCanExecuteChanged();
                IrAProyectosCommand.NotifyCanExecuteChanged();
                IrAEvaluacionCommand.NotifyCanExecuteChanged();
                OnPropertyChanged(nameof(MostrarNavegacion));
            }
        };
    }

    public GestionGrupoViewModel Grupo { get; }
    public GestionAsistenciaViewModel Asistencia { get; }
    public GestionAsistenciaMensualViewModel AsistenciaMensual { get; }
    public GestionProyectosViewModel? Proyectos { get; }
    public EvaluacionActividadesViewModel? Evaluacion { get; }
    public GestionExpedienteViewModel? Expediente { get; }

    public RelayCommand IrAGrupoCommand { get; }
    public RelayCommand IrAAsistenciaCommand { get; }
    public RelayCommand IrAProyectosCommand { get; }
    public RelayCommand IrAEvaluacionCommand { get; }
    public RelayCommand MostrarVistaMensualCommand { get; }
    public RelayCommand MostrarVistaDiariaCommand { get; }

    public bool MostrarVistaDiaria
    {
        get => _mostrarVistaDiaria;
        private set
        {
            if (SetProperty(ref _mostrarVistaDiaria, value))
            {
                OnPropertyChanged(nameof(MostrarVistaMensual));
                OnPropertyChanged(nameof(MostrarAsistenciaDiaria));
                OnPropertyChanged(nameof(MostrarAsistenciaMensual));
            }
        }
    }
    public bool MostrarVistaMensual => !MostrarVistaDiaria;
    public bool MostrarAsistenciaDiaria => MostrarAsistencia && !MostrarProyectos && !MostrarEvaluacion && MostrarVistaDiaria;
    public bool MostrarAsistenciaMensual => MostrarAsistencia && !MostrarProyectos && !MostrarEvaluacion && MostrarVistaMensual;

    public bool MostrarProyectos
    {
        get => _mostrarProyectos;
        private set
        {
            if (SetProperty(ref _mostrarProyectos, value))
            {
                OnPropertyChanged(nameof(MostrarGrupo));
                OnPropertyChanged(nameof(MostrarAsistenciaDiaria));
                OnPropertyChanged(nameof(MostrarAsistenciaMensual));
                IrAProyectosCommand.NotifyCanExecuteChanged();
                IrAGrupoCommand.NotifyCanExecuteChanged();
                IrAAsistenciaCommand.NotifyCanExecuteChanged();
                IrAEvaluacionCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public bool MostrarEvaluacion
    {
        get => _mostrarEvaluacion;
        private set
        {
            if (SetProperty(ref _mostrarEvaluacion, value))
            {
                OnPropertyChanged(nameof(MostrarGrupo));
                OnPropertyChanged(nameof(MostrarAsistenciaDiaria));
                OnPropertyChanged(nameof(MostrarAsistenciaMensual));
                IrAEvaluacionCommand.NotifyCanExecuteChanged();
                IrAProyectosCommand.NotifyCanExecuteChanged();
                IrAGrupoCommand.NotifyCanExecuteChanged();
                IrAAsistenciaCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public bool MostrarAsistencia
    {
        get => _mostrarAsistencia;
        private set
        {
            if (SetProperty(ref _mostrarAsistencia, value))
            {
                OnPropertyChanged(nameof(MostrarGrupo));
                OnPropertyChanged(nameof(MostrarAsistenciaDiaria));
                OnPropertyChanged(nameof(MostrarAsistenciaMensual));
                IrAGrupoCommand.NotifyCanExecuteChanged();
                IrAAsistenciaCommand.NotifyCanExecuteChanged();
                IrAEvaluacionCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public bool MostrarGrupo => !MostrarAsistencia && !MostrarProyectos && !MostrarEvaluacion;

    public bool MostrarNavegacion => Grupo.GrupoIdActual is not null;

    public bool SolicitarCerrar()
    {
        if (MostrarProyectos && Proyectos?.SolicitarSalir() == false) return false;
        if (MostrarEvaluacion && Evaluacion?.SolicitarSalir() == false) return false;
        if (MostrarAsistencia)
        {
            return MostrarVistaDiaria ? Asistencia.SolicitarCerrar() : AsistenciaMensual.SolicitarSalir();
        }
        return true;
    }

    private void IrAGrupo()
    {
        var puedeSalir = true;
        if (MostrarProyectos) puedeSalir = Proyectos?.SolicitarSalir() != false;
        else if (MostrarEvaluacion) puedeSalir = Evaluacion?.SolicitarSalir() != false;
        else if (MostrarAsistencia) puedeSalir = MostrarVistaDiaria ? Asistencia.SolicitarNavegarAGrupo() : AsistenciaMensual.SolicitarSalir();

        if (puedeSalir)
        {
            MostrarAsistencia = false;
            MostrarProyectos = false;
            MostrarEvaluacion = false;
        }
    }

    private void IrAAsistencia()
    {
        if (Grupo.GrupoIdActual is not { } grupoId) return;
        if (MostrarProyectos && Proyectos?.SolicitarSalir() == false) return;
        if (MostrarEvaluacion && Evaluacion?.SolicitarSalir() == false) return;

        AsistenciaMensual.Inicializar(grupoId);
        Asistencia.Inicializar(grupoId);
        MostrarVistaDiaria = false;
        MostrarProyectos = false;
        MostrarEvaluacion = false;
        MostrarAsistencia = true;
    }

    private void IrAProyectos()
    {
        if (Grupo.GrupoIdActual is not { } grupoId || Proyectos is null) return;
        if (MostrarAsistencia && !(MostrarVistaDiaria ? Asistencia.SolicitarNavegarAGrupo() : AsistenciaMensual.SolicitarSalir())) return;
        if (MostrarEvaluacion && Evaluacion?.SolicitarSalir() == false) return;

        Proyectos.Inicializar(grupoId);
        MostrarAsistencia = false;
        MostrarEvaluacion = false;
        MostrarProyectos = true;
    }

    private void IrAEvaluacion()
    {
        if (Grupo.GrupoIdActual is not { } grupoId || Evaluacion is null) return;
        if (MostrarAsistencia && !(MostrarVistaDiaria ? Asistencia.SolicitarNavegarAGrupo() : AsistenciaMensual.SolicitarSalir())) return;
        if (MostrarProyectos && Proyectos?.SolicitarSalir() == false) return;

        Evaluacion.Inicializar(grupoId);
        MostrarAsistencia = false;
        MostrarProyectos = false;
        MostrarEvaluacion = true;
    }
}