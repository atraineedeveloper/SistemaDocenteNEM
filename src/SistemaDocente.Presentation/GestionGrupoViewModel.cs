using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Presentation;

public sealed class GestionGrupoViewModel : ViewModelBase
{
    private readonly IGestionGrupoPresentacion _gestion;
    private readonly IAlmacenamientoEstadoAplicacion _estadoAplicacion;
    private readonly IServicioMensajes _mensajes;
    private readonly IServicioConfirmacion _confirmacion;
    private GrupoDetalle? _grupoConfirmado;
    private bool _estaOcupado;
    private bool _mostrarBienvenida = true;
    private bool _mostrarGestion;
    private bool _puedeOlvidarReferencia;
    private PanelEdicion _panelActual;
    private string _nombreNuevoGrupo = string.Empty;
    private string _nombreGrupo = string.Empty;
    private string _nombreEdicionGrupo = string.Empty;
    private string _nombreEstudianteEdicion = string.Empty;
    private string _numeroListaEdicion = string.Empty;
    private string _mensajeEdicion = string.Empty;
    private IReadOnlyList<EstudianteVisual> _estudiantes = Array.Empty<EstudianteVisual>();
    private EstudianteVisual? _estudianteSeleccionado;

    public GestionGrupoViewModel(
        IGestionGrupoPresentacion gestion,
        IAlmacenamientoEstadoAplicacion estadoAplicacion,
        IServicioMensajes mensajes,
        IServicioConfirmacion confirmacion)
    {
        ArgumentNullException.ThrowIfNull(gestion);
        ArgumentNullException.ThrowIfNull(estadoAplicacion);
        ArgumentNullException.ThrowIfNull(mensajes);
        ArgumentNullException.ThrowIfNull(confirmacion);
        _gestion = gestion;
        _estadoAplicacion = estadoAplicacion;
        _mensajes = mensajes;
        _confirmacion = confirmacion;

        CrearGrupoCommand = new RelayCommand(CrearGrupo, PuedeCrearGrupo);
        AbrirCambioNombreCommand = new RelayCommand(AbrirCambioNombre, PuedeAdministrar);
        GuardarNombreGrupoCommand = new RelayCommand(GuardarNombreGrupo, PuedeGuardarEdicion);
        AbrirAgregarEstudianteCommand = new RelayCommand(AbrirAgregarEstudiante, PuedeAdministrar);
        AbrirEditarEstudianteCommand = new RelayCommand(AbrirEditarEstudiante, PuedeEditarSeleccionado);
        GuardarEstudianteCommand = new RelayCommand(GuardarEstudiante, PuedeGuardarEdicion);
        CancelarEdicionCommand = new RelayCommand(CancelarEdicion, () => !EstaOcupado && PanelActual != PanelEdicion.Ninguno);
        DesactivarEstudianteCommand = new RelayCommand(DesactivarEstudiante, PuedeDesactivar);
        ReactivarEstudianteCommand = new RelayCommand(ReactivarEstudiante, PuedeReactivar);
        OlvidarReferenciaCommand = new RelayCommand(OlvidarReferencia, () => !EstaOcupado && PuedeOlvidarReferencia);
    }

    public RelayCommand CrearGrupoCommand { get; }
    public RelayCommand AbrirCambioNombreCommand { get; }
    public RelayCommand GuardarNombreGrupoCommand { get; }
    public RelayCommand AbrirAgregarEstudianteCommand { get; }
    public RelayCommand AbrirEditarEstudianteCommand { get; }
    public RelayCommand GuardarEstudianteCommand { get; }
    public RelayCommand CancelarEdicionCommand { get; }
    public RelayCommand DesactivarEstudianteCommand { get; }
    public RelayCommand ReactivarEstudianteCommand { get; }
    public RelayCommand OlvidarReferenciaCommand { get; }

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

    public bool MostrarBienvenida
    {
        get => _mostrarBienvenida;
        private set => SetProperty(ref _mostrarBienvenida, value);
    }

    public bool MostrarGestion
    {
        get => _mostrarGestion;
        private set => SetProperty(ref _mostrarGestion, value);
    }

    public bool PuedeOlvidarReferencia
    {
        get => _puedeOlvidarReferencia;
        private set
        {
            if (SetProperty(ref _puedeOlvidarReferencia, value))
            {
                OlvidarReferenciaCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public PanelEdicion PanelActual
    {
        get => _panelActual;
        private set
        {
            if (SetProperty(ref _panelActual, value))
            {
                OnPropertyChanged(nameof(MostrarEditorGrupo));
                OnPropertyChanged(nameof(MostrarEditorEstudiante));
                NotificarComandos();
            }
        }
    }

    public bool MostrarEditorGrupo => PanelActual == PanelEdicion.NombreGrupo;
    public bool MostrarEditorEstudiante =>
        PanelActual is PanelEdicion.AgregarEstudiante or PanelEdicion.EditarEstudiante;

    public string TituloEditorEstudiante =>
        PanelActual == PanelEdicion.AgregarEstudiante ? "Agregar estudiante" : "Editar estudiante";

    public string NombreNuevoGrupo
    {
        get => _nombreNuevoGrupo;
        set
        {
            if (SetProperty(ref _nombreNuevoGrupo, value))
            {
                CrearGrupoCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public string NombreGrupo
    {
        get => _nombreGrupo;
        private set => SetProperty(ref _nombreGrupo, value);
    }

    public string NombreEdicionGrupo
    {
        get => _nombreEdicionGrupo;
        set => SetProperty(ref _nombreEdicionGrupo, value);
    }

    public string NombreEstudianteEdicion
    {
        get => _nombreEstudianteEdicion;
        set => SetProperty(ref _nombreEstudianteEdicion, value);
    }

    public string NumeroListaEdicion
    {
        get => _numeroListaEdicion;
        set => SetProperty(ref _numeroListaEdicion, value);
    }

    public string MensajeEdicion
    {
        get => _mensajeEdicion;
        private set => SetProperty(ref _mensajeEdicion, value);
    }

    public IReadOnlyList<EstudianteVisual> Estudiantes
    {
        get => _estudiantes;
        private set => SetProperty(ref _estudiantes, value);
    }

    public EstudianteVisual? EstudianteSeleccionado
    {
        get => _estudianteSeleccionado;
        set
        {
            if (SetProperty(ref _estudianteSeleccionado, value))
            {
                NotificarComandos();
            }
        }
    }

    public void Inicializar()
    {
        EjecutarOcupado(() =>
        {
            var referencia = _estadoAplicacion.Cargar();
            if (referencia.Estado == EstadoLecturaReferencia.Ausente)
            {
                MostrarBienvenidaSegura();
                return;
            }

            if (referencia.Estado == EstadoLecturaReferencia.Invalida || referencia.GrupoId is null)
            {
                _mensajes.MostrarError("No fue posible leer la configuración local. Puedes crear tu grupo nuevamente.");
                MostrarBienvenidaSegura();
                return;
            }

            try
            {
                AplicarGrupoConfirmado(_gestion.CargarGrupo(referencia.GrupoId.Value));
            }
            catch (GrupoNoEncontradoException)
            {
                _mensajes.MostrarError("El grupo configurado ya no existe. Puedes olvidar esta referencia.");
                MostrarBienvenidaSegura();
                PuedeOlvidarReferencia = true;
            }
        });
    }

    private void CrearGrupo()
    {
        EjecutarEdicion(() =>
        {
            var grupo = _gestion.CrearGrupo(NombreNuevoGrupo);
            _estadoAplicacion.Guardar(grupo.GrupoId);
            AplicarGrupoConfirmado(grupo);
            NombreNuevoGrupo = string.Empty;
        });
    }

    private void AbrirCambioNombre()
    {
        NombreEdicionGrupo = NombreGrupo;
        MensajeEdicion = string.Empty;
        PanelActual = PanelEdicion.NombreGrupo;
    }

    private void GuardarNombreGrupo()
    {
        if (_grupoConfirmado is null)
        {
            return;
        }

        EjecutarEdicion(() =>
        {
            var grupo = _gestion.CambiarNombreGrupo(_grupoConfirmado.GrupoId, NombreEdicionGrupo);
            AplicarGrupoConfirmado(grupo);
        });
    }

    private void AbrirAgregarEstudiante()
    {
        NombreEstudianteEdicion = string.Empty;
        NumeroListaEdicion = string.Empty;
        MensajeEdicion = string.Empty;
        PanelActual = PanelEdicion.AgregarEstudiante;
        OnPropertyChanged(nameof(TituloEditorEstudiante));
    }

    private void AbrirEditarEstudiante()
    {
        if (EstudianteSeleccionado is null)
        {
            return;
        }

        NombreEstudianteEdicion = EstudianteSeleccionado.Nombre;
        NumeroListaEdicion = EstudianteSeleccionado.NumeroLista.ToString(System.Globalization.CultureInfo.CurrentCulture);
        MensajeEdicion = string.Empty;
        PanelActual = PanelEdicion.EditarEstudiante;
        OnPropertyChanged(nameof(TituloEditorEstudiante));
    }

    private void GuardarEstudiante()
    {
        if (_grupoConfirmado is null)
        {
            return;
        }

        if (!int.TryParse(NumeroListaEdicion, out var numeroLista))
        {
            MensajeEdicion = "Escribe un número de lista entero mayor que cero.";
            return;
        }

        EjecutarEdicion(() =>
        {
            if (PanelActual == PanelEdicion.AgregarEstudiante)
            {
                _gestion.AgregarEstudiante(_grupoConfirmado.GrupoId, NombreEstudianteEdicion, numeroLista);
            }
            else if (EstudianteSeleccionado is not null)
            {
                _gestion.EditarEstudiante(
                    _grupoConfirmado.GrupoId,
                    EstudianteSeleccionado.Id,
                    NombreEstudianteEdicion,
                    numeroLista);
            }

            RefrescarGrupo();
        });
    }

    private void CancelarEdicion()
    {
        PanelActual = PanelEdicion.Ninguno;
        MensajeEdicion = string.Empty;
    }

    private void DesactivarEstudiante()
    {
        if (_grupoConfirmado is null || EstudianteSeleccionado is null
            || !_confirmacion.ConfirmarDesactivacion(EstudianteSeleccionado.Nombre))
        {
            return;
        }

        EjecutarGeneral(() =>
        {
            _gestion.DesactivarEstudiante(_grupoConfirmado.GrupoId, EstudianteSeleccionado.Id);
            RefrescarGrupo();
        });
    }

    private void ReactivarEstudiante()
    {
        if (_grupoConfirmado is null || EstudianteSeleccionado is null)
        {
            return;
        }

        EjecutarGeneral(() =>
        {
            _gestion.ReactivarEstudiante(_grupoConfirmado.GrupoId, EstudianteSeleccionado.Id);
            RefrescarGrupo();
        });
    }

    private void OlvidarReferencia()
    {
        EjecutarGeneral(() =>
        {
            _estadoAplicacion.Olvidar();
            MostrarBienvenidaSegura();
        });
    }

    private void RefrescarGrupo()
    {
        if (_grupoConfirmado is not null)
        {
            AplicarGrupoConfirmado(_gestion.CargarGrupo(_grupoConfirmado.GrupoId));
        }
    }

    private void AplicarGrupoConfirmado(GrupoDetalle grupo)
    {
        _grupoConfirmado = grupo;
        NombreGrupo = grupo.NombreVisible;
        Estudiantes = grupo.Estudiantes.Select(Proyectar).ToArray();
        EstudianteSeleccionado = null;
        PanelActual = PanelEdicion.Ninguno;
        MensajeEdicion = string.Empty;
        PuedeOlvidarReferencia = false;
        MostrarBienvenida = false;
        MostrarGestion = true;
    }

    private void MostrarBienvenidaSegura()
    {
        _grupoConfirmado = null;
        NombreGrupo = string.Empty;
        Estudiantes = Array.Empty<EstudianteVisual>();
        EstudianteSeleccionado = null;
        PanelActual = PanelEdicion.Ninguno;
        MostrarGestion = false;
        MostrarBienvenida = true;
    }

    private void EjecutarEdicion(Action action)
    {
        MensajeEdicion = string.Empty;
        EjecutarOcupado(action, true);
    }

    private void EjecutarGeneral(Action action) => EjecutarOcupado(action);

    private void EjecutarOcupado(Action action, bool mostrarEnEdicion = false)
    {
        if (EstaOcupado)
        {
            return;
        }

        EstaOcupado = true;
        try
        {
            action();
        }
        catch (DomainValidationException exception)
        {
            MostrarErrorCorregible(exception.Message, mostrarEnEdicion);
        }
        catch (DomainConflictException exception)
        {
            MostrarErrorCorregible(exception.Message, mostrarEnEdicion);
        }
        catch (GrupoNoEncontradoException)
        {
            _mensajes.MostrarError("El grupo ya no existe. Puedes olvidar la referencia local.");
            PuedeOlvidarReferencia = true;
        }
        catch (ErrorPersistenciaAplicacionException)
        {
            _mensajes.MostrarError("No fue posible guardar o cargar la información. Intenta nuevamente.");
        }
        catch (IOException)
        {
            _mensajes.MostrarError("No fue posible guardar o cargar la configuración local.");
        }
        catch (UnauthorizedAccessException)
        {
            _mensajes.MostrarError("No fue posible guardar o cargar la configuración local.");
        }
        finally
        {
            EstaOcupado = false;
        }
    }

    private void MostrarErrorCorregible(string mensaje, bool mostrarEnEdicion)
    {
        if (mostrarEnEdicion)
        {
            MensajeEdicion = mensaje;
        }
        else
        {
            _mensajes.MostrarError(mensaje);
        }
    }

    private bool PuedeCrearGrupo() => !EstaOcupado && MostrarBienvenida;
    private bool PuedeAdministrar() => !EstaOcupado && MostrarGestion && PanelActual == PanelEdicion.Ninguno;
    private bool PuedeGuardarEdicion() => !EstaOcupado && PanelActual != PanelEdicion.Ninguno;
    private bool PuedeEditarSeleccionado() => PuedeAdministrar() && EstudianteSeleccionado is not null;
    private bool PuedeDesactivar() => PuedeAdministrar() && EstudianteSeleccionado?.EstaActivo == true;
    private bool PuedeReactivar() => PuedeAdministrar() && EstudianteSeleccionado?.EstaActivo == false;

    private void NotificarComandos()
    {
        CrearGrupoCommand.NotifyCanExecuteChanged();
        AbrirCambioNombreCommand.NotifyCanExecuteChanged();
        GuardarNombreGrupoCommand.NotifyCanExecuteChanged();
        AbrirAgregarEstudianteCommand.NotifyCanExecuteChanged();
        AbrirEditarEstudianteCommand.NotifyCanExecuteChanged();
        GuardarEstudianteCommand.NotifyCanExecuteChanged();
        CancelarEdicionCommand.NotifyCanExecuteChanged();
        DesactivarEstudianteCommand.NotifyCanExecuteChanged();
        ReactivarEstudianteCommand.NotifyCanExecuteChanged();
        OlvidarReferenciaCommand.NotifyCanExecuteChanged();
    }

    private static EstudianteVisual Proyectar(EstudianteDetalle estudiante) =>
        new(estudiante.EstudianteId, estudiante.NombreVisible, estudiante.NumeroLista, estudiante.EstaActivo);
}