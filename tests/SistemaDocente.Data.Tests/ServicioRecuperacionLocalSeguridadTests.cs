using System.IO.Compression;

using Microsoft.Data.Sqlite;

using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Data.Tests;

public sealed class ServicioRecuperacionLocalSeguridadTests : IDisposable
{
    private readonly string _directorio = Path.Combine(
        Path.GetTempPath(),
        "SistemaDocenteNEM-RecoverySecurity-" + Guid.NewGuid().ToString("N"));
    private readonly string _rutaBase;
    private readonly string _rutaEstado;
    private readonly string _directorioSeguridad;

    public ServicioRecuperacionLocalSeguridadTests()
    {
        Directory.CreateDirectory(_directorio);
        _rutaBase = Path.Combine(_directorio, "sistema-docente.db");
        _rutaEstado = Path.Combine(_directorio, "app-state.json");
        _directorioSeguridad = Path.Combine(_directorio, "backups", "safety");
    }

    [Fact]
    public void BackupDatabaseCapturaCambiosConfirmadosEnWalActivo()
    {
        var grupo = CrearGrupo("Cuarto A");
        var servicio = CrearServicio(_directorioSeguridad);
        var rutaRespaldo = Path.Combine(_directorio, "wal.sdocbackup");

        using (var conexion = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = _rutaBase,
            Mode = SqliteOpenMode.ReadWrite,
            Pooling = false,
        }.ToString()))
        {
            conexion.Open();
            using (var wal = conexion.CreateCommand())
            {
                wal.CommandText = "PRAGMA journal_mode = WAL;";
                wal.ExecuteScalar();
            }
            using (var cambio = conexion.CreateCommand())
            {
                cambio.CommandText = "UPDATE grupos SET nombre = 'Desde WAL' WHERE id = $id;";
                cambio.Parameters.AddWithValue("$id", grupo.Id.Valor.ToString());
                Assert.Equal(1, cambio.ExecuteNonQuery());
            }

            servicio.CrearRespaldo(
                rutaRespaldo,
                new DateTimeOffset(2026, 8, 8, 3, 0, 0, TimeSpan.Zero),
                "1.0-test");
        }

        var mutado = new PersistenciaGrupoSqlite(_rutaBase).Cargar(grupo.Id)!;
        mutado.Renombrar("Después del respaldo");
        new PersistenciaGrupoSqlite(_rutaBase).Guardar(mutado);

        servicio.Restaurar(
            rutaRespaldo,
            new DateTimeOffset(2026, 8, 8, 3, 1, 0, TimeSpan.Zero),
            "1.0-test");

        Assert.Equal("Desde WAL", new PersistenciaGrupoSqlite(_rutaBase).Cargar(grupo.Id)!.NombreVisible);
    }

    [Fact]
    public void InspeccionRechazaRutaTraversalEnZip()
    {
        CrearGrupo("Tercero A");
        var servicio = CrearServicio(_directorioSeguridad);
        var ruta = CrearRespaldoSinEstado(servicio, "traversal.sdocbackup");

        using (var archivo = ZipFile.Open(ruta, ZipArchiveMode.Update))
        {
            var entrada = archivo.CreateEntry("../escape.txt");
            using var salida = entrada.Open();
            salida.WriteByte(1);
        }

        var error = Assert.Throws<RecuperacionLocalException>(() => servicio.Inspeccionar(ruta));

        Assert.Equal(CategoriaErrorRecuperacionLocal.PaqueteInvalido, error.Categoria);
        Assert.False(File.Exists(Path.Combine(_directorio, "escape.txt")));
    }

    [Fact]
    public void InspeccionRechazaEntradaDeBaseDuplicada()
    {
        CrearGrupo("Tercero B");
        var servicio = CrearServicio(_directorioSeguridad);
        var ruta = CrearRespaldoSinEstado(servicio, "duplicado.sdocbackup");

        using (var archivo = ZipFile.Open(ruta, ZipArchiveMode.Update))
        {
            var entrada = archivo.CreateEntry("data/sistema-docente.db");
            using var salida = entrada.Open();
            salida.WriteByte(1);
        }

        var error = Assert.Throws<RecuperacionLocalException>(() => servicio.Inspeccionar(ruta));

        Assert.Equal(CategoriaErrorRecuperacionLocal.PaqueteInvalido, error.Categoria);
    }

    [Fact]
    public void InspeccionRechazaChecksumAlteradoAntesDeAbrirSqlite()
    {
        CrearGrupo("Tercero C");
        var servicio = CrearServicio(_directorioSeguridad);
        var ruta = CrearRespaldoSinEstado(servicio, "checksum.sdocbackup");

        using (var archivo = ZipFile.Open(ruta, ZipArchiveMode.Update))
        {
            var entrada = Assert.Single(
                archivo.Entries,
                item => item.FullName == "data/sistema-docente.db");
            byte[] bytes;
            using (var origen = entrada.Open())
            using (var memoria = new MemoryStream())
            {
                origen.CopyTo(memoria);
                bytes = memoria.ToArray();
            }
            bytes[^1] ^= 0x01;
            entrada.Delete();
            var reemplazo = archivo.CreateEntry("data/sistema-docente.db");
            using var destino = reemplazo.Open();
            destino.Write(bytes, 0, bytes.Length);
        }

        var error = Assert.Throws<RecuperacionLocalException>(() => servicio.Inspeccionar(ruta));

        Assert.Equal(CategoriaErrorRecuperacionLocal.PaqueteInvalido, error.Categoria);
        Assert.Contains("SHA-256", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void InspeccionRechazaVersionDePaqueteFutura()
    {
        CrearGrupo("Quinto A");
        var servicio = CrearServicio(_directorioSeguridad);
        var ruta = CrearRespaldoSinEstado(servicio, "formato-futuro.sdocbackup");

        using (var archivo = ZipFile.Open(ruta, ZipArchiveMode.Update))
        {
            var entrada = Assert.Single(archivo.Entries, item => item.FullName == "manifest.json");
            string json;
            using (var lector = new StreamReader(entrada.Open()))
            {
                json = lector.ReadToEnd();
            }
            var modificado = json.Replace(
                "\"formatVersion\": 1",
                "\"formatVersion\": 2",
                StringComparison.Ordinal);
            Assert.NotEqual(json, modificado);
            entrada.Delete();
            var reemplazo = archivo.CreateEntry("manifest.json");
            using var escritor = new StreamWriter(reemplazo.Open());
            escritor.Write(modificado);
        }

        var error = Assert.Throws<RecuperacionLocalException>(() => servicio.Inspeccionar(ruta));

        Assert.Equal(CategoriaErrorRecuperacionLocal.PaqueteIncompatible, error.Categoria);
    }

    [Fact]
    public void InspeccionRechazaEsquemaSqliteFuturo()
    {
        CrearGrupo("Sexto A");
        using (var conexion = AbrirConexion())
        using (var comando = conexion.CreateCommand())
        {
            comando.CommandText = "PRAGMA user_version = 999;";
            comando.ExecuteNonQuery();
        }
        var servicio = CrearServicio(_directorioSeguridad);
        var ruta = CrearRespaldoSinEstado(servicio, "db-futura.sdocbackup");

        var error = Assert.Throws<RecuperacionLocalException>(() => servicio.Inspeccionar(ruta));

        Assert.Equal(CategoriaErrorRecuperacionLocal.PaqueteIncompatible, error.Categoria);
    }

    [Fact]
    public void FalloDelRespaldoSeguridadBloqueaRestoreSinMoverBaseViva()
    {
        var grupo = CrearGrupo("Cuarto B");
        var servicioBueno = CrearServicio(_directorioSeguridad);
        var ruta = CrearRespaldoSinEstado(servicioBueno, "antes.sdocbackup");
        var mutado = new PersistenciaGrupoSqlite(_rutaBase).Cargar(grupo.Id)!;
        mutado.Renombrar("Estado actual que debe sobrevivir");
        new PersistenciaGrupoSqlite(_rutaBase).Guardar(mutado);

        var bloqueador = Path.Combine(_directorio, "no-es-directorio");
        File.WriteAllText(bloqueador, "x");
        var servicioBloqueado = CrearServicio(bloqueador);

        var error = Assert.Throws<RecuperacionLocalException>(() => servicioBloqueado.Restaurar(
            ruta,
            new DateTimeOffset(2026, 8, 8, 3, 5, 0, TimeSpan.Zero),
            "1.0-test"));

        Assert.Equal(CategoriaErrorRecuperacionLocal.RespaldoSeguridad, error.Categoria);
        Assert.Equal(
            "Estado actual que debe sobrevivir",
            new PersistenciaGrupoSqlite(_rutaBase).Cargar(grupo.Id)!.NombreVisible);
    }

    [Fact]
    public void FalloDePublicacionRestauraArchivosOriginalesYConservaSafetyBackup()
    {
        var grupo = CrearGrupo("Quinto B");
        File.WriteAllText(_rutaEstado, "{\"estado\":\"original\"}");
        var servicio = CrearServicio(_directorioSeguridad);
        var ruta = Path.Combine(_directorio, "original.sdocbackup");
        servicio.CrearRespaldo(
            ruta,
            new DateTimeOffset(2026, 8, 8, 3, 6, 0, TimeSpan.Zero),
            "1.0-test");

        var mutado = new PersistenciaGrupoSqlite(_rutaBase).Cargar(grupo.Id)!;
        mutado.Renombrar("Versión viva antes del fallo");
        new PersistenciaGrupoSqlite(_rutaBase).Guardar(mutado);
        File.WriteAllText(_rutaEstado, "{\"estado\":\"vivo\"}");

        using var bloqueoEstado = new FileStream(
            _rutaEstado,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read);
        var error = Assert.Throws<RecuperacionLocalException>(() => servicio.Restaurar(
            ruta,
            new DateTimeOffset(2026, 8, 8, 3, 7, 0, TimeSpan.Zero),
            "1.0-test"));

        Assert.Equal(CategoriaErrorRecuperacionLocal.Publicacion, error.Categoria);
        Assert.Contains(_directorioSeguridad, error.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(
            "Versión viva antes del fallo",
            new PersistenciaGrupoSqlite(_rutaBase).Cargar(grupo.Id)!.NombreVisible);
        Assert.Equal("{\"estado\":\"vivo\"}", File.ReadAllText(_rutaEstado));
        Assert.Single(Directory.GetFiles(_directorioSeguridad, "*.sdocbackup"));
    }

    private Grupo CrearGrupo(string nombre)
    {
        var grupo = Grupo.Crear(nombre);
        grupo.AgregarEstudiante("Alumno de prueba", 1);
        new PersistenciaGrupoSqlite(_rutaBase).Guardar(grupo);
        return grupo;
    }

    private ServicioRecuperacionLocalSqlite CrearServicio(string directorioSeguridad) =>
        new(
            _rutaBase,
            _rutaEstado,
            directorioSeguridad,
            ModoAlmacenamientoLocal.Produccion);

    private string CrearRespaldoSinEstado(
        ServicioRecuperacionLocalSqlite servicio,
        string nombre)
    {
        if (File.Exists(_rutaEstado))
        {
            File.Delete(_rutaEstado);
        }
        var ruta = Path.Combine(_directorio, nombre);
        servicio.CrearRespaldo(
            ruta,
            new DateTimeOffset(2026, 8, 8, 3, 10, 0, TimeSpan.Zero),
            "1.0-test");
        return ruta;
    }

    private SqliteConnection AbrirConexion()
    {
        var conexion = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = _rutaBase,
            Mode = SqliteOpenMode.ReadWrite,
            Pooling = false,
        }.ToString());
        conexion.Open();
        return conexion;
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (Directory.Exists(_directorio))
        {
            Directory.Delete(_directorio, recursive: true);
        }
    }
}