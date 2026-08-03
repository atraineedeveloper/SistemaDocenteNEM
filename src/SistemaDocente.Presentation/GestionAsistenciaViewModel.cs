using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Presentation;

public sealed class GestionAsistenciaViewModel : ViewModelBase
{
    private readonly IGestionAsistenciaPresentacion _gestion;
    private readonly IRelojLocal _reloj;
    private readonly IDialogoCambiosPendientes _dialogo;
    private readonly IServicioMensajes _mensajes;
    private GrupoId? _grupoId;
    private AsistenciaDiaDetalle? _confirmado;
    private DateTime? _fechaSeleccionada;
    private IReadOnlyList<AsistenciaEstudianteVisual> _estudiantes = [];
    private bool _esPersistido;
    private bool _estaOcupado;

    public GestionAsistenciaViewModel(
        IGestionAsistenciaPresentacion gestion,
        IRelojLocal reloj,
        IDialogoCambiosPendientes dialogo,
        IServicioMensajes mensajes)
    {
        ArgumentNullException.ThrowIfNull(gestion);
        ArgumentNullException.ThrowIfNull(reloj);
        ArgumentNullException.ThrowIfNull(dialogo);
        ArgumentNullException.ThrowIfNull(mensajes);
        _gestion = gestion;
        _reloj = reloj;
        _dialogo = dialogo;
        _mensajes = mensajes;
        GuardarCommand = new RelayCommand(() => Guardar(), PuedeGuardar);
        MarcarTodosPresentesCommand = new RelayCommand(MarcarTodosPresentes, () => !EstaOcupado && Estudiantes.Count > 0);
    }

    public RelayCommand GuardarCommand { get; }

    public RelayCommand MarcarTodosPresentesCommand { get; }

    public DateTime? FechaSeleccionada
    {
        get => _fechaSeleccionada;
        set
        {
            if (value is null)
            {
                _mensajes.MostrarError("Selecciona una fecha válida.");
                OnPropertyChanged();
                return;
            }

            var fechaNueva = DateOnly.FromDateTime(value.Value);
            if (_fechaSeleccionada is not null
                && DateOnly.FromDateTime(_fechaSeleccionada.Value) == fechaNueva)
            {
                return;
            }

            if (_fechaSeleccionada is not null && !ConfirmarSalidaPendiente())
            {
                OnPropertyChanged();
                return;
            }

            if (SetProperty(ref _fechaSeleccionada, value.Value.Date))
            {
                CargarFecha(fechaNueva);
            }
        }
    }

    public IReadOnlyList<AsistenciaEstudianteVisual> Estudiantes
    {
        get => _estudiantes;
        private set => SetProperty(ref _estudiantes, value);
    }

    public bool EsPersistido
    {
        get => _esPersistido;
        private set
        {
            if (SetProperty(ref _esPersistido, value))
            {
                NotificarEstado();
            }
        }
    }

    public bool EstaOcupado
    {
        get => _estaOcupado;
        private set
        {
            if (SetProperty(ref _estaOcupado, value))
            {
                NotificarComandos();
            }
        }
    }

    public bool TieneCambios => !EsPersistido || HayDiferenciasConConfirmado();

    public string EstadoGuardado => !EsPersistido
        ? "Sin guardar"
        : TieneCambios ? "Cambios sin guardar" : "Guardado";

    public int Total => Estudiantes.Count;

    public int Presentes => Contar(EstadoAsistencia.Presente);

    public int Faltas => Contar(EstadoAsistencia.Falta);

    public int Retardos => Contar(EstadoAsistencia.Retardo);

    public int Justificadas => Contar(EstadoAsistencia.Justificada);

    public void Inicializar(GrupoId grupoId)
    {
        _grupoId = grupoId;
        _fechaSeleccionada = _reloj.Hoy.ToDateTime(TimeOnly.MinValue);
        OnPropertyChanged(nameof(FechaSeleccionada));
        CargarFecha(_reloj.Hoy);
    }

    public bool SolicitarNavegarAGrupo() => ConfirmarSalidaPendiente();

    public bool SolicitarCerrar() => ConfirmarSalidaPendiente();

    private void CargarFecha(DateOnly fecha)
    {
        if (_grupoId is null)
        {
            return;
        }

        Ejecutar(() => Aplicar(_gestion.Preparar(_grupoId.Value, fecha)));
    }

    private bool Guardar()
    {
        if (_grupoId is null || _fechaSeleccionada is null || !PuedeGuardar())
        {
            return false;
        }

        var exito = false;
        Ejecutar(() =>
        {
            var resultado = _gestion.Guardar(
                _grupoId.Value,
                DateOnly.FromDateTime(_fechaSeleccionada.Value),
                Estudiantes.Select(x => new EntradaEstadoAsistencia(
                    x.EstudianteId,
                    x.Estado)).ToArray());
            Aplicar(resultado);
            exito = true;
        });
        return exito;
    }

    private void MarcarTodosPresentes()
    {
        foreach (var estudiante in Estudiantes)
        {
            estudiante.Estado = EstadoAsistencia.Presente;
        }

        NotificarEstado();
    }

    private bool ConfirmarSalidaPendiente()
    {
        if (!TieneCambios)
        {
            return true;
        }

        return _dialogo.ConfirmarCambiosPendientes() switch
        {
            DecisionCambiosPendientes.Guardar => Guardar(),
            DecisionCambiosPendientes.Descartar => true,
            _ => false,
        };
    }

    private void Aplicar(AsistenciaDiaDetalle detalle)
    {
        _confirmado = detalle;
        Estudiantes = detalle.Estudiantes.Select(x => new AsistenciaEstudianteVisual(
            x.EstudianteId,
            x.NombreVisible,
            x.NumeroLista,
            x.Estado,
            x.EstaActivoActualmente,
            NotificarEstado)).ToArray();
        EsPersistido = detalle.EsPersistido;
        NotificarEstado();
    }

    private void Ejecutar(Action accion)
    {
        if (EstaOcupado)
        {
            return;
        }

        EstaOcupado = true;
        try
        {
            accion();
        }
        catch (DomainValidationException exception)
        {
            _mensajes.MostrarError(exception.Message);
        }
        catch (DomainConflictException exception)
        {
            _mensajes.MostrarError(exception.Message);
        }
        catch (GrupoNoEncontradoException)
        {
            _mensajes.MostrarError("El grupo ya no existe.");
        }
        catch (ErrorPersistenciaAplicacionException)
        {
            _mensajes.MostrarError("No fue posible guardar o cargar la asistencia. Intenta nuevamente.");
        }
        finally
        {
            EstaOcupado = false;
        }
    }

    private bool HayDiferenciasConConfirmado()
    {
        if (_confirmado is null || _confirmado.Estudiantes.Count != Estudiantes.Count)
        {
            return true;
        }

        return _confirmado.Estudiantes.Zip(Estudiantes).Any(
            par => par.First.EstudianteId != par.Second.EstudianteId
                || par.First.Estado != par.Second.Estado);
    }

    private bool PuedeGuardar() => !EstaOcupado && TieneCambios && _grupoId is not null;

    private int Contar(EstadoAsistencia estado) => Estudiantes.Count(x => x.Estado == estado);

    private void NotificarEstado()
    {
        OnPropertyChanged(nameof(TieneCambios));
        OnPropertyChanged(nameof(EstadoGuardado));
        OnPropertyChanged(nameof(Total));
        OnPropertyChanged(nameof(Presentes));
        OnPropertyChanged(nameof(Faltas));
        OnPropertyChanged(nameof(Retardos));
        OnPropertyChanged(nameof(Justificadas));
        NotificarComandos();
    }

    private void NotificarComandos()
    {
        GuardarCommand.NotifyCanExecuteChanged();
        MarcarTodosPresentesCommand.NotifyCanExecuteChanged();
    }
}