using Microsoft.Data.Sqlite;

using SistemaDocente.Core;

namespace SistemaDocente.Data;

public sealed class PersistenciaGrupoSqlite
{
    private readonly string _cadenaConexion;

    public PersistenciaGrupoSqlite(string rutaArchivo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rutaArchivo);

        RutaArchivo = Path.GetFullPath(rutaArchivo);
        _cadenaConexion = new SqliteConnectionStringBuilder
        {
            DataSource = RutaArchivo,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Pooling = false,
        }.ToString();
    }

    public string RutaArchivo { get; }

    public void Inicializar()
    {
        try
        {
            CrearDirectorioContenedor();
            using var conexion = AbrirConexion();
            EsquemaSqlite.Inicializar(conexion);
        }
        catch (SchemaIncompatibleException)
        {
            throw;
        }
        catch (Exception exception) when (EsErrorDeInfraestructura(exception))
        {
            throw new DataAccessException(
                "No fue posible inicializar la base SQLite.",
                exception);
        }
    }

    public void Guardar(Grupo grupo)
    {
        ArgumentNullException.ThrowIfNull(grupo);
        Inicializar();

        try
        {
            using var conexion = AbrirConexion();
            using var transaccion = conexion.BeginTransaction();

            GuardarGrupo(conexion, transaccion, grupo);

            foreach (var estudiante in grupo.Estudiantes)
            {
                VerificarPertenencia(conexion, transaccion, grupo.Id, estudiante.Id);
                GuardarEstudiante(conexion, transaccion, grupo.Id, estudiante);
            }

            transaccion.Commit();
        }
        catch (DataIntegrityException)
        {
            throw;
        }
        catch (SqliteException exception)
        {
            throw new DataIntegrityException(
                "SQLite rechazó el guardado del agregado.",
                exception);
        }
        catch (Exception exception) when (EsErrorDeInfraestructura(exception))
        {
            throw new DataAccessException(
                "No fue posible guardar el agregado.",
                exception);
        }
    }

    public Grupo? Cargar(GrupoId grupoId)
    {
        if (grupoId == default)
        {
            throw new ArgumentException("La identidad del grupo no puede estar vacía.", nameof(grupoId));
        }

        Inicializar();

        try
        {
            using var conexion = AbrirConexion();
            var nombreGrupo = LeerNombreGrupo(conexion, grupoId);

            if (nombreGrupo is null)
            {
                return null;
            }

            var estudiantes = LeerEstudiantes(conexion, grupoId);

            try
            {
                return Grupo.Rehidratar(grupoId, nombreGrupo, estudiantes);
            }
            catch (Exception exception) when (
                exception is DomainValidationException or DomainConflictException)
            {
                throw new DataIntegrityException(
                    "Los datos persistidos no forman un agregado válido.",
                    exception);
            }
        }
        catch (DataIntegrityException)
        {
            throw;
        }
        catch (SqliteException exception)
        {
            throw new DataAccessException(
                "No fue posible cargar el agregado.",
                exception);
        }
        catch (FormatException exception)
        {
            throw new DataIntegrityException(
                "La base contiene una identidad que no es un Guid válido.",
                exception);
        }
    }

    private static bool EsErrorDeInfraestructura(Exception exception) =>
        exception is IOException or UnauthorizedAccessException or SqliteException;

    private void CrearDirectorioContenedor()
    {
        var directorio = Path.GetDirectoryName(RutaArchivo);

        if (!string.IsNullOrEmpty(directorio))
        {
            Directory.CreateDirectory(directorio);
        }
    }

    private SqliteConnection AbrirConexion()
    {
        var conexion = new SqliteConnection(_cadenaConexion);

        try
        {
            conexion.Open();
            using var comando = conexion.CreateCommand();
            comando.CommandText = "PRAGMA foreign_keys = ON;";
            comando.ExecuteNonQuery();
            return conexion;
        }
        catch
        {
            conexion.Dispose();
            throw;
        }
    }

    private static void GuardarGrupo(
        SqliteConnection conexion,
        SqliteTransaction transaccion,
        Grupo grupo)
    {
        using var comando = conexion.CreateCommand();
        comando.Transaction = transaccion;
        comando.CommandText = """
            INSERT INTO grupos (id, nombre)
            VALUES ($id, $nombre)
            ON CONFLICT(id) DO UPDATE SET nombre = excluded.nombre;
            """;
        comando.Parameters.AddWithValue("$id", Formatear(grupo.Id.Valor));
        comando.Parameters.AddWithValue("$nombre", grupo.NombreVisible);
        comando.ExecuteNonQuery();
    }

    private static void VerificarPertenencia(
        SqliteConnection conexion,
        SqliteTransaction transaccion,
        GrupoId grupoId,
        EstudianteId estudianteId)
    {
        using var comando = conexion.CreateCommand();
        comando.Transaction = transaccion;
        comando.CommandText = "SELECT grupo_id FROM estudiantes WHERE id = $id;";
        comando.Parameters.AddWithValue("$id", Formatear(estudianteId.Valor));
        var grupoAlmacenado = comando.ExecuteScalar() as string;

        if (grupoAlmacenado is not null
            && !string.Equals(
                grupoAlmacenado,
                Formatear(grupoId.Valor),
                StringComparison.OrdinalIgnoreCase))
        {
            throw new DataIntegrityException(
                $"El estudiante {estudianteId} ya pertenece a otro grupo.");
        }
    }

    private static void GuardarEstudiante(
        SqliteConnection conexion,
        SqliteTransaction transaccion,
        GrupoId grupoId,
        Estudiante estudiante)
    {
        using var comando = conexion.CreateCommand();
        comando.Transaction = transaccion;
        comando.CommandText = """
            INSERT INTO estudiantes (
                id,
                grupo_id,
                nombre,
                numero_lista,
                activo)
            VALUES (
                $id,
                $grupoId,
                $nombre,
                $numeroLista,
                $activo)
            ON CONFLICT(id) DO UPDATE SET
                nombre = excluded.nombre,
                numero_lista = excluded.numero_lista,
                activo = excluded.activo;
            """;
        comando.Parameters.AddWithValue("$id", Formatear(estudiante.Id.Valor));
        comando.Parameters.AddWithValue("$grupoId", Formatear(grupoId.Valor));
        comando.Parameters.AddWithValue("$nombre", estudiante.NombreVisible);
        comando.Parameters.AddWithValue("$numeroLista", estudiante.NumeroLista);
        comando.Parameters.AddWithValue("$activo", estudiante.EstaActivo ? 1 : 0);
        comando.ExecuteNonQuery();
    }

    private static string? LeerNombreGrupo(SqliteConnection conexion, GrupoId grupoId)
    {
        using var comando = conexion.CreateCommand();
        comando.CommandText = "SELECT nombre FROM grupos WHERE id = $id;";
        comando.Parameters.AddWithValue("$id", Formatear(grupoId.Valor));
        return comando.ExecuteScalar() as string;
    }

    private static List<DatosEstudianteRehidratado> LeerEstudiantes(
        SqliteConnection conexion,
        GrupoId grupoId)
    {
        using var comando = conexion.CreateCommand();
        comando.CommandText = """
            SELECT id, nombre, numero_lista, activo
            FROM estudiantes
            WHERE grupo_id = $grupoId
            ORDER BY rowid;
            """;
        comando.Parameters.AddWithValue("$grupoId", Formatear(grupoId.Valor));

        var estudiantes = new List<DatosEstudianteRehidratado>();
        using var lector = comando.ExecuteReader();

        while (lector.Read())
        {
            estudiantes.Add(
                new DatosEstudianteRehidratado(
                    EstudianteId.DesdeGuid(Guid.Parse(lector.GetString(0))),
                    lector.GetString(1),
                    lector.GetInt32(2),
                    lector.GetInt32(3) == 1));
        }

        return estudiantes;
    }

    private static string Formatear(Guid valor) => valor.ToString("D");
}