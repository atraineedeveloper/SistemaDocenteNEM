using System.IO.Compression;
using System.Text;

using Microsoft.Data.Sqlite;

using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Data.Tests;

public sealed class ServicioRecuperacionLocalProtegidaTests : IDisposable
{
    private const string Contrasena = "Frase segura ñandú 2026";

    private readonly string _directorio = Path.Combine(
        Path.GetTempPath(),
        "SistemaDocenteNEM-ProtectedRecoveryTests-" + Guid.NewGuid().ToString("N"));
    private readonly string _rutaBase;
    private readonly string _rutaEstado;
    private readonly string _directorioSeguridad;

    public ServicioRecuperacionLocalProtegidaTests()
    {
        Directory.CreateDirectory(_directorio);
        _rutaBase = Path.Combine(_directorio, "sistema-docente.db");
        _rutaEstado = Path.Combine(_directorio, "app-state.json");
        _directorioSeguridad = Path.Combine(_directorio, "backups", "safety");
    }

    [Fact]
    public void CrearProtegidoProduceV2SinMetadatosDeAulaEnEncabezado()
    {
        CrearGrupo("Cuarto A", "Ana", 1);
        File.WriteAllText(_rutaEstado, "{\"grupo\":\"cuarto-a\"}");
        var servicio = CrearServicio();
        var ruta = Path.Combine(_directorio, "protegido.sdocbackup");

        var resultado = servicio.CrearRespaldoProtegido(
            ruta,
            new DateTimeOffset(2026, 8, 11, 17, 0, 0, TimeSpan.Zero),
            "2.0-test",
            Contrasena.ToCharArray());

        Assert.True(File.Exists(ruta));
        Assert.Equal(ruta, resultado.RutaArchivo);
        Assert.Equal(TipoProteccionRespaldoLocal.Contrasena, servicio.DetectarProteccion(ruta));

        using var archivo = ZipFile.OpenRead(ruta);
        Assert.Equal(2, archivo.Entries.Count);
        var entradaProteccion = Assert.Single(
            archivo.Entries,
            x => x.FullName == "protection.json");
        using var lector = new StreamReader(entradaProteccion.Open(), Encoding.UTF8);
        var encabezado = lector.ReadToEnd();

        Assert.Contains("\"formatVersion\":2", encabezado, StringComparison.Ordinal);
        Assert.Contains("PBKDF2-HMAC-SHA256", encabezado, StringComparison.Ordinal);
        Assert.Contains("AES-256-GCM-CHUNKED", encabezado, StringComparison.Ordinal);
        Assert.DoesNotContain("Cuarto A", encabezado, StringComparison.Ordinal);
        Assert.DoesNotContain("Ana", encabezado, StringComparison.Ordinal);
        Assert.DoesNotContain("Production", encabezado, StringComparison.Ordinal);
        Assert.DoesNotContain(Contrasena, encabezado, StringComparison.Ordinal);
    }

    [Fact]
    public void V2SeInspeccionaConContrasenaYV1SigueSinRequerirla()
    {
        CrearGrupo("Quinto A", "Luis", 1);
        var servicio = CrearServicio();
        var rutaV1 = Path.Combine(_directorio, "normal.sdocbackup");
        var rutaV2 = Path.Combine(_directorio, "protegido.sdocbackup");
        var instante = new DateTimeOffset(2026, 8, 11, 17, 1, 0, TimeSpan.Zero);

        servicio.CrearRespaldo(rutaV1, instante, "2.0-test");
        servicio.CrearRespaldoProtegido(
            rutaV2,
            instante.AddMinutes(1),
            "2.0-test",
            Contrasena.ToCharArray());

        Assert.Equal(TipoProteccionRespaldoLocal.Ninguna, servicio.DetectarProteccion(rutaV1));
        Assert.True(servicio.Inspeccionar(rutaV1).EsCompatible);

        var requiere = Assert.Throws<RecuperacionLocalException>(() => servicio.Inspeccionar(rutaV2));
        Assert.Equal(CategoriaErrorRecuperacionLocal.ContrasenaRequerida, requiere.Categoria);

        var inspeccionV2 = servicio.InspeccionarProtegido(rutaV2, Contrasena.ToCharArray());
        Assert.True(inspeccionV2.EsCompatible);
        Assert.Equal(rutaV2, inspeccionV2.RutaArchivo);
        Assert.Equal("2.0-test", inspeccionV2.VersionAplicacion);
    }

    [Fact]
    public void ContrasenaIncorrectaNoExponeSiFalloFueClaveOTamper()
    {
        CrearGrupo("Sexto A", "Marta", 1);
        var servicio = CrearServicio();
        var ruta = Path.Combine(_directorio, "protegido.sdocbackup");
        servicio.CrearRespaldoProtegido(
            ruta,
            new DateTimeOffset(2026, 8, 11, 17, 2, 0, TimeSpan.Zero),
            "2.0-test",
            Contrasena.ToCharArray());

        var error = Assert.Throws<RecuperacionLocalException>(() =>
            servicio.InspeccionarProtegido(
                ruta,
                "Otra frase segura 2026".ToCharArray()));

        Assert.Equal(CategoriaErrorRecuperacionLocal.PaqueteInvalido, error.Categoria);
        Assert.Contains("incorrecta", error.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("dañado", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void MismoPasswordGeneraEncabezadosYCifradosDistintos()
    {
        CrearGrupo("Tercero A", "Sofía", 1);
        var servicio = CrearServicio();
        var rutaA = Path.Combine(_directorio, "a.sdocbackup");
        var rutaB = Path.Combine(_directorio, "b.sdocbackup");
        var instante = new DateTimeOffset(2026, 8, 11, 17, 3, 0, TimeSpan.Zero);

        servicio.CrearRespaldoProtegido(rutaA, instante, "2.0-test", Contrasena.ToCharArray());
        servicio.CrearRespaldoProtegido(rutaB, instante, "2.0-test", Contrasena.ToCharArray());

        var (encabezadoA, payloadA) = LeerComponentes(rutaA);
        var (encabezadoB, payloadB) = LeerComponentes(rutaB);
        Assert.NotEqual(encabezadoA, encabezadoB);
        Assert.NotEqual(payloadA, payloadB);
    }

    [Fact]
    public void RestaurarV2ReutilizaSafetyBackupV1YRecuperaSnapshot()
    {
        var grupo = CrearGrupo("Cuarto A", "Ana", 1);
        File.WriteAllText(_rutaEstado, "{\"estado\":\"original\"}");
        var servicio = CrearServicio();
        var ruta = Path.Combine(_directorio, "protegido.sdocbackup");
        servicio.CrearRespaldoProtegido(
            ruta,
            new DateTimeOffset(2026, 8, 11, 17, 4, 0, TimeSpan.Zero),
            "2.0-test",
            Contrasena.ToCharArray());

        var mutado = new PersistenciaGrupoSqlite(_rutaBase).Cargar(grupo.Id)!;
        mutado.Renombrar("Cuarto A mutado");
        mutado.AgregarEstudiante("Bruno", 2);
        new PersistenciaGrupoSqlite(_rutaBase).Guardar(mutado);
        File.WriteAllText(_rutaEstado, "{\"estado\":\"mutado\"}");

        var resultado = servicio.RestaurarProtegido(
            ruta,
            new DateTimeOffset(2026, 8, 11, 17, 5, 0, TimeSpan.Zero),
            "2.0-test",
            Contrasena.ToCharArray());

        var restaurado = new PersistenciaGrupoSqlite(_rutaBase).Cargar(grupo.Id)!;
        Assert.Equal("Cuarto A", restaurado.NombreVisible);
        Assert.Equal("Ana", Assert.Single(restaurado.Estudiantes).NombreVisible);
        Assert.Equal("{\"estado\":\"original\"}", File.ReadAllText(_rutaEstado));
        Assert.Equal(ruta, resultado.RutaArchivoOrigen);
        Assert.True(File.Exists(resultado.RutaRespaldoSeguridad));
        Assert.Equal(
            TipoProteccionRespaldoLocal.Ninguna,
            servicio.DetectarProteccion(resultado.RutaRespaldoSeguridad));
    }

    private static (string Encabezado, string PayloadHash) LeerComponentes(string ruta)
    {
        using var archivo = ZipFile.OpenRead(ruta);
        var proteccion = Assert.Single(archivo.Entries, x => x.FullName == "protection.json");
        var payload = Assert.Single(archivo.Entries, x => x.FullName == "payload.bin");
        using var lector = new StreamReader(proteccion.Open(), Encoding.UTF8);
        var encabezado = lector.ReadToEnd();
        using var flujo = payload.Open();
        var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(flujo));
        return (encabezado, hash);
    }

    private ServicioRecuperacionLocalProtegida CrearServicio() =>
        new(new ServicioRecuperacionLocalSqlite(
            _rutaBase,
            _rutaEstado,
            _directorioSeguridad,
            ModoAlmacenamientoLocal.Produccion));

    private Grupo CrearGrupo(string nombreGrupo, string nombreEstudiante, int numeroLista)
    {
        var grupo = Grupo.Crear(nombreGrupo);
        grupo.AgregarEstudiante(nombreEstudiante, numeroLista);
        new PersistenciaGrupoSqlite(_rutaBase).Guardar(grupo);
        return grupo;
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (Directory.Exists(_directorio)) Directory.Delete(_directorio, recursive: true);
    }
}