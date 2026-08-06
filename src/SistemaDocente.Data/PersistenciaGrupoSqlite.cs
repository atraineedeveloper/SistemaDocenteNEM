using System.Globalization;
using Microsoft.Data.Sqlite;

using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Data;

public sealed class PersistenciaGrupoSqlite : IAlmacenamientoGrupos
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

        try
        {
            GuardarInterno(grupo);
        }
        catch (DataAccessException exception)
        {
            throw Traducir(exception);
        }
    }

    private void GuardarInterno(Grupo grupo)
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

        try
        {
            return CargarInterno(grupoId);
        }
        catch (DataAccessException exception)
        {
            throw Traducir(exception);
        }
    }

    public bool Existe(GrupoId grupoId)
    {
        if (grupoId == default)
        {
            throw new ArgumentException("La identidad del grupo no puede estar vacía.", nameof(grupoId));
        }

        try
        {
            Inicializar();
            using var conexion = AbrirConexion();
            using var comando = conexion.CreateCommand();
            comando.CommandText = "SELECT EXISTS(SELECT 1 FROM grupos WHERE id = $id);";
            comando.Parameters.AddWithValue("$id", Formatear(grupoId.Valor));
            return comando.ExecuteScalar() is long resultado && resultado == 1;
        }
        catch (DataAccessException exception)
        {
            throw Traducir(exception);
        }
        catch (SqliteException exception)
        {
            throw Traducir(
                new DataAccessException("No fue posible comprobar la existencia del grupo.", exception));
        }
    }

    private Grupo? CargarInterno(GrupoId grupoId)
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

    private static ErrorPersistenciaAplicacionException Traducir(DataAccessException exception) =>
        new("Ocurrió un error al acceder a la persistencia de grupos.", exception);

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
                primer_apellido,
                segundo_apellido,
                nombres,
                fecha_nacimiento,
                genero,
                fecha_ingreso,
                observaciones,
                numero_lista,
                activo)
            VALUES (
                $id,
                $grupoId,
                $nombre,
                $primerApellido,
                $segundoApellido,
                $nombres,
                $fechaNacimiento,
                $genero,
                $fechaIngreso,
                $observaciones,
                $numeroLista,
                $activo)
            ON CONFLICT(id, grupo_id) DO UPDATE SET
                nombre = excluded.nombre,
                primer_apellido = excluded.primer_apellido,
                segundo_apellido = excluded.segundo_apellido,
                nombres = excluded.nombres,
                fecha_nacimiento = excluded.fecha_nacimiento,
                genero = excluded.genero,
                fecha_ingreso = excluded.fecha_ingreso,
                observaciones = excluded.observaciones,
                numero_lista = excluded.numero_lista,
                activo = excluded.activo;
            """;
        comando.Parameters.AddWithValue("$id", Formatear(estudiante.Id.Valor));
        comando.Parameters.AddWithValue("$grupoId", Formatear(grupoId.Valor));
        comando.Parameters.AddWithValue("$nombre", estudiante.NombreVisible);
        comando.Parameters.AddWithValue("$primerApellido", estudiante.PrimerApellido);
        comando.Parameters.AddWithValue("$segundoApellido", estudiante.SegundoApellido);
        comando.Parameters.AddWithValue("$nombres", estudiante.Nombres);
        comando.Parameters.AddWithValue("$fechaNacimiento", (object?)estudiante.FechaNacimiento?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? DBNull.Value);
        comando.Parameters.AddWithValue("$genero", (int)estudiante.Genero);
        comando.Parameters.AddWithValue("$fechaIngreso", (object?)estudiante.FechaIngreso?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? DBNull.Value);
        comando.Parameters.AddWithValue("$observaciones", estudiante.Observaciones);
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
            SELECT id, nombre, primer_apellido, segundo_apellido, nombres, fecha_nacimiento, genero, fecha_ingreso, observaciones, numero_lista, activo
            FROM estudiantes
            WHERE grupo_id = $grupoId
            ORDER BY rowid;
            """;
        comando.Parameters.AddWithValue("$grupoId", Formatear(grupoId.Valor));

        var estudiantes = new List<DatosEstudianteRehidratado>();
        using var lector = comando.ExecuteReader();

        while (lector.Read())
        {
            var fechaNacStr = lector.IsDBNull(5) ? null : lector.GetString(5);
            DateOnly? fechaNac = string.IsNullOrEmpty(fechaNacStr) ? null : DateOnly.ParseExact(fechaNacStr, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            var fechaIngStr = lector.IsDBNull(7) ? null : lector.GetString(7);
            DateOnly? fechaIng = string.IsNullOrEmpty(fechaIngStr) ? null : DateOnly.ParseExact(fechaIngStr, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            estudiantes.Add(
                new DatosEstudianteRehidratado(
                    EstudianteId.DesdeGuid(Guid.Parse(lector.GetString(0))),
                    lector.GetString(1),
                    lector.IsDBNull(2) ? "" : lector.GetString(2),
                    lector.IsDBNull(3) ? "" : lector.GetString(3),
                    lector.IsDBNull(4) ? "" : lector.GetString(4),
                    fechaNac,
                    (GeneroEstudiante)(lector.IsDBNull(6) ? 0 : lector.GetInt32(6)),
                    fechaIng,
                    lector.IsDBNull(8) ? "" : lector.GetString(8),
                    lector.GetInt32(9),
                    lector.GetInt32(10) == 1));
        }

        return estudiantes;
    }

    private static string Formatear(Guid valor) => valor.ToString("D");
}