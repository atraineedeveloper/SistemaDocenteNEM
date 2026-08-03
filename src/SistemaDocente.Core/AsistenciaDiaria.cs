using System.Collections.ObjectModel;

namespace SistemaDocente.Core;

public sealed class AsistenciaDiaria
{
    private readonly List<RegistroAsistencia> _registros;
    private readonly ReadOnlyCollection<RegistroAsistencia> _vistaRegistros;

    private AsistenciaDiaria(
        GrupoId grupoId,
        DateOnly fecha,
        List<RegistroAsistencia> registros)
    {
        GrupoId = grupoId;
        Fecha = fecha;
        _registros = registros;
        _vistaRegistros = _registros.AsReadOnly();
    }

    public GrupoId GrupoId { get; }

    public DateOnly Fecha { get; }

    public IReadOnlyList<RegistroAsistencia> Registros => _vistaRegistros;

    public static AsistenciaDiaria Crear(
        GrupoId grupoId,
        DateOnly fecha,
        IReadOnlyCollection<EstadoEstudianteAsistencia> estados)
    {
        ArgumentNullException.ThrowIfNull(estados);
        return Construir(
            grupoId,
            fecha,
            estados.Select(x => (x.EstudianteId, x.Estado)));
    }

    public static AsistenciaDiaria Rehidratar(
        GrupoId grupoId,
        DateOnly fecha,
        IReadOnlyCollection<DatosRegistroAsistenciaRehidratado> registros)
    {
        ArgumentNullException.ThrowIfNull(registros);
        return Construir(
            grupoId,
            fecha,
            registros.Select(x => (x.EstudianteId, x.Estado)));
    }

    public void CambiarEstado(EstudianteId estudianteId, EstadoAsistencia estado)
    {
        ValidarEstudianteId(estudianteId);
        ValidarEstado(estado);

        var registro = _registros.Find(x => x.EstudianteId == estudianteId)
            ?? throw new DomainConflictException(
                "El estudiante no forma parte del padrón histórico del día.");

        registro.CambiarEstado(estado);
    }

    private static AsistenciaDiaria Construir(
        GrupoId grupoId,
        DateOnly fecha,
        IEnumerable<(EstudianteId EstudianteId, EstadoAsistencia Estado)> datos)
    {
        if (grupoId == default)
        {
            throw new DomainValidationException("La identidad del grupo no puede estar vacía.");
        }

        var identidades = new HashSet<EstudianteId>();
        var registros = new List<RegistroAsistencia>();

        foreach (var dato in datos)
        {
            ValidarEstudianteId(dato.EstudianteId);
            ValidarEstado(dato.Estado);

            if (!identidades.Add(dato.EstudianteId))
            {
                throw new DomainValidationException(
                    "Cada estudiante debe aparecer una sola vez en la asistencia diaria.");
            }

            registros.Add(new RegistroAsistencia(dato.EstudianteId, dato.Estado));
        }

        return new AsistenciaDiaria(grupoId, fecha, registros);
    }

    private static void ValidarEstudianteId(EstudianteId estudianteId)
    {
        if (estudianteId == default)
        {
            throw new DomainValidationException(
                "La identidad del estudiante no puede estar vacía.");
        }
    }

    private static void ValidarEstado(EstadoAsistencia estado)
    {
        if (!Enum.IsDefined(estado))
        {
            throw new DomainValidationException("El estado de asistencia no es válido.");
        }
    }
}