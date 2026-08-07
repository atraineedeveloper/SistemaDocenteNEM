namespace SistemaDocente.Presentation;

public sealed class MainWindowViewModel : ViewModelBase
{
    private bool _mostrarAsistencia;
    private bool _mostrarProyectos;
    private bool _mostrarEvaluacion;

    public MainWindowViewModel(
        GestionGrupoViewModel grupo,
        GestionAsistenciaViewModel asistencia,
        GestionAsistenciaMensualViewModel asistenciaMensual,
        GestionProyectosViewModel? proyectos = null,
        EvaluacionActividadesViewModel? evaluacion = null,
        GestionExpedienteViewModel? expediente = null,
        bool modoDemostracion = false)
    {
        ArgumentNullException.ThrowIfNull(grupo);
        ArgumentNullException.ThrowIfNull(asistencia);
        ArgumentNullException.ThrowIfNull(asistenciaMensual);

        Grupo = grupo;
        ModuloAsistencia = new ModuloAsistenciaViewModel(asistencia, asistenciaMensual);
        Proyectos = proyectos;
        Evaluacion = evaluacion;
        Expediente = expediente;
        ModoDemostracion = modoDemostracion;

        IrAGrupoCommand = new RelayCommand(IrAGrupo, () => (MostrarAsistencia || MostrarProyectos || MostrarEvaluacion) && !Asistencia.EstaOcupado);
        IrAAsistenciaCommand = new RelayCommand(IrAAsistencia, () => !MostrarAsistencia && Grupo.GrupoIdActual is not null);
        IrAProyectosCommand = new RelayCommand(IrAProyectos, () => !MostrarProyectos && Grupo.GrupoIdActual is not null && Proyectos is not null);
        IrAEvaluacionCommand = new RelayCommand(IrAEvaluacion, () => !MostrarEvaluacion && Grupo.GrupoIdActual is not null && Evaluacion is not null);
        MostrarVistaMensualCommand = ModuloAsistencia.MostrarVistaMensualCommand;
        MostrarVistaDiariaCommand = ModuloAsistencia.MostrarVistaDiariaCommand;

        Grupo.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(GestionGrupoViewModel.GrupoIdActual))
            {
                IrAAsistenciaCommand.NotifyCanExecuteChanged();
                IrAProyectosCommand.NotifyCanExecuteChanged();
                IrAEvaluacionCommand.NotifyCanExecuteChanged();
                OnPropertyChanged(nameof(MostrarNavegacion));
                OnPropertyChanged(nameof(TituloVentana));
            }

            if (args.PropertyName == nameof(GestionGrupoViewModel.EstaOcupado))
            {
                OnPropertyChanged(nameof(EstaOcupado));
            }
        };

        Asistencia.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(GestionAsistenciaViewModel.EstaOcupado))
            {
                OnPropertyChanged(nameof(EstaOcupado));
                IrAGrupoCommand.NotifyCanExecuteChanged();
            }
        };

        AsistenciaMensual.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(GestionAsistenciaMensualViewModel.EstaOcupado))
            {
                OnPropertyChanged(nameof(EstaOcupado));
            }
        };

        ModuloAsistencia.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName is nameof(ModuloAsistenciaViewModel.MostrarDiaria)
                or nameof(ModuloAsistenciaViewModel.MostrarMensual))
            {
                OnPropertyChanged(nameof(MostrarVistaDiaria));
                OnPropertyChanged(nameof(MostrarVistaMensual));
                OnPropertyChanged(nameof(MostrarAsistenciaDiaria));
                OnPropertyChanged(nameof(MostrarAsistenciaMensual));
                OnPropertyChanged(nameof(TituloVentana));
            }
        };

        if (Proyectos is not null)
        {
            Proyectos.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(GestionProyectosViewModel.EstaOcupado))
                {
                    OnPropertyChanged(nameof(EstaOcupado));
                }
            };
        }

        if (Evaluacion is not null)
        {
            Evaluacion.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(EvaluacionActividadesViewModel.EstaOcupado))
                {
                    OnPropertyChanged(nameof(EstaOcupado));
                }
            };
        }

        if (Expediente is not null)
        {
            Expediente.PropertyChanged += (_, _) =>
            {
                // El expediente no expone EstaOcupado actualmente.
            };
        }
    }

    public bool EstaOcupado =>
        Grupo.EstaOcupado ||
        Asistencia.EstaOcupado ||
        AsistenciaMensual.EstaOcupado ||
        (Proyectos?.EstaOcupado ?? false) ||
        (Evaluacion?.EstaOcupado ?? false);

    public GestionGrupoViewModel Grupo { get; }
    public ModuloAsistenciaViewModel ModuloAsistencia { get; }
    public GestionAsistenciaViewModel Asistencia => ModuloAsistencia.Diaria;
    public GestionAsistenciaMensualViewModel AsistenciaMensual => ModuloAsistencia.Mensual;
    public GestionProyectosViewModel? Proyectos { get; }
    public EvaluacionActividadesViewModel? Evaluacion { get; }
    public GestionExpedienteViewModel? Expediente { get; }
    public bool ModoDemostracion { get; }

    public RelayCommand IrAGrupoCommand { get; }
    public RelayCommand IrAAsistenciaCommand { get; }
    public RelayCommand IrAProyectosCommand { get; }
    public RelayCommand IrAEvaluacionCommand { get; }
    public RelayCommand MostrarVistaMensualCommand { get; }
    public RelayCommand MostrarVistaDiariaCommand { get; }

    public bool MostrarVistaDiaria => ModuloAsistencia.MostrarDiaria;
    public bool MostrarVistaMensual => ModuloAsistencia.MostrarMensual;
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
                OnPropertyChanged(nameof(TituloVentana));
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
                OnPropertyChanged(nameof(TituloVentana));
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
                OnPropertyChanged(nameof(TituloVentana));
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

    public string TituloVentana
    {
        get
        {
            var titulo = ModoDemostracion ? "Sistema Docente Local · DEMO" : "Sistema Docente Local";
            if (!string.IsNullOrWhiteSpace(Grupo.NombreGrupo)) titulo += " - " + Grupo.NombreGrupo;
            if (MostrarAsistenciaDiaria) titulo += " - Asistencia diaria";
            else if (MostrarAsistenciaMensual) titulo += " - Asistencia mensual";
            else if (MostrarProyectos) titulo += " - Proyectos";
            else if (MostrarEvaluacion) titulo += " - Evaluación";
            else if (MostrarGrupo) titulo += " - Grupo";
            return titulo;
        }
    }

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
        ModuloAsistencia.MostrarVistaMensual();
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