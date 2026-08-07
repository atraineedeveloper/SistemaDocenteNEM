using SistemaDocente.Core;
using SistemaDocente.Reporting;

namespace SistemaDocente.Application;

public sealed class GestionReportesCasosUso
{
    private readonly IAlmacenamientoGrupos _grupos;
    private readonly IAlmacenamientoAsistencias _asistencias;
    private readonly IAlmacenamientoProyectos _proyectos;
    private readonly IAlmacenamientoActividadesProyecto _actividades;
    private readonly IAlmacenamientoExpedientes _expedientes;
    private readonly IAlmacenamientoContextoGrupo _contextos;

    public GestionReportesCasosUso(
        IAlmacenamientoGrupos grupos,
        IAlmacenamientoAsistencias asistencias,
        IAlmacenamientoProyectos proyectos,
        IAlmacenamientoActividadesProyecto actividades,
        IAlmacenamientoExpedientes expedientes,
        IAlmacenamientoContextoGrupo contextos)
    {
        _grupos = grupos ?? throw new ArgumentNullException(nameof(grupos));
        _asistencias = asistencias ?? throw new ArgumentNullException(nameof(asistencias));
        _proyectos = proyectos ?? throw new ArgumentNullException(nameof(proyectos));
        _actividades = actividades ?? throw new ArgumentNullException(nameof(actividades));
        _expedientes = expedientes ?? throw new ArgumentNullException(nameof(expedientes));
        _contextos = contextos ?? throw new ArgumentNullException(nameof(contextos));
    }

    public ReporteIndividualAlumno GenerarIndividual(GrupoId grupoId, EstudianteId estudianteId)
    {
        var grupo = CargarGrupo(grupoId);
        var estudiante = grupo.Estudiantes.SingleOrDefault(x => x.Id == estudianteId)
            ?? throw new DomainConflictException("El estudiante no pertenece al grupo.");
        var contexto = _contextos.Cargar(grupoId) ?? ContextoGrupo.Crear(grupoId);
        var fuente = ConstruirFuente(grupo, estudiante);
        return GeneradorReportes.CrearIndividual(contexto, grupo.NombreVisible, fuente);
    }

    public ReporteGrupal GenerarGrupal(GrupoId grupoId)
    {
        var grupo = CargarGrupo(grupoId);
        var contexto = _contextos.Cargar(grupoId) ?? ContextoGrupo.Crear(grupoId);
        var fuentes = grupo.Estudiantes
            .OrderBy(x => x.NumeroLista)
            .ThenBy(x => x.NombreVisible, StringComparer.Ordinal)
            .Select(x => ConstruirFuente(grupo, x))
            .ToArray();
        return GeneradorReportes.CrearGrupal(contexto, grupo.NombreVisible, fuentes);
    }

    private EstudianteReporteFuente ConstruirFuente(Grupo grupo, Estudiante estudiante)
    {
        var asistencias = _asistencias.CargarIntervalo(grupo.Id, DateOnly.MinValue, DateOnly.MaxValue);
        var meses = asistencias
            .Select(a => new
            {
                a.Fecha,
                Registro = a.Registros.SingleOrDefault(r => r.EstudianteId == estudiante.Id),
            })
            .Where(x => x.Registro is not null)
            .GroupBy(x => (x.Fecha.Year, x.Fecha.Month))
            .Select(x => new AsistenciaMesFuente(
                x.Key.Year,
                x.Key.Month,
                x.OrderBy(r => r.Fecha).Select(r => r.Registro!.Estado).ToArray()))
            .OrderBy(x => x.Anio)
            .ThenBy(x => x.Mes)
            .ToArray();

        var actividades = new List<ActividadReporteFuente>();
        foreach (var proyecto in _proyectos.ListarPorGrupo(grupo.Id))
        {
            foreach (var actividad in _actividades.ListarPorProyecto(proyecto.Id))
            {
                if (actividad.Estado == EstadoActividad.Anulada) continue;
                var entrega = actividad.Entregas.FirstOrDefault(x => x.EstudianteId == estudiante.Id);
                if (entrega is null) continue;
                actividades.Add(new ActividadReporteFuente(
                    proyecto.Nombre,
                    actividad.Titulo,
                    actividad.FechaRealizacion,
                    entrega.EstadoEntrega,
                    entrega.NivelLogro,
                    entrega.Observacion));
            }
        }

        var expediente = _expedientes.ObtenerExpediente(estudiante.Id, grupo.Id);
        return new EstudianteReporteFuente(
            estudiante.Id,
            estudiante.NumeroLista,
            estudiante.NombreVisible,
            estudiante.Genero,
            estudiante.Edad,
            estudiante.EstaActivo,
            meses,
            actividades,
            expediente.ObtenerNotasPorTipo(TipoNotaPedagogica.Fortaleza).Select(x => x.Contenido).ToArray(),
            expediente.ObtenerNotasPorTipo(TipoNotaPedagogica.Dificultad).Select(x => x.Contenido).ToArray(),
            expediente.ObtenerNotasPorTipo(TipoNotaPedagogica.ApoyoAplicado).Select(x => x.Contenido).ToArray(),
            expediente.ObtenerNotasPorTipo(TipoNotaPedagogica.ObservacionCronologica).OrderByDescending(x => x.FechaHoraRegistro).Select(x => x.Contenido).ToArray(),
            expediente.Acuerdos.OrderByDescending(x => x.FechaReunion).Select(x => $"{x.Motivo}: {x.AcuerdoConvenido}").ToArray());
    }

    private Grupo CargarGrupo(GrupoId grupoId) =>
        _grupos.Cargar(grupoId) ?? throw new GrupoNoEncontradoException("El grupo no existe.");
}
