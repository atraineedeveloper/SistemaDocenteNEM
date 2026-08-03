using System.Globalization;

using Microsoft.Data.Sqlite;

using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Data;

public sealed class PersistenciaAsistenciaSqlite : IAlmacenamientoAsistencias
{
    private const string FormatoFecha = "yyyy-MM-dd";
    private readonly string _cadenaConexion;
    private readonly PersistenciaGrupoSqlite _inicializador;

    public PersistenciaAsistenciaSqlite(string rutaArchivo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rutaArchivo);
        RutaArchivo = Path.GetFullPath(rutaArchivo);
        _inicializador = new PersistenciaGrupoSqlite(RutaArchivo);
        _cadenaConexion = new SqliteConnectionStringBuilder
        {
            DataSource = RutaArchivo,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Pooling = false,
        }.ToString();
    }

    public string RutaArchivo { get; }

    public AsistenciaDiaria? Cargar(GrupoId grupoId, DateOnly fecha)
    {
        ValidarGrupoId(grupoId);

        try
        {
            _inicializador.Inicializar();
            using var conexion = AbrirConexion();
            using var comandoDia = conexion.CreateCommand();
            comandoDia.CommandText = """
                SELECT fecha
                FROM asistencias_diarias
                WHERE grupo_id = $grupoId AND fecha = $fecha;
                """;
            comandoDia.Parameters.AddWithValue("$grupoId", Formatear(grupoId.Valor));
            comandoDia.Parameters.AddWithValue("$fecha", Formatear(fecha));
            var fechaAlmacenada = comandoDia.ExecuteScalar() as string;

            if (fechaAlmacenada is null)
            {
                return null;
            }

            var fechaValidada = AnalizarFecha(fechaAlmacenada);
            var registros = LeerRegistros(conexion, grupoId, fechaValidada);

            try
            {
                return AsistenciaDiaria.Rehidratar(grupoId, fechaValidada, registros);
            }
            catch (Exception exception) when (
                exception is DomainValidationException or DomainConflictException)
            {
                throw new DataIntegrityException(
                    "Los datos de asistencia no forman un agregado válido.",
                    exception);
            }
        }
        catch (DataAccessException exception)
        {
            throw Traducir(exception);
        }
        catch (SqliteException exception)
        {
            throw Traducir(new DataAccessException("No fue posible cargar la asistencia.", exception));
        }
    }

    public bool Existe(GrupoId grupoId, DateOnly fecha)
    {
        ValidarGrupoId(grupoId);

        try
        {
            _inicializador.Inicializar();
            using var conexion = AbrirConexion();
            using var comando = conexion.CreateCommand();
            comando.CommandText = """
                SELECT EXISTS(
                    SELECT 1 FROM asistencias_diarias
                    WHERE grupo_id = $grupoId AND fecha = $fecha);
                """;
            comando.Parameters.AddWithValue("$grupoId", Formatear(grupoId.Valor));
            comando.Parameters.AddWithValue("$fecha", Formatear(fecha));
            return comando.ExecuteScalar() is long resultado && resultado == 1;
        }
        catch (DataAccessException exception)
        {
            throw Traducir(exception);
        }
        catch (SqliteException exception)
        {
            throw Traducir(new DataAccessException(
                "No fue posible comprobar la existencia de la asistencia.",
                exception));
        }
    }

    public IReadOnlyList<AsistenciaDiaria> CargarIntervalo(
        GrupoId grupoId,
        DateOnly desde,
        DateOnly hasta)
    {
        ValidarGrupoId(grupoId);
        if (desde > hasta)
        {
            throw new ArgumentException("La fecha inicial no puede ser posterior a la final.", nameof(desde));
        }

        try
        {
            _inicializador.Inicializar();
            using var conexion = AbrirConexion();
            using var comando = conexion.CreateCommand();
            comando.CommandText = """
                SELECT d.fecha, r.estudiante_id, r.estado
                FROM asistencias_diarias AS d
                LEFT JOIN registros_asistencia AS r
                  ON r.grupo_id = d.grupo_id AND r.fecha = d.fecha
                WHERE d.grupo_id = $grupoId
                  AND d.fecha BETWEEN $desde AND $hasta
                ORDER BY d.fecha, r.rowid;
                """;
            comando.Parameters.AddWithValue("$grupoId", Formatear(grupoId.Valor));
            comando.Parameters.AddWithValue("$desde", Formatear(desde));
            comando.Parameters.AddWithValue("$hasta", Formatear(hasta));
            var datos = new SortedDictionary<DateOnly, List<DatosRegistroAsistenciaRehidratado>>();
            using var lector = comando.ExecuteReader();

            while (lector.Read())
            {
                var fecha = AnalizarFecha(lector.GetString(0));
                if (!datos.TryGetValue(fecha, out var registros))
                {
                    registros = [];
                    datos.Add(fecha, registros);
                }

                if (!lector.IsDBNull(1))
                {
                    registros.Add(new(
                        EstudianteId.DesdeGuid(Guid.Parse(lector.GetString(1))),
                        (EstadoAsistencia)lector.GetInt32(2)));
                }
            }

            return datos.Select(x => AsistenciaDiaria.Rehidratar(grupoId, x.Key, x.Value)).ToArray();
        }
        catch (DataAccessException exception)
        {
            throw Traducir(exception);
        }
        catch (Exception exception) when (
            exception is SqliteException or FormatException or DomainValidationException or DomainConflictException)
        {
            throw Traducir(new DataIntegrityException(
                "No fue posible cargar el intervalo de asistencia.",
                exception));
        }
    }

    public void Guardar(AsistenciaDiaria asistencia)
    {
        ArgumentNullException.ThrowIfNull(asistencia);

        try
        {
            _inicializador.Inicializar();
            using var conexion = AbrirConexion();
            using var transaccion = conexion.BeginTransaction();
            GuardarDia(conexion, transaccion, asistencia);

            foreach (var registro in asistencia.Registros)
            {
                GuardarRegistro(conexion, transaccion, asistencia, registro);
            }

            transaccion.Commit();
        }
        catch (DataAccessException exception)
        {
            throw Traducir(exception);
        }
        catch (SqliteException exception)
        {
            throw Traducir(new DataIntegrityException(
                "SQLite rechazó el guardado de la asistencia.",
                exception));
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

    private static void GuardarDia(
        SqliteConnection conexion,
        SqliteTransaction transaccion,
        AsistenciaDiaria asistencia)
    {
        using var comando = conexion.CreateCommand();
        comando.Transaction = transaccion;
        comando.CommandText = """
            INSERT INTO asistencias_diarias (grupo_id, fecha)
            VALUES ($grupoId, $fecha)
            ON CONFLICT(grupo_id, fecha) DO NOTHING;
            """;
        comando.Parameters.AddWithValue("$grupoId", Formatear(asistencia.GrupoId.Valor));
        comando.Parameters.AddWithValue("$fecha", Formatear(asistencia.Fecha));
        comando.ExecuteNonQuery();
    }

    private static void GuardarRegistro(
        SqliteConnection conexion,
        SqliteTransaction transaccion,
        AsistenciaDiaria asistencia,
        RegistroAsistencia registro)
    {
        using var comando = conexion.CreateCommand();
        comando.Transaction = transaccion;
        comando.CommandText = """
            INSERT INTO registros_asistencia (
                grupo_id, fecha, estudiante_id, estado)
            VALUES ($grupoId, $fecha, $estudianteId, $estado)
            ON CONFLICT(grupo_id, fecha, estudiante_id) DO UPDATE SET
                estado = excluded.estado;
            """;
        comando.Parameters.AddWithValue("$grupoId", Formatear(asistencia.GrupoId.Valor));
        comando.Parameters.AddWithValue("$fecha", Formatear(asistencia.Fecha));
        comando.Parameters.AddWithValue("$estudianteId", Formatear(registro.EstudianteId.Valor));
        comando.Parameters.AddWithValue("$estado", (int)registro.Estado);
        comando.ExecuteNonQuery();
    }

    private static List<DatosRegistroAsistenciaRehidratado> LeerRegistros(
        SqliteConnection conexion,
        GrupoId grupoId,
        DateOnly fecha)
    {
        using var comando = conexion.CreateCommand();
        comando.CommandText = """
            SELECT estudiante_id, estado
            FROM registros_asistencia
            WHERE grupo_id = $grupoId AND fecha = $fecha
            ORDER BY rowid;
            """;
        comando.Parameters.AddWithValue("$grupoId", Formatear(grupoId.Valor));
        comando.Parameters.AddWithValue("$fecha", Formatear(fecha));
        var resultado = new List<DatosRegistroAsistenciaRehidratado>();
        using var lector = comando.ExecuteReader();

        while (lector.Read())
        {
            try
            {
                resultado.Add(new DatosRegistroAsistenciaRehidratado(
                    EstudianteId.DesdeGuid(Guid.Parse(lector.GetString(0))),
                    (EstadoAsistencia)lector.GetInt32(1)));
            }
            catch (Exception exception) when (exception is FormatException or DomainValidationException)
            {
                throw new DataIntegrityException(
                    "La asistencia contiene una identidad inválida.",
                    exception);
            }
        }

        return resultado;
    }

    private static DateOnly AnalizarFecha(string valor)
    {
        if (!DateOnly.TryParseExact(
                valor,
                FormatoFecha,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var fecha)
            || !string.Equals(Formatear(fecha), valor, StringComparison.Ordinal))
        {
            throw new DataIntegrityException("La asistencia contiene una fecha no canónica.");
        }

        return fecha;
    }

    private static void ValidarGrupoId(GrupoId grupoId)
    {
        if (grupoId == default)
        {
            throw new ArgumentException("La identidad del grupo no puede estar vacía.", nameof(grupoId));
        }
    }

    private static string Formatear(Guid valor) => valor.ToString("D");

    private static string Formatear(DateOnly fecha) =>
        fecha.ToString(FormatoFecha, CultureInfo.InvariantCulture);

    private static ErrorPersistenciaAplicacionException Traducir(DataAccessException exception) =>
        new("Ocurrió un error al acceder a la persistencia de asistencia.", exception);
}