using System.Globalization;
using Microsoft.Data.Sqlite;
using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Data;

public sealed class PersistenciaExpedienteSqlite : IAlmacenamientoExpedientes
{
    private readonly string _cadenaConexion;

    public PersistenciaExpedienteSqlite(string baseSqlite)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(baseSqlite);
        _cadenaConexion = new SqliteConnectionStringBuilder
        {
            DataSource = baseSqlite,
            Mode = SqliteOpenMode.ReadWriteCreate,
        }.ConnectionString;
    }

    private SqliteConnection AbrirConexion()
    {
        var conexion = new SqliteConnection(_cadenaConexion);
        try
        {
            conexion.Open();
            using (var cmd = conexion.CreateCommand())
            {
                cmd.CommandText = "PRAGMA foreign_keys = ON;";
                cmd.ExecuteNonQuery();
            }
            EsquemaSqlite.Inicializar(conexion);
            return conexion;
        }
        catch
        {
            conexion.Dispose();
            throw;
        }
    }

    public ExpedienteEstudiante ObtenerExpediente(EstudianteId estudianteId, GrupoId grupoId)
    {
        try
        {
            using var conexion = AbrirConexion();

            var notas = new List<NotaPedagogica>();
            using (var cmd = conexion.CreateCommand())
            {
                cmd.CommandText = """
                    SELECT nota_id, tipo, contenido, fecha_hora_registro
                    FROM notas_pedagogicas_estudiantes
                    WHERE estudiante_id = $estudiante_id AND grupo_id = $grupo_id
                    ORDER BY fecha_hora_registro DESC;
                    """;
                cmd.Parameters.AddWithValue("$estudiante_id", estudianteId.Valor.ToString());
                cmd.Parameters.AddWithValue("$grupo_id", grupoId.Valor.ToString());

                using var lector = cmd.ExecuteReader();
                while (lector.Read())
                {
                    var id = lector.GetGuid(0);
                    var tipo = (TipoNotaPedagogica)lector.GetInt32(1);
                    var contenido = lector.GetString(2);
                    var fechaHora = DateTime.ParseExact(lector.GetString(3), "o", CultureInfo.InvariantCulture);
                    notas.Add(new NotaPedagogica(id, tipo, contenido, fechaHora));
                }
            }

            var acuerdos = new List<AcuerdoTutor>();
            using (var cmd = conexion.CreateCommand())
            {
                cmd.CommandText = """
                    SELECT acuerdo_id, motivo, acuerdo_convenido, fecha_reunion, fecha_seguimiento
                    FROM acuerdos_tutores_estudiantes
                    WHERE estudiante_id = $estudiante_id AND grupo_id = $grupo_id
                    ORDER BY fecha_reunion DESC;
                    """;
                cmd.Parameters.AddWithValue("$estudiante_id", estudianteId.Valor.ToString());
                cmd.Parameters.AddWithValue("$grupo_id", grupoId.Valor.ToString());

                using var lector = cmd.ExecuteReader();
                while (lector.Read())
                {
                    var id = lector.GetGuid(0);
                    var motivo = lector.GetString(1);
                    var acuerdo = lector.GetString(2);
                    var fechaReunion = DateOnly.ParseExact(lector.GetString(3), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    DateOnly? fechaSeguimiento = lector.IsDBNull(4) ? null : DateOnly.ParseExact(lector.GetString(4), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    acuerdos.Add(new AcuerdoTutor(id, motivo, acuerdo, fechaReunion, fechaSeguimiento));
                }
            }

            return new ExpedienteEstudiante(estudianteId, grupoId, notas, acuerdos);
        }
        catch (SqliteException ex)
        {
            throw new DataAccessException("Ocurrió un error al consultar el expediente del estudiante.", ex);
        }
    }

    public void RegistrarNotaPedagogica(EstudianteId estudianteId, GrupoId grupoId, TipoNotaPedagogica tipo, string contenido, DateTime fechaHora)
    {
        ValidadorContenidoPedagogico.ValidarTextoPedagogico(contenido, nameof(contenido));

        try
        {
            using var conexion = AbrirConexion();

            using var cmd = conexion.CreateCommand();
            cmd.CommandText = """
                INSERT INTO notas_pedagogicas_estudiantes (nota_id, estudiante_id, grupo_id, tipo, contenido, fecha_hora_registro)
                VALUES ($nota_id, $estudiante_id, $grupo_id, $tipo, $contenido, $fecha_hora_registro);
                """;
            cmd.Parameters.AddWithValue("$nota_id", Guid.NewGuid().ToString());
            cmd.Parameters.AddWithValue("$estudiante_id", estudianteId.Valor.ToString());
            cmd.Parameters.AddWithValue("$grupo_id", grupoId.Valor.ToString());
            cmd.Parameters.AddWithValue("$tipo", (int)tipo);
            cmd.Parameters.AddWithValue("$contenido", contenido.Trim());
            cmd.Parameters.AddWithValue("$fecha_hora_registro", fechaHora.ToString("o", CultureInfo.InvariantCulture));
            cmd.ExecuteNonQuery();
        }
        catch (SqliteException ex)
        {
            throw new DataAccessException("Ocurrió un error al registrar la nota pedagógica.", ex);
        }
    }

    public void RegistrarAcuerdoTutor(EstudianteId estudianteId, GrupoId grupoId, string motivo, string acuerdo, DateOnly fechaReunion, DateOnly? fechaSeguimiento)
    {
        ValidadorContenidoPedagogico.ValidarTextoPedagogico(motivo, nameof(motivo));
        ValidadorContenidoPedagogico.ValidarTextoPedagogico(acuerdo, nameof(acuerdo));

        try
        {
            using var conexion = AbrirConexion();

            using var cmd = conexion.CreateCommand();
            cmd.CommandText = """
                INSERT INTO acuerdos_tutores_estudiantes (acuerdo_id, estudiante_id, grupo_id, motivo, acuerdo_convenido, fecha_reunion, fecha_seguimiento)
                VALUES ($acuerdo_id, $estudiante_id, $grupo_id, $motivo, $acuerdo_convenido, $fecha_reunion, $fecha_seguimiento);
                """;
            cmd.Parameters.AddWithValue("$acuerdo_id", Guid.NewGuid().ToString());
            cmd.Parameters.AddWithValue("$estudiante_id", estudianteId.Valor.ToString());
            cmd.Parameters.AddWithValue("$grupo_id", grupoId.Valor.ToString());
            cmd.Parameters.AddWithValue("$motivo", motivo.Trim());
            cmd.Parameters.AddWithValue("$acuerdo_convenido", acuerdo.Trim());
            cmd.Parameters.AddWithValue("$fecha_reunion", fechaReunion.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            cmd.Parameters.AddWithValue("$fecha_seguimiento", fechaSeguimiento.HasValue ? fechaSeguimiento.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : DBNull.Value);
            cmd.ExecuteNonQuery();
        }
        catch (SqliteException ex)
        {
            throw new DataAccessException("Ocurrió un error al registrar el acuerdo con tutores.", ex);
        }
    }
}
