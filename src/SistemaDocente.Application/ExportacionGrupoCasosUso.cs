using System.Globalization;
using System.Text;

using SistemaDocente.Core;

namespace SistemaDocente.Application;

public sealed class ExportacionGrupoCasosUso
{
    private readonly IAlmacenamientoGrupos _grupos;
    private readonly IAlmacenamientoAsistencias _asistencias;
    private readonly IAlmacenamientoProyectos _proyectos;
    private readonly IAlmacenamientoActividadesProyecto _actividades;
    private readonly IAlmacenamientoExpedientes _expedientes;
    private readonly IAlmacenamientoContextoGrupo _contextos;
    private readonly IExportadorTabular _exportador;

    public ExportacionGrupoCasosUso(
        IAlmacenamientoGrupos grupos,
        IAlmacenamientoAsistencias asistencias,
        IAlmacenamientoProyectos proyectos,
        IAlmacenamientoActividadesProyecto actividades,
        IAlmacenamientoExpedientes expedientes,
        IAlmacenamientoContextoGrupo contextos,
        IExportadorTabular exportador)
    {
        _grupos = grupos ?? throw new ArgumentNullException(nameof(grupos));
        _asistencias = asistencias ?? throw new ArgumentNullException(nameof(asistencias));
        _proyectos = proyectos ?? throw new ArgumentNullException(nameof(proyectos));
        _actividades = actividades ?? throw new ArgumentNullException(nameof(actividades));
        _expedientes = expedientes ?? throw new ArgumentNullException(nameof(expedientes));
        _contextos = contextos ?? throw new ArgumentNullException(nameof(contextos));
        _exportador = exportador ?? throw new ArgumentNullException(nameof(exportador));
    }

    public PlanExportacionGrupo Preparar(
        SolicitudExportacionGrupo solicitud,
        DateOnly fechaReferencia)
    {
        ArgumentNullException.ThrowIfNull(solicitud);
        ValidarSolicitud(solicitud);

        var grupo = _grupos.Cargar(solicitud.GrupoId)
            ?? throw new GrupoNoEncontradoException("El grupo no existe.");
        var contexto = _contextos.Cargar(grupo.Id) ?? ContextoGrupo.Crear(grupo.Id);
        var proyectos = FiltrarProyectos(grupo.Id, solicitud.ProyectoId);
        var estudiantes = grupo.Estudiantes.ToDictionary(estudiante => estudiante.Id);

        var hojas = new List<(ConjuntoExportacionGrupo Conjunto, HojaTabularSalida Hoja)>();
        foreach (var conjunto in solicitud.Conjuntos.Distinct())
        {
            var hoja = conjunto switch
            {
                ConjuntoExportacionGrupo.Contexto => CrearContexto(grupo, contexto),
                ConjuntoExportacionGrupo.Alumnos => CrearAlumnos(
                    grupo,
                    solicitud.IncluirObservacionesEstudiante),
                ConjuntoExportacionGrupo.Asistencia => CrearAsistencia(
                    grupo,
                    estudiantes,
                    solicitud.AsistenciaDesde!.Value,
                    solicitud.AsistenciaHasta!.Value),
                ConjuntoExportacionGrupo.Proyectos => CrearProyectos(proyectos),
                ConjuntoExportacionGrupo.Actividades => CrearActividades(proyectos),
                ConjuntoExportacionGrupo.Evaluacion => CrearEvaluacion(
                    proyectos,
                    estudiantes,
                    solicitud.IncluirObservacionesEvaluacion),
                ConjuntoExportacionGrupo.Seguimiento => CrearSeguimiento(grupo),
                _ => throw new DomainValidationException("El conjunto de exportación no es válido."),
            };
            hojas.Add((conjunto, hoja));
        }

        var resumen = hojas
            .Select(item => new ResumenConjuntoExportado(
                item.Conjunto,
                item.Hoja.Nombre,
                item.Hoja.Filas.Count))
            .ToArray();
        var contieneDatosSensibles = solicitud.IncluirObservacionesEstudiante
            || solicitud.IncluirObservacionesEvaluacion
            || solicitud.Conjuntos.Contains(ConjuntoExportacionGrupo.Seguimiento);
        var nombreArchivo = CrearNombreArchivoSugerido(
            grupo,
            contexto,
            solicitud,
            fechaReferencia);

        return new PlanExportacionGrupo(
            grupo.Id,
            grupo.NombreVisible,
            solicitud.Formato,
            nombreArchivo,
            new DocumentoTabularSalida(hojas.Select(item => item.Hoja).ToArray()),
            resumen,
            contieneDatosSensibles);
    }

    public ResultadoExportacionGrupo Exportar(
        PlanExportacionGrupo plan,
        string rutaArchivo)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentException.ThrowIfNullOrWhiteSpace(rutaArchivo);

        _exportador.Exportar(plan.Documento, rutaArchivo, plan.Formato);
        return new ResultadoExportacionGrupo(
            Path.GetFullPath(rutaArchivo),
            plan.GrupoId,
            plan.NombreGrupo,
            plan.Formato,
            plan.Conjuntos,
            plan.ContieneDatosSensibles);
    }

    private static void ValidarSolicitud(SolicitudExportacionGrupo solicitud)
    {
        if (solicitud.GrupoId == default)
        {
            throw new DomainValidationException("Selecciona un grupo para exportar.");
        }

        if (!Enum.IsDefined(solicitud.Formato))
        {
            throw new DomainValidationException("El formato de exportación no es válido.");
        }

        if (solicitud.Conjuntos.Count == 0 || solicitud.Conjuntos.Any(conjunto => !Enum.IsDefined(conjunto)))
        {
            throw new DomainValidationException("Selecciona al menos un conjunto de datos válido.");
        }

        if (solicitud.Formato == FormatoExportacionTabular.Csv && solicitud.Conjuntos.Distinct().Count() != 1)
        {
            throw new DomainValidationException("CSV permite exportar exactamente un conjunto de datos por archivo.");
        }

        if (solicitud.Conjuntos.Contains(ConjuntoExportacionGrupo.Asistencia))
        {
            if (!solicitud.AsistenciaDesde.HasValue || !solicitud.AsistenciaHasta.HasValue)
            {
                throw new DomainValidationException("Selecciona el periodo de asistencia que deseas exportar.");
            }

            if (solicitud.AsistenciaDesde > solicitud.AsistenciaHasta)
            {
                throw new DomainValidationException("La fecha inicial de asistencia no puede ser posterior a la final.");
            }
        }
    }

    private ProyectoDidactico[] FiltrarProyectos(
        GrupoId grupoId,
        ProyectoId? proyectoId)
    {
        var proyectos = _proyectos.ListarPorGrupo(grupoId);
        if (!proyectoId.HasValue)
        {
            return proyectos.OrderBy(proyecto => proyecto.FechaInicio).ThenBy(proyecto => proyecto.Nombre).ToArray();
        }

        var filtrado = proyectos.Where(proyecto => proyecto.Id == proyectoId.Value).ToArray();
        if (filtrado.Length == 0)
        {
            throw new DomainConflictException("El proyecto seleccionado no pertenece al grupo.");
        }

        return filtrado;
    }

    private static HojaTabularSalida CrearContexto(Grupo grupo, ContextoGrupo contexto)
    {
        var filas = new List<FilaTabularSalida>
        {
            Fila("Grupo", grupo.NombreVisible),
            Fila("Escuela", contexto.NombreEscuela),
            Fila("CCT", contexto.Cct),
            Fila("Entidad federativa", contexto.EntidadFederativa),
            Fila("Municipio / alcaldía", contexto.Municipio),
            Fila("Localidad", contexto.Localidad),
            Fila("Organización escolar", FormatearOrganizacion(contexto.OrganizacionEscolar)),
            Fila("Ciclo escolar", contexto.CicloEscolar),
            Fila("Clave de grupo", contexto.Grupo),
            Fila("Turno", contexto.Turno),
            Fila("Grados atendidos", contexto.GradosTexto),
            Fila("Modalidad", contexto.ModalidadGrupo),
            Fila("Fase(s) NEM", contexto.FasesNemTexto),
            Fila("Docente responsable", contexto.DocenteResponsable),
            Fila("Responsable desde", contexto.ResponsableDesde),
            Fila("Responsable hasta", contexto.ResponsableHasta),
            Fila("Hora de entrada", contexto.HoraEntrada?.ToString("HH:mm", CultureInfo.InvariantCulture) ?? string.Empty),
            Fila("Hora de salida", contexto.HoraSalida?.ToString("HH:mm", CultureInfo.InvariantCulture) ?? string.Empty),
        };

        return new HojaTabularSalida(
            "Contexto",
            [new("Campo"), new("Valor")],
            filas);
    }

    private static HojaTabularSalida CrearAlumnos(
        Grupo grupo,
        bool incluirObservaciones)
    {
        var columnas = new List<ColumnaTabularSalida>
        {
            new("Número de lista"),
            new("Nombre"),
            new("Primer apellido"),
            new("Segundo apellido"),
            new("Nombres"),
            new("Grado"),
            new("Fecha de nacimiento"),
            new("Edad"),
            new("Género"),
            new("Fecha de ingreso"),
            new("Estado"),
        };
        if (incluirObservaciones)
        {
            columnas.Add(new("Observaciones pedagógicas"));
        }

        var filas = grupo.Estudiantes
            .OrderBy(estudiante => estudiante.NumeroLista)
            .ThenBy(estudiante => estudiante.NombreVisible, StringComparer.CurrentCulture)
            .Select(estudiante =>
            {
                var celdas = new List<CeldaTabularSalida>
                {
                    CeldaTabularSalida.DesdeNumero(estudiante.NumeroLista),
                    CeldaTabularSalida.DesdeTexto(estudiante.NombreVisible),
                    CeldaTabularSalida.DesdeTexto(estudiante.PrimerApellido),
                    CeldaTabularSalida.DesdeTexto(estudiante.SegundoApellido),
                    CeldaTabularSalida.DesdeTexto(estudiante.Nombres),
                    CeldaTabularSalida.DesdeTexto(CatalogoNemPrimaria.FormatearGrado(estudiante.Grado)),
                    Celda(estudiante.FechaNacimiento),
                    estudiante.Edad.HasValue
                        ? CeldaTabularSalida.DesdeNumero(estudiante.Edad.Value)
                        : CeldaTabularSalida.Vacia,
                    CeldaTabularSalida.DesdeTexto(FormatearGenero(estudiante.Genero)),
                    Celda(estudiante.FechaIngreso),
                    CeldaTabularSalida.DesdeTexto(estudiante.EstaActivo ? "Activo" : "Inactivo"),
                };
                if (incluirObservaciones)
                {
                    celdas.Add(CeldaTabularSalida.DesdeTexto(estudiante.Observaciones));
                }

                return new FilaTabularSalida(celdas);
            })
            .ToArray();

        return new HojaTabularSalida("Alumnos", columnas, filas);
    }

    private HojaTabularSalida CrearAsistencia(
        Grupo grupo,
        Dictionary<EstudianteId, Estudiante> estudiantes,
        DateOnly desde,
        DateOnly hasta)
    {
        var filas = _asistencias.CargarIntervalo(grupo.Id, desde, hasta)
            .OrderBy(asistencia => asistencia.Fecha)
            .SelectMany(asistencia => asistencia.Registros.Select(registro =>
            {
                estudiantes.TryGetValue(registro.EstudianteId, out var estudiante);
                return FilaTabularSalida.Crear(
                    CeldaTabularSalida.DesdeFecha(asistencia.Fecha),
                    estudiante is null
                        ? CeldaTabularSalida.Vacia
                        : CeldaTabularSalida.DesdeNumero(estudiante.NumeroLista),
                    CeldaTabularSalida.DesdeTexto(estudiante?.NombreVisible ?? "Estudiante histórico"),
                    CeldaTabularSalida.DesdeTexto(estudiante is null
                        ? string.Empty
                        : CatalogoNemPrimaria.FormatearGrado(estudiante.Grado)),
                    CeldaTabularSalida.DesdeTexto(FormatearAsistencia(registro.Estado)));
            }))
            .ToArray();

        return new HojaTabularSalida(
            "Asistencia",
            [new("Fecha"), new("Número de lista"), new("Alumno"), new("Grado"), new("Estado")],
            filas);
    }

    private static HojaTabularSalida CrearProyectos(IReadOnlyList<ProyectoDidactico> proyectos)
    {
        var filas = proyectos.Select(proyecto => FilaTabularSalida.Crear(
            CeldaTabularSalida.DesdeTexto(proyecto.Nombre),
            CeldaTabularSalida.DesdeTexto(proyecto.Descripcion),
            CeldaTabularSalida.DesdeFecha(proyecto.FechaInicio),
            CeldaTabularSalida.DesdeFecha(proyecto.FechaTermino),
            CeldaTabularSalida.DesdeTexto(FormatearEstadoProyecto(proyecto.Estado)),
            CeldaTabularSalida.DesdeTexto(CatalogoPlaneacionNem.FormatearMetodologia(proyecto.Metodologia)),
            CeldaTabularSalida.DesdeTexto(CatalogoNemPrimaria.FormatearGrados(proyecto.GradosObjetivo)),
            CeldaTabularSalida.DesdeTexto(proyecto.Observaciones)))
            .ToArray();

        return new HojaTabularSalida(
            "Proyectos",
            [
                new("Proyecto"), new("Descripción"), new("Fecha de inicio"), new("Fecha de término"),
                new("Estado"), new("Metodología NEM"), new("Grados objetivo"), new("Observaciones"),
            ],
            filas);
    }

    private HojaTabularSalida CrearActividades(IReadOnlyList<ProyectoDidactico> proyectos)
    {
        var filas = proyectos.SelectMany(proyecto =>
            _actividades.ListarPorProyecto(proyecto.Id)
                .OrderBy(actividad => actividad.FechaRealizacion)
                .Select(actividad => FilaTabularSalida.Crear(
                    CeldaTabularSalida.DesdeTexto(proyecto.Nombre),
                    CeldaTabularSalida.DesdeTexto(actividad.Titulo),
                    CeldaTabularSalida.DesdeTexto(actividad.Descripcion),
                    CeldaTabularSalida.DesdeFecha(actividad.FechaRealizacion),
                    CeldaTabularSalida.DesdeTexto(actividad.Estado == EstadoActividad.Activa ? "Activa" : "Anulada"),
                    CeldaTabularSalida.DesdeTexto(CatalogoPlaneacionNem.FormatearCampo(actividad.CampoFormativo)),
                    CeldaTabularSalida.DesdeTexto(CatalogoNemPrimaria.FormatearGrados(actividad.GradosObjetivo)),
                    CeldaTabularSalida.DesdeTexto(actividad.ObservacionesGenerales))))
            .ToArray();

        return new HojaTabularSalida(
            "Actividades",
            [
                new("Proyecto"), new("Actividad"), new("Descripción"), new("Fecha"), new("Estado"),
                new("Campo formativo"), new("Grados objetivo"), new("Observaciones generales"),
            ],
            filas);
    }

    private HojaTabularSalida CrearEvaluacion(
        IReadOnlyList<ProyectoDidactico> proyectos,
        Dictionary<EstudianteId, Estudiante> estudiantes,
        bool incluirObservaciones)
    {
        var columnas = new List<ColumnaTabularSalida>
        {
            new("Proyecto"),
            new("Actividad"),
            new("Fecha"),
            new("Número de lista"),
            new("Alumno"),
            new("Grado"),
            new("Resultado"),
            new("Estado de entrega"),
            new("Nivel de logro"),
        };
        if (incluirObservaciones)
        {
            columnas.Add(new("Observación de evaluación"));
        }

        var filas = new List<FilaTabularSalida>();
        foreach (var proyecto in proyectos)
        {
            foreach (var actividad in _actividades.ListarPorProyecto(proyecto.Id).OrderBy(x => x.FechaRealizacion))
            {
                if (actividad.Estado == EstadoActividad.Anulada)
                {
                    continue;
                }

                foreach (var entrega in actividad.Entregas)
                {
                    estudiantes.TryGetValue(entrega.EstudianteId, out var estudiante);
                    var celdas = new List<CeldaTabularSalida>
                    {
                        CeldaTabularSalida.DesdeTexto(proyecto.Nombre),
                        CeldaTabularSalida.DesdeTexto(actividad.Titulo),
                        CeldaTabularSalida.DesdeFecha(actividad.FechaRealizacion),
                        estudiante is null
                            ? CeldaTabularSalida.Vacia
                            : CeldaTabularSalida.DesdeNumero(estudiante.NumeroLista),
                        CeldaTabularSalida.DesdeTexto(estudiante?.NombreVisible ?? "Estudiante histórico"),
                        CeldaTabularSalida.DesdeTexto(estudiante is null
                            ? string.Empty
                            : CatalogoNemPrimaria.FormatearGrado(estudiante.Grado)),
                        CeldaTabularSalida.DesdeTexto(FormatearResultado(entrega.EstadoEntrega, entrega.NivelLogro)),
                        CeldaTabularSalida.DesdeTexto(FormatearEntrega(entrega.EstadoEntrega)),
                        CeldaTabularSalida.DesdeTexto(FormatearNivel(entrega.NivelLogro)),
                    };
                    if (incluirObservaciones)
                    {
                        celdas.Add(CeldaTabularSalida.DesdeTexto(entrega.Observacion));
                    }

                    filas.Add(new FilaTabularSalida(celdas));
                }
            }
        }

        return new HojaTabularSalida("Evaluacion", columnas, filas);
    }

    private HojaTabularSalida CrearSeguimiento(Grupo grupo)
    {
        var filas = new List<FilaTabularSalida>();
        foreach (var estudiante in grupo.Estudiantes.OrderBy(x => x.NumeroLista))
        {
            var expediente = _expedientes.ObtenerExpediente(estudiante.Id, grupo.Id);
            foreach (var nota in expediente.Notas.OrderBy(nota => nota.FechaHoraRegistro))
            {
                filas.Add(FilaTabularSalida.Crear(
                    CeldaTabularSalida.DesdeNumero(estudiante.NumeroLista),
                    CeldaTabularSalida.DesdeTexto(estudiante.NombreVisible),
                    CeldaTabularSalida.DesdeTexto("Nota pedagógica"),
                    CeldaTabularSalida.DesdeTexto(FormatearTipoNota(nota.Tipo)),
                    CeldaTabularSalida.DesdeFecha(DateOnly.FromDateTime(nota.FechaHoraRegistro)),
                    CeldaTabularSalida.DesdeTexto(nota.Contenido),
                    CeldaTabularSalida.Vacia));
            }

            foreach (var acuerdo in expediente.Acuerdos.OrderBy(acuerdo => acuerdo.FechaReunion))
            {
                filas.Add(FilaTabularSalida.Crear(
                    CeldaTabularSalida.DesdeNumero(estudiante.NumeroLista),
                    CeldaTabularSalida.DesdeTexto(estudiante.NombreVisible),
                    CeldaTabularSalida.DesdeTexto("Acuerdo con tutor"),
                    CeldaTabularSalida.DesdeTexto(acuerdo.Motivo),
                    CeldaTabularSalida.DesdeFecha(acuerdo.FechaReunion),
                    CeldaTabularSalida.DesdeTexto(acuerdo.AcuerdoConvenido),
                    Celda(acuerdo.FechaSeguimiento)));
            }
        }

        return new HojaTabularSalida(
            "Seguimiento",
            [
                new("Número de lista"), new("Alumno"), new("Tipo de registro"), new("Categoría / motivo"),
                new("Fecha"), new("Contenido / acuerdo"), new("Fecha de seguimiento"),
            ],
            filas);
    }

    private static string CrearNombreArchivoSugerido(
        Grupo grupo,
        ContextoGrupo contexto,
        SolicitudExportacionGrupo solicitud,
        DateOnly fechaReferencia)
    {
        var extension = solicitud.Formato == FormatoExportacionTabular.Xlsx ? ".xlsx" : ".csv";
        var identidad = contexto.GradosAtendidos.Count > 1
            ? "Multigrado_" + string.Join('-', contexto.GradosAtendidos.Select(grado => (int)grado))
            : CrearIdentidadUnigrado(contexto, grupo.NombreVisible);
        var ciclo = string.IsNullOrWhiteSpace(contexto.CicloEscolar) ? "SinCiclo" : contexto.CicloEscolar;
        var prefijo = solicitud.Formato == FormatoExportacionTabular.Csv
            ? solicitud.Conjuntos.Single().ToString()
            : "Grupo";
        var bruto = $"{prefijo}_{identidad}_{ciclo}_{fechaReferencia:yyyy-MM-dd}{extension}";
        return SanitizarNombreArchivo(bruto);
    }

    private static string CrearIdentidadUnigrado(ContextoGrupo contexto, string nombreVisible)
    {
        if (contexto.GradosAtendidos.Count == 1)
        {
            var grado = ((int)contexto.GradosAtendidos[0]).ToString(CultureInfo.InvariantCulture);
            var grupo = string.IsNullOrWhiteSpace(contexto.Grupo) ? string.Empty : contexto.Grupo.Trim();
            return grado + grupo;
        }

        return string.IsNullOrWhiteSpace(nombreVisible) ? "Grupo" : nombreVisible;
    }

    private static string SanitizarNombreArchivo(string valor)
    {
        var invalidos = Path.GetInvalidFileNameChars().ToHashSet();
        var builder = new StringBuilder(valor.Length);
        foreach (var caracter in valor)
        {
            builder.Append(invalidos.Contains(caracter) ? '_' : caracter);
        }

        return builder.ToString()
            .Replace(' ', '_')
            .Replace("__", "_", StringComparison.Ordinal);
    }

    private static FilaTabularSalida Fila(string campo, string? valor) =>
        FilaTabularSalida.Crear(
            CeldaTabularSalida.DesdeTexto(campo),
            CeldaTabularSalida.DesdeTexto(valor));

    private static FilaTabularSalida Fila(string campo, DateOnly? valor) =>
        FilaTabularSalida.Crear(
            CeldaTabularSalida.DesdeTexto(campo),
            Celda(valor));

    private static CeldaTabularSalida Celda(DateOnly? valor) => valor.HasValue
        ? CeldaTabularSalida.DesdeFecha(valor.Value)
        : CeldaTabularSalida.Vacia;

    private static string FormatearOrganizacion(OrganizacionEscolar organizacion) => organizacion switch
    {
        OrganizacionEscolar.Unitaria => "Unitaria / unidocente",
        OrganizacionEscolar.Bidocente => "Bidocente",
        OrganizacionEscolar.Tridocente => "Tridocente",
        OrganizacionEscolar.Tetradocente => "Tetradocente",
        OrganizacionEscolar.Pentadocente => "Pentadocente",
        OrganizacionEscolar.Completa => "Organización completa",
        _ => "No especificada",
    };

    private static string FormatearGenero(GeneroEstudiante genero) => genero switch
    {
        GeneroEstudiante.Hombre => "Hombre",
        GeneroEstudiante.Mujer => "Mujer",
        _ => "No especificado",
    };

    private static string FormatearAsistencia(EstadoAsistencia estado) => estado switch
    {
        EstadoAsistencia.Presente => "Presente",
        EstadoAsistencia.Falta => "Falta",
        EstadoAsistencia.Retardo => "Retardo",
        EstadoAsistencia.Justificada => "Falta justificada",
        _ => "No especificado",
    };

    private static string FormatearEstadoProyecto(EstadoProyecto estado) => estado switch
    {
        EstadoProyecto.Borrador => "Borrador",
        EstadoProyecto.EnCurso => "En curso",
        EstadoProyecto.Finalizado => "Finalizado",
        _ => "No especificado",
    };

    private static string FormatearEntrega(EstadoEntregaActividad estado) => estado switch
    {
        EstadoEntregaActividad.Pendiente => "Pendiente",
        EstadoEntregaActividad.Entregada => "Entregada",
        EstadoEntregaActividad.NoEntregada => "No entregada",
        _ => "No especificada",
    };

    private static string FormatearNivel(NivelLogro nivel) => nivel switch
    {
        NivelLogro.Pendiente => "Pendiente",
        NivelLogro.Domina => "Domina",
        NivelLogro.Suficiente => "Suficiente",
        NivelLogro.EnProceso => "En proceso",
        NivelLogro.RequiereApoyo => "Requiere apoyo",
        NivelLogro.NoEntrego => "No entregó (legacy)",
        _ => "No especificado",
    };

    private static string FormatearResultado(
        EstadoEntregaActividad estado,
        NivelLogro nivel)
    {
        if (estado == EstadoEntregaActividad.NoEntregada || nivel == NivelLogro.NoEntrego)
        {
            return "No entregó";
        }

        if (estado == EstadoEntregaActividad.Entregada && nivel == NivelLogro.Pendiente)
        {
            return "Entregada · pendiente de evaluación";
        }

        if (nivel != NivelLogro.Pendiente)
        {
            return FormatearNivel(nivel);
        }

        return "Pendiente";
    }

    private static string FormatearTipoNota(TipoNotaPedagogica tipo) => tipo switch
    {
        TipoNotaPedagogica.Fortaleza => "Fortaleza",
        TipoNotaPedagogica.Dificultad => "Dificultad",
        TipoNotaPedagogica.ApoyoAplicado => "Apoyo aplicado",
        TipoNotaPedagogica.ObservacionCronologica => "Observación cronológica",
        _ => "Nota pedagógica",
    };
}
