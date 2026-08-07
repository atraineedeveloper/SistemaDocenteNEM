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
            DatosLegacy? datos;
            using (var comando = conexion.CreateCommand())
            {
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

                datos = new DatosLegacy(
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
            }

            var (organizacion, entidadCatalogo, municipioCatalogo) = LeerContextoNem(conexion, grupoId);
            var grados = LeerGrados(conexion, grupoId);

            return ContextoGrupo.Crear(
                grupoId,
                datos.CicloEscolar,
                datos.NombreEscuela,
                datos.Cct,
                string.IsNullOrWhiteSpace(entidadCatalogo) ? datos.EntidadFederativa : entidadCatalogo,
                string.IsNullOrWhiteSpace(municipioCatalogo) ? datos.Municipio : municipioCatalogo,
                datos.Localidad,
                datos.Grado,
                datos.Grupo,
                datos.Turno,
                datos.EtapaCognoscitiva,
                datos.DocenteResponsable,
                datos.ResponsableDesde,
                datos.ResponsableHasta,
                datos.HoraEntrada,
                datos.HoraSalida,
                organizacion,
                grados);
        });
    }

    public void Guardar(ContextoGrupo contexto)
    {
        ArgumentNullException.ThrowIfNull(contexto);
        Ejecutar(() =>
        {
            using var conexion = Abrir();
            using var transaccion = conexion.BeginTransaction();
            try
            {
                GuardarCompatibilidad(conexion, transaccion, contexto);
                GuardarContextoNem(conexion, transaccion, contexto);
                GuardarGrados(conexion, transaccion, contexto);
                transaccion.Commit();
            }
            catch
            {
                transaccion.Rollback();
                throw;
            }
            return true;
        });
    }

    private static void GuardarCompatibilidad(
        SqliteConnection conexion,
        SqliteTransaction transaccion,
        ContextoGrupo contexto)
    {
        using var comando = conexion.CreateCommand();
        comando.Transaction = transaccion;
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
        comando.Parameters.AddWithValue("$grado", contexto.GradosTexto);
        comando.Parameters.AddWithValue("$grupo", contexto.Grupo);
        comando.Parameters.AddWithValue("$turno", contexto.Turno);
        comando.Parameters.AddWithValue("$etapa", (int)contexto.EtapaCognoscitiva);
        comando.Parameters.AddWithValue("$docente", contexto.DocenteResponsable);
        comando.Parameters.AddWithValue("$desde", FechaDb(contexto.ResponsableDesde));
        comando.Parameters.AddWithValue("$hasta", FechaDb(contexto.ResponsableHasta));
        comando.Parameters.AddWithValue("$entrada", HoraDb(contexto.HoraEntrada));
        comando.Parameters.AddWithValue("$salida", HoraDb(contexto.HoraSalida));
        comando.ExecuteNonQuery();
    }

    private static void GuardarContextoNem(
        SqliteConnection conexion,
        SqliteTransaction transaccion,
        ContextoGrupo contexto)
    {
        using var comando = conexion.CreateCommand();
        comando.Transaction = transaccion;
        comando.CommandText = """
            INSERT INTO contexto_nem_grupo (
                grupo_id, organizacion_escolar, entidad_catalogo, municipio_catalogo)
            VALUES ($grupo, $organizacion, $entidad, $municipio)
            ON CONFLICT(grupo_id) DO UPDATE SET
                organizacion_escolar = excluded.organizacion_escolar,
                entidad_catalogo = excluded.entidad_catalogo,
                municipio_catalogo = excluded.municipio_catalogo;
            """;
        comando.Parameters.AddWithValue("$grupo", contexto.GrupoId.ToString());
        comando.Parameters.AddWithValue("$organizacion", (int)contexto.OrganizacionEscolar);
        comando.Parameters.AddWithValue("$entidad", contexto.EntidadFederativa);
        comando.Parameters.AddWithValue("$municipio", contexto.Municipio);
        comando.ExecuteNonQuery();
    }

    private static void GuardarGrados(
        SqliteConnection conexion,
        SqliteTransaction transaccion,
        ContextoGrupo contexto)
    {
        var grados = CatalogoNemPrimaria.NormalizarGrados(contexto.GradosAtendidos);

        using (var borrar = conexion.CreateCommand())
        {
            borrar.Transaction = transaccion;
            borrar.CommandText = "DELETE FROM grados_grupo WHERE grupo_id = $grupo;";
            borrar.Parameters.AddWithValue("$grupo", contexto.GrupoId.ToString());
            borrar.ExecuteNonQuery();
        }

        foreach (var grado in grados)
        {
            using var insertar = conexion.CreateCommand();
            insertar.Transaction = transaccion;
            insertar.CommandText = "INSERT INTO grados_grupo (grupo_id, grado) VALUES ($grupo, $grado);";
            insertar.Parameters.AddWithValue("$grupo", contexto.GrupoId.ToString());
            insertar.Parameters.AddWithValue("$grado", (int)grado);
            insertar.ExecuteNonQuery();
        }

        // In a one-grade classroom there is no ambiguity: every student belongs to that
        // grade. This also gives deterministic structured data to legacy/demo rosters.
        if (grados.Count == 1)
        {
            using var asignar = conexion.CreateCommand();
            asignar.Transaction = transaccion;
            asignar.CommandText = """
                INSERT INTO grados_estudiante (grupo_id, estudiante_id, grado)
                SELECT grupo_id, id, $grado
                FROM estudiantes
                WHERE grupo_id = $grupo
                ON CONFLICT(grupo_id, estudiante_id) DO UPDATE SET grado = excluded.grado;
                """;
            asignar.Parameters.AddWithValue("$grupo", contexto.GrupoId.ToString());
            asignar.Parameters.AddWithValue("$grado", (int)grados[0]);
            asignar.ExecuteNonQuery();
        }
    }

    private static (OrganizacionEscolar Organizacion, string Entidad, string Municipio) LeerContextoNem(
        SqliteConnection conexion,
        GrupoId grupoId)
    {
        using var comando = conexion.CreateCommand();
        comando.CommandText = """
            SELECT organizacion_escolar, entidad_catalogo, municipio_catalogo
            FROM contexto_nem_grupo WHERE grupo_id = $grupo;
            """;
        comando.Parameters.AddWithValue("$grupo", grupoId.ToString());
        using var lector = comando.ExecuteReader();
        return lector.Read()
            ? ((OrganizacionEscolar)lector.GetInt32(0), lector.GetString(1), lector.GetString(2))
            : (OrganizacionEscolar.NoEspecificada, string.Empty, string.Empty);
    }

    private static List<GradoPrimaria> LeerGrados(SqliteConnection conexion, GrupoId grupoId)
    {
        using var comando = conexion.CreateCommand();
        comando.CommandText = "SELECT grado FROM grados_grupo WHERE grupo_id = $grupo ORDER BY grado;";
        comando.Parameters.AddWithValue("$grupo", grupoId.ToString());
        var grados = new List<GradoPrimaria>();
        using var lector = comando.ExecuteReader();
        while (lector.Read()) grados.Add((GradoPrimaria)lector.GetInt32(0));
        return grados;
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
        EsquemaNemMultigradoSqlite.Inicializar(conexion);
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

    private sealed record DatosLegacy(
        string CicloEscolar,
        string NombreEscuela,
        string Cct,
        string EntidadFederativa,
        string Municipio,
        string Localidad,
        string Grado,
        string Grupo,
        string Turno,
        EtapaDesarrolloCognoscitivo EtapaCognoscitiva,
        string DocenteResponsable,
        DateOnly? ResponsableDesde,
        DateOnly? ResponsableHasta,
        TimeOnly? HoraEntrada,
        TimeOnly? HoraSalida);
}