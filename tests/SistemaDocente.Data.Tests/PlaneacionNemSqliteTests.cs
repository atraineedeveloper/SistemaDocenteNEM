using System.Globalization;

using Microsoft.Data.Sqlite;

using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Data.Tests;

public sealed class PlaneacionNemSqliteTests
{
    [Fact]
    public void ProyectoYActividadConservanMetadatosNemEnRoundTrip()
    {
        using var baseDatos = new BaseSqliteTemporal();
        var grupo = Grupo.Crear("Multigrado");
        var estudiante = grupo.AgregarEstudiante("Ana", 1, grado: GradoPrimaria.Segundo);
        baseDatos.Persistencia.Guardar(grupo);
        var persistencia = new PersistenciaProyectosSqlite(baseDatos.Ruta);

        var proyecto = ProyectoDidactico.Crear(
            grupo.Id,
            "Agua comunitaria",
            "",
            new DateOnly(2026, 9, 1),
            new DateOnly(2026, 9, 30),
            "",
            MetodologiaProyectoNem.IndagacionSteam,
            [GradoPrimaria.Segundo, GradoPrimaria.Tercero]);
        persistencia.Guardar(proyecto, null);

        var actividad = ActividadProyecto.Crear(
            proyecto.Id,
            grupo.Id,
            "Consumo de agua",
            "",
            new DateOnly(2026, 9, 10),
            "",
            proyecto.FechaInicio,
            proyecto.FechaTermino,
            [estudiante.Id],
            CampoFormativoNem.SaberesPensamientoCientifico,
            [GradoPrimaria.Segundo]);
        persistencia.Guardar(actividad, null);

        var proyectoLeido = persistencia.Cargar(proyecto.Id);
        var actividadLeida = ((IAlmacenamientoActividadesProyecto)persistencia).Cargar(actividad.Id);

        Assert.NotNull(proyectoLeido);
        Assert.Equal(MetodologiaProyectoNem.IndagacionSteam, proyectoLeido.Metodologia);
        Assert.Equal(
            [GradoPrimaria.Segundo, GradoPrimaria.Tercero],
            proyectoLeido.GradosObjetivo);
        Assert.NotNull(actividadLeida);
        Assert.Equal(CampoFormativoNem.SaberesPensamientoCientifico, actividadLeida.CampoFormativo);
        Assert.Equal([GradoPrimaria.Segundo], actividadLeida.GradosObjetivo);
    }

    [Fact]
    public void MigracionLegacyNoInventaPlaneacionPedagogica()
    {
        using var baseDatos = new BaseSqliteTemporal();
        var grupo = Grupo.Crear("Cuarto A");
        var estudiante = grupo.AgregarEstudiante("Ana", 1, grado: GradoPrimaria.Cuarto);
        baseDatos.Persistencia.Guardar(grupo);
        var proyectoId = ProyectoId.Crear();
        var actividadId = ActividadId.Crear();

        using (var conexion = baseDatos.AbrirConexion())
        using (var transaccion = conexion.BeginTransaction())
        {
            using var proyecto = conexion.CreateCommand();
            proyecto.Transaction = transaccion;
            proyecto.CommandText = """
                INSERT INTO proyectos_didacticos(
                    proyecto_id,grupo_id,nombre,descripcion,fecha_inicio,fecha_termino,estado,observaciones,version)
                VALUES($id,$grupo,'Legacy','', '2026-09-01','2026-09-30',0,'',1)
                """;
            proyecto.Parameters.AddWithValue("$id", proyectoId.ToString());
            proyecto.Parameters.AddWithValue("$grupo", grupo.Id.ToString());
            proyecto.ExecuteNonQuery();

            using var actividad = conexion.CreateCommand();
            actividad.Transaction = transaccion;
            actividad.CommandText = """
                INSERT INTO actividades_proyecto(
                    actividad_id,proyecto_id,grupo_id,titulo,descripcion,fecha_realizacion,
                    observaciones_generales,estado,version)
                VALUES($id,$proyecto,$grupo,'Legacy','', '2026-09-10','',0,1)
                """;
            actividad.Parameters.AddWithValue("$id", actividadId.ToString());
            actividad.Parameters.AddWithValue("$proyecto", proyectoId.ToString());
            actividad.Parameters.AddWithValue("$grupo", grupo.Id.ToString());
            actividad.ExecuteNonQuery();

            using var entrega = conexion.CreateCommand();
            entrega.Transaction = transaccion;
            entrega.CommandText = """
                INSERT INTO entregas_actividad(
                    actividad_id,estudiante_id,grupo_id,estado_entrega,observacion)
                VALUES($actividad,$estudiante,$grupo,0,'')
                """;
            entrega.Parameters.AddWithValue("$actividad", actividadId.ToString());
            entrega.Parameters.AddWithValue("$estudiante", estudiante.Id.ToString());
            entrega.Parameters.AddWithValue("$grupo", grupo.Id.ToString());
            entrega.ExecuteNonQuery();
            transaccion.Commit();
        }

        var persistencia = new PersistenciaProyectosSqlite(baseDatos.Ruta);
        var proyectoLeido = persistencia.Cargar(proyectoId);
        var actividadLeida = ((IAlmacenamientoActividadesProyecto)persistencia).Cargar(actividadId);

        Assert.NotNull(proyectoLeido);
        Assert.Equal(MetodologiaProyectoNem.NoEspecificada, proyectoLeido.Metodologia);
        Assert.Empty(proyectoLeido.GradosObjetivo);
        Assert.NotNull(actividadLeida);
        Assert.Equal(CampoFormativoNem.NoEspecificado, actividadLeida.CampoFormativo);
        Assert.Empty(actividadLeida.GradosObjetivo);

        using var verificacion = baseDatos.AbrirConexion();
        using var version = verificacion.CreateCommand();
        version.CommandText = "SELECT version FROM esquema_extensiones WHERE nombre='nem-planeacion-proyectos'";
        Assert.Equal(1L, Convert.ToInt64(version.ExecuteScalar(), CultureInfo.InvariantCulture));
        using var userVersion = verificacion.CreateCommand();
        userVersion.CommandText = "PRAGMA user_version";
        Assert.Equal(6L, Convert.ToInt64(userVersion.ExecuteScalar(), CultureInfo.InvariantCulture));
    }

    [Fact]
    public void AperturaActualCreaMetadatosParaFilaLegacyPosteriorALaExtension()
    {
        using var baseDatos = new BaseSqliteTemporal();
        var grupo = Grupo.Crear("Quinto A");
        baseDatos.Persistencia.Guardar(grupo);
        var persistencia = new PersistenciaProyectosSqlite(baseDatos.Ruta);

        var inicial = ProyectoDidactico.Crear(
            grupo.Id,
            "Inicial",
            "",
            new DateOnly(2026, 9, 1),
            new DateOnly(2026, 9, 30),
            "");
        persistencia.Guardar(inicial, null);

        var legacyPosterior = ProyectoId.Crear();
        using (var conexion = baseDatos.AbrirConexion())
        using (var comando = conexion.CreateCommand())
        {
            comando.CommandText = """
                INSERT INTO proyectos_didacticos(
                    proyecto_id,grupo_id,nombre,descripcion,fecha_inicio,fecha_termino,estado,observaciones,version)
                VALUES($id,$grupo,'Posterior','', '2026-10-01','2026-10-31',0,'',1)
                """;
            comando.Parameters.AddWithValue("$id", legacyPosterior.ToString());
            comando.Parameters.AddWithValue("$grupo", grupo.Id.ToString());
            comando.ExecuteNonQuery();
        }

        var leido = persistencia.Cargar(legacyPosterior);

        Assert.NotNull(leido);
        Assert.Equal(MetodologiaProyectoNem.NoEspecificada, leido.Metodologia);
        using var verificacion = baseDatos.AbrirConexion();
        using var comandoVerificacion = verificacion.CreateCommand();
        comandoVerificacion.CommandText = "SELECT metodologia FROM proyectos_nem WHERE proyecto_id=$id";
        comandoVerificacion.Parameters.AddWithValue("$id", legacyPosterior.ToString());
        Assert.Equal(0L, Convert.ToInt64(comandoVerificacion.ExecuteScalar(), CultureInfo.InvariantCulture));
    }
}