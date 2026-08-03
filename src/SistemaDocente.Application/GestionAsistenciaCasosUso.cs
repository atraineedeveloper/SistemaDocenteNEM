using SistemaDocente.Core;

namespace SistemaDocente.Application;

public sealed class GestionAsistenciaCasosUso : IGestionAsistenciaCasosUso
{
    private readonly IAlmacenamientoGrupos _grupos;
    private readonly IAlmacenamientoAsistencias _asistencias;
    private readonly ICalendarioLectivo _calendario;

    public GestionAsistenciaCasosUso(
        IAlmacenamientoGrupos grupos,
        IAlmacenamientoAsistencias asistencias,
        ICalendarioLectivo? calendario = null)
    {
        ArgumentNullException.ThrowIfNull(grupos);
        ArgumentNullException.ThrowIfNull(asistencias);
        _grupos = grupos;
        _asistencias = asistencias;
        _calendario = calendario ?? new CalendarioLectivoLunesAViernes();
    }

    public AsistenciaDiaDetalle? Cargar(GrupoId grupoId, DateOnly fecha)
    {
        var grupo = CargarGrupoRequerido(grupoId);
        var asistencia = _asistencias.Cargar(grupoId, fecha);
        return asistencia is null ? null : Proyectar(grupo, asistencia, true);
    }

    public AsistenciaDiaDetalle Preparar(GrupoId grupoId, DateOnly fecha)
    {
        var grupo = CargarGrupoRequerido(grupoId);
        var existente = _asistencias.Cargar(grupoId, fecha);

        if (existente is not null)
        {
            return Proyectar(grupo, existente, true);
        }

        var nueva = AsistenciaDiaria.Crear(
            grupoId,
            fecha,
            grupo.EstudiantesActivos
                .Select(x => new EstadoEstudianteAsistencia(
                    x.Id,
                    EstadoAsistencia.Presente))
                .ToArray());

        return Proyectar(grupo, nueva, false);
    }

    public bool Existe(GrupoId grupoId, DateOnly fecha) =>
        _asistencias.Existe(grupoId, fecha);

    public AsistenciaMesDetalle CargarMes(GrupoId grupoId, int anio, int mes)
    {
        if (anio < DateOnly.MinValue.Year || anio > DateOnly.MaxValue.Year)
        {
            throw new DomainValidationException("El año seleccionado no es válido.");
        }

        if (mes is < 1 or > 12)
        {
            throw new DomainValidationException("El mes seleccionado no es válido.");
        }

        var grupo = CargarGrupoRequerido(grupoId);
        var desde = new DateOnly(anio, mes, 1);
        var hasta = new DateOnly(anio, mes, DateTime.DaysInMonth(anio, mes));
        var existentes = _asistencias.CargarIntervalo(grupoId, desde, hasta)
            .ToDictionary(x => x.Fecha);
        var fechasLectivas = Enumerable.Range(1, hasta.Day)
            .Select(numero => new DateOnly(anio, mes, numero))
            .Where(_calendario.EsLaborable)
            .ToArray();
        var dias = fechasLectivas
            .Select((fecha, indice) => CrearColumna(
                fecha,
                existentes,
                fecha.DayOfWeek == DayOfWeek.Friday && indice < fechasLectivas.Length - 1))
            .ToArray();
        var existentesLectivos = existentes
            .Where(x => _calendario.EsLaborable(x.Key))
            .ToDictionary();
        var estudiantesActuales = grupo.Estudiantes.ToDictionary(x => x.Id);
        var identidades = grupo.EstudiantesActivos.Select(x => x.Id)
            .Concat(existentesLectivos.Values.SelectMany(x => x.Registros.Select(r => r.EstudianteId)))
            .Distinct()
            .ToArray();
        var estudiantes = identidades.Select(id => CrearEstudianteMes(
                estudiantesActuales.TryGetValue(id, out var estudiante)
                    ? estudiante
                    : throw new DomainConflictException("El historial contiene un estudiante ajeno al grupo."),
                dias,
                existentesLectivos))
            .OrderBy(x => x.NumeroLista)
            .ThenBy(x => x.NombreVisible, StringComparer.Ordinal)
            .ThenBy(x => x.EstudianteId.Valor)
            .ToArray();

        return new AsistenciaMesDetalle(grupoId, anio, mes, dias, estudiantes);
    }

    public AsistenciaDiaDetalle Guardar(
        GrupoId grupoId,
        DateOnly fecha,
        IReadOnlyCollection<EntradaEstadoAsistencia> entradas)
    {
        ArgumentNullException.ThrowIfNull(entradas);
        var grupo = CargarGrupoRequerido(grupoId);
        var existente = _asistencias.Cargar(grupoId, fecha);
        var snapshot = entradas.ToArray();

        if (existente is null)
        {
            var esperados = grupo.EstudiantesActivos.Select(x => x.Id).ToHashSet();
            ValidarEntradas(snapshot, esperados);
            existente = AsistenciaDiaria.Crear(
                grupoId,
                fecha,
                snapshot.Select(x => new EstadoEstudianteAsistencia(
                    x.EstudianteId,
                    x.Estado)).ToArray());
        }
        else
        {
            var esperados = existente.Registros.Select(x => x.EstudianteId).ToHashSet();
            ValidarEntradas(snapshot, esperados);

            foreach (var entrada in snapshot)
            {
                existente.CambiarEstado(entrada.EstudianteId, entrada.Estado);
            }
        }

        _asistencias.Guardar(existente);
        return Proyectar(grupo, existente, true);
    }

    public ResultadoGuardadoMes GuardarMes(
        GrupoId grupoId,
        IReadOnlyCollection<EntradaDiaAsistencia> dias)
    {
        ArgumentNullException.ThrowIfNull(dias);
        var ordenados = dias.OrderBy(x => x.Fecha).ToArray();
        var guardadas = new List<DateOnly>();

        foreach (var dia in ordenados)
        {
            try
            {
                Guardar(grupoId, dia.Fecha, dia.Entradas);
                guardadas.Add(dia.Fecha);
            }
            catch (ErrorPersistenciaAplicacionException exception)
            {
                throw new GuardadoMesInterrumpidoException(
                    dia.Fecha,
                    guardadas,
                    ordenados.Where(x => x.Fecha >= dia.Fecha).Select(x => x.Fecha).ToArray(),
                    exception);
            }
        }

        return new ResultadoGuardadoMes(guardadas.ToArray(), []);
    }

    private static AsistenciaDiaColumnaDetalle CrearColumna(
        DateOnly fecha,
        Dictionary<DateOnly, AsistenciaDiaria> existentes,
        bool esCierreSemana) =>
        new(
            fecha,
            fecha.Day,
            fecha.DayOfWeek switch
            {
                DayOfWeek.Monday => "L",
                DayOfWeek.Tuesday => "M",
                DayOfWeek.Wednesday => "M",
                DayOfWeek.Thursday => "J",
                DayOfWeek.Friday => "V",
                DayOfWeek.Saturday => "S",
                _ => "D",
            },
            true,
            existentes.ContainsKey(fecha),
            esCierreSemana);

    private static AsistenciaEstudianteMesDetalle CrearEstudianteMes(
        Estudiante estudiante,
        IReadOnlyList<AsistenciaDiaColumnaDetalle> dias,
        Dictionary<DateOnly, AsistenciaDiaria> existentes)
    {
        var celdas = dias.Select(dia => CrearCelda(estudiante, dia, existentes)).ToArray();
        var confirmadas = celdas.Where(x => x.Tipo == TipoCeldaAsistencia.Confirmada).ToArray();
        var presentes = confirmadas.Count(x => x.Estado == EstadoAsistencia.Presente);
        var faltas = confirmadas.Count(x => x.Estado == EstadoAsistencia.Falta);
        var retardos = confirmadas.Count(x => x.Estado == EstadoAsistencia.Retardo);
        var justificadas = confirmadas.Count(x => x.Estado == EstadoAsistencia.Justificada);
        double? porcentaje = confirmadas.Length == 0
            ? null
            : (presentes + retardos) * 100d / confirmadas.Length;
        return new(
            estudiante.Id,
            estudiante.NumeroLista,
            estudiante.NombreVisible,
            estudiante.EstaActivo,
            celdas,
            presentes,
            faltas,
            retardos,
            justificadas,
            porcentaje);
    }

    private static AsistenciaCeldaDetalle CrearCelda(
        Estudiante estudiante,
        AsistenciaDiaColumnaDetalle dia,
        Dictionary<DateOnly, AsistenciaDiaria> existentes)
    {
        if (!dia.EsLaborable)
        {
            return new(dia.Fecha, null, TipoCeldaAsistencia.NoAplicable);
        }

        if (existentes.TryGetValue(dia.Fecha, out var asistencia))
        {
            var registro = asistencia.Registros.SingleOrDefault(x => x.EstudianteId == estudiante.Id);
            return registro is null
                ? new(dia.Fecha, null, TipoCeldaAsistencia.NoAplicable)
                : new(dia.Fecha, registro.Estado, TipoCeldaAsistencia.Confirmada);
        }

        return estudiante.EstaActivo
            ? new(dia.Fecha, EstadoAsistencia.Presente, TipoCeldaAsistencia.Borrador)
            : new(dia.Fecha, null, TipoCeldaAsistencia.NoAplicable);
    }

    private Grupo CargarGrupoRequerido(GrupoId grupoId) =>
        _grupos.Cargar(grupoId)
        ?? throw new GrupoNoEncontradoException($"No existe el grupo {grupoId}.");

    private static void ValidarEntradas(
        IReadOnlyCollection<EntradaEstadoAsistencia> entradas,
        HashSet<EstudianteId> esperados)
    {
        var recibidos = new HashSet<EstudianteId>();

        foreach (var entrada in entradas)
        {
            if (entrada is null)
            {
                throw new DomainValidationException("Las entradas de asistencia no pueden ser nulas.");
            }

            if (!Enum.IsDefined(entrada.Estado))
            {
                throw new DomainValidationException("El estado de asistencia no es válido.");
            }

            if (!recibidos.Add(entrada.EstudianteId))
            {
                throw new DomainValidationException(
                    "Cada estudiante debe aparecer exactamente una vez.");
            }
        }

        if (!recibidos.SetEquals(esperados))
        {
            throw new DomainConflictException(
                "Las entradas deben coincidir exactamente con el padrón del día.");
        }
    }

    private static AsistenciaDiaDetalle Proyectar(
        Grupo grupo,
        AsistenciaDiaria asistencia,
        bool esPersistido)
    {
        var estudiantes = grupo.Estudiantes.ToDictionary(x => x.Id);
        var detalles = asistencia.Registros.Select(registro =>
        {
            if (!estudiantes.TryGetValue(registro.EstudianteId, out var estudiante))
            {
                throw new DomainConflictException(
                    "El padrón histórico contiene un estudiante ajeno a la matrícula.");
            }

            return new AsistenciaEstudianteDetalle(
                estudiante.Id,
                estudiante.NombreVisible,
                estudiante.NumeroLista,
                registro.Estado,
                estudiante.EstaActivo);
        })
        .OrderBy(x => x.NumeroLista)
        .ThenBy(x => x.NombreVisible, StringComparer.Ordinal)
        .ThenBy(x => x.EstudianteId.Valor)
        .ToArray();

        return new AsistenciaDiaDetalle(
            grupo.Id,
            asistencia.Fecha,
            esPersistido,
            detalles);
    }
}