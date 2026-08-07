using SistemaDocente.Core;

namespace SistemaDocente.Presentation;

public sealed class MainWindowViewModel : ViewModelBase
{
    private bool _mostrarInicio = true;
    private bool _mostrarAsistencia;
    private bool _mostrarProyectos;
    private bool _mostrarEvaluacion;
    private bool _mostrarReportes;

    public MainWindowViewModel(
        GestionGrupoViewModel grupo,
        GestionAsistenciaViewModel asistencia,
        GestionAsistenciaMensualViewModel asistenciaMensual,
        GestionProyectosViewModel? proyectos = null,
        EvaluacionActividadesViewModel? evaluacion = null,
        GestionExpedienteViewModel? expediente = null,
        bool modoDemostracion = false,
        GestionReportesViewModel? reportes = null)
    {
        ArgumentNullException.ThrowIfNull(grupo);
        ArgumentNullException.ThrowIfNull(asistencia);
        ArgumentNullException.ThrowIfNull(asistenciaMensual);

        Grupo = grupo;
        ModuloAsistencia = new ModuloAsistenciaViewModel(asistencia, asistenciaMensual);
        Proyectos = proyectos;
        Evaluacion = evaluacion;
        Expediente = expediente;
        Reportes = reportes;
        ModoDemostracion = modoDemostracion;

        IrAInicioCommand = new RelayCommand(MostrarInicioGrupos, () => !EstaOcupado);
        CrearGrupoDesdeInicioCommand = new RelayCommand(CrearGrupoDesdeInicio, () => !EstaOcupado);
        IrAGrupoCommand = new RelayCommand(
            IrAGrupo,
            () => (MostrarInicio || MostrarAsistencia || MostrarProyectos || MostrarEvaluacion || MostrarReportes) && !Asistencia.EstaOcupado);
        IrAAsistenciaCommand = new RelayCommand(
            IrAAsistencia,
            () => !MostrarInicio && !MostrarAsistencia && Grupo.GrupoIdActual is not null);
        IrAProyectosCommand = new RelayCommand(
            IrAProyectos,
            () => !MostrarInicio && !MostrarProyectos && Grupo.GrupoIdActual is not null && Proyectos is not null);
        IrAEvaluacionCommand = new RelayCommand(
            IrAEvaluacion,
            () => !MostrarInicio && !MostrarEvaluacion && Grupo.GrupoIdActual is not null && Evaluacion is not null);
        IrAReportesCommand = new RelayCommand(
            IrAReportes,
            () => !MostrarInicio && !MostrarReportes && Grupo.GrupoIdActual is not null && Reportes is not null);
        MostrarVistaMensualCommand = ModuloAsistencia.MostrarVistaMensualCommand;
        MostrarVistaDiariaCommand = ModuloAsistencia.MostrarVistaDiariaCommand;

        Grupo.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(GestionGrupoViewModel.GrupoIdActual))
            {
                IrAAsistenciaCommand.NotifyCanExecuteChanged();
                IrAProyectosCommand.NotifyCanExecuteChanged();
                IrAEvaluacionCommand.NotifyCanExecuteChanged();
                IrAReportesCommand.NotifyCanExecuteChanged();
                OnPropertyChanged(nameof(MostrarNavegacion));
                OnPropertyChanged(nameof(MostrarGrupo));
                OnPropertyChanged(nameof(TituloVentana));
            }

            if (args.PropertyName == nameof(GestionGrupoViewModel.EstaOcupado))
            {
                OnPropertyChanged(nameof(EstaOcupado));
                IrAInicioCommand.NotifyCanExecuteChanged();
                CrearGrupoDesdeInicioCommand.NotifyCanExecuteChanged();
            }
        };

        Asistencia.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(GestionAsistenciaViewModel.EstaOcupado))
            {
                OnPropertyChanged(nameof(EstaOcupado));
                IrAGrupoCommand.NotifyCanExecuteChanged();
                IrAInicioCommand.NotifyCanExecuteChanged();
                CrearGrupoDesdeInicioCommand.NotifyCanExecuteChanged();
            }
        };

        AsistenciaMensual.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(GestionAsistenciaMensualViewModel.EstaOcupado))
            {
                OnPropertyChanged(nameof(EstaOcupado));
                IrAInicioCommand.NotifyCanExecuteChanged();
                CrearGrupoDesdeInicioCommand.NotifyCanExecuteChanged();
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
                    IrAInicioCommand.NotifyCanExecuteChanged();
                    CrearGrupoDesdeInicioCommand.NotifyCanExecuteChanged();
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
                    IrAInicioCommand.NotifyCanExecuteChanged();
                    CrearGrupoDesdeInicioCommand.NotifyCanExecuteChanged();
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
    public GestionReportesViewModel? Reportes { get; }
    public bool ModoDemostracion { get; }

    public RelayCommand IrAInicioCommand { get; }
    public RelayCommand CrearGrupoDesdeInicioCommand { get; }
    public RelayCommand IrAGrupoCommand { get; }
    public RelayCommand IrAAsistenciaCommand { get; }
    public RelayCommand IrAProyectosCommand { get; }
    public RelayCommand IrAEvaluacionCommand { get; }
    public RelayCommand IrAReportesCommand { get; }
    public RelayCommand MostrarVistaMensualCommand { get; }
    public RelayCommand MostrarVistaDiariaCommand { get; }

    public bool MostrarVistaDiaria => ModuloAsistencia.MostrarDiaria;
    public bool MostrarVistaMensual => ModuloAsistencia.MostrarMensual;
    public bool MostrarAsistenciaDiaria => MostrarAsistencia && !MostrarInicio && !MostrarProyectos && !MostrarEvaluacion && !MostrarReportes && MostrarVistaDiaria;
    public bool MostrarAsistenciaMensual => MostrarAsistencia && !MostrarInicio && !MostrarProyectos && !MostrarEvaluacion && !MostrarReportes && MostrarVistaMensual;

    public bool MostrarInicio
    {
        get => _mostrarInicio;
        private set
        {
            if (SetProperty(ref _mostrarInicio, value))
            {
                OnPropertyChanged(nameof(MostrarGrupo));
                OnPropertyChanged(nameof(MostrarNavegacion));
                NotificarNavegacion();
            }
        }
    }

    public bool MostrarProyectos
    {
        get => _mostrarProyectos;
        private set
        {
            if (SetProperty(ref _mostrarProyectos, value))
            {
                NotificarNavegacion();
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
                NotificarNavegacion();
            }
        }
    }

    public bool MostrarReportes
    {
        get => _mostrarReportes;
        private set
        {
            if (SetProperty(ref _mostrarReportes, value))
            {
                NotificarNavegacion();
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
                NotificarNavegacion();
            }
        }
    }

    public bool MostrarGrupo => !MostrarInicio && !MostrarAsistencia && !MostrarProyectos && !MostrarEvaluacion && !MostrarReportes;

    public bool MostrarNavegacion => !MostrarInicio && Grupo.GrupoIdActual is not null;

    public string TituloVentana
    {
        get
        {
            var titulo = ModoDemostracion ? "Sistema Docente Local · DEMO" : "Sistema Docente Local";
            if (MostrarInicio) return titulo + " - Mis grupos";
            if (!string.IsNullOrWhiteSpace(Grupo.NombreGrupo)) titulo += " - " + Grupo.NombreGrupo;
            if (MostrarAsistenciaDiaria) titulo += " - Asistencia diaria";
            else if (MostrarAsistenciaMensual) titulo += " - Asistencia mensual";
            else if (MostrarProyectos) titulo += " - Proyectos";
            else if (MostrarEvaluacion) titulo += " - Evaluación";
            else if (MostrarReportes) titulo += " - Reportes";
            else if (MostrarGrupo) titulo += " - Grupo";
            return titulo;
        }
    }

    public bool CambiarGrupo(GrupoId grupoId)
    {
        if (!PuedeSalirDelModuloActual()) return false;

        Grupo.CargarGrupoPorId(grupoId);
        MostrarSoloGrupo();
        MostrarInicio = false;
        return true;
    }

    public void MostrarInicioGrupos()
    {
        if (!PuedeSalirDelModuloActual()) return;

        MostrarAsistencia = false;
        MostrarProyectos = false;
        MostrarEvaluacion = false;
        MostrarReportes = false;
        MostrarInicio = true;
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

    private void CrearGrupoDesdeInicio()
    {
        if (!PuedeSalirDelModuloActual()) return;

        Grupo.AbrirNuevoGrupoCommand.Execute(null);
        MostrarSoloGrupo();
        MostrarInicio = false;
    }

    private void IrAGrupo()
    {
        var puedeSalir = PuedeSalirDelModuloActual();
        if (puedeSalir)
        {
            MostrarSoloGrupo();
            MostrarInicio = false;
        }
    }

    private void IrAAsistencia()
    {
        if (Grupo.GrupoIdActual is not { } grupoId) return;
        if (!PuedeSalirDelModuloActual()) return;

        AsistenciaMensual.Inicializar(grupoId);
        Asistencia.Inicializar(grupoId);
        ModuloAsistencia.MostrarVistaMensual();
        MostrarInicio = false;
        MostrarProyectos = false;
        MostrarEvaluacion = false;
        MostrarReportes = false;
        MostrarAsistencia = true;
    }

    private void IrAProyectos()
    {
        if (Grupo.GrupoIdActual is not { } grupoId || Proyectos is null) return;
        if (!PuedeSalirDelModuloActual()) return;

        Proyectos.Inicializar(grupoId);
        MostrarInicio = false;
        MostrarAsistencia = false;
        MostrarEvaluacion = false;
        MostrarReportes = false;
        MostrarProyectos = true;
    }

    private void IrAEvaluacion()
    {
        if (Grupo.GrupoIdActual is not { } grupoId || Evaluacion is null) return;
        if (!PuedeSalirDelModuloActual()) return;

        Evaluacion.Inicializar(grupoId);
        MostrarInicio = false;
        MostrarAsistencia = false;
        MostrarProyectos = false;
        MostrarReportes = false;
        MostrarEvaluacion = true;
    }

    private void IrAReportes()
    {
        if (Grupo.GrupoIdActual is not { } grupoId || Reportes is null) return;
        if (!PuedeSalirDelModuloActual()) return;

        Reportes.Inicializar(grupoId);
        MostrarInicio = false;
        MostrarAsistencia = false;
        MostrarProyectos = false;
        MostrarEvaluacion = false;
        MostrarReportes = true;
    }

    private bool PuedeSalirDelModuloActual()
    {
        if (MostrarProyectos) return Proyectos?.SolicitarSalir() != false;
        if (MostrarEvaluacion) return Evaluacion?.SolicitarSalir() != false;
        if (MostrarAsistencia)
        {
            return MostrarVistaDiaria
                ? Asistencia.SolicitarNavegarAGrupo()
                : AsistenciaMensual.SolicitarSalir();
        }
        return true;
    }

    private void MostrarSoloGrupo()
    {
        MostrarAsistencia = false;
        MostrarProyectos = false;
        MostrarEvaluacion = false;
        MostrarReportes = false;
    }

    private void NotificarNavegacion()
    {
        OnPropertyChanged(nameof(MostrarGrupo));
        OnPropertyChanged(nameof(MostrarNavegacion));
        OnPropertyChanged(nameof(TituloVentana));
        OnPropertyChanged(nameof(MostrarAsistenciaDiaria));
        OnPropertyChanged(nameof(MostrarAsistenciaMensual));
        IrAGrupoCommand.NotifyCanExecuteChanged();
        IrAAsistenciaCommand.NotifyCanExecuteChanged();
        IrAProyectosCommand.NotifyCanExecuteChanged();
        IrAEvaluacionCommand.NotifyCanExecuteChanged();
        IrAReportesCommand.NotifyCanExecuteChanged();
    }
}
