using System.Globalization;

using Microsoft.Data.Sqlite;

using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Data;

public sealed class PersistenciaProyectosSqlite : IAlmacenamientoProyectos, IAlmacenamientoActividadesProyecto
{
    private readonly string _ruta;
    private readonly string _cadena;

    public PersistenciaProyectosSqlite(string rutaArchivo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rutaArchivo);
        _ruta = Path.GetFullPath(rutaArchivo);
        _cadena = new SqliteConnectionStringBuilder
        {
            DataSource = _ruta,
            Mode = SqliteOpenMode.ReadWriteCreate,
            DefaultTimeout = 15,
            Pooling = false,
        }.ToString();
    }

    public ProyectoDidactico? Cargar(ProyectoId proyectoId) => Ejecutar(() =>
    {
        using var conexion = Abrir();
        return CargarProyecto(conexion, proyectoId);
    });

    public IReadOnlyList<ProyectoDidactico> ListarPorGrupo(GrupoId grupoId) =>
        Ejecutar<IReadOnlyList<ProyectoDidactico>>(() =>
        {
            using var conexion = Abrir();
            using var comando = conexion.CreateCommand();
            comando.CommandText = """
                SELECT proyecto_id
                FROM proyectos_didacticos
                WHERE grupo_id=$grupo
                ORDER BY estado, fecha_inicio DESC, nombre, proyecto_id
                """;
            comando.Parameters.AddWithValue("$grupo", grupoId.ToString());
            using var lector = comando.ExecuteReader();
            var ids = new List<ProyectoId>();
            while (lector.Read())
            {
                ids.Add(ProyectoId.DesdeGuid(Guid.Parse(lector.GetString(0))));
            }
            lector.Close();
            return ids.Select(id => CargarProyecto(conexion, id)!).ToArray();
        });

    public void Guardar(ProyectoDidactico proyecto, int? versionEsperada) => Ejecutar(() =>
    {
        using var conexion = Abrir();
        using var transaccion = conexion.BeginTransaction();
        using var comando = conexion.CreateCommand();
        comando.Transaction = transaccion;
        if (versionEsperada is null)
        {
            comando.CommandText = """
                INSERT INTO proyectos_didacticos(
                    proyecto_id,grupo_id,nombre,descripcion,fecha_inicio,fecha_termino,estado,observaciones,version)
                VALUES($id,$grupo,$nombre,$descripcion,$inicio,$termino,$estado,$observaciones,1)
                """;
        }
        else
        {
            comando.CommandText = """
                UPDATE proyectos_didacticos
                SET nombre=$nombre,
                    descripcion=$descripcion,
                    fecha_inicio=$inicio,
                    fecha_termino=$termino,
                    estado=$estado,
                    observaciones=$observaciones,
                    version=version+1
                WHERE proyecto_id=$id AND grupo_id=$grupo AND version=$version
                """;
            comando.Parameters.AddWithValue("$version", versionEsperada.Value);
        }

        ParametrosProyecto(comando, proyecto);
        if (comando.ExecuteNonQuery() != 1)
        {
            throw new ConflictoConcurrenciaException("El proyecto fue modificado por otra operación.");
        }

        GuardarMetadatosProyecto(conexion, transaccion, proyecto);
        transaccion.Commit();
    });

    public IReadOnlyList<DateOnly> FechasActividadesFueraDeRango(
        ProyectoId proyectoId,
        DateOnly inicio,
        DateOnly termino) =>
        Ejecutar<IReadOnlyList<DateOnly>>(() =>
        {
            using var conexion = Abrir();
            using var comando = conexion.CreateCommand();
            comando.CommandText = """
                SELECT fecha_realizacion
                FROM actividades_proyecto
                WHERE proyecto_id=$id
                  AND (fecha_realizacion < $inicio OR fecha_realizacion > $termino)
                ORDER BY fecha_realizacion
                """;
            comando.Parameters.AddWithValue("$id", proyectoId.ToString());
            comando.Parameters.AddWithValue("$inicio", Fecha(inicio));
            comando.Parameters.AddWithValue("$termino", Fecha(termino));
            using var lector = comando.ExecuteReader();
            var fechas = new List<DateOnly>();
            while (lector.Read())
            {
                fechas.Add(LeerFecha(lector.GetString(0)));
            }
            return fechas.ToArray();
        });

    public bool TieneActividades(ProyectoId proyectoId) => Ejecutar(() =>
    {
        using var conexion = Abrir();
        using var comando = conexion.CreateCommand();
        comando.CommandText = "SELECT EXISTS(SELECT 1 FROM actividades_proyecto WHERE proyecto_id=$id)";
        comando.Parameters.AddWithValue("$id", proyectoId.ToString());
        return Convert.ToInt64(comando.ExecuteScalar(), CultureInfo.InvariantCulture) == 1;
    });

    public void Eliminar(ProyectoId proyectoId, int versionEsperada) => Ejecutar(() =>
    {
        using var conexion = Abrir();
        using var comando = conexion.CreateCommand();
        comando.CommandText = """
            DELETE FROM proyectos_didacticos
            WHERE proyecto_id=$id
              AND version=$version
              AND estado=0
              AND NOT EXISTS(
                  SELECT 1 FROM actividades_proyecto WHERE proyecto_id=$id)
            """;
        comando.Parameters.AddWithValue("$id", proyectoId.ToString());
        comando.Parameters.AddWithValue("$version", versionEsperada);
        if (comando.ExecuteNonQuery() != 1)
        {
            throw new ConflictoConcurrenciaException("El proyecto no puede eliminarse o cambió.");
        }
    });

    ActividadProyecto? IAlmacenamientoActividadesProyecto.Cargar(ActividadId actividadId) =>
        CargarActividad(actividadId);

    public IReadOnlyList<ActividadProyecto> ListarPorProyecto(ProyectoId proyectoId) =>
        Ejecutar<IReadOnlyList<ActividadProyecto>>(() =>
        {
            using var conexion = Abrir();
            using var comando = conexion.CreateCommand();
            comando.CommandText = """
                SELECT actividad_id
                FROM actividades_proyecto
                WHERE proyecto_id=$id
                ORDER BY fecha_realizacion,titulo,actividad_id
                """;
            comando.Parameters.AddWithValue("$id", proyectoId.ToString());
            using var lector = comando.ExecuteReader();
            var ids = new List<ActividadId>();
            while (lector.Read())
            {
                ids.Add(ActividadId.DesdeGuid(Guid.Parse(lector.GetString(0))));
            }
            lector.Close();
            return ids.Select(id => CargarActividad(conexion, id)!).ToArray();
        });

    public void Guardar(ActividadProyecto actividad, int? versionEsperada) => Ejecutar(() =>
    {
        using var conexion = Abrir();
        using var transaccion = conexion.BeginTransaction();
        using var comando = conexion.CreateCommand();
        comando.Transaction = transaccion;
        if (versionEsperada is null)
        {
            comando.CommandText = """
                INSERT INTO actividades_proyecto(
                    actividad_id,proyecto_id,grupo_id,titulo,descripcion,fecha_realizacion,
                    observaciones_generales,estado,version)
                VALUES($id,$proyecto,$grupo,$titulo,$descripcion,$fecha,$observaciones,$estado,1)
                """;
        }
        else
        {
            comando.CommandText = """
                UPDATE actividades_proyecto
                SET titulo=$titulo,
                    descripcion=$descripcion,
                    fecha_realizacion=$fecha,
                    observaciones_generales=$observaciones,
                    estado=$estado,
                    version=version+1
                WHERE actividad_id=$id
                  AND proyecto_id=$proyecto
                  AND grupo_id=$grupo
                  AND version=$version
                """;
            comando.Parameters.AddWithValue("$version", versionEsperada.Value);
        }

        ParametrosActividad(comando, actividad);
        if (comando.ExecuteNonQuery() != 1)
        {
            throw new ConflictoConcurrenciaException("La actividad fue modificada por otra operación.");
        }

        foreach (var entrega in actividad.Entregas)
        {
            using var detalle = conexion.CreateCommand();
            detalle.Transaction = transaccion;
            detalle.CommandText = """
                INSERT INTO entregas_actividad(
                    actividad_id,estudiante_id,grupo_id,estado_entrega,observacion)
                VALUES($actividad,$estudiante,$grupo,$nivel,$observacion)
                ON CONFLICT(actividad_id,estudiante_id) DO UPDATE SET
                    estado_entrega=excluded.estado_entrega,
                    observacion=excluded.observacion
                """;
            detalle.Parameters.AddWithValue("$actividad", actividad.Id.ToString());
            detalle.Parameters.AddWithValue("$estudiante", entrega.EstudianteId.ToString());
            detalle.Parameters.AddWithValue("$grupo", actividad.GrupoId.ToString());
            detalle.Parameters.AddWithValue("$nivel", (int)entrega.NivelLogro);
            detalle.Parameters.AddWithValue("$observacion", entrega.Observacion);
            detalle.ExecuteNonQuery();

            using var estadoEntrega = conexion.CreateCommand();
            estadoEntrega.Transaction = transaccion;
            estadoEntrega.CommandText = """
                INSERT INTO estados_entrega_actividad(actividad_id,estudiante_id,estado_entrega)
                VALUES($actividad,$estudiante,$estado)
                ON CONFLICT(actividad_id,estudiante_id) DO UPDATE SET
                    estado_entrega=excluded.estado_entrega
                """;
            estadoEntrega.Parameters.AddWithValue("$actividad", actividad.Id.ToString());
            estadoEntrega.Parameters.AddWithValue("$estudiante", entrega.EstudianteId.ToString());
            estadoEntrega.Parameters.AddWithValue("$estado", (int)entrega.EstadoEntrega);
            estadoEntrega.ExecuteNonQuery();
        }

        GuardarMetadatosActividad(conexion, transaccion, actividad);
        transaccion.Commit();
    });

    public void Eliminar(ActividadId actividadId, int versionEsperada) => Ejecutar(() =>
    {
        using var conexion = Abrir();
        using var transaccion = conexion.BeginTransaction();
        using var entregas = conexion.CreateCommand();
        entregas.Transaction = transaccion;
        entregas.CommandText = """
            DELETE FROM entregas_actividad
            WHERE actividad_id=$id
              AND NOT EXISTS(
                  SELECT 1
                  FROM entregas_actividad e
                  LEFT JOIN estados_entrega_actividad s
                    ON s.actividad_id=e.actividad_id AND s.estudiante_id=e.estudiante_id
                  WHERE e.actividad_id=$id
                    AND (e.estado_entrega<>0 OR COALESCE(s.estado_entrega,0)<>0))
              AND EXISTS(
                  SELECT 1 FROM actividades_proyecto
                  WHERE actividad_id=$id AND version=$version)
            """;
        entregas.Parameters.AddWithValue("$id", actividadId.ToString());
        entregas.Parameters.AddWithValue("$version", versionEsperada);
        entregas.ExecuteNonQuery();

        using var actividad = conexion.CreateCommand();
        actividad.Transaction = transaccion;
        actividad.CommandText = """
            DELETE FROM actividades_proyecto
            WHERE actividad_id=$id
              AND version=$version
              AND NOT EXISTS(
                  SELECT 1 FROM entregas_actividad WHERE actividad_id=$id)
            """;
        actividad.Parameters.AddWithValue("$id", actividadId.ToString());
        actividad.Parameters.AddWithValue("$version", versionEsperada);
        if (actividad.ExecuteNonQuery() != 1)
        {
            throw new ConflictoConcurrenciaException("La actividad no puede eliminarse o cambió.");
        }
        transaccion.Commit();
    });

    private ActividadProyecto? CargarActividad(ActividadId id) => Ejecutar(() =>
    {
        using var conexion = Abrir();
        return CargarActividad(conexion, id);
    });

    private static ProyectoDidactico? CargarProyecto(SqliteConnection conexion, ProyectoId id)
    {
        using var comando = conexion.CreateCommand();
        comando.CommandText = """
            SELECT p.proyecto_id,
                   p.grupo_id,
                   p.nombre,
                   p.descripcion,
                   p.fecha_inicio,
                   p.fecha_termino,
                   p.estado,
                   p.observaciones,
                   p.version,
                   COALESCE(n.metodologia,0)
            FROM proyectos_didacticos p
            LEFT JOIN proyectos_nem n ON n.proyecto_id=p.proyecto_id
            WHERE p.proyecto_id=$id
            """;
        comando.Parameters.AddWithValue("$id", id.ToString());
        using var lector = comando.ExecuteReader();
        if (!lector.Read()) return null;

        var proyectoId = ProyectoId.DesdeGuid(Guid.Parse(lector.GetString(0)));
        var grupoId = GrupoId.DesdeGuid(Guid.Parse(lector.GetString(1)));
        var nombre = lector.GetString(2);
        var descripcion = lector.GetString(3);
        var inicio = LeerFecha(lector.GetString(4));
        var termino = LeerFecha(lector.GetString(5));
        var estado = (EstadoProyecto)lector.GetInt32(6);
        var observaciones = lector.GetString(7);
        var version = lector.GetInt32(8);
        var metodologia = (MetodologiaProyectoNem)lector.GetInt32(9);
        lector.Close();

        var grados = LeerGradosProyecto(conexion, proyectoId);
        return ProyectoDidactico.Rehidratar(
            proyectoId,
            grupoId,
            nombre,
            descripcion,
            inicio,
            termino,
            estado,
            observaciones,
            version,
            metodologia,
            grados);
    }

    private static ActividadProyecto? CargarActividad(SqliteConnection conexion, ActividadId id)
    {
        using var comando = conexion.CreateCommand();
        comando.CommandText = """
            SELECT a.actividad_id,
                   a.proyecto_id,
                   a.grupo_id,
                   a.titulo,
                   a.descripcion,
                   a.fecha_realizacion,
                   a.observaciones_generales,
                   a.estado,
                   a.version,
                   COALESCE(n.campo_formativo,0)
            FROM actividades_proyecto a
            LEFT JOIN actividades_nem n ON n.actividad_id=a.actividad_id
            WHERE a.actividad_id=$id
            """;
        comando.Parameters.AddWithValue("$id", id.ToString());
        using var lector = comando.ExecuteReader();
        if (!lector.Read()) return null;

        var actividadId = ActividadId.DesdeGuid(Guid.Parse(lector.GetString(0)));
        var proyectoId = ProyectoId.DesdeGuid(Guid.Parse(lector.GetString(1)));
        var grupoId = GrupoId.DesdeGuid(Guid.Parse(lector.GetString(2)));
        var titulo = lector.GetString(3);
        var descripcion = lector.GetString(4);
        var fecha = LeerFecha(lector.GetString(5));
        var observaciones = lector.GetString(6);
        var estado = (EstadoActividad)lector.GetInt32(7);
        var version = lector.GetInt32(8);
        var campoFormativo = (CampoFormativoNem)lector.GetInt32(9);
        lector.Close();

        using var detalles = conexion.CreateCommand();
        detalles.CommandText = """
            SELECT e.estudiante_id,
                   COALESCE(
                       s.estado_entrega,
                       CASE e.estado_entrega WHEN 5 THEN 2 WHEN 0 THEN 0 ELSE 1 END),
                   CASE WHEN e.estado_entrega = 5 THEN 0 ELSE e.estado_entrega END,
                   e.observacion
            FROM entregas_actividad e
            LEFT JOIN estados_entrega_actividad s
              ON s.actividad_id=e.actividad_id AND s.estudiante_id=e.estudiante_id
            WHERE e.actividad_id=$id
            ORDER BY e.estudiante_id
            """;
        detalles.Parameters.AddWithValue("$id", id.ToString());
        using var lectorDetalles = detalles.ExecuteReader();
        var entregas = new List<DatosEntregaActividadRehidratada>();
        while (lectorDetalles.Read())
        {
            entregas.Add(new(
                EstudianteId.DesdeGuid(Guid.Parse(lectorDetalles.GetString(0))),
                (EstadoEntregaActividad)lectorDetalles.GetInt32(1),
                (NivelLogro)lectorDetalles.GetInt32(2),
                lectorDetalles.GetString(3)));
        }
        lectorDetalles.Close();

        var grados = LeerGradosActividad(conexion, actividadId);
        return ActividadProyecto.Rehidratar(
            actividadId,
            proyectoId,
            grupoId,
            titulo,
            descripcion,
            fecha,
            observaciones,
            estado,
            version,
            entregas,
            campoFormativo,
            grados);
    }

    private SqliteConnection Abrir()
    {
        new PersistenciaGrupoSqlite(_ruta).Inicializar();
        var conexion = new SqliteConnection(_cadena);
        conexion.Open();
        using (var comando = conexion.CreateCommand())
        {
            comando.CommandText = "PRAGMA foreign_keys = ON; PRAGMA busy_timeout = 5000;";
            comando.ExecuteNonQuery();
        }
        EsquemaReportesSqlite.Inicializar(conexion);
        EsquemaPlaneacionNemSqlite.Inicializar(conexion);
        return conexion;
    }

    private static IReadOnlyList<GradoPrimaria> LeerGradosProyecto(
        SqliteConnection conexion,
        ProyectoId proyectoId)
    {
        using var comando = conexion.CreateCommand();
        comando.CommandText = "SELECT grado FROM grados_proyecto WHERE proyecto_id=$id ORDER BY grado";
        comando.Parameters.AddWithValue("$id", proyectoId.ToString());
        using var lector = comando.ExecuteReader();
        var grados = new List<GradoPrimaria>();
        while (lector.Read())
        {
            grados.Add((GradoPrimaria)lector.GetInt32(0));
        }
        return grados.ToArray();
    }

    private static IReadOnlyList<GradoPrimaria> LeerGradosActividad(
        SqliteConnection conexion,
        ActividadId actividadId)
    {
        using var comando = conexion.CreateCommand();
        comando.CommandText = "SELECT grado FROM grados_actividad WHERE actividad_id=$id ORDER BY grado";
        comando.Parameters.AddWithValue("$id", actividadId.ToString());
        using var lector = comando.ExecuteReader();
        var grados = new List<GradoPrimaria>();
        while (lector.Read())
        {
            grados.Add((GradoPrimaria)lector.GetInt32(0));
        }
        return grados.ToArray();
    }

    private static void GuardarMetadatosProyecto(
        SqliteConnection conexion,
        SqliteTransaction transaccion,
        ProyectoDidactico proyecto)
    {
        using (var metadata = conexion.CreateCommand())
        {
            metadata.Transaction = transaccion;
            metadata.CommandText = """
                INSERT INTO proyectos_nem(proyecto_id,metodologia)
                VALUES($id,$metodologia)
                ON CONFLICT(proyecto_id) DO UPDATE SET metodologia=excluded.metodologia
                """;
            metadata.Parameters.AddWithValue("$id", proyecto.Id.ToString());
            metadata.Parameters.AddWithValue("$metodologia", (int)proyecto.Metodologia);
            metadata.ExecuteNonQuery();
        }

        using (var eliminar = conexion.CreateCommand())
        {
            eliminar.Transaction = transaccion;
            eliminar.CommandText = "DELETE FROM grados_proyecto WHERE proyecto_id=$id";
            eliminar.Parameters.AddWithValue("$id", proyecto.Id.ToString());
            eliminar.ExecuteNonQuery();
        }

        foreach (var grado in proyecto.GradosObjetivo)
        {
            using var insertar = conexion.CreateCommand();
            insertar.Transaction = transaccion;
            insertar.CommandText = "INSERT INTO grados_proyecto(proyecto_id,grado) VALUES($id,$grado)";
            insertar.Parameters.AddWithValue("$id", proyecto.Id.ToString());
            insertar.Parameters.AddWithValue("$grado", (int)grado);
            insertar.ExecuteNonQuery();
        }
    }

    private static void GuardarMetadatosActividad(
        SqliteConnection conexion,
        SqliteTransaction transaccion,
        ActividadProyecto actividad)
    {
        using (var metadata = conexion.CreateCommand())
        {
            metadata.Transaction = transaccion;
            metadata.CommandText = """
                INSERT INTO actividades_nem(actividad_id,campo_formativo)
                VALUES($id,$campo)
                ON CONFLICT(actividad_id) DO UPDATE SET campo_formativo=excluded.campo_formativo
                """;
            metadata.Parameters.AddWithValue("$id", actividad.Id.ToString());
            metadata.Parameters.AddWithValue("$campo", (int)actividad.CampoFormativo);
            metadata.ExecuteNonQuery();
        }

        using (var eliminar = conexion.CreateCommand())
        {
            eliminar.Transaction = transaccion;
            eliminar.CommandText = "DELETE FROM grados_actividad WHERE actividad_id=$id";
            eliminar.Parameters.AddWithValue("$id", actividad.Id.ToString());
            eliminar.ExecuteNonQuery();
        }

        foreach (var grado in actividad.GradosObjetivo)
        {
            using var insertar = conexion.CreateCommand();
            insertar.Transaction = transaccion;
            insertar.CommandText = "INSERT INTO grados_actividad(actividad_id,grado) VALUES($id,$grado)";
            insertar.Parameters.AddWithValue("$id", actividad.Id.ToString());
            insertar.Parameters.AddWithValue("$grado", (int)grado);
            insertar.ExecuteNonQuery();
        }
    }

    private static void ParametrosProyecto(SqliteCommand c, ProyectoDidactico p)
    {
        c.Parameters.AddWithValue("$id", p.Id.ToString());
        c.Parameters.AddWithValue("$grupo", p.GrupoId.ToString());
        c.Parameters.AddWithValue("$nombre", p.Nombre);
        c.Parameters.AddWithValue("$descripcion", p.Descripcion);
        c.Parameters.AddWithValue("$inicio", Fecha(p.FechaInicio));
        c.Parameters.AddWithValue("$termino", Fecha(p.FechaTermino));
        c.Parameters.AddWithValue("$estado", (int)p.Estado);
        c.Parameters.AddWithValue("$observaciones", p.Observaciones);
    }

    private static void ParametrosActividad(SqliteCommand c, ActividadProyecto a)
    {
        c.Parameters.AddWithValue("$id", a.Id.ToString());
        c.Parameters.AddWithValue("$proyecto", a.ProyectoId.ToString());
        c.Parameters.AddWithValue("$grupo", a.GrupoId.ToString());
        c.Parameters.AddWithValue("$titulo", a.Titulo);
        c.Parameters.AddWithValue("$descripcion", a.Descripcion);
        c.Parameters.AddWithValue("$fecha", Fecha(a.FechaRealizacion));
        c.Parameters.AddWithValue("$observaciones", a.ObservacionesGenerales);
        c.Parameters.AddWithValue("$estado", (int)a.Estado);
    }

    private static string Fecha(DateOnly f) =>
        f.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static DateOnly LeerFecha(string f) =>
        DateOnly.TryParseExact(f, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fecha)
            && Fecha(fecha) == f
            ? fecha
            : throw new InvalidDataException("Fecha SQLite no canónica.");

    private static T Ejecutar<T>(Func<T> accion)
    {
        try
        {
            return accion();
        }
        catch (ConflictoConcurrenciaException)
        {
            throw;
        }
        catch (ErrorPersistenciaAplicacionException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new ErrorPersistenciaAplicacionException(
                "No fue posible acceder a proyectos y actividades.",
                e);
        }
    }

    private static void Ejecutar(Action accion) => Ejecutar(() =>
    {
        accion();
        return true;
    });
}