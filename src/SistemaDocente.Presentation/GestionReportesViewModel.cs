using SistemaDocente.Application;
using SistemaDocente.Core;
using SistemaDocente.Reporting;

namespace SistemaDocente.Presentation;

public sealed record EstudianteReporteVisual(
    EstudianteId EstudianteId,
    int NumeroLista,
    string Nombre,
    bool EstaActivo)
{
    public string Descripcion => $"{NumeroLista}. {Nombre}";
}

public sealed class GestionReportesViewModel : ViewModelBase
{
    private readonly GestionGrupoCasosUso _grupos;
    private readonly GestionReportesCasosUso _reportes;
    private readonly ExportacionReportesPdfPresentacion? _exportacionPdf;
    private GrupoId? _grupoId;
    private IReadOnlyList<EstudianteReporteVisual> _estudiantes = Array.Empty<EstudianteReporteVisual>();
    private EstudianteReporteVisual? _estudianteSeleccionado;
    private ReporteIndividualAlumno? _reporteIndividual;
    private ReporteGrupal? _reporteGrupal;
    private bool _mostrarIndividual = true;
    private string _mensaje = string.Empty;

    public GestionReportesViewModel(
        GestionGrupoCasosUso grupos,
        GestionReportesCasosUso reportes,
        IExportadorReportesPdf? exportadorPdf = null)
    {
        _grupos = grupos ?? throw new ArgumentNullException(nameof(grupos));
        _reportes = reportes ?? throw new ArgumentNullException(nameof(reportes));
        _exportacionPdf = exportadorPdf is null ? null : new ExportacionReportesPdfPresentacion(exportadorPdf);
        MostrarIndividualCommand = new RelayCommand(MostrarIndividual);
        MostrarGrupalCommand = new RelayCommand(MostrarGrupal);
        RefrescarCommand = new RelayCommand(Refrescar, () => _grupoId is not null);
    }

    public RelayCommand MostrarIndividualCommand { get; }
    public RelayCommand MostrarGrupalCommand { get; }
    public RelayCommand RefrescarCommand { get; }
    public GrupoId? GrupoIdActual => _grupoId;

    public IReadOnlyList<EstudianteReporteVisual> Estudiantes
    {
        get => _estudiantes;
        private set => SetProperty(ref _estudiantes, value);
    }

    public EstudianteReporteVisual? EstudianteSeleccionado
    {
        get => _estudianteSeleccionado;
        set
        {
            if (SetProperty(ref _estudianteSeleccionado, value) && _mostrarIndividual)
            {
                CargarIndividual();
            }
        }
    }

    public ReporteIndividualAlumno? ReporteIndividual
    {
        get => _reporteIndividual;
        private set
        {
            if (SetProperty(ref _reporteIndividual, value))
            {
                OnPropertyChanged(nameof(PuedeExportarPdf));
            }
        }
    }

    public ReporteGrupal? ReporteGrupal
    {
        get => _reporteGrupal;
        private set
        {
            if (SetProperty(ref _reporteGrupal, value))
            {
                OnPropertyChanged(nameof(PuedeExportarPdf));
            }
        }
    }

    public bool MostrarIndividualActivo
    {
        get => _mostrarIndividual;
        private set
        {
            if (SetProperty(ref _mostrarIndividual, value))
            {
                OnPropertyChanged(nameof(MostrarGrupalActivo));
                OnPropertyChanged(nameof(PuedeExportarPdf));
            }
        }
    }

    public bool MostrarGrupalActivo => !MostrarIndividualActivo;

    public bool PuedeExportarPdf =>
        _exportacionPdf is not null
        && (MostrarIndividualActivo ? ReporteIndividual is not null : ReporteGrupal is not null);

    public string AdvertenciaPdf => _exportacionPdf is null
        ? string.Empty
        : ExportacionReportesPdfPresentacion.AdvertenciaPrivacidad;

    public string Mensaje
    {
        get => _mensaje;
        private set => SetProperty(ref _mensaje, value);
    }

    public void Inicializar(GrupoId grupoId)
    {
        _grupoId = grupoId;
        OnPropertyChanged(nameof(GrupoIdActual));
        var estudiantes = _grupos.ObtenerTodosLosEstudiantes(grupoId)
            .OrderBy(x => x.NumeroLista)
            .ThenBy(x => x.NombreVisible, StringComparer.Ordinal)
            .Select(x => new EstudianteReporteVisual(x.EstudianteId, x.NumeroLista, x.NombreVisible, x.EstaActivo))
            .ToArray();
        Estudiantes = estudiantes;
        _estudianteSeleccionado = estudiantes.FirstOrDefault(x => x.EstaActivo) ?? estudiantes.FirstOrDefault();
        OnPropertyChanged(nameof(EstudianteSeleccionado));
        RefrescarCommand.NotifyCanExecuteChanged();
        Refrescar();
    }

    public void Refrescar()
    {
        if (_grupoId is not { } grupoId) return;
        try
        {
            Mensaje = string.Empty;
            ReporteGrupal = _reportes.GenerarGrupal(grupoId);
            if (EstudianteSeleccionado is not null)
            {
                ReporteIndividual = _reportes.GenerarIndividual(grupoId, EstudianteSeleccionado.EstudianteId);
            }
        }
        catch (Exception ex) when (ex is DomainValidationException or DomainConflictException or ErrorPersistenciaAplicacionException)
        {
            Mensaje = "No fue posible actualizar los reportes con la información disponible.";
        }
    }

    public string CrearNombrePdfSugerido(DateOnly fecha)
    {
        if (_exportacionPdf is null) return $"{IdentidadProducto.NombreSeguroArchivo}_Reporte_{fecha:yyyy-MM-dd}.pdf";
        if (MostrarIndividualActivo && ReporteIndividual is { } individual)
        {
            return ExportacionReportesPdfPresentacion.CrearNombreArchivo(individual, fecha);
        }
        if (!MostrarIndividualActivo && ReporteGrupal is { } grupal)
        {
            return ExportacionReportesPdfPresentacion.CrearNombreArchivo(grupal, fecha);
        }
        return $"{IdentidadProducto.NombreSeguroArchivo}_Reporte_{fecha:yyyy-MM-dd}.pdf";
    }

    public bool ExportarPdf(string rutaArchivo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rutaArchivo);
        if (_exportacionPdf is null)
        {
            Mensaje = "La exportación PDF no está disponible en esta instalación.";
            return false;
        }

        try
        {
            if (MostrarIndividualActivo && ReporteIndividual is { } individual)
            {
                _exportacionPdf.Exportar(individual, rutaArchivo);
            }
            else if (!MostrarIndividualActivo && ReporteGrupal is { } grupal)
            {
                _exportacionPdf.Exportar(grupal, rutaArchivo);
            }
            else
            {
                Mensaje = "Genera el reporte antes de guardarlo como PDF.";
                return false;
            }

            Mensaje = "Reporte PDF guardado correctamente.";
            return true;
        }
        catch (Exception exception) when (
            exception is ExportacionReportePdfException
                or IOException
                or UnauthorizedAccessException)
        {
            Mensaje = "No fue posible guardar el reporte PDF. Verifica la ubicación e intenta nuevamente.";
            return false;
        }
    }

    private void MostrarIndividual()
    {
        MostrarIndividualActivo = true;
        CargarIndividual();
    }

    private void MostrarGrupal()
    {
        MostrarIndividualActivo = false;
        if (_grupoId is not null) Refrescar();
    }

    private void CargarIndividual()
    {
        if (_grupoId is not { } grupoId || EstudianteSeleccionado is null) return;
        try
        {
            Mensaje = string.Empty;
            ReporteIndividual = _reportes.GenerarIndividual(grupoId, EstudianteSeleccionado.EstudianteId);
        }
        catch (Exception ex) when (ex is DomainValidationException or DomainConflictException or ErrorPersistenciaAplicacionException)
        {
            Mensaje = "No fue posible generar el reporte individual seleccionado.";
        }
    }
}