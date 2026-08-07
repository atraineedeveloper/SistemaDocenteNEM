namespace SistemaDocente.Presentation;

/// <summary>
/// Frontera de presentación del módulo de asistencia. Agrupa las vistas diaria y mensual
/// sin acoplar la vista WPF al MainWindowViewModel completo.
/// </summary>
public sealed class ModuloAsistenciaViewModel : ViewModelBase
{
    private bool _mostrarDiaria;

    public ModuloAsistenciaViewModel(
        GestionAsistenciaViewModel diaria,
        GestionAsistenciaMensualViewModel mensual)
    {
        ArgumentNullException.ThrowIfNull(diaria);
        ArgumentNullException.ThrowIfNull(mensual);

        Diaria = diaria;
        Mensual = mensual;
        MostrarVistaDiariaCommand = new RelayCommand(MostrarVistaDiaria);
        MostrarVistaMensualCommand = new RelayCommand(MostrarVistaMensual);
    }

    public GestionAsistenciaViewModel Diaria { get; }
    public GestionAsistenciaMensualViewModel Mensual { get; }

    public RelayCommand MostrarVistaDiariaCommand { get; }
    public RelayCommand MostrarVistaMensualCommand { get; }

    public bool MostrarDiaria
    {
        get => _mostrarDiaria;
        private set
        {
            if (SetProperty(ref _mostrarDiaria, value))
            {
                OnPropertyChanged(nameof(MostrarMensual));
            }
        }
    }

    public bool MostrarMensual => !MostrarDiaria;

    public void MostrarVistaDiaria() => MostrarDiaria = true;

    public void MostrarVistaMensual() => MostrarDiaria = false;
}