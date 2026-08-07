using SistemaDocente.Core;

namespace SistemaDocente.Application;

public enum CampoImportacionEstudiante
{
    Ignorar = 0,
    NumeroLista = 1,
    NombreCompleto = 2,
    PrimerApellido = 3,
    SegundoApellido = 4,
    Nombres = 5,
    FechaNacimiento = 6,
    Genero = 7,
    FechaIngreso = 8,
    Grado = 9,
    Observaciones = 10,
}

public sealed record MapeoColumnaImportacion(
    int IndiceColumna,
    CampoImportacionEstudiante Campo);

public enum EstadoFilaImportacion
{
    Lista = 0,
    RequiereRevision = 1,
    Invalida = 2,
    Excluida = 3,
}

public enum SeveridadProblemaImportacion
{
    Revision = 0,
    Invalido = 1,
}

public sealed record ProblemaImportacion(
    CampoImportacionEstudiante? Campo,
    string Codigo,
    string Mensaje,
    SeveridadProblemaImportacion Severidad);

public sealed record FilaImportacionEstudiante(
    int NumeroOrigen,
    string NumeroListaTexto,
    string NombreCompleto,
    string PrimerApellido,
    string SegundoApellido,
    string Nombres,
    string FechaNacimientoTexto,
    string GeneroTexto,
    string FechaIngresoTexto,
    string GradoTexto,
    string Observaciones,
    bool Excluida = false,
    bool ImportarDuplicadoProbableComoNuevo = false)
{
    public int? NumeroLista { get; init; }

    public DateOnly? FechaNacimiento { get; init; }

    public GeneroEstudiante Genero { get; init; } = GeneroEstudiante.NoEspecificado;

    public DateOnly? FechaIngreso { get; init; }

    public GradoPrimaria Grado { get; init; } = GradoPrimaria.NoEspecificado;

    public bool GradoPredeterminadoPorGrupo { get; init; }

    public string NombreVisible { get; init; } = string.Empty;

    public EstadoFilaImportacion Estado { get; init; } = EstadoFilaImportacion.Invalida;

    public IReadOnlyList<ProblemaImportacion> Problemas { get; init; } = Array.Empty<ProblemaImportacion>();
}

public sealed record PreviaImportacionEstudiantes(
    GrupoId GrupoId,
    IReadOnlyList<FilaImportacionEstudiante> Filas)
{
    public int Listas => Filas.Count(fila => fila.Estado == EstadoFilaImportacion.Lista);

    public int RequierenRevision => Filas.Count(fila => fila.Estado == EstadoFilaImportacion.RequiereRevision);

    public int Invalidas => Filas.Count(fila => fila.Estado == EstadoFilaImportacion.Invalida);

    public int Excluidas => Filas.Count(fila => fila.Estado == EstadoFilaImportacion.Excluida);

    public bool PuedeConfirmarse =>
        Filas.Any(fila => fila.Estado == EstadoFilaImportacion.Lista) &&
        Filas.All(fila => fila.Estado is EstadoFilaImportacion.Lista or EstadoFilaImportacion.Excluida);
}

public sealed record ResultadoImportacionEstudiantes(
    bool Completada,
    int Importados,
    int Excluidos,
    IReadOnlyList<EstudianteId> EstudiantesCreados,
    PreviaImportacionEstudiantes? PreviaPendiente = null);
