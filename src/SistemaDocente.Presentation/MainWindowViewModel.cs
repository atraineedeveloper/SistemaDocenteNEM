namespace SistemaDocente.Presentation;

public sealed class MainWindowViewModel : ViewModelBase
{
    private bool _mostrarAsistencia;
    private bool _mostrarVistaDiaria;

    public MainWindowViewModel(
        GestionGrupoViewModel grupo,
        GestionAsistenciaViewModel asistencia,
        GestionAsistenciaMensualViewModel asistenciaMensual)
    {
        ArgumentNullException.ThrowIfNull(grupo);
        ArgumentNullException.ThrowIfNull(asistencia);
        Grupo = grupo;
        Asistencia = asistencia;
        AsistenciaMensual = asistenciaMensual;
        IrAGrupoCommand = new RelayCommand(IrAGrupo, () => MostrarAsistencia && !Asistencia.EstaOcupado);
        IrAAsistenciaCommand = new RelayCommand(IrAAsistencia, () => !MostrarAsistencia && Grupo.GrupoIdActual is not null);
        MostrarVistaMensualCommand = new RelayCommand(() => MostrarVistaDiaria = false);
        MostrarVistaDiariaCommand = new RelayCommand(() => MostrarVistaDiaria = true);
        Grupo.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(GestionGrupoViewModel.GrupoIdActual))
            {
                IrAAsistenciaCommand.NotifyCanExecuteChanged();
                OnPropertyChanged(nameof(MostrarNavegacion));
            }
        };
    }

    public GestionGrupoViewModel Grupo { get; }

    public GestionAsistenciaViewModel Asistencia { get; }
    public GestionAsistenciaMensualViewModel AsistenciaMensual { get; }

    public RelayCommand IrAGrupoCommand { get; }

    public RelayCommand IrAAsistenciaCommand { get; }
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
    public bool MostrarAsistenciaDiaria => MostrarAsistencia && MostrarVistaDiaria;
    public bool MostrarAsistenciaMensual => MostrarAsistencia && MostrarVistaMensual;

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

    public bool MostrarGrupo => !MostrarAsistencia;

    public bool MostrarNavegacion => Grupo.GrupoIdActual is not null;

    public bool SolicitarCerrar() => !MostrarAsistencia || (MostrarVistaDiaria ? Asistencia.SolicitarCerrar() : AsistenciaMensual.SolicitarSalir());

    private void IrAGrupo()
    {
        if (MostrarVistaDiaria ? Asistencia.SolicitarNavegarAGrupo() : AsistenciaMensual.SolicitarSalir())
        {
            MostrarAsistencia = false;
        }
    }

    private void IrAAsistencia()
    {
        if (Grupo.GrupoIdActual is not { } grupoId)
        {
            return;
        }

        AsistenciaMensual.Inicializar(grupoId);
        Asistencia.Inicializar(grupoId);
        MostrarVistaDiaria = false;
        MostrarAsistencia = true;
    }
}