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
        actividad.ActualizarEntregas([new(estudiante.Id, NivelLogro.Domina, "Bien")]);

        almacenamiento.Guardar(actividad, null);
        var reabierta = new PersistenciaProyectosSqlite(_base.Ruta);
        var cargada = ((IAlmacenamientoActividadesProyecto)reabierta).Cargar(actividad.Id)!;

        Assert.Equal(NivelLogro.Domina, Assert.Single(cargada.Entregas).NivelLogro);
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
        Assert.Equal(6L, consulta.ExecuteScalar());
    }

    [Fact]
    public void MigraVersionTresSimuladaACuatroSinPerderDatosDeEntregas()
    {
        var grupo = CrearGrupo();
        var estudiante = grupo.AgregarEstudiante("Estudiante", 1);
        _base.Persistencia.Guardar(grupo);
        var almacenamiento = new PersistenciaProyectosSqlite(_base.Ruta);
        var proyecto = ProyectoDidactico.Crear(grupo.Id, "P", "", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), "");
        almacenamiento.Guardar(proyecto, null);
        var actividad = ActividadProyecto.Crear(proyecto.Id, grupo.Id, "A", "",
            new DateOnly(2026, 1, 10), "", proyecto.FechaInicio, proyecto.FechaTermino, [estudiante.Id]);
        actividad.ActualizarEntregas([new(estudiante.Id, NivelLogro.Domina, "Bien")]);
        almacenamiento.Guardar(actividad, null);

        using (var conexion = _base.AbrirConexion())
        using (var comando = conexion.CreateCommand())
        {
            comando.CommandText = """
                CREATE TABLE entregas_actividad_temp AS SELECT * FROM entregas_actividad;
                DROP TABLE entregas_actividad;
                CREATE TABLE entregas_actividad (
                    actividad_id TEXT NOT NULL,
                    estudiante_id TEXT NOT NULL,
                    grupo_id TEXT NOT NULL,
                    estado_entrega INTEGER NOT NULL CHECK (estado_entrega IN (0, 1, 2)),
                    observacion TEXT NOT NULL CHECK (length(observacion) <= 500),
                    PRIMARY KEY (actividad_id, estudiante_id),
                    FOREIGN KEY (actividad_id, grupo_id) REFERENCES actividades_proyecto(actividad_id, grupo_id) ON DELETE RESTRICT,
                    FOREIGN KEY (estudiante_id, grupo_id) REFERENCES estudiantes(id, grupo_id) ON DELETE RESTRICT
                );
                INSERT INTO entregas_actividad SELECT * FROM entregas_actividad_temp;
                DROP TABLE entregas_actividad_temp;
                CREATE INDEX IF NOT EXISTS ix_entregas_estudiante ON entregas_actividad(estudiante_id);
                PRAGMA user_version = 3;
                """;
            comando.ExecuteNonQuery();
        }

        _base.Persistencia.Inicializar();

        using var verificacion = _base.AbrirConexion();
        using var consulta = verificacion.CreateCommand();
        consulta.CommandText = "PRAGMA user_version";
        Assert.Equal(6L, consulta.ExecuteScalar());

        var reabierta = new PersistenciaProyectosSqlite(_base.Ruta);
        var cargada = ((IAlmacenamientoActividadesProyecto)reabierta).Cargar(actividad.Id)!;
        Assert.Equal(NivelLogro.Domina, Assert.Single(cargada.Entregas).NivelLogro);
    }

    private Grupo CrearGrupo()
    {
        var grupo = Grupo.Crear("Grupo"); _base.Persistencia.Guardar(grupo); return grupo;
    }

    public void Dispose() => _base.Dispose();
}