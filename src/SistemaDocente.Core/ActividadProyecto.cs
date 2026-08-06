using System.Collections.ObjectModel;

namespace SistemaDocente.Core;

public sealed class ActividadProyecto
{
    private const int MaximoTitulo = 200;
    private const int MaximoTexto = 2000;
    private const int MaximoObservacionEntrega = 500;
    private readonly List<EntregaActividad> _entregas;
    private readonly ReadOnlyCollection<EntregaActividad> _vistaEntregas;

    private ActividadProyecto(ActividadId id, ProyectoId proyectoId, GrupoId grupoId, string titulo,
        string descripcion, DateOnly fechaRealizacion, string observacionesGenerales,
        EstadoActividad estado, int version, List<EntregaActividad> entregas)
    {
        Id = id; ProyectoId = proyectoId; GrupoId = grupoId; Titulo = titulo; Descripcion = descripcion;
        FechaRealizacion = fechaRealizacion; ObservacionesGenerales = observacionesGenerales;
        Estado = estado; Version = version; _entregas = entregas; _vistaEntregas = _entregas.AsReadOnly();
    }

    public ActividadId Id { get; }
    public ProyectoId ProyectoId { get; }
    public GrupoId GrupoId { get; }
    public string Titulo { get; private set; }
    public string Descripcion { get; private set; }
    public DateOnly FechaRealizacion { get; private set; }
    public string ObservacionesGenerales { get; private set; }
    public EstadoActividad Estado { get; private set; }
    public int Version { get; }
    public IReadOnlyList<EntregaActividad> Entregas => _vistaEntregas;

    public static ActividadProyecto Crear(ProyectoId proyectoId, GrupoId grupoId, string titulo,
        string? descripcion, DateOnly fechaRealizacion, string? observacionesGenerales,
        DateOnly inicioProyecto, DateOnly terminoProyecto, IReadOnlyCollection<EstudianteId> estudiantes)
    {
        ArgumentNullException.ThrowIfNull(estudiantes);
        var datos = ValidarDatos(proyectoId, grupoId, titulo, descripcion, fechaRealizacion,
            observacionesGenerales, inicioProyecto, terminoProyecto);
        var ids = ValidarIdentidades(estudiantes);
        return new(ActividadId.Crear(), proyectoId, grupoId, datos.Titulo, datos.Descripcion,
            fechaRealizacion, datos.Observaciones, EstadoActividad.Activa, 1,
            ids.Select(id => new EntregaActividad(id, NivelLogro.Pendiente, string.Empty)).ToList());
    }

    public static ActividadProyecto Rehidratar(ActividadId id, ProyectoId proyectoId, GrupoId grupoId,
        string titulo, string? descripcion, DateOnly fechaRealizacion, string? observacionesGenerales,
        EstadoActividad estado, int version, IReadOnlyCollection<DatosEntregaActividadRehidratada> entregas)
    {
        if (id == default) throw new DomainValidationException("La identidad de la actividad no puede estar vacía.");
        ArgumentNullException.ThrowIfNull(entregas);
        if (!Enum.IsDefined(estado)) throw new DomainValidationException("El estado de la actividad no es válido.");
        if (version <= 0) throw new DomainValidationException("La versión de la actividad debe ser positiva.");
        var datos = ValidarDatos(proyectoId, grupoId, titulo, descripcion, fechaRealizacion,
            observacionesGenerales, DateOnly.MinValue, DateOnly.MaxValue, true);
        var ids = new HashSet<EstudianteId>();
        var registros = new List<EntregaActividad>();
        foreach (var entrega in entregas)
        {
            if (entrega.EstudianteId == default || !ids.Add(entrega.EstudianteId))
                throw new DomainValidationException("Las identidades de entrega deben ser válidas y únicas.");
            ValidarNivelLogro(entrega.NivelLogro);
            registros.Add(new(entrega.EstudianteId, entrega.NivelLogro, ValidarObservacion(entrega.Observacion)));
        }
        return new(id, proyectoId, grupoId, datos.Titulo, datos.Descripcion, fechaRealizacion,
            datos.Observaciones, estado, version, registros);
    }

    public void Actualizar(string titulo, string? descripcion, DateOnly fechaRealizacion,
        string? observacionesGenerales, DateOnly inicioProyecto, DateOnly terminoProyecto)
    {
        AsegurarEditable();
        var datos = ValidarDatos(ProyectoId, GrupoId, titulo, descripcion, fechaRealizacion,
            observacionesGenerales, inicioProyecto, terminoProyecto);
        Titulo = datos.Titulo; Descripcion = datos.Descripcion;
        FechaRealizacion = fechaRealizacion; ObservacionesGenerales = datos.Observaciones;
    }

    public void ActualizarEntregas(IReadOnlyCollection<DatosEntregaActividadRehidratada> entregas)
    {
        AsegurarEditable(); ArgumentNullException.ThrowIfNull(entregas);
        var snapshot = entregas.ToArray();
        if (snapshot.Length != _entregas.Count || snapshot.Select(x => x.EstudianteId).Distinct().Count() != snapshot.Length
            || snapshot.Select(x => x.EstudianteId).ToHashSet().SetEquals(_entregas.Select(x => x.EstudianteId)) == false)
            throw new DomainValidationException("Debe proporcionarse exactamente el padrón histórico completo.");
        var validadas = snapshot.Select(x => { ValidarNivelLogro(x.NivelLogro); return new DatosEntregaActividadRehidratada(x.EstudianteId, x.NivelLogro, ValidarObservacion(x.Observacion)); }).ToArray();
        foreach (var datos in validadas)
        {
            var entrega = _entregas.Single(x => x.EstudianteId == datos.EstudianteId);
            entrega.NivelLogro = datos.NivelLogro; entrega.Observacion = datos.Observacion;
        }
    }

    public void Anular() { AsegurarEditable(); Estado = EstadoActividad.Anulada; }

    private void AsegurarEditable()
    {
        if (Estado == EstadoActividad.Anulada) throw new DomainConflictException("Una actividad anulada no puede editarse.");
    }

    private static (string Titulo, string Descripcion, string Observaciones) ValidarDatos(
        ProyectoId proyectoId, GrupoId grupoId, string titulo, string? descripcion,
        DateOnly fecha, string? observaciones, DateOnly inicio, DateOnly termino, bool exigirNormalizados = false)
    {
        if (proyectoId == default || grupoId == default) throw new DomainValidationException("Las identidades de pertenencia son obligatorias.");
        var tituloV = NormalizadorNombreVisible.NormalizarYValidar(titulo, MaximoTitulo, "El título de la actividad");
        var descripcionV = ValidarTexto(descripcion, MaximoTexto, "La descripción");
        var observacionesV = ValidarTexto(observaciones, MaximoTexto, "Las observaciones generales");
        if (exigirNormalizados && (titulo != tituloV || (descripcion ?? string.Empty) != descripcionV || (observaciones ?? string.Empty) != observacionesV))
            throw new DomainValidationException("Los textos de la actividad deben estar normalizados.");
        if (fecha < inicio || fecha > termino) throw new DomainValidationException("La fecha de realización debe estar dentro del periodo del proyecto.");
        return (tituloV, descripcionV, observacionesV);
    }

    private static EstudianteId[] ValidarIdentidades(IEnumerable<EstudianteId> estudiantes)
    {
        var ids = estudiantes.ToArray();
        if (ids.Any(x => x == default) || ids.Distinct().Count() != ids.Length)
            throw new DomainValidationException("El padrón debe contener identidades válidas y únicas.");
        return ids;
    }

    private static string ValidarObservacion(string? valor) => ValidarTexto(valor, MaximoObservacionEntrega, "La observación de entrega");

    private static string ValidarTexto(string? valor, int maximo, string campo)
    {
        var texto = string.IsNullOrWhiteSpace(valor) ? string.Empty : valor.Trim();
        if (texto.Length > maximo) throw new DomainValidationException($"{campo} no puede exceder {maximo} caracteres.");
        return texto;
    }

    private static void ValidarNivelLogro(NivelLogro nivel)
    {
        if (!Enum.IsDefined(nivel)) throw new DomainValidationException("El nivel de logro no es válido.");
    }
}