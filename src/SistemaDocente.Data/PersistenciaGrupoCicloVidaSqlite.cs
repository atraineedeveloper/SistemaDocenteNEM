using Microsoft.Data.Sqlite;

using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Data;

public sealed class PersistenciaGrupoCicloVidaSqlite : IAlmacenamientoGrupos
{
    private readonly PersistenciaGrupoSqlite _inner;
    private readonly string _cadenaConexion;

    public PersistenciaGrupoCicloVidaSqlite(PersistenciaGrupoSqlite inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _cadenaConexion = new SqliteConnectionStringBuilder
        {
            DataSource = _inner.RutaArchivo,
            Mode = SqliteOpenMode.ReadWriteCreate,
            DefaultTimeout = 15,
            Pooling = false,
        }.ToString();
    }

    public Grupo? Cargar(GrupoId grupoId)
    {
        Inicializar();
        var grupo = _inner.Cargar(grupoId);
        if (grupo is null)
        {
            return null;
        }

        using var conexion = AbrirConexion();
        return AplicarEstado(grupo, LeerArchivado(conexion, grupoId));
    }

    public bool Existe(GrupoId grupoId) => _inner.Existe(grupoId);

    public void Guardar(Grupo grupo)
    {
        ArgumentNullException.ThrowIfNull(grupo);
        _inner.Guardar(grupo);
        Inicializar();

        try
        {
            using var conexion = AbrirConexion();
            using var comando = conexion.CreateCommand();
            comando.CommandText = """
                INSERT INTO ciclo_vida_grupo (grupo_id, archivado)
                VALUES ($grupoId, $archivado)
                ON CONFLICT(grupo_id) DO UPDATE SET archivado = excluded.archivado;
                """;
            comando.Parameters.AddWithValue("$grupoId", Formatear(grupo.Id.Valor));
            comando.Parameters.AddWithValue("$archivado", grupo.EstaArchivado ? 1 : 0);
            comando.ExecuteNonQuery();
        }
        catch (SqliteException exception)
        {
            throw new ErrorPersistenciaAplicacionException(
                "No fue posible guardar el estado de ciclo de vida del grupo.",
                exception);
        }
    }

    public IReadOnlyList<Grupo> ListarTodos()
    {
        Inicializar();
        var grupos = _inner.ListarTodos();
        using var conexion = AbrirConexion();
        return grupos
            .Select(grupo => AplicarEstado(grupo, LeerArchivado(conexion, grupo.Id)))
            .OrderBy(grupo => grupo.NombreVisible, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(grupo => grupo.Id.Valor)
            .ToArray();
    }

    public ResumenEliminacionGrupo ObtenerResumenEliminacion(GrupoId grupoId)
    {
        if (grupoId == default)
        {
            throw new ArgumentException("La identidad del grupo no puede estar vacía.", nameof(grupoId));
        }

        Inicializar();
        if (!_inner.Existe(grupoId))
        {
            return new ResumenEliminacionGrupo(0, 0, 0, 0, 0, 0);
        }

        try
        {
            using var conexion = AbrirConexion();
            var id = Formatear(grupoId.Valor);
            var estudiantes = Contar(conexion, "SELECT COUNT(*) FROM estudiantes WHERE grupo_id = $grupoId;", id);
            var diasAsistencia = Contar(conexion, "SELECT COUNT(*) FROM asistencias_diarias WHERE grupo_id = $grupoId;", id);
            var proyectos = Contar(conexion, "SELECT COUNT(*) FROM proyectos_didacticos WHERE grupo_id = $grupoId;", id);
            var actividades = Contar(conexion, "SELECT COUNT(*) FROM actividades_proyecto WHERE grupo_id = $grupoId;", id);
            var entregas = Contar(conexion, "SELECT COUNT(*) FROM entregas_actividad WHERE grupo_id = $grupoId;", id);

            var configuracion = Contar(
                conexion,
                """
                SELECT COUNT(*)
                FROM configuracion_grupo
                WHERE grupo_id = $grupoId
                  AND (
                      length(trim(ciclo_escolar)) > 0
                      OR length(trim(nombre_escuela)) > 0
                      OR length(trim(cct)) > 0
                      OR length(trim(entidad_federativa)) > 0
                      OR length(trim(municipio)) > 0
                      OR length(trim(localidad)) > 0
                      OR length(trim(grado)) > 0
                      OR length(trim(grupo)) > 0
                      OR length(trim(turno)) > 0
                      OR etapa_cognoscitiva <> 0
                      OR length(trim(docente_responsable)) > 0
                      OR responsable_desde IS NOT NULL
                      OR responsable_hasta IS NOT NULL
                      OR hora_entrada IS NOT NULL
                      OR hora_salida IS NOT NULL
                  );
                """,
                id);
            configuracion += Contar(conexion, "SELECT COUNT(*) FROM grados_grupo WHERE grupo_id = $grupoId;", id);
            configuracion += Contar(
                conexion,
                """
                SELECT COUNT(*)
                FROM contexto_nem_grupo
                WHERE grupo_id = $grupoId
                  AND (
                      organizacion_escolar <> 0
                      OR length(trim(entidad_catalogo)) > 0
                      OR length(trim(municipio_catalogo)) > 0
                  );
                """,
                id);

            return new ResumenEliminacionGrupo(
                estudiantes,
                diasAsistencia,
                proyectos,
                actividades,
                entregas,
                configuracion);
        }
        catch (SqliteException exception)
        {
            throw new ErrorPersistenciaAplicacionException(
                "No fue posible revisar el contenido asociado al grupo.",
                exception);
        }
    }

    public void Eliminar(GrupoId grupoId)
    {
        if (grupoId == default)
        {
            throw new ArgumentException("La identidad del grupo no puede estar vacía.", nameof(grupoId));
        }

        Inicializar();

        try
        {
            using var conexion = AbrirConexion();
            using var transaccion = conexion.BeginTransaction();
            var id = Formatear(grupoId.Valor);

            EjecutarEliminacion(conexion, transaccion, "DELETE FROM entregas_actividad WHERE grupo_id = $grupoId;", id);
            EjecutarEliminacion(conexion, transaccion, "DELETE FROM actividades_proyecto WHERE grupo_id = $grupoId;", id);
            EjecutarEliminacion(conexion, transaccion, "DELETE FROM proyectos_didacticos WHERE grupo_id = $grupoId;", id);
            EjecutarEliminacion(conexion, transaccion, "DELETE FROM registros_asistencia WHERE grupo_id = $grupoId;", id);
            EjecutarEliminacion(conexion, transaccion, "DELETE FROM asistencias_diarias WHERE grupo_id = $grupoId;", id);
            EjecutarEliminacion(conexion, transaccion, "DELETE FROM estudiantes WHERE grupo_id = $grupoId;", id);
            EjecutarEliminacion(conexion, transaccion, "DELETE FROM grupos WHERE id = $grupoId;", id);

            transaccion.Commit();
        }
        catch (SqliteException exception)
        {
            throw new ErrorPersistenciaAplicacionException(
                "No fue posible eliminar el grupo de forma completa y segura.",
                exception);
        }
    }

    private void Inicializar()
    {
        _inner.Inicializar();
        try
        {
            using var conexion = AbrirConexion();
            EsquemaCicloVidaGrupoSqlite.Inicializar(conexion);
        }
        catch (SchemaIncompatibleException)
        {
            throw;
        }
        catch (SqliteException exception)
        {
            throw new ErrorPersistenciaAplicacionException(
                "No fue posible inicializar el ciclo de vida de grupos.",
                exception);
        }
    }

    private SqliteConnection AbrirConexion()
    {
        var conexion = new SqliteConnection(_cadenaConexion);
        conexion.Open();
        using var comando = conexion.CreateCommand();
        comando.CommandText = "PRAGMA foreign_keys = ON; PRAGMA busy_timeout = 5000;";
        comando.ExecuteNonQuery();
        return conexion;
    }

    private static bool LeerArchivado(SqliteConnection conexion, GrupoId grupoId)
    {
        using var comando = conexion.CreateCommand();
        comando.CommandText = "SELECT archivado FROM ciclo_vida_grupo WHERE grupo_id = $grupoId;";
        comando.Parameters.AddWithValue("$grupoId", Formatear(grupoId.Valor));
        return comando.ExecuteScalar() is long valor && valor == 1;
    }

    private static Grupo AplicarEstado(Grupo grupo, bool archivado)
    {
        if (grupo.EstaArchivado == archivado)
        {
            return grupo;
        }

        var estudiantes = grupo.Estudiantes
            .Select(estudiante => new DatosEstudianteRehidratado(
                estudiante.Id,
                estudiante.NombreVisible,
                estudiante.PrimerApellido,
                estudiante.SegundoApellido,
                estudiante.Nombres,
                estudiante.FechaNacimiento,
                estudiante.Genero,
                estudiante.FechaIngreso,
                estudiante.Observaciones,
                estudiante.NumeroLista,
                estudiante.EstaActivo,
                estudiante.Grado))
            .ToArray();

        return Grupo.Rehidratar(grupo.Id, grupo.NombreVisible, estudiantes, archivado);
    }

    private static int Contar(SqliteConnection conexion, string sql, string grupoId)
    {
        using var comando = conexion.CreateCommand();
        comando.CommandText = sql;
        comando.Parameters.AddWithValue("$grupoId", grupoId);
        return Convert.ToInt32(comando.ExecuteScalar() ?? 0L, System.Globalization.CultureInfo.InvariantCulture);
    }

    private static void EjecutarEliminacion(
        SqliteConnection conexion,
        SqliteTransaction transaccion,
        string sql,
        string grupoId)
    {
        using var comando = conexion.CreateCommand();
        comando.Transaction = transaccion;
        comando.CommandText = sql;
        comando.Parameters.AddWithValue("$grupoId", grupoId);
        comando.ExecuteNonQuery();
    }

    private static string Formatear(Guid valor) => valor.ToString("D");
}
