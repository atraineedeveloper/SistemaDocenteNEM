namespace SistemaDocente.Core;

public sealed class ProyectoDidactico
{
    private const int MaximoNombre = 150;
    private const int MaximoTexto = 2000;

    private ProyectoDidactico(
        ProyectoId id,
        GrupoId grupoId,
        string nombre,
        string descripcion,
        DateOnly fechaInicio,
        DateOnly fechaTermino,
        EstadoProyecto estado,
        string observaciones,
        int version,
        MetodologiaProyectoNem metodologia,
        IReadOnlyList<GradoPrimaria> gradosObjetivo)
    {
        Id = id;
        GrupoId = grupoId;
        Nombre = nombre;
        Descripcion = descripcion;
        FechaInicio = fechaInicio;
        FechaTermino = fechaTermino;
        Estado = estado;
        Observaciones = observaciones;
        Version = version;
        Metodologia = metodologia;
        GradosObjetivo = gradosObjetivo;
    }

    public ProyectoId Id { get; }
    public GrupoId GrupoId { get; }
    public string Nombre { get; private set; }
    public string Descripcion { get; private set; }
    public DateOnly FechaInicio { get; private set; }
    public DateOnly FechaTermino { get; private set; }
    public EstadoProyecto Estado { get; private set; }
    public string Observaciones { get; private set; }
    public int Version { get; }
    public MetodologiaProyectoNem Metodologia { get; private set; }
    public IReadOnlyList<GradoPrimaria> GradosObjetivo { get; private set; }

    public static ProyectoDidactico Crear(
        GrupoId grupoId,
        string nombre,
        string? descripcion,
        DateOnly fechaInicio,
        DateOnly fechaTermino,
        string? observaciones) =>
        Crear(
            grupoId,
            nombre,
            descripcion,
            fechaInicio,
            fechaTermino,
            observaciones,
            MetodologiaProyectoNem.NoEspecificada,
            []);

    public static ProyectoDidactico Crear(
        GrupoId grupoId,
        string nombre,
        string? descripcion,
        DateOnly fechaInicio,
        DateOnly fechaTermino,
        string? observaciones,
        MetodologiaProyectoNem metodologia,
        IEnumerable<GradoPrimaria>? gradosObjetivo)
    {
        ValidarIdentidad(grupoId);
        var datos = ValidarDatos(nombre, descripcion, fechaInicio, fechaTermino, observaciones);
        var planeacion = ValidarPlaneacion(metodologia, gradosObjetivo);
        return new(
            ProyectoId.Crear(),
            grupoId,
            datos.Nombre,
            datos.Descripcion,
            fechaInicio,
            fechaTermino,
            EstadoProyecto.Borrador,
            datos.Observaciones,
            1,
            planeacion.Metodologia,
            planeacion.Grados);
    }

    public static ProyectoDidactico Rehidratar(
        ProyectoId id,
        GrupoId grupoId,
        string nombre,
        string? descripcion,
        DateOnly fechaInicio,
        DateOnly fechaTermino,
        EstadoProyecto estado,
        string? observaciones,
        int version) =>
        Rehidratar(
            id,
            grupoId,
            nombre,
            descripcion,
            fechaInicio,
            fechaTermino,
            estado,
            observaciones,
            version,
            MetodologiaProyectoNem.NoEspecificada,
            []);

    public static ProyectoDidactico Rehidratar(
        ProyectoId id,
        GrupoId grupoId,
        string nombre,
        string? descripcion,
        DateOnly fechaInicio,
        DateOnly fechaTermino,
        EstadoProyecto estado,
        string? observaciones,
        int version,
        MetodologiaProyectoNem metodologia,
        IEnumerable<GradoPrimaria>? gradosObjetivo)
    {
        if (id == default) throw new DomainValidationException("La identidad del proyecto no puede estar vacía.");
        ValidarIdentidad(grupoId);
        ValidarEstado(estado);
        if (version <= 0) throw new DomainValidationException("La versión del proyecto debe ser positiva.");
        var datos = ValidarDatos(nombre, descripcion, fechaInicio, fechaTermino, observaciones, true);
        var planeacion = ValidarPlaneacion(metodologia, gradosObjetivo);
        return new(
            id,
            grupoId,
            datos.Nombre,
            datos.Descripcion,
            fechaInicio,
            fechaTermino,
            estado,
            datos.Observaciones,
            version,
            planeacion.Metodologia,
            planeacion.Grados);
    }

    public void Actualizar(
        string nombre,
        string? descripcion,
        DateOnly fechaInicio,
        DateOnly fechaTermino,
        string? observaciones) =>
        Actualizar(
            nombre,
            descripcion,
            fechaInicio,
            fechaTermino,
            observaciones,
            Metodologia,
            GradosObjetivo);

    public void Actualizar(
        string nombre,
        string? descripcion,
        DateOnly fechaInicio,
        DateOnly fechaTermino,
        string? observaciones,
        MetodologiaProyectoNem metodologia,
        IEnumerable<GradoPrimaria>? gradosObjetivo)
    {
        var datos = ValidarDatos(nombre, descripcion, fechaInicio, fechaTermino, observaciones);
        var planeacion = ValidarPlaneacion(metodologia, gradosObjetivo);
        Nombre = datos.Nombre;
        Descripcion = datos.Descripcion;
        FechaInicio = fechaInicio;
        FechaTermino = fechaTermino;
        Observaciones = datos.Observaciones;
        Metodologia = planeacion.Metodologia;
        GradosObjetivo = planeacion.Grados;
    }

    public void Iniciar()
    {
        if (Estado != EstadoProyecto.Borrador) throw new DomainConflictException("Sólo un proyecto Borrador puede iniciarse.");
        Estado = EstadoProyecto.EnCurso;
    }

    public void Finalizar()
    {
        if (Estado != EstadoProyecto.EnCurso) throw new DomainConflictException("Sólo un proyecto En curso puede finalizarse.");
        Estado = EstadoProyecto.Finalizado;
    }

    public void Reabrir()
    {
        if (Estado != EstadoProyecto.Finalizado) throw new DomainConflictException("Sólo un proyecto Finalizado puede reabrirse.");
        Estado = EstadoProyecto.EnCurso;
    }

    private static (string Nombre, string Descripcion, string Observaciones) ValidarDatos(
        string nombre,
        string? descripcion,
        DateOnly fechaInicio,
        DateOnly fechaTermino,
        string? observaciones,
        bool exigirNormalizados = false)
    {
        var nombreValidado = NormalizadorNombreVisible.NormalizarYValidar(nombre, MaximoNombre, "El nombre del proyecto");
        var descripcionValidada = ValidarOpcional(descripcion, MaximoTexto, "La descripción");
        var observacionesValidadas = ValidarOpcional(observaciones, MaximoTexto, "Las observaciones");
        if (exigirNormalizados && (nombre != nombreValidado || (descripcion ?? string.Empty) != descripcionValidada || (observaciones ?? string.Empty) != observacionesValidadas))
            throw new DomainValidationException("Los textos del proyecto deben estar normalizados.");
        if (fechaInicio > fechaTermino) throw new DomainValidationException("La fecha inicial no puede ser posterior a la fecha final.");
        return (nombreValidado, descripcionValidada, observacionesValidadas);
    }

    private static (MetodologiaProyectoNem Metodologia, IReadOnlyList<GradoPrimaria> Grados) ValidarPlaneacion(
        MetodologiaProyectoNem metodologia,
        IEnumerable<GradoPrimaria>? gradosObjetivo)
    {
        if (!CatalogoPlaneacionNem.EsMetodologiaValida(metodologia))
        {
            throw new DomainValidationException("La metodología NEM del proyecto no es válida.");
        }

        return (metodologia, CatalogoPlaneacionNem.NormalizarGradosObjetivo(gradosObjetivo));
    }

    private static string ValidarOpcional(string? valor, int maximo, string campo)
    {
        var normalizado = string.IsNullOrWhiteSpace(valor) ? string.Empty : string.Join(' ', valor.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        if (normalizado.Length > maximo) throw new DomainValidationException($"{campo} no puede exceder {maximo} caracteres.");
        return normalizado;
    }

    private static void ValidarIdentidad(GrupoId grupoId)
    {
        if (grupoId == default) throw new DomainValidationException("La identidad del grupo no puede estar vacía.");
    }

    private static void ValidarEstado(EstadoProyecto estado)
    {
        if (!Enum.IsDefined(estado)) throw new DomainValidationException("El estado del proyecto no es válido.");
    }
}