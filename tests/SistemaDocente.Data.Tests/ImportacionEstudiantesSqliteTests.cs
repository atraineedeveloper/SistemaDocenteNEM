using Microsoft.Data.Sqlite;

using SistemaDocente.Application;
using SistemaDocente.Core;
using SistemaDocente.Data;

namespace SistemaDocente.Data.Tests;

public sealed class ImportacionEstudiantesSqliteTests
{
    [Fact]
    public void ConfirmarImportacionPersisteTodasLasFilasEnUnSoloAgregado()
    {
        var ruta = CrearRutaTemporal();

        try
        {
            var grupos = new PersistenciaGrupoSqlite(ruta);
            var contextos = new PersistenciaContextoGrupoSqlite(ruta);
            var grupo = Grupo.Crear("4.º A");
            grupos.Guardar(grupo);
            contextos.Guardar(ContextoGrupo.Crear(
                grupo.Id,
                gradosAtendidos: [GradoPrimaria.Cuarto]));
            var casosUso = new ImportacionEstudiantesCasosUso(grupos, contextos);
            var previa = casosUso.Revalidar(
                grupo.Id,
                [
                    CrearFila(2, "1", "Ana López"),
                    CrearFila(3, "2", "Luis Pérez"),
                ]);

            var resultado = casosUso.Confirmar(grupo.Id, previa.Filas);

            Assert.True(resultado.Completada);
            Assert.Equal(2, resultado.Importados);
            var recargado = Assert.IsType<Grupo>(grupos.Cargar(grupo.Id));
            Assert.Equal(2, recargado.Estudiantes.Count);
            Assert.All(recargado.Estudiantes, estudiante => Assert.Equal(GradoPrimaria.Cuarto, estudiante.Grado));
        }
        finally
        {
            EliminarArchivosSqlite(ruta);
        }
    }

    [Fact]
    public void FalloEnSegundaInsercionRevierteTodaLaImportacion()
    {
        var ruta = CrearRutaTemporal();

        try
        {
            var grupos = new PersistenciaGrupoSqlite(ruta);
            var contextos = new PersistenciaContextoGrupoSqlite(ruta);
            var grupo = Grupo.Crear("4.º A");
            grupos.Guardar(grupo);
            contextos.Guardar(ContextoGrupo.Crear(
                grupo.Id,
                gradosAtendidos: [GradoPrimaria.Cuarto]));
            CrearTriggerDeFallo(ruta);

            var casosUso = new ImportacionEstudiantesCasosUso(grupos, contextos);
            var previa = casosUso.Revalidar(
                grupo.Id,
                [
                    CrearFila(2, "1", "Ana López"),
                    CrearFila(3, "2", "Falla Controlada"),
                ]);

            Assert.Throws<ErrorPersistenciaAplicacionException>(
                () => casosUso.Confirmar(grupo.Id, previa.Filas));

            var recargado = Assert.IsType<Grupo>(grupos.Cargar(grupo.Id));
            Assert.Empty(recargado.Estudiantes);
        }
        finally
        {
            EliminarArchivosSqlite(ruta);
        }
    }

    private static FilaImportacionEstudiante CrearFila(
        int numeroOrigen,
        string numeroLista,
        string nombreCompleto) =>
        new(
            numeroOrigen,
            numeroLista,
            nombreCompleto,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty);

    private static void CrearTriggerDeFallo(string ruta)
    {
        using var conexion = new SqliteConnection($"Data Source={ruta};Pooling=False");
        conexion.Open();
        using var comando = conexion.CreateCommand();
        comando.CommandText = """
            CREATE TRIGGER prueba_importacion_falla
            BEFORE INSERT ON estudiantes
            WHEN NEW.nombre = 'Falla Controlada'
            BEGIN
                SELECT RAISE(ABORT, 'fallo controlado de importación');
            END;
            """;
        comando.ExecuteNonQuery();
    }

    private static string CrearRutaTemporal() =>
        Path.Combine(Path.GetTempPath(), $"sistema-docente-import-{Guid.NewGuid():N}.sqlite");

    private static void EliminarArchivosSqlite(string ruta)
    {
        foreach (var archivo in new[] { ruta, $"{ruta}-wal", $"{ruta}-shm" })
        {
            if (File.Exists(archivo))
            {
                File.Delete(archivo);
            }
        }
    }
}
