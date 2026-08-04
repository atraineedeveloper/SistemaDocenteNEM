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
        int version)
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

    public static ProyectoDidactico Crear(
        GrupoId grupoId,
        string nombre,
        string? descripcion,
        DateOnly fechaInicio,
        DateOnly fechaTermino,
        string? observaciones)
    {
        ValidarIdentidad(grupoId);
        var datos = ValidarDatos(nombre, descripcion, fechaInicio, fechaTermino, observaciones);
        return new(ProyectoId.Crear(), grupoId, datos.Nombre, datos.Descripcion,
            fechaInicio, fechaTermino, EstadoProyecto.Borrador, datos.Observaciones, 1);
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
        int version)
    {
        if (id == default) throw new DomainValidationException("La identidad del proyecto no puede estar vacía.");
        ValidarIdentidad(grupoId);
        ValidarEstado(estado);
        if (version <= 0) throw new DomainValidationException("La versión del proyecto debe ser positiva.");
        var datos = ValidarDatos(nombre, descripcion, fechaInicio, fechaTermino, observaciones, true);
        return new(id, grupoId, datos.Nombre, datos.Descripcion, fechaInicio, fechaTermino, estado, datos.Observaciones, version);
    }

    public void Actualizar(
        string nombre,
        string? descripcion,
        DateOnly fechaInicio,
        DateOnly fechaTermino,
        string? observaciones)
    {
        var datos = ValidarDatos(nombre, descripcion, fechaInicio, fechaTermino, observaciones);
        Nombre = datos.Nombre;
        Descripcion = datos.Descripcion;
        FechaInicio = fechaInicio;
        FechaTermino = fechaTermino;
        Observaciones = datos.Observaciones;
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