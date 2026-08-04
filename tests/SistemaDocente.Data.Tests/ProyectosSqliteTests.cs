using Microsoft.Data.Sqlite;

using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Data.Tests;

public sealed class ProyectosSqliteTests : IDisposable
{
    private readonly BaseSqliteTemporal _base = new();

    [Fact]
    public void GuardaRecargaProyectoYDetectaConcurrencia()
    {
        var grupo = CrearGrupo();
        var almacenamiento = new PersistenciaProyectosSqlite(_base.Ruta);
        var proyecto = ProyectoDidactico.Crear(grupo.Id, "Proyecto", "Descripción",
            new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), "Observación");

        almacenamiento.Guardar(proyecto, null);
        var cargado = almacenamiento.Cargar(proyecto.Id)!;
        cargado.Iniciar();
        almacenamiento.Guardar(cargado, cargado.Version);

        Assert.Equal(EstadoProyecto.EnCurso, almacenamiento.Cargar(proyecto.Id)!.Estado);
        Assert.Throws<ConflictoConcurrenciaException>(() => almacenamiento.Guardar(cargado, cargado.Version));
    }

    [Fact]
    public void GuardaActividadYEntregasEnUnaUnidadReabrible()
    {
        var grupo = CrearGrupo();
        var estudiante = grupo.AgregarEstudiante("Ana", 1);
        _base.Persistencia.Guardar(grupo);
        var almacenamiento = new PersistenciaProyectosSqlite(_base.Ruta);
        var proyecto = ProyectoDidactico.Crear(grupo.Id, "P", "", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), "");
        almacenamiento.Guardar(proyecto, null);
        var actividad = ActividadProyecto.Crear(proyecto.Id, grupo.Id, "A", "",
            new DateOnly(2026, 1, 10), "", proyecto.FechaInicio, proyecto.FechaTermino, [estudiante.Id]);
        actividad.ActualizarEntregas([new(estudiante.Id, EstadoEntrega.Entregada, "Bien")]);

        almacenamiento.Guardar(actividad, null);
        var reabierta = new PersistenciaProyectosSqlite(_base.Ruta);
        var cargada = ((IAlmacenamientoActividadesProyecto)reabierta).Cargar(actividad.Id)!;

        Assert.Equal(EstadoEntrega.Entregada, Assert.Single(cargada.Entregas).Estado);
        Assert.Single(reabierta.ListarPorProyecto(proyecto.Id));
    }

    [Fact]
    public void MigraVersionDosSimuladaATresSinPerderGrupo()
    {
        var grupo = CrearGrupo();
        using (var conexion = _base.AbrirConexion())
        using (var comando = conexion.CreateCommand())
        {
            comando.CommandText = "DROP INDEX ix_entregas_estudiante; DROP INDEX ix_actividades_proyecto_fecha; DROP INDEX ix_proyectos_grupo_estado_fecha; DROP TABLE entregas_actividad; DROP TABLE actividades_proyecto; DROP TABLE proyectos_didacticos; PRAGMA user_version=2;";
            comando.ExecuteNonQuery();
        }

        _base.Persistencia.Inicializar();

        Assert.NotNull(_base.Persistencia.Cargar(grupo.Id));
        using var verificacion = _base.AbrirConexion(); using var consulta = verificacion.CreateCommand(); consulta.CommandText = "PRAGMA user_version";
        Assert.Equal(3L, consulta.ExecuteScalar());
    }

    private Grupo CrearGrupo()
    {
        var grupo = Grupo.Crear("Grupo"); _base.Persistencia.Guardar(grupo); return grupo;
    }

    public void Dispose() => _base.Dispose();
}