using System.Globalization;

using Microsoft.Data.Sqlite;

using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Data;

public sealed class PersistenciaContextoGrupoSqlite : IAlmacenamientoContextoGrupo
{
    private readonly string _ruta;
    private readonly string _cadena;

    public PersistenciaContextoGrupoSqlite(string rutaArchivo)
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

    public ContextoGrupo? Cargar(GrupoId grupoId)
    {
        if (grupoId == default) throw new ArgumentException("La identidad del grupo no puede estar vacía.", nameof(grupoId));
        return Ejecutar(() =>
        {
            using var conexion = Abrir();
            using var comando = conexion.CreateCommand();
            comando.CommandText = """
                SELECT ciclo_escolar, nombre_escuela, cct, entidad_federativa, municipio, localidad,
                       grado, grupo, turno, etapa_cognoscitiva, docente_responsable,
                       responsable_desde, responsable_hasta, hora_entrada, hora_salida
                FROM configuracion_grupo
                WHERE grupo_id = $grupo;
                """;
            comando.Parameters.AddWithValue("$grupo", grupoId.ToString());
            using var lector = comando.ExecuteReader();
            if (!lector.Read()) return null;

            return ContextoGrupo.Crear(
                grupoId,
                lector.GetString(0),
                lector.GetString(1),
                lector.GetString(2),
                lector.GetString(3),
                lector.GetString(4),
                lector.GetString(5),
                lector.GetString(6),
                lector.GetString(7),
                lector.GetString(8),
                (EtapaDesarrolloCognoscitivo)lector.GetInt32(9),
                lector.GetString(10),
                LeerFechaOpcional(lector, 11),
                LeerFechaOpcional(lector, 12),
                LeerHoraOpcional(lector, 13),
                LeerHoraOpcional(lector, 14));
        });
    }

    public void Guardar(ContextoGrupo contexto)
    {
        ArgumentNullException.ThrowIfNull(contexto);
        Ejecutar(() =>
        {
            using var conexion = Abrir();
            using var comando = conexion.CreateCommand();
            comando.CommandText = """
                INSERT INTO configuracion_grupo (
                    grupo_id, ciclo_escolar, nombre_escuela, cct, entidad_federativa, municipio,
                    localidad, grado, grupo, turno, etapa_cognoscitiva, docente_responsable,
                    responsable_desde, responsable_hasta, hora_entrada, hora_salida)
                VALUES (
                    $grupoId, $ciclo, $escuela, $cct, $entidad, $municipio,
                    $localidad, $grado, $grupo, $turno, $etapa, $docente,
                    $desde, $hasta, $entrada, $salida)
                ON CONFLICT(grupo_id) DO UPDATE SET
                    ciclo_escolar = excluded.ciclo_escolar,
                    nombre_escuela = excluded.nombre_escuela,
                    cct = excluded.cct,
                    entidad_federativa = excluded.entidad_federativa,
                    municipio = excluded.municipio,
                    localidad = excluded.localidad,
                    grado = excluded.grado,
                    grupo = excluded.grupo,
                    turno = excluded.turno,
                    etapa_cognoscitiva = excluded.etapa_cognoscitiva,
                    docente_responsable = excluded.docente_responsable,
                    responsable_desde = excluded.responsable_desde,
                    responsable_hasta = excluded.responsable_hasta,
                    hora_entrada = excluded.hora_entrada,
                    hora_salida = excluded.hora_salida;
                """;
            comando.Parameters.AddWithValue("$grupoId", contexto.GrupoId.ToString());
            comando.Parameters.AddWithValue("$ciclo", contexto.CicloEscolar);
            comando.Parameters.AddWithValue("$escuela", contexto.NombreEscuela);
            comando.Parameters.AddWithValue("$cct", contexto.Cct);
            comando.Parameters.AddWithValue("$entidad", contexto.EntidadFederativa);
            comando.Parameters.AddWithValue("$municipio", contexto.Municipio);
            comando.Parameters.AddWithValue("$localidad", contexto.Localidad);
            comando.Parameters.AddWithValue("$grado", contexto.Grado);
            comando.Parameters.AddWithValue("$grupo", contexto.Grupo);
            comando.Parameters.AddWithValue("$turno", contexto.Turno);
            comando.Parameters.AddWithValue("$etapa", (int)contexto.EtapaCognoscitiva);
            comando.Parameters.AddWithValue("$docente", contexto.DocenteResponsable);
            comando.Parameters.AddWithValue("$desde", FechaDb(contexto.ResponsableDesde));
            comando.Parameters.AddWithValue("$hasta", FechaDb(contexto.ResponsableHasta));
            comando.Parameters.AddWithValue("$entrada", HoraDb(contexto.HoraEntrada));
            comando.Parameters.AddWithValue("$salida", HoraDb(contexto.HoraSalida));
            comando.ExecuteNonQuery();
            return true;
        });
    }

    private SqliteConnection Abrir()
    {
        new PersistenciaGrupoSqlite(_ruta).Inicializar();
        var conexion = new SqliteConnection(_cadena);
        conexion.Open();
        using var comando = conexion.CreateCommand();
        comando.CommandText = "PRAGMA foreign_keys = ON; PRAGMA busy_timeout = 5000;";
        comando.ExecuteNonQuery();
        return conexion;
    }

    private static object FechaDb(DateOnly? fecha) =>
        fecha?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? (object)DBNull.Value;

    private static object HoraDb(TimeOnly? hora) =>
        hora?.ToString("HH:mm", CultureInfo.InvariantCulture) ?? (object)DBNull.Value;

    private static DateOnly? LeerFechaOpcional(SqliteDataReader lector, int indice) =>
        lector.IsDBNull(indice)
            ? null
            : DateOnly.ParseExact(lector.GetString(indice), "yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static TimeOnly? LeerHoraOpcional(SqliteDataReader lector, int indice) =>
        lector.IsDBNull(indice)
            ? null
            : TimeOnly.ParseExact(lector.GetString(indice), "HH:mm", CultureInfo.InvariantCulture);

    private static T Ejecutar<T>(Func<T> accion)
    {
        try
        {
            return accion();
        }
        catch (ErrorPersistenciaAplicacionException)
        {
            throw;
        }
        catch (Exception exception) when (exception is SqliteException or IOException or UnauthorizedAccessException)
        {
            throw new ErrorPersistenciaAplicacionException(
                "No fue posible acceder a la configuración del grupo.",
                exception);
        }
    }
}
