using System.Globalization;

using SistemaDocente.Application;
using SistemaDocente.Core;
using SistemaDocente.Reporting;

namespace SistemaDocente.Cli;

public sealed class EjecutorCli
{
    private readonly string? _localApplicationData;

    public EjecutorCli(string? localApplicationData = null)
    {
        _localApplicationData = localApplicationData;
    }

    public int Ejecutar(IReadOnlyList<string> argumentos, TextWriter salida, TextWriter errores)
    {
        ArgumentNullException.ThrowIfNull(argumentos);
        ArgumentNullException.ThrowIfNull(salida);
        ArgumentNullException.ThrowIfNull(errores);

        var quiereJson = argumentos.Any(x => string.Equals(x, "--json", StringComparison.OrdinalIgnoreCase));
        var modoDemo = argumentos.Any(x => string.Equals(x, "--demo", StringComparison.OrdinalIgnoreCase));
        var comandoSeguro = ResolverNombreComando(argumentos);
        ServiciosCli? servicios = null;

        try
        {
            var args = ArgumentosCli.Analizar(argumentos);
            if (args.Posicion.Count == 0 || args.Tiene("--help"))
            {
                EscribirAyuda(salida);
                return 0;
            }

            var comando = string.Join(' ', args.Posicion).ToLowerInvariant();
            comandoSeguro = comando;
            if (comando == "capabilities")
            {
                return Emitir(
                    salida,
                    quiereJson,
                    Exito(
                        comando,
                        modoDemo,
                        CrearCapacidades(),
                        Privacidad("D0")));
            }

            servicios = new ServiciosCli(modoDemo, _localApplicationData);
            return comando switch
            {
                "status" => EjecutarStatus(args, servicios, salida, quiereJson),
                "groups list" => EjecutarGrupos(args, servicios, salida, quiereJson),
                "students list" => EjecutarEstudiantes(args, servicios, salida, quiereJson),
                "students deactivate" => EjecutarEstadoEstudiante(args, servicios, salida, quiereJson, false),
                "students reactivate" => EjecutarEstadoEstudiante(args, servicios, salida, quiereJson, true),
                "attendance show" => EjecutarAsistenciaMostrar(args, servicios, salida, quiereJson),
                "attendance set" => EjecutarAsistenciaCambiar(args, servicios, salida, quiereJson),
                "agent context" => EjecutarContextoAgente(args, servicios, salida, quiereJson),
                "agent recommend" => EjecutarRecomendaciones(args, servicios, salida, quiereJson),
                _ => throw new ArgumentException("El comando solicitado no está soportado."),
            };
        }
        catch (Exception exception)
        {
            var error = ClasificarError(exception);
            if (error.Code == "internal_error")
            {
                servicios ??= CrearServiciosParaDiagnostico(modoDemo);
                servicios?.Diagnosticos.Registrar(
                    exception,
                    CategoriaEventoDiagnostico.FalloComandoTerminal);
            }

            var respuesta = new RespuestaCli(
                "1",
                comandoSeguro,
                modoDemo ? "demo" : "production",
                false,
                null,
                Privacidad("D0"),
                [],
                error);

            if (quiereJson)
            {
                salida.WriteLine(SerializadorCli.Json(respuesta));
            }
            else
            {
                errores.WriteLine($"{error.Code}: {error.Message}");
            }

            return CodigoSalida(error.Code);
        }
    }

    private ServiciosCli? CrearServiciosParaDiagnostico(bool modoDemo)
    {
        try
        {
            return new ServiciosCli(modoDemo, _localApplicationData);
        }
        catch
        {
            return null;
        }
    }

    private static int EjecutarStatus(
        ArgumentosCli args,
        ServiciosCli servicios,
        TextWriter salida,
        bool json)
    {
        AsegurarSinOpcionesPersonalesInnecesarias(args);
        var grupos = servicios.Grupos.ListarGrupos();
        var data = new
        {
            product = IdentidadProducto.Nombre,
            version = IdentidadProducto.Version,
            mode = servicios.ModoTexto,
            groupCount = grupos.Count,
            storageReady = true,
            networkAccess = false,
        };
        return Emitir(salida, json, Exito("status", servicios.ModoDemostracion, data, Privacidad("D0")));
    }

    private static int EjecutarGrupos(
        ArgumentosCli args,
        ServiciosCli servicios,
        TextWriter salida,
        bool json)
    {
        var incluir = args.Tiene("--include-personal-data");
        var data = servicios.Grupos.ListarGrupos().Select(grupo => new
        {
            groupId = grupo.GrupoId.Valor,
            name = incluir ? grupo.NombreVisible : null,
            studentCount = grupo.Estudiantes.Count,
            activeStudentCount = grupo.Estudiantes.Count(x => x.EstaActivo),
        }).ToArray();
        return Emitir(
            salida,
            json,
            Exito(
                "groups list",
                servicios.ModoDemostracion,
                data,
                Privacidad(incluir ? "D2" : "D1", incluir)));
    }

    private static int EjecutarEstudiantes(
        ArgumentosCli args,
        ServiciosCli servicios,
        TextWriter salida,
        bool json)
    {
        var grupoId = LeerGrupoId(args);
        var incluir = args.Tiene("--include-personal-data");
        var data = servicios.Grupos.ObtenerTodosLosEstudiantes(grupoId).Select(estudiante => new
        {
            studentId = estudiante.EstudianteId.Valor,
            number = estudiante.NumeroLista,
            name = incluir ? estudiante.NombreVisible : null,
            active = estudiante.EstaActivo,
            grade = estudiante.Grado,
        }).ToArray();
        return Emitir(
            salida,
            json,
            Exito(
                "students list",
                servicios.ModoDemostracion,
                data,
                Privacidad("D2", incluir)));
    }

    private static int EjecutarEstadoEstudiante(
        ArgumentosCli args,
        ServiciosCli servicios,
        TextWriter salida,
        bool json,
        bool activar)
    {
        var grupoId = LeerGrupoId(args);
        var estudianteId = LeerEstudianteId(args);
        var aplicar = args.Tiene("--apply");
        var estudiante = servicios.Grupos.ObtenerTodosLosEstudiantes(grupoId)
            .SingleOrDefault(x => x.EstudianteId == estudianteId)
            ?? throw new DomainConflictException("El estudiante indicado no pertenece al grupo.");
        var estadoAnterior = estudiante.EstaActivo;

        if (aplicar && estadoAnterior != activar)
        {
            estudiante = activar
                ? servicios.Grupos.ReactivarEstudiante(grupoId, estudianteId)
                : servicios.Grupos.DesactivarEstudiante(grupoId, estudianteId);
        }

        var data = new
        {
            dryRun = !aplicar,
            applied = aplicar,
            studentId = estudianteId.Valor,
            previousActive = estadoAnterior,
            targetActive = activar,
            resultingActive = aplicar ? estudiante.EstaActivo : activar,
        };
        var command = activar ? "students reactivate" : "students deactivate";
        return Emitir(
            salida,
            json,
            Exito(command, servicios.ModoDemostracion, data, Privacidad("D2"),
                aplicar ? [] : ["Dry run: no se modificaron datos. Use --apply para persistir."]));
    }

    private static int EjecutarAsistenciaMostrar(
        ArgumentosCli args,
        ServiciosCli servicios,
        TextWriter salida,
        bool json)
    {
        var grupoId = LeerGrupoId(args);
        var fecha = LeerFecha(args);
        var incluir = args.Tiene("--include-personal-data");
        var asistencia = servicios.Asistencia.Cargar(grupoId, fecha);
        object data = asistencia is null
            ? new { groupId = grupoId.Valor, date = fecha, exists = false }
            : new
            {
                groupId = grupoId.Valor,
                date = fecha,
                exists = true,
                students = asistencia.Estudiantes.Select(x => new
                {
                    studentId = x.EstudianteId.Valor,
                    number = x.NumeroLista,
                    name = incluir ? x.NombreVisible : null,
                    state = x.Estado,
                    active = x.EstaActivoActualmente,
                }).ToArray(),
            };
        return Emitir(
            salida,
            json,
            Exito(
                "attendance show",
                servicios.ModoDemostracion,
                data,
                Privacidad("D3", incluir)));
    }

    private static int EjecutarAsistenciaCambiar(
        ArgumentosCli args,
        ServiciosCli servicios,
        TextWriter salida,
        bool json)
    {
        var grupoId = LeerGrupoId(args);
        var estudianteId = LeerEstudianteId(args);
        var fecha = LeerFecha(args);
        var estado = LeerEstadoAsistencia(args);
        var aplicar = args.Tiene("--apply");
        var asistencia = servicios.Asistencia.Preparar(grupoId, fecha);
        var actual = asistencia.Estudiantes.SingleOrDefault(x => x.EstudianteId == estudianteId)
            ?? throw new DomainConflictException("El estudiante no pertenece al padrón de asistencia del día.");

        if (aplicar)
        {
            var entradas = asistencia.Estudiantes
                .Select(x => new EntradaEstadoAsistencia(
                    x.EstudianteId,
                    x.EstudianteId == estudianteId ? estado : x.Estado))
                .ToArray();
            servicios.Asistencia.Guardar(grupoId, fecha, entradas);
        }

        var data = new
        {
            dryRun = !aplicar,
            applied = aplicar,
            groupId = grupoId.Valor,
            studentId = estudianteId.Valor,
            date = fecha,
            previousState = actual.Estado,
            targetState = estado,
            persistedBefore = asistencia.EsPersistido,
        };
        return Emitir(
            salida,
            json,
            Exito(
                "attendance set",
                servicios.ModoDemostracion,
                data,
                Privacidad("D3"),
                aplicar ? [] : ["Dry run: no se modificaron datos. Use --apply para persistir."]));
    }

    private static int EjecutarContextoAgente(
        ArgumentosCli args,
        ServiciosCli servicios,
        TextWriter salida,
        bool json)
    {
        var grupoId = LeerGrupoId(args);
        var incluir = args.Tiene("--include-personal-data");
        var contexto = servicios.ContextoAgente.GenerarGrupo(grupoId, incluir);
        return Emitir(
            salida,
            json,
            Exito(
                "agent context",
                servicios.ModoDemostracion,
                contexto,
                Privacidad("D3", incluir)));
    }

    private static int EjecutarRecomendaciones(
        ArgumentosCli args,
        ServiciosCli servicios,
        TextWriter salida,
        bool json)
    {
        if (args.Tiene("--include-personal-data"))
            throw new ArgumentException("agent recommend no necesita datos personales en V1.");
        var grupoId = LeerGrupoId(args);
        var reporte = servicios.Reportes.GenerarGrupal(grupoId);
        var recomendaciones = AnalizadorPracticaDocente.AnalizarGrupo(reporte);
        var data = new
        {
            groupId = grupoId.Valor,
            recommendationCount = recomendaciones.Count,
            recommendations = recomendaciones,
        };
        return Emitir(
            salida,
            json,
            Exito("agent recommend", servicios.ModoDemostracion, data, Privacidad("D1")));
    }

    private static object CrearCapacidades() => new
    {
        schemaVersion = "1",
        commands = new[]
        {
            "status",
            "groups list",
            "students list",
            "students deactivate",
            "students reactivate",
            "attendance show",
            "attendance set",
            "agent context",
            "agent recommend",
        },
        mutationPolicy = "dry-run-unless-apply",
        destructiveDeleteCommands = false,
        acceptsSensitiveFreeTextArguments = false,
        networkAccess = false,
    };

    private static GrupoId LeerGrupoId(ArgumentosCli args) =>
        GrupoId.DesdeGuid(LeerGuidRequerido(args, "--group"));

    private static EstudianteId LeerEstudianteId(ArgumentosCli args) =>
        EstudianteId.DesdeGuid(LeerGuidRequerido(args, "--student"));

    private static Guid LeerGuidRequerido(ArgumentosCli args, string opcion)
    {
        var valor = args.Valor(opcion);
        if (string.IsNullOrWhiteSpace(valor) || !Guid.TryParse(valor, out var guid) || guid == Guid.Empty)
            throw new ArgumentException($"{opcion} requiere un GUID válido.");
        return guid;
    }

    private static DateOnly LeerFecha(ArgumentosCli args)
    {
        var valor = args.Valor("--date");
        if (string.IsNullOrWhiteSpace(valor)
            || !DateOnly.TryParseExact(valor, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fecha))
        {
            throw new ArgumentException("--date requiere formato yyyy-MM-dd.");
        }
        return fecha;
    }

    private static EstadoAsistencia LeerEstadoAsistencia(ArgumentosCli args)
    {
        var valor = args.Valor("--state")?.Trim().ToUpperInvariant();
        return valor switch
        {
            "P" or "PRESENTE" => EstadoAsistencia.Presente,
            "F" or "FALTA" => EstadoAsistencia.Falta,
            "R" or "RETARDO" => EstadoAsistencia.Retardo,
            "J" or "JUSTIFICADA" => EstadoAsistencia.Justificada,
            _ => throw new ArgumentException("--state acepta P, F, R o J."),
        };
    }

    private static void AsegurarSinOpcionesPersonalesInnecesarias(ArgumentosCli args)
    {
        if (args.Tiene("--include-personal-data"))
            throw new ArgumentException("Este comando no necesita --include-personal-data.");
    }

    private static RespuestaCli Exito(
        string command,
        bool demo,
        object? data,
        PrivacidadSalidaCli privacidad,
        IReadOnlyList<string>? warnings = null) =>
        new(
            "1",
            command,
            demo ? "demo" : "production",
            true,
            data,
            privacidad,
            warnings ?? []);

    private static PrivacidadSalidaCli Privacidad(string clasificacion, bool personal = false) =>
        new(clasificacion, personal, false, false);

    private static int Emitir(TextWriter salida, bool json, RespuestaCli respuesta)
    {
        if (json)
        {
            salida.WriteLine(SerializadorCli.Json(respuesta));
        }
        else
        {
            salida.WriteLine($"{IdentidadProducto.Nombre} {IdentidadProducto.VersionVisible} · {respuesta.Command}");
            salida.WriteLine(respuesta.Success ? "Operación completada." : "La operación no pudo completarse.");
            foreach (var warning in respuesta.Warnings) salida.WriteLine($"Aviso: {warning}");
            salida.WriteLine("Use --json para obtener la salida estructurada para agentes.");
        }
        return 0;
    }

    private static ErrorSalidaCli ClasificarError(Exception exception) => exception switch
    {
        ArgumentException or FormatException => new("invalid_arguments", "Los argumentos del comando no son válidos."),
        GrupoNoEncontradoException => new("not_found", "No se encontró el recurso solicitado."),
        DomainValidationException => new("validation_error", "La operación no cumple las reglas de validación de AulaRaíz."),
        DomainConflictException => new("conflict", "La operación entra en conflicto con el estado actual de los datos."),
        ErrorPersistenciaAplicacionException or IOException or UnauthorizedAccessException =>
            new("storage_error", "No fue posible completar la operación sobre el almacenamiento local."),
        _ => new("internal_error", "AulaRaíz no pudo completar el comando. Revisa el diagnóstico local seguro."),
    };

    private static int CodigoSalida(string code) => code switch
    {
        "invalid_arguments" => 2,
        "validation_error" => 3,
        "not_found" => 4,
        "conflict" => 5,
        "storage_error" => 6,
        _ => 1,
    };

    private static string ResolverNombreComando(IReadOnlyList<string> args) =>
        string.Join(' ', args.Where(x => !x.StartsWith("--", StringComparison.Ordinal)).Take(2)).ToLowerInvariant();

    private static void EscribirAyuda(TextWriter salida)
    {
        salida.WriteLine("AulaRaíz CLI");
        salida.WriteLine("Use 'aularaiz capabilities --json' para descubrir la interfaz para agentes.");
        salida.WriteLine("Las mutaciones son dry-run salvo que se indique --apply.");
        salida.WriteLine("Production es el perfil predeterminado; use --demo para el perfil aislado de demostración.");
    }
}