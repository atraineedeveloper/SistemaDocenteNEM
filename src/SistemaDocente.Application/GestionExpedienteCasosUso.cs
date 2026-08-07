using SistemaDocente.Core;

namespace SistemaDocente.Application;

public sealed class GestionExpedienteCasosUso
{
    private readonly IAlmacenamientoGrupos _almacenamientoGrupos;
    private readonly IAlmacenamientoAsistencias _almacenamientoAsistencia;
    private readonly IAlmacenamientoProyectos _almacenamientoProyectos;
    private readonly IAlmacenamientoActividadesProyecto _almacenamientoActividades;
    private readonly IAlmacenamientoExpedientes _almacenamientoExpedientes;

    public GestionExpedienteCasosUso(
        IAlmacenamientoGrupos almacenamientoGrupos,
        IAlmacenamientoAsistencias almacenamientoAsistencia,
        IAlmacenamientoProyectos almacenamientoProyectos,
        IAlmacenamientoActividadesProyecto almacenamientoActividades,
        IAlmacenamientoExpedientes almacenamientoExpedientes)
    {
        ArgumentNullException.ThrowIfNull(almacenamientoGrupos);
        ArgumentNullException.ThrowIfNull(almacenamientoAsistencia);
        ArgumentNullException.ThrowIfNull(almacenamientoProyectos);
        ArgumentNullException.ThrowIfNull(almacenamientoActividades);
        ArgumentNullException.ThrowIfNull(almacenamientoExpedientes);

        _almacenamientoGrupos = almacenamientoGrupos;
        _almacenamientoAsistencia = almacenamientoAsistencia;
        _almacenamientoProyectos = almacenamientoProyectos;
        _almacenamientoActividades = almacenamientoActividades;
        _almacenamientoExpedientes = almacenamientoExpedientes;
    }

    public ExpedienteEstudianteDetalle ConsultarExpediente(GrupoId grupoId, EstudianteId estudianteId)
    {
        var grupo = _almacenamientoGrupos.Cargar(grupoId)
            ?? throw new DomainConflictException("El grupo especificado no existe.");

        var estudiante = grupo.Estudiantes.FirstOrDefault(e => e.Id == estudianteId)
            ?? throw new DomainConflictException("El estudiante especificado no existe en el grupo.");

        var asistenciasHistorial = _almacenamientoAsistencia.CargarIntervalo(grupoId, DateOnly.MinValue, DateOnly.MaxValue);
        var registrosEstudiante = asistenciasHistorial
            .SelectMany(a => a.Registros)
            .Where(r => r.EstudianteId == estudianteId)
            .ToArray();

        var totalDias = registrosEstudiante.Length;
        var presentes = registrosEstudiante.Count(r => r.Estado == EstadoAsistencia.Presente);
        var faltas = registrosEstudiante.Count(r => r.Estado == EstadoAsistencia.Falta);
        var retardos = registrosEstudiante.Count(r => r.Estado == EstadoAsistencia.Retardo);
        var justificadas = registrosEstudiante.Count(r => r.Estado == EstadoAsistencia.Justificada);
        var porcentajeAsistencia = totalDias > 0 ? (double)(presentes + retardos) / totalDias * 100.0 : 100.0;
        var resumenAsistencia = new ResumenAsistenciaEstudiante(totalDias, presentes, faltas, retardos, justificadas, porcentajeAsistencia);

        var proyectos = _almacenamientoProyectos.ListarPorGrupo(grupoId);
        var entregas = new List<HistorialEntregaEstudiante>();
        foreach (var p in proyectos)
        {
            var actividades = _almacenamientoActividades.ListarPorProyecto(p.Id);
            foreach (var a in actividades)
            {
                var actDetalle = _almacenamientoActividades.Cargar(a.Id);
                if (actDetalle is not null)
                {
                    var entregaEst = actDetalle.Entregas.FirstOrDefault(e => e.EstudianteId == estudianteId);
                    if (entregaEst is not null)
                    {
                        entregas.Add(new HistorialEntregaEstudiante(
                            p.Nombre,
                            a.Titulo,
                            a.FechaRealizacion,
                            entregaEst.EstadoEntrega,
                            entregaEst.NivelLogro,
                            entregaEst.Observacion));
                    }
                }
            }
        }

        var expediente = _almacenamientoExpedientes.ObtenerExpediente(estudianteId, grupoId);

        var alertas = new List<AlertaPedagogica>();
        if (totalDias >= 5 && porcentajeAsistencia < 80.0)
        {
            alertas.Add(new AlertaPedagogica(NivelGravedadAlerta.AtencionRequerida, $"Asistencia del {porcentajeAsistencia:F1}% (presenta {faltas} faltas registradas)."));
        }

        var reqApoyo = entregas.Count(e => e.NivelLogro == NivelLogro.RequiereApoyo);
        var noEntrego = entregas.Count(e => e.EstadoEntrega == EstadoEntregaActividad.NoEntregada);
        if (reqApoyo > 0 || noEntrego > 0)
        {
            alertas.Add(new AlertaPedagogica(NivelGravedadAlerta.Informativa, $"Presenta {reqApoyo} actividades en nivel 'Requiere apoyo' y {noEntrego} sin entregar."));
        }

        return new ExpedienteEstudianteDetalle(
            estudianteId,
            grupoId,
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
            resumenAsistencia,
            entregas,
            expediente.ObtenerNotasPorTipo(TipoNotaPedagogica.Fortaleza),
            expediente.ObtenerNotasPorTipo(TipoNotaPedagogica.Dificultad),
            expediente.ObtenerNotasPorTipo(TipoNotaPedagogica.ApoyoAplicado),
            expediente.ObtenerNotasPorTipo(TipoNotaPedagogica.ObservacionCronologica),
            expediente.Acuerdos,
            alertas);
    }

    public void RegistrarNotaPedagogica(GrupoId grupoId, EstudianteId estudianteId, TipoNotaPedagogica tipo, string contenido)
    {
        ValidadorContenidoPedagogico.ValidarTextoPedagogico(contenido, nameof(contenido));

        _almacenamientoExpedientes.RegistrarNotaPedagogica(estudianteId, grupoId, tipo, contenido, DateTime.Now);
    }

    public void RegistrarAcuerdoTutor(GrupoId grupoId, EstudianteId estudianteId, string motivo, string acuerdo, DateOnly fechaReunion, DateOnly? fechaSeguimiento)
    {
        ValidadorContenidoPedagogico.ValidarTextoPedagogico(motivo, nameof(motivo));
        ValidadorContenidoPedagogico.ValidarTextoPedagogico(acuerdo, nameof(acuerdo));

        _almacenamientoExpedientes.RegistrarAcuerdoTutor(estudianteId, grupoId, motivo, acuerdo, fechaReunion, fechaSeguimiento);
    }
}