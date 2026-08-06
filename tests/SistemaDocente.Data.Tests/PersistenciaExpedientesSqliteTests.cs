using Microsoft.Data.Sqlite;
using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Data.Tests;

public sealed class PersistenciaExpedientesSqliteTests
{
    [Fact]
    public void RegistrarYObtenerExpedienteEstudianteFunciona()
    {
        var archivo = Path.Combine(Path.GetTempPath(), $"test_expediente_{Guid.NewGuid():N}.sqlite");
        try
        {
            var grupoId = GrupoId.DesdeGuid(Guid.NewGuid());
            var estId = EstudianteId.DesdeGuid(Guid.NewGuid());

            new PersistenciaGrupoSqlite(archivo).Inicializar();
            using (var conexion = new SqliteConnection($"Data Source={archivo}"))
            {
                conexion.Open();
                using var cmd = conexion.CreateCommand();
                cmd.CommandText = """
                    INSERT INTO grupos VALUES ($g, 'Grupo Test');
                    INSERT INTO estudiantes (id, grupo_id, nombre, numero_lista, activo) VALUES ($e, $g, 'Estudiante Test', 1, 1);
                    """;
                cmd.Parameters.AddWithValue("$g", grupoId.Valor.ToString());
                cmd.Parameters.AddWithValue("$e", estId.Valor.ToString());
                cmd.ExecuteNonQuery();
            }

            var repo = new PersistenciaExpedienteSqlite(archivo);
            repo.RegistrarNotaPedagogica(estId, grupoId, TipoNotaPedagogica.Fortaleza, "Muy dedicado en lectura", DateTime.Now);
            repo.RegistrarAcuerdoTutor(estudianteId: estId, grupoId: grupoId, motivo: "Bajo cumplimiento", acuerdo: "Apoyo diario en casa", fechaReunion: new DateOnly(2026, 2, 1), fechaSeguimiento: new DateOnly(2026, 2, 15));

            var exp = repo.ObtenerExpediente(estId, grupoId);

            Assert.Single(exp.Notas);
            Assert.Equal(TipoNotaPedagogica.Fortaleza, exp.Notas[0].Tipo);
            Assert.Equal("Muy dedicado en lectura", exp.Notas[0].Contenido);

            Assert.Single(exp.Acuerdos);
            Assert.Equal("Bajo cumplimiento", exp.Acuerdos[0].Motivo);
            Assert.Equal("Apoyo diario en casa", exp.Acuerdos[0].AcuerdoConvenido);
            Assert.Equal(new DateOnly(2026, 2, 15), exp.Acuerdos[0].FechaSeguimiento);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (File.Exists(archivo)) File.Delete(archivo);
        }
    }

    [Fact]
    public void RegistrarNotaPedagogicaParaEstudianteInexistenteFallaPorLlaveForanea()
    {
        var archivo = Path.Combine(Path.GetTempPath(), $"test_expediente_fk_{Guid.NewGuid():N}.sqlite");
        try
        {
            new PersistenciaGrupoSqlite(archivo).Inicializar();
            var repo = new PersistenciaExpedienteSqlite(archivo);

            var grupoIdHuerfano = GrupoId.DesdeGuid(Guid.NewGuid());
            var estudianteIdHuerfano = EstudianteId.DesdeGuid(Guid.NewGuid());

            var ex = Assert.Throws<DataAccessException>(() =>
                repo.RegistrarNotaPedagogica(estudianteIdHuerfano, grupoIdHuerfano, TipoNotaPedagogica.Fortaleza, "Observación huerfana", DateTime.Now));

            Assert.IsType<SqliteException>(ex.InnerException);
            Assert.Equal(19, ((SqliteException)ex.InnerException!).SqliteErrorCode); // SQLITE_CONSTRAINT
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (File.Exists(archivo)) File.Delete(archivo);
        }
    }

    [Fact]
    public void EliminarGrupoBorraExpedientesEnCascada()
    {
        var archivo = Path.Combine(Path.GetTempPath(), $"test_expediente_cascada_{Guid.NewGuid():N}.sqlite");
        try
        {
            var repoGrupo = new PersistenciaGrupoSqlite(archivo);
            repoGrupo.Inicializar();

            var grupo = Grupo.Crear("Primero B");
            var est = grupo.AgregarEstudiante("Estudiante Borrable", 1);
            repoGrupo.Guardar(grupo);

            var repoExp = new PersistenciaExpedienteSqlite(archivo);
            repoExp.RegistrarNotaPedagogica(est.Id, grupo.Id, TipoNotaPedagogica.Fortaleza, "Excelente trabajo", DateTime.Now);

            Assert.Single(repoExp.ObtenerExpediente(est.Id, grupo.Id).Notas);

            using (var conexion = new SqliteConnection($"Data Source={archivo}"))
            {
                conexion.Open();
                using var cmdFk = conexion.CreateCommand();
                cmdFk.CommandText = "PRAGMA foreign_keys = ON;";
                cmdFk.ExecuteNonQuery();

                using var cmd = conexion.CreateCommand();
                cmd.CommandText = "DELETE FROM estudiantes WHERE id = $id AND grupo_id = $grupo_id;";
                cmd.Parameters.AddWithValue("$id", est.Id.Valor.ToString());
                cmd.Parameters.AddWithValue("$grupo_id", grupo.Id.Valor.ToString());
                cmd.ExecuteNonQuery();
            }

            Assert.Empty(repoExp.ObtenerExpediente(est.Id, grupo.Id).Notas);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (File.Exists(archivo)) File.Delete(archivo);
        }
    }

    [Fact]
    public void RegistrarNotaPedagogicaConTextoClinicoEnPersistenciaSqliteFalla()
    {
        var archivo = Path.Combine(Path.GetTempPath(), $"test_expediente_clinico_{Guid.NewGuid():N}.sqlite");
        try
        {
            var repoGrupo = new PersistenciaGrupoSqlite(archivo);
            repoGrupo.Inicializar();

            var grupo = Grupo.Crear("Primero C");
            var est = grupo.AgregarEstudiante("Estudiante Clinico", 1);
            repoGrupo.Guardar(grupo);

            var repoExp = new PersistenciaExpedienteSqlite(archivo);

            var ex = Assert.Throws<DomainValidationException>(() =>
                repoExp.RegistrarNotaPedagogica(est.Id, grupo.Id, TipoNotaPedagogica.Dificultad, "Diagnóstico de TDAH", DateTime.Now));

            Assert.Contains("términos de carácter médico o clínico", ex.Message);
            Assert.Empty(repoExp.ObtenerExpediente(est.Id, grupo.Id).Notas);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (File.Exists(archivo)) File.Delete(archivo);
        }
    }
}
