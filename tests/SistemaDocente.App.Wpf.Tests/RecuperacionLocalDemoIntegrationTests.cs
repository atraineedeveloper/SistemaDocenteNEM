using Microsoft.Data.Sqlite;

using SistemaDocente.App.Wpf.Demo;
using SistemaDocente.Application;
using SistemaDocente.Data;

namespace SistemaDocente.App.Wpf.Tests;

public sealed class RecuperacionLocalDemoIntegrationTests
{
    [Fact]
    public void DemoSeRespaldaSeMutaYSeRecuperaAlReabrirAlmacenamiento()
    {
        var directorio = Path.Combine(
            Path.GetTempPath(),
            "SistemaDocenteNEM-RecoveryDemo-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directorio);
        var baseSqlite = Path.Combine(directorio, "sistema-docente.db");
        var estado = Path.Combine(directorio, "app-state.json");
        var seguridad = Path.Combine(directorio, "backups", "safety");
        var respaldo = Path.Combine(directorio, "demo-original.sdocbackup");

        try
        {
            var grupos = new PersistenciaGrupoSqlite(baseSqlite);
            var asistencias = new PersistenciaAsistenciaSqlite(baseSqlite);
            var proyectos = new PersistenciaProyectosSqlite(baseSqlite);
            var expedientes = new PersistenciaExpedienteSqlite(baseSqlite);
            var contextos = new PersistenciaContextoGrupoSqlite(baseSqlite);
            var grupoId = DemoDataSeeder.AsegurarDatos(
                grupos,
                asistencias,
                proyectos,
                expedientes);
            DemoContextSeeder.AsegurarContexto(contextos, grupoId);
            File.WriteAllText(estado, $"{{\"grupoId\":\"{grupoId.Valor}\"}}");

            var grupoOriginal = grupos.Cargar(grupoId)!;
            var nombreOriginal = grupoOriginal.NombreVisible;
            Assert.Equal(31, grupoOriginal.Estudiantes.Count);
            Assert.Equal(3, proyectos.ListarPorGrupo(grupoId).Count);

            var servicio = new ServicioRecuperacionLocalSqlite(
                baseSqlite,
                estado,
                seguridad,
                ModoAlmacenamientoLocal.Demostracion);
            servicio.CrearRespaldo(
                respaldo,
                new DateTimeOffset(2026, 8, 8, 4, 15, 0, TimeSpan.Zero),
                "demo-test");

            var mutado = grupos.Cargar(grupoId)!;
            mutado.Renombrar("Grupo Demo alterado después del respaldo");
            mutado.AgregarEstudiante("Alumno temporal para restauración", 99);
            grupos.Guardar(mutado);
            Assert.Equal(32, grupos.Cargar(grupoId)!.Estudiantes.Count);

            var resultado = servicio.Restaurar(
                respaldo,
                new DateTimeOffset(2026, 8, 8, 4, 16, 0, TimeSpan.Zero),
                "demo-test");
            Assert.True(resultado.ReinicioRequerido);

            SqliteConnection.ClearAllPools();
            var gruposReabiertos = new PersistenciaGrupoSqlite(baseSqlite);
            var proyectosReabiertos = new PersistenciaProyectosSqlite(baseSqlite);
            var restaurado = gruposReabiertos.Cargar(grupoId)!;

            Assert.Equal(nombreOriginal, restaurado.NombreVisible);
            Assert.Equal(31, restaurado.Estudiantes.Count);
            Assert.Equal(30, restaurado.EstudiantesActivos.Count);
            Assert.DoesNotContain(
                restaurado.Estudiantes,
                estudiante => estudiante.NombreVisible == "Alumno temporal para restauración");
            Assert.Equal(3, proyectosReabiertos.ListarPorGrupo(grupoId).Count);
            Assert.True(File.Exists(resultado.RutaRespaldoSeguridad));
            Assert.Contains(grupoId.Valor.ToString(), File.ReadAllText(estado), StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (Directory.Exists(directorio))
            {
                Directory.Delete(directorio, recursive: true);
            }
        }
    }
}