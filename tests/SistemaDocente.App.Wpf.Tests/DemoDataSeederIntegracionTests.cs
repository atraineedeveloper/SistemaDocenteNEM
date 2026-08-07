using Microsoft.Data.Sqlite;

using SistemaDocente.App.Wpf.Demo;
using SistemaDocente.Core;
using SistemaDocente.Data;

namespace SistemaDocente.App.Wpf.Tests;

public sealed class DemoDataSeederIntegracionTests
{
    [Fact]
    public void SeederDemoCreaPlaneacionNemCompletaEIdempotente()
    {
        var directorio = Path.Combine(
            Path.GetTempPath(),
            "SistemaDocenteNEM.Demo.Tests",
            Guid.NewGuid().ToString("N"));
        var ruta = Path.Combine(directorio, "demo.db");
        Directory.CreateDirectory(directorio);

        try
        {
            var grupos = new PersistenciaGrupoSqlite(ruta);
            var asistencias = new PersistenciaAsistenciaSqlite(ruta);
            var proyectos = new PersistenciaProyectosSqlite(ruta);
            var expedientes = new PersistenciaExpedienteSqlite(ruta);

            var grupoId = DemoDataSeeder.AsegurarDatos(
                grupos,
                asistencias,
                proyectos,
                expedientes);
            var segundoGrupoId = DemoDataSeeder.AsegurarDatos(
                grupos,
                asistencias,
                proyectos,
                expedientes);

            Assert.Equal(grupoId, segundoGrupoId);
            Assert.Equal(2, grupos.ListarTodos().Count);

            var grupo = grupos.Cargar(grupoId);
            Assert.NotNull(grupo);
            Assert.Equal(31, grupo.Estudiantes.Count);
            Assert.Equal(30, grupo.EstudiantesActivos.Count);
            Assert.All(
                grupo.Estudiantes,
                estudiante => Assert.Equal(GradoPrimaria.Cuarto, estudiante.Grado));

            var proyectosDemo = proyectos.ListarPorGrupo(grupoId);
            Assert.Equal(3, proyectosDemo.Count);
            Assert.All(proyectosDemo, proyecto =>
            {
                Assert.NotEqual(MetodologiaProyectoNem.NoEspecificada, proyecto.Metodologia);
                Assert.Equal([GradoPrimaria.Cuarto], proyecto.GradosObjetivo);
            });

            var actividades = proyectosDemo
                .SelectMany(proyecto => proyectos.ListarPorProyecto(proyecto.Id))
                .ToArray();
            Assert.Equal(16, actividades.Length);
            Assert.All(actividades, actividad =>
            {
                Assert.NotEqual(CampoFormativoNem.NoEspecificado, actividad.CampoFormativo);
                Assert.Equal([GradoPrimaria.Cuarto], actividad.GradosObjetivo);
            });
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (Directory.Exists(directorio))
            {
                Directory.Delete(directorio, true);
            }
        }
    }
}
