using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;

using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Presentation;

public enum PasoImportacionEstudiantes
{
    Archivo = 0,
    Columnas = 1,
    Previa = 2,
    Confirmacion = 3,
    Resultado = 4,
}

public sealed record OpcionCampoImportacion(
    CampoImportacionEstudiante Campo,
    string Etiqueta);

public sealed record OpcionDelimitadorCsv(
    char Delimitador,
    string Etiqueta);

public sealed class ColumnaImportacionVisual : ViewModelBase
{
    private CampoImportacionEstudiante _campo;

    public ColumnaImportacionVisual(
        int indice,
        string encabezado,
        CampoImportacionEstudiante campo)
    {
        Indice = indice;
        Encabezado = encabezado;
        _campo = campo;
    }

    public int Indice { get; }

    public string Encabezado { get; }

    public CampoImportacionEstudiante Campo
    {
        get => _campo;
        set => SetProperty(ref _campo, value);
    }
}

public sealed class FilaImportacionVisual : ViewModelBase
{
    private string _numeroListaTexto = string.Empty;
    private string _nombreCompleto = string.Empty;
    private string _primerApellido = string.Empty;
    private string _segundoApellido = string.Empty;
    private string _nombres = string.Empty;
    private string _fechaNacimientoTexto = string.Empty;
    private string _generoTexto = string.Empty;
    private string _fechaIngresoTexto = string.Empty;
    private string _gradoTexto = string.Empty;
    private string _observaciones = string.Empty;
    private bool _excluida;
    private bool _importarDuplicadoComoNuevo;
    private EstadoFilaImportacion _estado;
    private string _resumenProblemas = string.Empty;
    private string _gradoResuelto = string.Empty;

    public FilaImportacionVisual(FilaImportacionEstudiante fila)
    {
        NumeroOrigen = fila.NumeroOrigen;
        Aplicar(fila);
    }

    public int NumeroOrigen { get; }

    public string NumeroListaTexto
    {
        get => _numeroListaTexto;
        set => SetProperty(ref _numeroListaTexto, value);
    }

    public string NombreCompleto
    {
        get => _nombreCompleto;
        set => SetProperty(ref _nombreCompleto, value);
    }

    public string PrimerApellido
    {
        get => _primerApellido;
        set => SetProperty(ref _primerApellido, value);
    }

    public string SegundoApellido
    {
        get => _segundoApellido;
        set => SetProperty(ref _segundoApellido, value);
    }

    public string Nombres
    {
        get => _nombres;
        set => SetProperty(ref _nombres, value);
    }

    public string FechaNacimientoTexto
    {
        get => _fechaNacimientoTexto;
        set => SetProperty(ref _fechaNacimientoTexto, value);
    }

    public string GeneroTexto
    {
        get => _generoTexto;
        set => SetProperty(ref _generoTexto, value);
    }

    public string FechaIngresoTexto
    {
        get => _fechaIngresoTexto;
        set => SetProperty(ref _fechaIngresoTexto, value);
    }

    public string GradoTexto
    {
        get => _gradoTexto;
        set => SetProperty(ref _gradoTexto, value);
    }

    public string Observaciones
    {
        get => _observaciones;
        set => SetProperty(ref _observaciones, value);
    }

    public bool Excluida
    {
        get => _excluida;
        set => SetProperty(ref _excluida, value);
    }

    public bool ImportarDuplicadoComoNuevo
    {
        get => _importarDuplicadoComoNuevo;
        set => SetProperty(ref _importarDuplicadoComoNuevo, value);
    }

    public EstadoFilaImportacion Estado
    {
        get => _estado;
        private set
        {
            if (SetProperty(ref _estado, value))
            {
                OnPropertyChanged(nameof(EstadoTexto));
            }
        }
    }

    public string EstadoTexto => Estado switch
    {
        EstadoFilaImportacion.Lista => "Lista",
        EstadoFilaImportacion.RequiereRevision => "Requiere revisión",
        EstadoFilaImportacion.Invalida => "Inválida",
        EstadoFilaImportacion.Excluida => "Excluida",
        _ => "Desconocido",
    };

    public string ResumenProblemas
    {
        get => _resumenProblemas;
        private set => SetProperty(ref _resumenProblemas, value);
    }

    public string GradoResuelto
    {
        get => _gradoResuelto;
        private set => SetProperty(ref _gradoResuelto, value);
    }

    public FilaImportacionEstudiante CrearModelo() =>
        new(
            NumeroOrigen,
            NumeroListaTexto,
            NombreCompleto,
            PrimerApellido,
            SegundoApellido,
            Nombres,
            FechaNacimientoTexto,
            GeneroTexto,
            FechaIngresoTexto,
            GradoTexto,
            Observaciones,
            Excluida,
            ImportarDuplicadoComoNuevo)
        {
            Grado = ParsearGradoResuelto(),
            GradoPredeterminadoPorGrupo = string.IsNullOrWhiteSpace(GradoTexto) &&
                !string.IsNullOrWhiteSpace(GradoResuelto),
        };

    public void Aplicar(FilaImportacionEstudiante fila)
    {
        NumeroListaTexto = fila.NumeroListaTexto;
        NombreCompleto = fila.NombreCompleto;
        PrimerApellido = fila.PrimerApellido;
        SegundoApellido = fila.SegundoApellido;
        Nombres = fila.Nombres;
        FechaNacimientoTexto = fila.FechaNacimientoTexto;
        GeneroTexto = fila.GeneroTexto;
        FechaIngresoTexto = fila.FechaIngresoTexto;
        GradoTexto = fila.GradoTexto;
        Observaciones = fila.Observaciones;
        Excluida = fila.Excluida;
        ImportarDuplicadoComoNuevo = fila.ImportarDuplicadoProbableComoNuevo;
        Estado = fila.Estado;
        ResumenProblemas = string.Join(" · ", fila.Problemas.Select(problema => problema.Mensaje));
        GradoResuelto = CatalogoNemPrimaria.EsGradoReal(fila.Grado)
            ? CatalogoNemPrimaria.FormatearGrado(fila.Grado)
            : string.Empty;
    }

    private GradoPrimaria ParsearGradoResuelto() =>
        CatalogoNemPrimaria.TryParseGradoLegacy(GradoResuelto, out var grado)
            ? grado
            : GradoPrimaria.NoEspecificado;
}

public sealed class ImportacionEstudiantesViewModel : ViewModelBase
{
    private static readonly IReadOnlyList<OpcionCampoImportacion> OpcionesCamposInternas =
    [
        new(CampoImportacionEstudiante.Ignorar, "Ignorar columna"),
        new(CampoImportacionEstudiante.NumeroLista, "Número de lista"),
        new(CampoImportacionEstudiante.NombreCompleto, "Nombre completo"),
        new(CampoImportacionEstudiante.PrimerApellido, "Primer apellido"),
        new(CampoImportacionEstudiante.SegundoApellido, "Segundo apellido"),
        new(CampoImportacionEstudiante.Nombres, "Nombres"),
        new(CampoImportacionEstudiante.FechaNacimiento, "Fecha de nacimiento"),
        new(CampoImportacionEstudiante.Genero, "Género"),
        new(CampoImportacionEstudiante.FechaIngreso, "Fecha de ingreso"),
        new(CampoImportacionEstudiante.Grado, "Grado"),
        new(CampoImportacionEstudiante.Observaciones, "Observaciones"),
    ];

    private static readonly IReadOnlyList<OpcionDelimitadorCsv> OpcionesDelimitadoresCsvInternas =
    [
        new(',', "Coma (, )"),
        new(';', "Punto y coma (;)"),
        new('\t', "Tabulador"),
    ];

    private readonly ILectorImportacionTabular _lector;
    private readonly ImportacionEstudiantesCasosUso _casosUso;
    private readonly IReadOnlyList<OpcionCampoImportacion> _opcionesCampos = OpcionesCamposInternas;
    private readonly IReadOnlyList<OpcionDelimitadorCsv> _opcionesDelimitadoresCsv = OpcionesDelimitadoresCsvInternas;
    private readonly ObservableCollection<ColumnaImportacionVisual> _columnas = [];
    private readonly ObservableCollection<FilaImportacionVisual> _filas = [];
    private GrupoId? _grupoId;
    private DocumentoTabular? _documento;
    private HojaTabular? _hojaSeleccionada;
    private FilaImportacionVisual? _filaSeleccionada;
    private PasoImportacionEstudiantes _paso = PasoImportacionEstudiantes.Archivo;
    private bool _soloProblemas;
    private bool _requiereDelimitadorCsv;
    private OpcionDelimitadorCsv? _delimitadorCsvSeleccionado;
    private string _rutaArchivoPendiente = string.Empty;
    private string _mensaje = string.Empty;
    private string _nombreArchivo = string.Empty;
    private int _importados;
    private int _excluidosResultado;

    public ImportacionEstudiantesViewModel(
        ILectorImportacionTabular lector,
        ImportacionEstudiantesCasosUso casosUso)
    {
        ArgumentNullException.ThrowIfNull(lector);
        ArgumentNullException.ThrowIfNull(casosUso);
        _lector = lector;
        _casosUso = casosUso;

        GenerarPreviaCommand = new RelayCommand(GenerarPrevia, PuedeGenerarPrevia);
        RevalidarCommand = new RelayCommand(Revalidar, PuedeRevalidar);
        PrepararConfirmacionCommand = new RelayCommand(PrepararConfirmacion, PuedePrepararConfirmacion);
        ConfirmarCommand = new RelayCommand(Confirmar, PuedeConfirmar);
        VolverCommand = new RelayCommand(Volver, PuedeVolver);
        AlternarExclusionCommand = new RelayCommand(AlternarExclusion, () => FilaSeleccionada is not null);
        AutorizarDuplicadoCommand = new RelayCommand(AutorizarDuplicado, PuedeAutorizarDuplicado);
        ReintentarCsvCommand = new RelayCommand(ReintentarCsv, PuedeReintentarCsv);
    }

    public RelayCommand GenerarPreviaCommand { get; }

    public RelayCommand RevalidarCommand { get; }

    public RelayCommand PrepararConfirmacionCommand { get; }

    public RelayCommand ConfirmarCommand { get; }

    public RelayCommand VolverCommand { get; }

    public RelayCommand AlternarExclusionCommand { get; }

    public RelayCommand AutorizarDuplicadoCommand { get; }

    public RelayCommand ReintentarCsvCommand { get; }

    public IReadOnlyList<OpcionCampoImportacion> OpcionesCampos => _opcionesCampos;

    public IReadOnlyList<OpcionDelimitadorCsv> OpcionesDelimitadoresCsv => _opcionesDelimitadoresCsv;

    public IReadOnlyList<ColumnaImportacionVisual> Columnas => _columnas;

    public IReadOnlyList<FilaImportacionVisual> Filas => _filas;

    public IReadOnlyList<FilaImportacionVisual> FilasVisibles =>
        SoloProblemas
            ? _filas.Where(fila => fila.Estado is EstadoFilaImportacion.RequiereRevision or EstadoFilaImportacion.Invalida).ToArray()
            : _filas.ToArray();

    public IReadOnlyList<HojaTabular> Hojas => _documento?.Hojas ?? Array.Empty<HojaTabular>();

    public HojaTabular? HojaSeleccionada
    {
        get => _hojaSeleccionada;
        set
        {
            if (SetProperty(ref _hojaSeleccionada, value))
            {
                CrearColumnas();
                NotificarEstado();
            }
        }
    }

    public FilaImportacionVisual? FilaSeleccionada
    {
        get => _filaSeleccionada;
        set
        {
            if (SetProperty(ref _filaSeleccionada, value))
            {
                AlternarExclusionCommand.NotifyCanExecuteChanged();
                AutorizarDuplicadoCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public PasoImportacionEstudiantes Paso
    {
        get => _paso;
        private set
        {
            if (SetProperty(ref _paso, value))
            {
                OnPropertyChanged(nameof(MostrarArchivo));
                OnPropertyChanged(nameof(MostrarColumnas));
                OnPropertyChanged(nameof(MostrarPrevia));
                OnPropertyChanged(nameof(MostrarConfirmacion));
                OnPropertyChanged(nameof(MostrarResultado));
                OnPropertyChanged(nameof(TituloPaso));
                NotificarEstado();
            }
        }
    }

    public bool MostrarArchivo => Paso == PasoImportacionEstudiantes.Archivo;

    public bool MostrarColumnas => Paso == PasoImportacionEstudiantes.Columnas;

    public bool MostrarPrevia => Paso == PasoImportacionEstudiantes.Previa;

    public bool MostrarConfirmacion => Paso == PasoImportacionEstudiantes.Confirmacion;

    public bool MostrarResultado => Paso == PasoImportacionEstudiantes.Resultado;

    public string TituloPaso => Paso switch
    {
        PasoImportacionEstudiantes.Archivo => "Selecciona el archivo",
        PasoImportacionEstudiantes.Columnas => "Relaciona las columnas",
        PasoImportacionEstudiantes.Previa => "Revisa antes de importar",
        PasoImportacionEstudiantes.Confirmacion => "Confirma la importación",
        PasoImportacionEstudiantes.Resultado => "Importación completada",
        _ => "Importar alumnos",
    };

    public bool RequiereDelimitadorCsv
    {
        get => _requiereDelimitadorCsv;
        private set
        {
            if (SetProperty(ref _requiereDelimitadorCsv, value))
            {
                ReintentarCsvCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public OpcionDelimitadorCsv? DelimitadorCsvSeleccionado
    {
        get => _delimitadorCsvSeleccionado;
        set
        {
            if (SetProperty(ref _delimitadorCsvSeleccionado, value))
            {
                ReintentarCsvCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public string NombreArchivo
    {
        get => _nombreArchivo;
        private set => SetProperty(ref _nombreArchivo, value);
    }

    public string Mensaje
    {
        get => _mensaje;
        private set => SetProperty(ref _mensaje, value);
    }

    public bool SoloProblemas
    {
        get => _soloProblemas;
        set
        {
            if (SetProperty(ref _soloProblemas, value))
            {
                OnPropertyChanged(nameof(FilasVisibles));
            }
        }
    }

    public int Listas => _filas.Count(fila => fila.Estado == EstadoFilaImportacion.Lista);

    public int RequierenRevision => _filas.Count(fila => fila.Estado == EstadoFilaImportacion.RequiereRevision);

    public int Invalidas => _filas.Count(fila => fila.Estado == EstadoFilaImportacion.Invalida);

    public int Excluidas => _filas.Count(fila => fila.Estado == EstadoFilaImportacion.Excluida);

    public int Importados
    {
        get => _importados;
        private set => SetProperty(ref _importados, value);
    }

    public int ExcluidosResultado
    {
        get => _excluidosResultado;
        private set => SetProperty(ref _excluidosResultado, value);
    }

    public void Inicializar(GrupoId grupoId)
    {
        _grupoId = grupoId;
        _documento = null;
        _hojaSeleccionada = null;
        _columnas.Clear();
        _filas.Clear();
        NombreArchivo = string.Empty;
        Mensaje = string.Empty;
        _rutaArchivoPendiente = string.Empty;
        RequiereDelimitadorCsv = false;
        DelimitadorCsvSeleccionado = null;
        Importados = 0;
        ExcluidosResultado = 0;
        Paso = PasoImportacionEstudiantes.Archivo;
        NotificarConteos();
        OnPropertyChanged(nameof(Hojas));
        OnPropertyChanged(nameof(Columnas));
        OnPropertyChanged(nameof(Filas));
        OnPropertyChanged(nameof(FilasVisibles));
        NotificarEstado();
    }

    public bool CargarArchivo(string rutaArchivo)
    {
        if (_grupoId is null)
        {
            Mensaje = "Selecciona un grupo antes de importar alumnos.";
            return false;
        }

        _rutaArchivoPendiente = rutaArchivo;
        DelimitadorCsvSeleccionado = null;

        try
        {
            AplicarDocumento(_lector.Leer(rutaArchivo));
            return true;
        }
        catch (ImportacionTabularException exception)
        {
            RequiereDelimitadorCsv =
                exception.Codigo == "csv-delimiter-ambiguous" &&
                _lector is ILectorImportacionCsvConfigurable;
            Mensaje = exception.Message;
            return false;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            RequiereDelimitadorCsv = false;
            Mensaje = exception.Message;
            return false;
        }
    }

    private void ReintentarCsv()
    {
        if (_lector is not ILectorImportacionCsvConfigurable lectorCsv ||
            DelimitadorCsvSeleccionado is not { } opcion ||
            string.IsNullOrWhiteSpace(_rutaArchivoPendiente))
        {
            return;
        }

        try
        {
            AplicarDocumento(lectorCsv.LeerCsv(_rutaArchivoPendiente, opcion.Delimitador));
        }
        catch (ImportacionTabularException exception)
        {
            Mensaje = exception.Message;
        }
    }

    private void AplicarDocumento(DocumentoTabular documento)
    {
        _documento = documento;
        NombreArchivo = documento.NombreArchivo;
        OnPropertyChanged(nameof(Hojas));
        HojaSeleccionada = documento.Hojas.Count > 0
            ? documento.Hojas[0]
            : null;
        RequiereDelimitadorCsv = false;
        Mensaje = string.Empty;
        Paso = PasoImportacionEstudiantes.Columnas;
    }

    private void CrearColumnas()
    {
        _columnas.Clear();
        if (HojaSeleccionada is null)
        {
            OnPropertyChanged(nameof(Columnas));
            return;
        }

        for (var indice = 0; indice < HojaSeleccionada.Encabezados.Count; indice++)
        {
            var encabezado = HojaSeleccionada.Encabezados[indice].Texto;
            _columnas.Add(new ColumnaImportacionVisual(indice, encabezado, SugerirCampo(encabezado)));
        }

        OnPropertyChanged(nameof(Columnas));
        GenerarPreviaCommand.NotifyCanExecuteChanged();
    }

    private void GenerarPrevia()
    {
        if (_grupoId is not { } grupoId || HojaSeleccionada is null)
        {
            return;
        }

        try
        {
            var previa = _casosUso.CrearPrevia(
                grupoId,
                HojaSeleccionada,
                _columnas.Select(columna => new MapeoColumnaImportacion(columna.Indice, columna.Campo)).ToArray());
            AplicarPrevia(previa);
            Mensaje = string.Empty;
            Paso = PasoImportacionEstudiantes.Previa;
        }
        catch (Exception exception) when (exception is ArgumentException or GrupoNoEncontradoException)
        {
            Mensaje = exception.Message;
        }
    }

    private void Revalidar()
    {
        if (_grupoId is not { } grupoId)
        {
            return;
        }

        var previa = _casosUso.Revalidar(grupoId, _filas.Select(fila => fila.CrearModelo()).ToArray());
        AplicarPrevia(previa);
        Mensaje = previa.PuedeConfirmarse
            ? "Todas las filas incluidas están listas para importar."
            : "Corrige o excluye las filas que todavía requieren atención.";
    }

    private void PrepararConfirmacion()
    {
        Revalidar();
        if (PuedePrepararConfirmacion())
        {
            Paso = PasoImportacionEstudiantes.Confirmacion;
        }
    }

    private void Confirmar()
    {
        if (_grupoId is not { } grupoId)
        {
            return;
        }

        var resultado = _casosUso.Confirmar(
            grupoId,
            _filas.Select(fila => fila.CrearModelo()).ToArray());

        if (!resultado.Completada)
        {
            if (resultado.PreviaPendiente is not null)
            {
                AplicarPrevia(resultado.PreviaPendiente);
            }

            Mensaje = "El grupo cambió o quedan filas por revisar. No se guardó ningún alumno.";
            Paso = PasoImportacionEstudiantes.Previa;
            return;
        }

        Importados = resultado.Importados;
        ExcluidosResultado = resultado.Excluidos;
        Mensaje = string.Empty;
        Paso = PasoImportacionEstudiantes.Resultado;
    }

    private void Volver()
    {
        Paso = Paso switch
        {
            PasoImportacionEstudiantes.Columnas => PasoImportacionEstudiantes.Archivo,
            PasoImportacionEstudiantes.Previa => PasoImportacionEstudiantes.Columnas,
            PasoImportacionEstudiantes.Confirmacion => PasoImportacionEstudiantes.Previa,
            _ => Paso,
        };
    }

    private void AlternarExclusion()
    {
        if (FilaSeleccionada is not { } fila)
        {
            return;
        }

        fila.Excluida = !fila.Excluida;
        Revalidar();
    }

    private void AutorizarDuplicado()
    {
        if (FilaSeleccionada is not { } fila)
        {
            return;
        }

        fila.ImportarDuplicadoComoNuevo = true;
        Revalidar();
    }

    private void AplicarPrevia(PreviaImportacionEstudiantes previa)
    {
        var seleccionOrigen = FilaSeleccionada?.NumeroOrigen;
        _filas.Clear();
        foreach (var fila in previa.Filas)
        {
            _filas.Add(new FilaImportacionVisual(fila));
        }

        FilaSeleccionada = seleccionOrigen is null
            ? (_filas.Count > 0 ? _filas[0] : null)
            : _filas.FirstOrDefault(fila => fila.NumeroOrigen == seleccionOrigen) ?? (_filas.Count > 0 ? _filas[0] : null);
        OnPropertyChanged(nameof(Filas));
        OnPropertyChanged(nameof(FilasVisibles));
        NotificarConteos();
        NotificarEstado();
    }

    private bool PuedeGenerarPrevia() =>
        _grupoId is not null &&
        HojaSeleccionada is not null &&
        _columnas.Any(columna => columna.Campo == CampoImportacionEstudiante.NumeroLista) &&
        _columnas.Any(columna => columna.Campo is CampoImportacionEstudiante.NombreCompleto
            or CampoImportacionEstudiante.PrimerApellido
            or CampoImportacionEstudiante.Nombres);

    private bool PuedeRevalidar() =>
        Paso == PasoImportacionEstudiantes.Previa && _filas.Count > 0;

    private bool PuedePrepararConfirmacion() =>
        Paso == PasoImportacionEstudiantes.Previa &&
        _filas.Any(fila => fila.Estado == EstadoFilaImportacion.Lista) &&
        _filas.All(fila => fila.Estado is EstadoFilaImportacion.Lista or EstadoFilaImportacion.Excluida);

    private bool PuedeConfirmar() =>
        Paso == PasoImportacionEstudiantes.Confirmacion && PuedeConfirmarFilas();

    private bool PuedeConfirmarFilas() =>
        _filas.Any(fila => fila.Estado == EstadoFilaImportacion.Lista) &&
        _filas.All(fila => fila.Estado is EstadoFilaImportacion.Lista or EstadoFilaImportacion.Excluida);

    private bool PuedeVolver() =>
        Paso is PasoImportacionEstudiantes.Columnas or PasoImportacionEstudiantes.Previa or PasoImportacionEstudiantes.Confirmacion;

    private bool PuedeAutorizarDuplicado() =>
        FilaSeleccionada is { Estado: EstadoFilaImportacion.RequiereRevision } fila &&
        fila.ResumenProblemas.Contains("coincid", StringComparison.OrdinalIgnoreCase);

    private bool PuedeReintentarCsv() =>
        RequiereDelimitadorCsv &&
        DelimitadorCsvSeleccionado is not null &&
        _lector is ILectorImportacionCsvConfigurable &&
        !string.IsNullOrWhiteSpace(_rutaArchivoPendiente);

    private void NotificarConteos()
    {
        OnPropertyChanged(nameof(Listas));
        OnPropertyChanged(nameof(RequierenRevision));
        OnPropertyChanged(nameof(Invalidas));
        OnPropertyChanged(nameof(Excluidas));
    }

    private void NotificarEstado()
    {
        GenerarPreviaCommand.NotifyCanExecuteChanged();
        RevalidarCommand.NotifyCanExecuteChanged();
        PrepararConfirmacionCommand.NotifyCanExecuteChanged();
        ConfirmarCommand.NotifyCanExecuteChanged();
        VolverCommand.NotifyCanExecuteChanged();
        AlternarExclusionCommand.NotifyCanExecuteChanged();
        AutorizarDuplicadoCommand.NotifyCanExecuteChanged();
        ReintentarCsvCommand.NotifyCanExecuteChanged();
    }

    private static CampoImportacionEstudiante SugerirCampo(string encabezado)
    {
        var clave = NormalizarEncabezado(encabezado);
        if (clave is "no" or "num" or "numero" or "numerolista" or "numerodelista")
        {
            return CampoImportacionEstudiante.NumeroLista;
        }

        if (clave is "nombre" or "nombrecompleto" or "alumno" or "estudiante")
        {
            return CampoImportacionEstudiante.NombreCompleto;
        }

        if (clave is "primerapellido" or "apellidopaterno")
        {
            return CampoImportacionEstudiante.PrimerApellido;
        }

        if (clave is "segundoapellido" or "apellidomaterno")
        {
            return CampoImportacionEstudiante.SegundoApellido;
        }

        if (clave is "nombres" or "nombrepropio")
        {
            return CampoImportacionEstudiante.Nombres;
        }

        if (clave is "fechanacimiento" or "nacimiento")
        {
            return CampoImportacionEstudiante.FechaNacimiento;
        }

        if (clave is "genero" or "sexo")
        {
            return CampoImportacionEstudiante.Genero;
        }

        if (clave is "fechaingreso" or "ingreso")
        {
            return CampoImportacionEstudiante.FechaIngreso;
        }

        if (clave is "grado" or "gradoescolar")
        {
            return CampoImportacionEstudiante.Grado;
        }

        return clave is "observaciones" or "observacion" or "notas"
            ? CampoImportacionEstudiante.Observaciones
            : CampoImportacionEstudiante.Ignorar;
    }

    private static string NormalizarEncabezado(string valor)
    {
        var descompuesto = valor.Trim().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(descompuesto.Length);
        foreach (var caracter in descompuesto)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(caracter) == UnicodeCategory.NonSpacingMark ||
                !char.IsLetterOrDigit(caracter))
            {
                continue;
            }

            builder.Append(char.ToLowerInvariant(caracter));
        }

        return builder.ToString();
    }
}