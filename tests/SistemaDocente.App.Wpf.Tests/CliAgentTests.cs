using System.Text.Json;

using SistemaDocente.Application;
using SistemaDocente.Cli;
using SistemaDocente.Core;
using SistemaDocente.Data;

namespace SistemaDocente.App.Wpf.Tests;

public sealed class CliAgentTests
{
    [Fact]
    public void CapabilitiesAdvertisesOfflineNonDestructiveContract()
    {
        using var stdout = new StringWriter();
        using var stderr = new StringWriter();

        var exit = new EjecutorCli().Ejecutar(["capabilities", "--json"], stdout, stderr);

        Assert.Equal(0, exit);
        using var json = JsonDocument.Parse(stdout.ToString());
        var data = json.RootElement.GetProperty("data");
        Assert.False(data.GetProperty("destructiveDeleteCommands").GetBoolean());
        Assert.False(data.GetProperty("acceptsSensitiveFreeTextArguments").GetBoolean());
        Assert.False(data.GetProperty("networkAccess").GetBoolean());
        Assert.Equal("dry-run-unless-apply", data.GetProperty("mutationPolicy").GetString());
    }

    [Fact]
    public void DefaultStudentOutputOmitsNamesAndPersonalOptInIncludesThem()
    {
        using var entorno = EntornoCliTemporal.Crear();
        var (grupoId, _) = entorno.CrearGrupoConEstudiante("GRUPO_SECRETO", "ALUMNA_SECRETA");

        var minimo = entorno.Ejecutar(
            "students", "list", "--group", grupoId.ToString(), "--json");
        Assert.Equal(0, minimo.ExitCode);
        Assert.DoesNotContain("ALUMNA_SECRETA", minimo.Stdout, StringComparison.Ordinal);
        Assert.Contains(grupoId.ToString(), minimo.Stdout, StringComparison.OrdinalIgnoreCase);

        var personal = entorno.Ejecutar(
            "students", "list", "--group", grupoId.ToString(),
            "--include-personal-data", "--json");
        Assert.Equal(0, personal.ExitCode);
        Assert.Contains("ALUMNA_SECRETA", personal.Stdout, StringComparison.Ordinal);
        using var json = JsonDocument.Parse(personal.Stdout);
        Assert.True(json.RootElement.GetProperty("privacy").GetProperty("includesPersonalData").GetBoolean());
    }

    [Fact]
    public void AttendanceSetIsDryRunUntilApplyAndThenUsesPersistedRoster()
    {
        using var entorno = EntornoCliTemporal.Crear();
        var (grupoId, estudianteId) = entorno.CrearGrupoConEstudiante("Grupo", "Alumno");
        const string fecha = "2026-09-03";

        var dryRun = entorno.Ejecutar(
            "attendance", "set", "--group", grupoId.ToString(),
            "--student", estudianteId.ToString(), "--date", fecha,
            "--state", "F", "--json");
        Assert.Equal(0, dryRun.ExitCode);
        Assert.Null(entorno.Asistencias.Cargar(GrupoId.DesdeGuid(grupoId), new DateOnly(2026, 9, 3)));
        Assert.Contains("dryRun", dryRun.Stdout, StringComparison.Ordinal);

        var aplicado = entorno.Ejecutar(
            "attendance", "set", "--group", grupoId.ToString(),
            "--student", estudianteId.ToString(), "--date", fecha,
            "--state", "F", "--apply", "--json");
        Assert.Equal(0, aplicado.ExitCode);
        var guardada = entorno.Asistencias.Cargar(GrupoId.DesdeGuid(grupoId), new DateOnly(2026, 9, 3));
        Assert.NotNull(guardada);
        Assert.Equal(EstadoAsistencia.Falta, guardada.Registros.Single().Estado);
    }

    [Fact]
    public void StudentDeactivateIsDryRunUntilApply()
    {
        using var entorno = EntornoCliTemporal.Crear();
        var (grupoId, estudianteId) = entorno.CrearGrupoConEstudiante("Grupo", "Alumno");

        var dryRun = entorno.Ejecutar(
            "students", "deactivate", "--group", grupoId.ToString(),
            "--student", estudianteId.ToString(), "--json");
        Assert.Equal(0, dryRun.ExitCode);
        Assert.True(entorno.Grupos.Cargar(GrupoId.DesdeGuid(grupoId))!.Estudiantes.Single().EstaActivo);

        var aplicado = entorno.Ejecutar(
            "students", "deactivate", "--group", grupoId.ToString(),
            "--student", estudianteId.ToString(), "--apply", "--json");
        Assert.Equal(0, aplicado.ExitCode);
        Assert.False(entorno.Grupos.Cargar(GrupoId.DesdeGuid(grupoId))!.Estudiantes.Single().EstaActivo);
    }

    [Fact]
    public void AgentContextAndRecommendationsStayPseudonymousByDefault()
    {
        using var entorno = EntornoCliTemporal.Crear();
        var (grupoId, _) = entorno.CrearGrupoConEstudiante("GRUPO_SECRETO", "ALUMNA_SECRETA");

        var contexto = entorno.Ejecutar(
            "agent", "context", "--group", grupoId.ToString(), "--json");
        Assert.Equal(0, contexto.ExitCode);
        Assert.DoesNotContain("ALUMNA_SECRETA", contexto.Stdout, StringComparison.Ordinal);
        Assert.DoesNotContain("GRUPO_SECRETO", contexto.Stdout, StringComparison.Ordinal);
        using (var json = JsonDocument.Parse(contexto.Stdout))
        {
            var privacy = json.RootElement.GetProperty("privacy");
            Assert.False(privacy.GetProperty("includesPersonalData").GetBoolean());
            Assert.False(privacy.GetProperty("includesFreeText").GetBoolean());
            Assert.False(privacy.GetProperty("networkAccess").GetBoolean());
        }

        var recomendacion = entorno.Ejecutar(
            "agent", "recommend", "--group", grupoId.ToString(), "--json");
        Assert.Equal(0, recomendacion.ExitCode);
        Assert.DoesNotContain("ALUMNA_SECRETA", recomendacion.Stdout, StringComparison.Ordinal);
        Assert.Contains("evidence.insufficient", recomendacion.Stdout, StringComparison.Ordinal);
    }

    [Fact]
    public void ErrorJsonDoesNotExposeRawDomainMessage()
    {
        using var entorno = EntornoCliTemporal.Crear();
        var missingGroup = Guid.NewGuid();

        var resultado = entorno.Ejecutar(
            "students", "list", "--group", missingGroup.ToString(), "--json");

        Assert.NotEqual(0, resultado.ExitCode);
        Assert.Contains("not_found", resultado.Stdout, StringComparison.Ordinal);
        Assert.DoesNotContain("El grupo", resultado.Stdout, StringComparison.Ordinal);
        Assert.DoesNotContain("stackTrace", resultado.Stdout, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class EntornoCliTemporal : IDisposable
    {
        private EntornoCliTemporal(string raiz)
        {
            Raiz = raiz;
            var rutas = RutasAlmacenamientoLocal.DesdeLocalApplicationData(raiz, false);
            Grupos = new PersistenciaGrupoSqlite(rutas.BaseSqlite);
            Asistencias = new PersistenciaAsistenciaSqlite(rutas.BaseSqlite);
            Ejecutor = new EjecutorCli(raiz);
        }

        public string Raiz { get; }
        public PersistenciaGrupoSqlite Grupos { get; }
        public PersistenciaAsistenciaSqlite Asistencias { get; }
        public EjecutorCli Ejecutor { get; }

        public static EntornoCliTemporal Crear()
        {
            var raiz = Path.Combine(Path.GetTempPath(), $"aularaiz-cli-{Guid.NewGuid():N}");
            Directory.CreateDirectory(raiz);
            return new EntornoCliTemporal(raiz);
        }

        public (Guid GrupoId, Guid EstudianteId) CrearGrupoConEstudiante(string grupo, string estudiante)
        {
            var casos = new GestionGrupoCasosUso(Grupos);
            var creado = casos.CrearGrupo(grupo);
            var alumno = casos.AgregarEstudiante(creado.Id, estudiante, 1, grado: GradoPrimaria.Tercero);
            return (creado.Id.Valor, alumno.Id.Valor);
        }

        public ResultadoCli Ejecutar(params string[] args)
        {
            using var stdout = new StringWriter();
            using var stderr = new StringWriter();
            var exit = Ejecutor.Ejecutar(args, stdout, stderr);
            return new ResultadoCli(exit, stdout.ToString(), stderr.ToString());
        }

        public void Dispose()
        {
            if (Directory.Exists(Raiz)) Directory.Delete(Raiz, true);
        }
    }

    private sealed record ResultadoCli(int ExitCode, string Stdout, string Stderr);
}