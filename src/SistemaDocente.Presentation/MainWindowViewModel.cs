namespace SistemaDocente.Presentation;

public sealed class MainWindowViewModel : ViewModelBase
{
    private bool _mostrarAsistencia;
    private bool _mostrarProyectos;
    private bool _mostrarVistaDiaria;

    public MainWindowViewModel(
        GestionGrupoViewModel grupo,
        GestionAsistenciaViewModel asistencia,
        GestionAsistenciaMensualViewModel asistenciaMensual,
        GestionProyectosViewModel? proyectos = null)
    {
        ArgumentNullException.ThrowIfNull(grupo);
        ArgumentNullException.ThrowIfNull(asistencia);
        Grupo = grupo;
        Asistencia = asistencia;
        AsistenciaMensual = asistenciaMensual;
        Proyectos = proyectos;
        IrAGrupoCommand = new RelayCommand(IrAGrupo, () => (MostrarAsistencia || MostrarProyectos) && !Asistencia.EstaOcupado);
        IrAAsistenciaCommand = new RelayCommand(IrAAsistencia, () => !MostrarAsistencia && Grupo.GrupoIdActual is not null);
        IrAProyectosCommand = new RelayCommand(IrAProyectos, () => !MostrarProyectos && Grupo.GrupoIdActual is not null && Proyectos is not null);
        MostrarVistaMensualCommand = new RelayCommand(() => MostrarVistaDiaria = false);
        MostrarVistaDiariaCommand = new RelayCommand(() => MostrarVistaDiaria = true);
        Grupo.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(GestionGrupoViewModel.GrupoIdActual))
            {
                IrAAsistenciaCommand.NotifyCanExecuteChanged();
                IrAProyectosCommand.NotifyCanExecuteChanged();
                OnPropertyChanged(nameof(MostrarNavegacion));
            }
        };
    }

    public GestionGrupoViewModel Grupo { get; }

    public GestionAsistenciaViewModel Asistencia { get; }
    public GestionAsistenciaMensualViewModel AsistenciaMensual { get; }
    public GestionProyectosViewModel? Proyectos { get; }

    public RelayCommand IrAGrupoCommand { get; }

    public RelayCommand IrAAsistenciaCommand { get; }
    public RelayCommand IrAProyectosCommand { get; }
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
    public bool MostrarAsistenciaDiaria => MostrarAsistencia && !MostrarProyectos && MostrarVistaDiaria;
    public bool MostrarAsistenciaMensual => MostrarAsistencia && !MostrarProyectos && MostrarVistaMensual;
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
            }
        }
    }

    public bool MostrarGrupo => !MostrarAsistencia && !MostrarProyectos;

    public bool MostrarNavegacion => Grupo.GrupoIdActual is not null;

    public bool SolicitarCerrar() => MostrarProyectos ? Proyectos?.SolicitarSalir() != false : !MostrarAsistencia || (MostrarVistaDiaria ? Asistencia.SolicitarCerrar() : AsistenciaMensual.SolicitarSalir());

    private void IrAGrupo()
    {
        var puedeSalir = MostrarProyectos
            ? Proyectos?.SolicitarSalir() != false
            : MostrarVistaDiaria ? Asistencia.SolicitarNavegarAGrupo() : AsistenciaMensual.SolicitarSalir();
        if (puedeSalir)
        {
            MostrarAsistencia = false;
            MostrarProyectos = false;
        }
    }

    private void IrAAsistencia()
    {
        if (Grupo.GrupoIdActual is not { } grupoId)
        {
            return;
        }

        if (MostrarProyectos && Proyectos?.SolicitarSalir() == false)
        {
            return;
        }

        AsistenciaMensual.Inicializar(grupoId);
        Asistencia.Inicializar(grupoId);
        MostrarVistaDiaria = false;
        MostrarProyectos = false;
        MostrarAsistencia = true;
    }

    private void IrAProyectos()
    {
        if (Grupo.GrupoIdActual is not { } grupoId || Proyectos is null) return;
        if (MostrarAsistencia && !(MostrarVistaDiaria ? Asistencia.SolicitarNavegarAGrupo() : AsistenciaMensual.SolicitarSalir())) return;
        Proyectos.Inicializar(grupoId);
        MostrarAsistencia = false;
        MostrarProyectos = true;
    }
}