using Microsoft.Data.Sqlite;

using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Data.Tests;

public sealed class EstadoEntregaExplicitoSqliteTests : IDisposable
{
    private readonly BaseSqliteTemporal _base = new();

    [Fact]
    public void ExtensionAditivaConservaUserVersionSeisYReabreEntregaPendienteDeEvaluacion()
    {
        var grupo = Grupo.Crear("Cuarto A");
        var estudiante = grupo.AgregarEstudiante(
            "Ana López",
            1,
            fechaNacimiento: new DateOnly(2016, 4, 12),
            genero: GeneroEstudiante.Mujer,
            fechaIngreso: new DateOnly(2026, 8, 1));
        _base.Persistencia.Guardar(grupo);

        var persistenciaProyectos = new PersistenciaProyectosSqlite(_base.Ruta);
        var casosUso = new GestionProyectosActividadesCasosUso(
            _base.Persistencia,
            persistenciaProyectos,
            persistenciaProyectos);
        var proyecto = casosUso.CrearProyecto(
            grupo.Id,
            new EntradaProyecto(
                "Proyecto",
                "",
                new DateOnly(2026, 8, 1),
                new DateOnly(2026, 8, 31),
                ""));
        var actividad = casosUso.CrearActividad(
            proyecto.ProyectoId,
            new EntradaActividad(
                "Actividad",
                "",
                new DateOnly(2026, 8, 7),
                "",
                [new EntradaEntregaActividad(
                    estudiante.Id,
                    EstadoEntregaActividad.Entregada,
                    NivelLogro.Pendiente,
                    "Recibida; evaluación pendiente.")]));

        var persistenciaReabierta = new PersistenciaProyectosSqlite(_base.Ruta);
        var casosUsoReabiertos = new GestionProyectosActividadesCasosUso(
            _base.Persistencia,
            persistenciaReabierta,
            persistenciaReabierta);
        var cargada = casosUsoReabiertos.ObtenerActividad(actividad.ActividadId);
        var entrega = Assert.Single(cargada.Entregas);

        Assert.Equal(EstadoEntregaActividad.Entregada, entrega.EstadoEntrega);
        Assert.Equal(NivelLogro.Pendiente, entrega.NivelLogro);
        Assert.Equal("Recibida; evaluación pendiente.", entrega.Observacion);

        using var conexion = _base.AbrirConexion();
        Assert.Equal(6L, EjecutarEntero(conexion, "PRAGMA user_version;"));
        Assert.Equal(1L, EjecutarEntero(
            conexion,
            "SELECT version FROM esquema_extensiones WHERE nombre='reportes-contexto-entregas';"));
    }

    [Fact]
    public void ExtensionMigraNoEntregoLegadoSinReconstruirEsquemaBase()
    {
        var grupo = Grupo.Crear("Quinto B");
        var estudiante = grupo.AgregarEstudiante(
            "Luis Pérez",
            1,
            fechaNacimiento: new DateOnly(2015, 2, 10),
            genero: GeneroEstudiante.Hombre,
            fechaIngreso: new DateOnly(2026, 8, 1));
        _base.Persistencia.Guardar(grupo);

        var proyectoGuid = Guid.NewGuid();
        var actividadGuid = Guid.NewGuid();
        using (var conexion = _base.AbrirConexion())
        using (var transaccion = conexion.BeginTransaction())
        {
            Ejecutar(
                conexion,
                transaccion,
                """
                INSERT INTO proyectos_didacticos(
                    proyecto_id, grupo_id, nombre, descripcion, fecha_inicio, fecha_termino,
                    estado, observaciones, version)
                VALUES($proyecto, $grupo, 'Proyecto legado', '', '2026-08-01', '2026-08-31', 0, '', 1);
                """,
                ("$proyecto", proyectoGuid.ToString("D")),
                ("$grupo", grupo.Id.ToString()));
            Ejecutar(
                conexion,
                transaccion,
                """
                INSERT INTO actividades_proyecto(
                    actividad_id, proyecto_id, grupo_id, titulo, descripcion, fecha_realizacion,
                    observaciones_generales, estado, version)
                VALUES($actividad, $proyecto, $grupo, 'Actividad legada', '', '2026-08-07', '', 0, 1);
                """,
                ("$actividad", actividadGuid.ToString("D")),
                ("$proyecto", proyectoGuid.ToString("D")),
                ("$grupo", grupo.Id.ToString()));
            Ejecutar(
                conexion,
                transaccion,
                """
                INSERT INTO entregas_actividad(
                    actividad_id, estudiante_id, grupo_id, estado_entrega, observacion)
                VALUES($actividad, $estudiante, $grupo, 5, 'No entregó en formato legado');
                """,
                ("$actividad", actividadGuid.ToString("D")),
                ("$estudiante", estudiante.Id.ToString()),
                ("$grupo", grupo.Id.ToString()));
            transaccion.Commit();
        }

        var persistencia = new PersistenciaProyectosSqlite(_base.Ruta);
        var actividades = persistencia.ListarPorProyecto(ProyectoId.DesdeGuid(proyectoGuid));
        var actividad = Assert.Single(actividades);
        var entrega = Assert.Single(actividad.Entregas);

        Assert.Equal(EstadoEntregaActividad.NoEntregada, entrega.EstadoEntrega);
        Assert.Equal(NivelLogro.Pendiente, entrega.NivelLogro);
        Assert.Equal("No entregó en formato legado", entrega.Observacion);

        using var verificacion = _base.AbrirConexion();
        Assert.Equal(6L, EjecutarEntero(verificacion, "PRAGMA user_version;"));
        Assert.Equal(0L, EjecutarEntero(
            verificacion,
            "SELECT estado_entrega FROM entregas_actividad LIMIT 1;"));
        Assert.Equal(2L, EjecutarEntero(
            verificacion,
            "SELECT estado_entrega FROM estados_entrega_actividad LIMIT 1;"));
        Assert.Equal(1L, EjecutarEntero(
            verificacion,
            "SELECT version FROM esquema_extensiones WHERE nombre='reportes-contexto-entregas';"));
    }

    private static long EjecutarEntero(SqliteConnection conexion, string sql)
    {
        using var comando = conexion.CreateCommand();
        comando.CommandText = sql;
        return Convert.ToInt64(comando.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture);
    }

    private static void Ejecutar(
        SqliteConnection conexion,
        SqliteTransaction transaccion,
        string sql,
        params (string Nombre, object Valor)[] parametros)
    {
        using var comando = conexion.CreateCommand();
        comando.Transaction = transaccion;
        comando.CommandText = sql;
        foreach (var (nombre, valor) in parametros)
        {
            comando.Parameters.AddWithValue(nombre, valor);
        }
        comando.ExecuteNonQuery();
    }

    public void Dispose() => _base.Dispose();
}