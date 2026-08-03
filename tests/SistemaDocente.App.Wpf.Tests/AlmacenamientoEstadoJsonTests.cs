using System.Text.Json;

using SistemaDocente.Core;
using SistemaDocente.Presentation;

namespace SistemaDocente.App.Wpf.Tests;

public sealed class AlmacenamientoEstadoJsonTests : IDisposable
{
    private readonly string _directorio = Path.Combine(Path.GetTempPath(), "SistemaDocente.AppState.Tests", Guid.NewGuid().ToString("N"));
    private string Ruta => Path.Combine(_directorio, "app-state.json");

    [Fact]
    public void AusenteDevuelveBienvenida()
    {
        Assert.Equal(EstadoLecturaReferencia.Ausente, new AlmacenamientoEstadoJson(Ruta).Cargar().Estado);
    }

    [Theory]
    [InlineData("")]
    [InlineData("no-json")]
    [InlineData("{\"GrupoId\":\"00000000-0000-0000-0000-000000000000\"}")]
    [InlineData("{\"Otro\":\"valor\"}")]
    public void ContenidoInvalidoSeRechaza(string contenido)
    {
        Directory.CreateDirectory(_directorio);
        File.WriteAllText(Ruta, contenido);
        Assert.Equal(EstadoLecturaReferencia.Invalida, new AlmacenamientoEstadoJson(Ruta).Cargar().Estado);
    }

    [Fact]
    public void GuardarEscribeSoloGrupoIdYReemplazaAtomicamente()
    {
        var almacenamiento = new AlmacenamientoEstadoJson(Ruta);
        var primero = GrupoId.DesdeGuid(Guid.NewGuid());
        var segundo = GrupoId.DesdeGuid(Guid.NewGuid());
        almacenamiento.Guardar(primero);
        almacenamiento.Guardar(segundo);

        var resultado = almacenamiento.Cargar();
        Assert.Equal(EstadoLecturaReferencia.Valida, resultado.Estado);
        Assert.Equal(segundo, resultado.GrupoId);
        using var json = JsonDocument.Parse(File.ReadAllText(Ruta));
        Assert.Equal(["GrupoId"], json.RootElement.EnumerateObject().Select(x => x.Name));
        Assert.Empty(Directory.GetFiles(_directorio, "*.tmp"));
    }

    [Fact]
    public void FalloAntesDeEscribirNoSobrescribeReferenciaPrevia()
    {
        var almacenamiento = new AlmacenamientoEstadoJson(Ruta);
        var original = GrupoId.DesdeGuid(Guid.NewGuid());
        almacenamiento.Guardar(original);

        Assert.Throws<ArgumentException>(() => almacenamiento.Guardar(default));
        Assert.Equal(original, almacenamiento.Cargar().GrupoId);
    }

    [Fact]
    public void OlvidarEliminaSoloEstado()
    {
        var almacenamiento = new AlmacenamientoEstadoJson(Ruta);
        almacenamiento.Guardar(GrupoId.DesdeGuid(Guid.NewGuid()));
        almacenamiento.Olvidar();
        Assert.False(File.Exists(Ruta));
    }

    [Fact]
    public void RutasPredeterminadasSonExactas()
    {
        var rutas = RutasAplicacion.DesdeLocalApplicationData(@"C:\Local");
        Assert.Equal(@"C:\Local\SistemaDocenteNEM\data\sistema-docente.db", rutas.BaseSqlite);
        Assert.Equal(@"C:\Local\SistemaDocenteNEM\data\app-state.json", rutas.EstadoAplicacion);
    }

    public void Dispose()
    {
        if (Directory.Exists(_directorio)) Directory.Delete(_directorio, true);
    }
}