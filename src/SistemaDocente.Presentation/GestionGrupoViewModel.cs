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
    private string _filtroBusqueda = string.Empty;
    private GradoPrimaria _gradoEdicion;
    private IReadOnlyList<OpcionGradoPrimaria> _gradosDisponiblesEdicion = CrearOpcionesGrado(CatalogoNemPrimaria.TodosLosGrados);

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
        AbrirNuevoGrupoCommand = new RelayCommand(AbrirNuevoGrupo, () => !EstaOcupado);
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
    public RelayCommand AbrirNuevoGrupoCommand { get; }
    public RelayCommand AbrirCambioNombreCommand { get; }
    public RelayCommand GuardarNombreGrupoCommand { get; }
    public RelayCommand AbrirAgregarEstudianteCommand { get; }
    public RelayCommand AbrirEditarEstudianteCommand { get; }
    public RelayCommand GuardarEstudianteCommand { get; }
    public RelayCommand CancelarEdicionCommand { get; }
    public RelayCommand DesactivarEstudianteCommand { get; }
    public RelayCommand ReactivarEstudianteCommand { get; }
    public RelayCommand OlvidarReferenciaCommand { get; }

    public GrupoId? GrupoIdActual => _grupoConfirmado?.GrupoId;

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

    private string _primerApellidoEdicion = string.Empty;
    public string PrimerApellidoEdicion
    {
        get => _primerApellidoEdicion;
        set => SetProperty(ref _primerApellidoEdicion, value);
    }

    private string _segundoApellidoEdicion = string.Empty;
    public string SegundoApellidoEdicion
    {
        get => _segundoApellidoEdicion;
        set => SetProperty(ref _segundoApellidoEdicion, value);
    }

    private string _nombresEdicion = string.Empty;
    public string NombresEdicion
    {
        get => _nombresEdicion;
        set => SetProperty(ref _nombresEdicion, value);
    }

    private DateTime? _fechaNacimientoEdicion;
    public DateTime? FechaNacimientoEdicion
    {
        get => _fechaNacimientoEdicion;
        set => SetProperty(ref _fechaNacimientoEdicion, value);
    }

    private int _generoIndexEdicion;
    public int GeneroIndexEdicion
    {
        get => _generoIndexEdicion;
        set => SetProperty(ref _generoIndexEdicion, value);
    }

    private DateTime? _fechaIngresoEdicion;
    public DateTime? FechaIngresoEdicion
    {
        get => _fechaIngresoEdicion;
        set => SetProperty(ref _fechaIngresoEdicion, value);
    }

    private string _observacionesEdicion = string.Empty;
    public string ObservacionesEdicion
    {
        get => _observacionesEdicion;
        set => SetProperty(ref _observacionesEdicion, value);
    }

    public GradoPrimaria GradoEdicion
    {
        get => _gradoEdicion;
        set => SetProperty(ref _gradoEdicion, value);
    }

    public IReadOnlyList<OpcionGradoPrimaria> GradosDisponiblesEdicion
    {
        get => _gradosDisponiblesEdicion;
        private set => SetProperty(ref _gradosDisponiblesEdicion, value);
    }

    public string MensajeEdicion
    {
        get => _mensajeEdicion;
        private set => SetProperty(ref _mensajeEdicion, value);
    }

    public IReadOnlyList<EstudianteVisual> Estudiantes
    {
        get => _estudiantes;
        private set { if (SetProperty(ref _estudiantes, value)) OnPropertyChanged(nameof(EstudiantesFiltrados)); }
    }

    public string FiltroBusqueda
    {
        get => _filtroBusqueda;
        set { if (SetProperty(ref _filtroBusqueda, value)) OnPropertyChanged(nameof(EstudiantesFiltrados)); }
    }

    public IEnumerable<EstudianteVisual> EstudiantesFiltrados => string.IsNullOrWhiteSpace(_filtroBusqueda)
        ? _estudiantes
        : _estudiantes.Where(e =>
            e.Nombre.Contains(_filtroBusqueda, StringComparison.CurrentCultureIgnoreCase)
            || e.NumeroLista.ToString(System.Globalization.CultureInfo.CurrentCulture)
                .Contains(_filtroBusqueda, StringComparison.Ordinal)
            || e.GradoTexto.Contains(_filtroBusqueda, StringComparison.CurrentCultureIgnoreCase));

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

    private IReadOnlyList<GrupoDetalle> _gruposDisponibles = Array.Empty<GrupoDetalle>();
    private GrupoDetalle? _grupoSeleccionadoCombo;

    public IReadOnlyList<GrupoDetalle> GruposDisponibles
    {
        get => _gruposDisponibles;
        private set => SetProperty(ref _gruposDisponibles, value);
    }

    public GrupoDetalle? GrupoSeleccionadoCombo
    {
        get => _grupoSeleccionadoCombo;
        set
        {
            if (SetProperty(ref _grupoSeleccionadoCombo, value) && value is not null && value.GrupoId != GrupoIdActual)
            {
                CargarGrupoPorId(value.GrupoId);
            }
        }
    }

    public void Inicializar()
    {
        EjecutarOcupado(() =>
        {
            ActualizarListaGrupos();
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
                CargarGrupoPorId(referencia.GrupoId.Value);
            }
            catch (GrupoNoEncontradoException)
            {
                _mensajes.MostrarError("El grupo configurado ya no existe. Puedes olvidar esta referencia.");
                MostrarBienvenidaSegura();
                PuedeOlvidarReferencia = true;
            }
        });
    }

    public void CargarGrupoPorId(GrupoId id)
    {
        var grupo = _gestion.CargarGrupo(id);
        _estadoAplicacion.Guardar(grupo.GrupoId);
        AplicarGrupoConfirmado(grupo);
        _grupoSeleccionadoCombo = GruposDisponibles.FirstOrDefault(g => g.GrupoId == id);
        OnPropertyChanged(nameof(GrupoSeleccionadoCombo));
    }

    public void ConfigurarGradosDisponibles(IEnumerable<GradoPrimaria>? gradosConfigurados)
    {
        var grados = CatalogoNemPrimaria.NormalizarGrados(gradosConfigurados);
        if (grados.Count == 0)
        {
            grados = CatalogoNemPrimaria.TodosLosGrados;
        }

        if (PanelActual == PanelEdicion.EditarEstudiante
            && CatalogoNemPrimaria.EsGradoReal(GradoEdicion)
            && !grados.Contains(GradoEdicion))
        {
            grados = grados.Append(GradoEdicion).Distinct().OrderBy(g => (int)g).ToArray();
        }

        GradosDisponiblesEdicion = CrearOpcionesGrado(grados);

        if (PanelActual == PanelEdicion.AgregarEstudiante && grados.Count == 1)
        {
            GradoEdicion = grados[0];
        }
        else if (!grados.Contains(GradoEdicion))
        {
            GradoEdicion = GradoPrimaria.NoEspecificado;
        }
    }

    private void ActualizarListaGrupos()
    {
        GruposDisponibles = _gestion.ListarGrupos();
    }

    private void CrearGrupo()
    {
        EjecutarEdicion(() =>
        {
            var grupo = _gestion.CrearGrupo(NombreNuevoGrupo);
            ActualizarListaGrupos();
            CargarGrupoPorId(grupo.GrupoId);
            NombreNuevoGrupo = string.Empty;
        });
    }

    private void AbrirNuevoGrupo()
    {
        NombreNuevoGrupo = string.Empty;
        MensajeEdicion = string.Empty;
        MostrarBienvenidaSegura();
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
        PrimerApellidoEdicion = string.Empty;
        SegundoApellidoEdicion = string.Empty;
        NombresEdicion = string.Empty;
        NumeroListaEdicion = string.Empty;
        FechaNacimientoEdicion = null;
        GeneroIndexEdicion = 0;
        FechaIngresoEdicion = null;
        ObservacionesEdicion = string.Empty;
        GradoEdicion = GradoPrimaria.NoEspecificado;
        GradosDisponiblesEdicion = CrearOpcionesGrado(CatalogoNemPrimaria.TodosLosGrados);
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
        PrimerApellidoEdicion = EstudianteSeleccionado.PrimerApellido;
        SegundoApellidoEdicion = EstudianteSeleccionado.SegundoApellido;
        NombresEdicion = EstudianteSeleccionado.Nombres;
        NumeroListaEdicion = EstudianteSeleccionado.NumeroLista.ToString(System.Globalization.CultureInfo.CurrentCulture);
        FechaNacimientoEdicion = EstudianteSeleccionado.FechaNacimiento.HasValue
            ? EstudianteSeleccionado.FechaNacimiento.Value.ToDateTime(TimeOnly.MinValue)
            : null;
        GeneroIndexEdicion = (int)EstudianteSeleccionado.Genero;
        FechaIngresoEdicion = EstudianteSeleccionado.FechaIngreso.HasValue
            ? EstudianteSeleccionado.FechaIngreso.Value.ToDateTime(TimeOnly.MinValue)
            : null;
        ObservacionesEdicion = EstudianteSeleccionado.Observaciones;
        GradoEdicion = EstudianteSeleccionado.Grado;
        GradosDisponiblesEdicion = CrearOpcionesGrado(CatalogoNemPrimaria.TodosLosGrados);
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

        if (!CatalogoNemPrimaria.EsGradoReal(GradoEdicion))
        {
            MensajeEdicion = "Selecciona el grado de primaria del estudiante.";
            return;
        }

        DateOnly? fechaNac = FechaNacimientoEdicion.HasValue ? DateOnly.FromDateTime(FechaNacimientoEdicion.Value) : null;
        DateOnly? fechaIng = FechaIngresoEdicion.HasValue ? DateOnly.FromDateTime(FechaIngresoEdicion.Value) : null;
        var genero = (GeneroEstudiante)GeneroIndexEdicion;

        EjecutarEdicion(() =>
        {
            if (PanelActual == PanelEdicion.AgregarEstudiante)
            {
                _gestion.AgregarEstudianteConGrado(
                    _grupoConfirmado.GrupoId,
                    NombreEstudianteEdicion,
                    numeroLista,
                    PrimerApellidoEdicion,
                    SegundoApellidoEdicion,
                    NombresEdicion,
                    fechaNac,
                    genero,
                    fechaIng,
                    ObservacionesEdicion,
                    GradoEdicion);
            }
            else if (EstudianteSeleccionado is not null)
            {
                _gestion.EditarEstudianteConGrado(
                    _grupoConfirmado.GrupoId,
                    EstudianteSeleccionado.Id,
                    NombreEstudianteEdicion,
                    numeroLista,
                    PrimerApellidoEdicion,
                    SegundoApellidoEdicion,
                    NombresEdicion,
                    fechaNac,
                    genero,
                    fechaIng,
                    ObservacionesEdicion,
                    GradoEdicion);
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
        OnPropertyChanged(nameof(GrupoIdActual));
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
        OnPropertyChanged(nameof(GrupoIdActual));
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
        new(
            estudiante.EstudianteId,
            estudiante.NombreVisible,
            estudiante.PrimerApellido,
            estudiante.SegundoApellido,
            estudiante.Nombres,
            estudiante.FechaNacimiento,
            estudiante.Edad,
            estudiante.Genero,
            estudiante.FechaIngreso,
            estudiante.Observaciones,
            estudiante.NumeroLista,
            estudiante.EstaActivo,
            estudiante.Grado);

    private static OpcionGradoPrimaria[] CrearOpcionesGrado(IEnumerable<GradoPrimaria> grados) =>
        CatalogoNemPrimaria.NormalizarGrados(grados)
            .Select(grado => new OpcionGradoPrimaria(grado, CatalogoNemPrimaria.FormatearGrado(grado)))
            .ToArray();
}