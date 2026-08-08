using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;

using Microsoft.Data.Sqlite;

using SistemaDocente.Application;

namespace SistemaDocente.Data;

public sealed class ServicioRecuperacionLocalSqlite : IServicioRecuperacionLocal
{
    private const string IdentificadorFormato = "SistemaDocenteNEM.Backup";
    private const int VersionFormato = 1;
    private const string RutaManifiesto = "manifest.json";
    private const string RutaBaseDatosPaquete = "data/sistema-docente.db";
    private const string RutaEstadoPaquete = "data/app-state.json";
    private const long MaximoManifiestoBytes = 1024 * 1024;
    private const long MaximoEstadoBytes = 10 * 1024 * 1024;
    private const long MaximoBaseDatosBytes = 2L * 1024 * 1024 * 1024;

    private static readonly JsonSerializerOptions OpcionesJson = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    private readonly string _rutaBaseSqlite;
    private readonly string _rutaEstadoAplicacion;
    private readonly string _directorioRespaldosSeguridad;
    private readonly object _sincronizacion = new();

    public ServicioRecuperacionLocalSqlite(
        string rutaBaseSqlite,
        string rutaEstadoAplicacion,
        string directorioRespaldosSeguridad,
        ModoAlmacenamientoLocal modoActual)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rutaBaseSqlite);
        ArgumentException.ThrowIfNullOrWhiteSpace(rutaEstadoAplicacion);
        ArgumentException.ThrowIfNullOrWhiteSpace(directorioRespaldosSeguridad);

        _rutaBaseSqlite = Path.GetFullPath(rutaBaseSqlite);
        _rutaEstadoAplicacion = Path.GetFullPath(rutaEstadoAplicacion);
        _directorioRespaldosSeguridad = Path.GetFullPath(directorioRespaldosSeguridad);
        ModoActual = modoActual;
    }

    public ModoAlmacenamientoLocal ModoActual { get; }

    public ResultadoRespaldoLocal CrearRespaldo(
        string rutaDestino,
        DateTimeOffset ahoraUtc,
        string versionAplicacion)
    {
        lock (_sincronizacion)
        {
            return CrearRespaldoInterno(rutaDestino, ahoraUtc, versionAplicacion);
        }
    }

    public InspeccionRespaldoLocal Inspeccionar(string rutaRespaldo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rutaRespaldo);

        lock (_sincronizacion)
        {
            PreparacionRestauracion? preparacion = null;
            try
            {
                preparacion = PrepararRespaldo(rutaRespaldo);
                return CrearInspeccion(preparacion);
            }
            finally
            {
                if (preparacion is not null)
                {
                    EliminarDirectorioSilencioso(preparacion.DirectorioTemporal);
                }
            }
        }
    }

    public ResultadoRestauracionLocal Restaurar(
        string rutaRespaldo,
        DateTimeOffset ahoraUtc,
        string versionAplicacion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rutaRespaldo);
        ArgumentException.ThrowIfNullOrWhiteSpace(versionAplicacion);

        lock (_sincronizacion)
        {
            PreparacionRestauracion? preparacion = null;
            try
            {
                preparacion = PrepararRespaldo(rutaRespaldo);
                var rutaRespaldoSeguridad = CrearRespaldoSeguridad(ahoraUtc, versionAplicacion);
                return PublicarRestauracion(
                    preparacion,
                    rutaRespaldoSeguridad,
                    ahoraUtc.ToUniversalTime());
            }
            finally
            {
                if (preparacion is not null)
                {
                    EliminarDirectorioSilencioso(preparacion.DirectorioTemporal);
                }
            }
        }
    }

    private ResultadoRespaldoLocal CrearRespaldoInterno(
        string rutaDestino,
        DateTimeOffset ahoraUtc,
        string versionAplicacion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rutaDestino);
        ArgumentException.ThrowIfNullOrWhiteSpace(versionAplicacion);

        var destino = Path.GetFullPath(rutaDestino);
        var directorioDestino = Path.GetDirectoryName(destino)
            ?? throw new RecuperacionLocalException(
                CategoriaErrorRecuperacionLocal.AccesoArchivo,
                "No fue posible determinar la carpeta de destino del respaldo.");
        Directory.CreateDirectory(directorioDestino);

        var directorioTemporal = CrearDirectorioTemporal("backup");
        var temporalPaquete = Path.Combine(
            directorioDestino,
            $".{Path.GetFileName(destino)}.{Guid.NewGuid():N}.tmp");
        var advertencias = new List<string>();

        try
        {
            if (!File.Exists(_rutaBaseSqlite))
            {
                new PersistenciaGrupoSqlite(_rutaBaseSqlite).Inicializar();
            }

            var rutaInstantanea = Path.Combine(directorioTemporal, "sistema-docente.db");
            CrearInstantaneaSqlite(_rutaBaseSqlite, rutaInstantanea);
            ValidarIntegridadSqlite(rutaInstantanea);
            var versionBaseDatos = LeerVersionBaseDatos(rutaInstantanea);

            var estadoBytes = LeerEstadoAplicacionValido(advertencias);
            var componenteBase = CrearComponenteArchivo(
                "Base de datos SQLite",
                rutaInstantanea,
                requerido: true);
            ComponenteRespaldoLocal? componenteEstado = estadoBytes is null
                ? null
                : CrearComponenteBytes(
                    "Estado de aplicación",
                    estadoBytes,
                    requerido: false);

            var creadoUtc = ahoraUtc.ToUniversalTime();
            var manifiesto = new BackupManifest(
                IdentificadorFormato,
                VersionFormato,
                creadoUtc,
                versionAplicacion.Trim(),
                TextoModo(ModoActual),
                new BackupDatabaseComponent(
                    RutaBaseDatosPaquete,
                    versionBaseDatos,
                    componenteBase.TamanoBytes,
                    componenteBase.Sha256),
                new BackupStateComponent(
                    componenteEstado is not null,
                    componenteEstado is null ? null : RutaEstadoPaquete,
                    componenteEstado?.TamanoBytes ?? 0,
                    componenteEstado?.Sha256));

            EscribirPaquete(
                temporalPaquete,
                manifiesto,
                rutaInstantanea,
                estadoBytes);
            PublicarTemporal(temporalPaquete, destino);

            var componentes = componenteEstado is null
                ? new[] { componenteBase }
                : new[] { componenteBase, componenteEstado };
            return new ResultadoRespaldoLocal(
                destino,
                creadoUtc,
                versionAplicacion.Trim(),
                ModoActual,
                versionBaseDatos,
                new FileInfo(destino).Length,
                componentes,
                advertencias.ToArray());
        }
        catch (RecuperacionLocalException)
        {
            throw;
        }
        catch (Exception exception) when (
            exception is IOException
                or UnauthorizedAccessException
                or SqliteException
                or JsonException
                or InvalidDataException)
        {
            throw new RecuperacionLocalException(
                CategoriaErrorRecuperacionLocal.AccesoArchivo,
                "No fue posible crear el respaldo local.",
                exception);
        }
        finally
        {
            EliminarArchivoSilencioso(temporalPaquete);
            EliminarDirectorioSilencioso(directorioTemporal);
        }
    }

    private PreparacionRestauracion PrepararRespaldo(string rutaRespaldo)
    {
        var ruta = Path.GetFullPath(rutaRespaldo);
        if (!File.Exists(ruta))
        {
            throw new RecuperacionLocalException(
                CategoriaErrorRecuperacionLocal.AccesoArchivo,
                "El archivo de respaldo seleccionado no existe.");
        }

        var directorioTemporal = CrearDirectorioTemporal("restore");
        try
        {
            BackupManifest manifiesto;
            string rutaBaseExtraida;
            string? rutaEstadoExtraido = null;

            using (var flujo = new FileStream(
                       ruta,
                       FileMode.Open,
                       FileAccess.Read,
                       FileShare.Read))
            using (var archivo = new ZipArchive(flujo, ZipArchiveMode.Read, leaveOpen: false))
            {
                var entradas = IndexarEntradas(archivo);
                var entradaManifiesto = ObtenerEntradaRequerida(entradas, RutaManifiesto);
                if (entradaManifiesto.Length <= 0 || entradaManifiesto.Length > MaximoManifiestoBytes)
                {
                    throw PaqueteInvalido("El manifiesto del respaldo tiene un tamaño no válido.");
                }

                var manifiestoBytes = LeerEntradaLimitada(
                    entradaManifiesto,
                    MaximoManifiestoBytes,
                    "manifiesto");
                manifiesto = JsonSerializer.Deserialize<BackupManifest>(
                    manifiestoBytes,
                    OpcionesJson) ?? throw PaqueteInvalido("El manifiesto del respaldo está vacío.");
                ValidarManifiesto(manifiesto, entradas);

                var entradaBase = ObtenerEntradaRequerida(entradas, RutaBaseDatosPaquete);
                ValidarTamanoEntrada(
                    entradaBase,
                    manifiesto.Database.SizeBytes,
                    MaximoBaseDatosBytes,
                    "base de datos");
                rutaBaseExtraida = Path.Combine(directorioTemporal, "extraida.db");
                var hashBase = CopiarEntradaConHash(
                    entradaBase,
                    rutaBaseExtraida,
                    MaximoBaseDatosBytes);
                ValidarHash(manifiesto.Database.Sha256, hashBase, "base de datos");

                if (manifiesto.ApplicationState.Included)
                {
                    var entradaEstado = ObtenerEntradaRequerida(entradas, RutaEstadoPaquete);
                    ValidarTamanoEntrada(
                        entradaEstado,
                        manifiesto.ApplicationState.SizeBytes,
                        MaximoEstadoBytes,
                        "estado de aplicación");
                    rutaEstadoExtraido = Path.Combine(directorioTemporal, "app-state.json");
                    var hashEstado = CopiarEntradaConHash(
                        entradaEstado,
                        rutaEstadoExtraido,
                        MaximoEstadoBytes);
                    ValidarHash(
                        manifiesto.ApplicationState.Sha256!,
                        hashEstado,
                        "estado de aplicación");
                }
            }

            ValidarIntegridadSqlite(rutaBaseExtraida);
            var rutaBasePreparada = Path.Combine(directorioTemporal, "preparada.db");
            File.Copy(rutaBaseExtraida, rutaBasePreparada, overwrite: false);
            PrepararCompatibilidadSqlite(rutaBasePreparada);
            ValidarIntegridadSqlite(rutaBasePreparada);

            if (rutaEstadoExtraido is not null)
            {
                ValidarJson(File.ReadAllBytes(rutaEstadoExtraido));
            }

            var advertencias = new List<string>();
            if (!manifiesto.ApplicationState.Included)
            {
                advertencias.Add(
                    "El respaldo no contiene estado de navegación; el estado local anterior se limpiará al restaurar.");
            }

            return new PreparacionRestauracion(
                ruta,
                directorioTemporal,
                rutaBasePreparada,
                rutaEstadoExtraido,
                manifiesto,
                new FileInfo(ruta).Length,
                advertencias.ToArray());
        }
        catch (RecuperacionLocalException)
        {
            EliminarDirectorioSilencioso(directorioTemporal);
            throw;
        }
        catch (SchemaIncompatibleException exception)
        {
            EliminarDirectorioSilencioso(directorioTemporal);
            throw new RecuperacionLocalException(
                CategoriaErrorRecuperacionLocal.PaqueteIncompatible,
                "La base del respaldo no es compatible con esta versión de la aplicación.",
                exception);
        }
        catch (Exception exception) when (
            exception is IOException
                or UnauthorizedAccessException
                or InvalidDataException
                or JsonException
                or SqliteException)
        {
            EliminarDirectorioSilencioso(directorioTemporal);
            throw new RecuperacionLocalException(
                CategoriaErrorRecuperacionLocal.PaqueteInvalido,
                "El archivo seleccionado no es un respaldo válido o está dañado.",
                exception);
        }
    }

    private InspeccionRespaldoLocal CrearInspeccion(PreparacionRestauracion preparacion)
    {
        var manifiesto = preparacion.Manifiesto;
        return new InspeccionRespaldoLocal(
            preparacion.RutaOrigen,
            manifiesto.CreatedUtc,
            manifiesto.ApplicationVersion,
            ModoDesdeTexto(manifiesto.SourceMode),
            manifiesto.Database.UserVersion,
            preparacion.TamanoPaqueteBytes,
            CrearComponentes(manifiesto),
            preparacion.Advertencias,
            EsCompatible: true);
    }

    private string CrearRespaldoSeguridad(
        DateTimeOffset ahoraUtc,
        string versionAplicacion)
    {
        try
        {
            Directory.CreateDirectory(_directorioRespaldosSeguridad);
            var instante = ahoraUtc.ToUniversalTime();
            var nombre = $"before-restore-{instante:yyyyMMdd'T'HHmmss'Z'}-{Guid.NewGuid():N}.sdocbackup";
            var ruta = Path.Combine(_directorioRespaldosSeguridad, nombre);
            CrearRespaldoInterno(ruta, instante, versionAplicacion);
            return ruta;
        }
        catch (RecuperacionLocalException exception)
        {
            throw new RecuperacionLocalException(
                CategoriaErrorRecuperacionLocal.RespaldoSeguridad,
                "No fue posible crear el respaldo de seguridad obligatorio; la restauración no comenzó.",
                exception);
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException or SqliteException)
        {
            throw new RecuperacionLocalException(
                CategoriaErrorRecuperacionLocal.RespaldoSeguridad,
                "No fue posible crear el respaldo de seguridad obligatorio; la restauración no comenzó.",
                exception);
        }
    }

    private ResultadoRestauracionLocal PublicarRestauracion(
        PreparacionRestauracion preparacion,
        string rutaRespaldoSeguridad,
        DateTimeOffset restauradoUtc)
    {
        var directorioVivo = Path.GetDirectoryName(_rutaBaseSqlite)
            ?? throw new RecuperacionLocalException(
                CategoriaErrorRecuperacionLocal.Publicacion,
                "No fue posible determinar el directorio de almacenamiento local.");
        Directory.CreateDirectory(directorioVivo);

        var token = Guid.NewGuid().ToString("N");
        var nuevaBase = _rutaBaseSqlite + $".restore-new-{token}";
        var nuevoEstado = _rutaEstadoAplicacion + $".restore-new-{token}";
        var rollbackBase = _rutaBaseSqlite + $".restore-old-{token}";
        var rollbackEstado = _rutaEstadoAplicacion + $".restore-old-{token}";
        var rollbackWal = _rutaBaseSqlite + $"-wal.restore-old-{token}";
        var rollbackShm = _rutaBaseSqlite + $"-shm.restore-old-{token}";
        var advertencias = preparacion.Advertencias.ToList();

        try
        {
            File.Copy(preparacion.RutaBasePreparada, nuevaBase, overwrite: false);
            if (preparacion.RutaEstadoPreparado is not null)
            {
                File.Copy(preparacion.RutaEstadoPreparado, nuevoEstado, overwrite: false);
            }

            SqliteConnection.ClearAllPools();

            MoverSiExiste(_rutaBaseSqlite, rollbackBase);
            MoverSiExiste(_rutaEstadoAplicacion, rollbackEstado);
            MoverSiExiste(_rutaBaseSqlite + "-wal", rollbackWal);
            MoverSiExiste(_rutaBaseSqlite + "-shm", rollbackShm);

            File.Move(nuevaBase, _rutaBaseSqlite);
            if (preparacion.RutaEstadoPreparado is not null)
            {
                File.Move(nuevoEstado, _rutaEstadoAplicacion);
            }

            LimpiarRollbackConAdvertencia(
                [rollbackBase, rollbackEstado, rollbackWal, rollbackShm],
                advertencias);

            return new ResultadoRestauracionLocal(
                preparacion.RutaOrigen,
                rutaRespaldoSeguridad,
                restauradoUtc,
                ReinicioRequerido: true,
                advertencias.ToArray());
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException)
        {
            var erroresRollback = RestaurarRollback(
                rollbackBase,
                rollbackEstado,
                rollbackWal,
                rollbackShm);
            if (erroresRollback.Count > 0)
            {
                throw new RecuperacionLocalException(
                    CategoriaErrorRecuperacionLocal.Publicacion,
                    $"La restauración falló y no fue posible confirmar la recuperación completa de los archivos originales. Conserva el respaldo de seguridad: {rutaRespaldoSeguridad}",
                    new AggregateException(new[] { exception }.Concat(erroresRollback)));
            }

            throw new RecuperacionLocalException(
                CategoriaErrorRecuperacionLocal.Publicacion,
                $"La restauración no pudo publicarse y se restauraron los archivos locales anteriores. Se conservó el respaldo de seguridad: {rutaRespaldoSeguridad}",
                exception);
        }
        finally
        {
            EliminarArchivoSilencioso(nuevaBase);
            EliminarArchivoSilencioso(nuevoEstado);
        }
    }

    private IReadOnlyList<Exception> RestaurarRollback(
        string rollbackBase,
        string rollbackEstado,
        string rollbackWal,
        string rollbackShm)
    {
        var errores = new List<Exception>();
        SqliteConnection.ClearAllPools();
        RestaurarArchivoDesdeRollback(_rutaBaseSqlite, rollbackBase, errores);
        RestaurarArchivoDesdeRollback(_rutaEstadoAplicacion, rollbackEstado, errores);
        RestaurarArchivoDesdeRollback(_rutaBaseSqlite + "-wal", rollbackWal, errores);
        RestaurarArchivoDesdeRollback(_rutaBaseSqlite + "-shm", rollbackShm, errores);
        return errores;
    }

    private static void RestaurarArchivoDesdeRollback(
        string rutaViva,
        string rutaRollback,
        List<Exception> errores)
    {
        if (!File.Exists(rutaRollback))
        {
            return;
        }

        try
        {
            if (File.Exists(rutaViva))
            {
                File.Delete(rutaViva);
            }
            File.Move(rutaRollback, rutaViva);
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException)
        {
            errores.Add(exception);
        }
    }

    private static void LimpiarRollbackConAdvertencia(
        IEnumerable<string> rutas,
        List<string> advertencias)
    {
        foreach (var ruta in rutas)
        {
            if (!File.Exists(ruta))
            {
                continue;
            }

            try
            {
                File.Delete(ruta);
            }
            catch (Exception exception) when (
                exception is IOException or UnauthorizedAccessException)
            {
                advertencias.Add(
                    $"La restauración terminó, pero no fue posible eliminar un archivo temporal de reversión: {Path.GetFileName(ruta)}.");
            }
        }
    }

    private static Dictionary<string, ZipArchiveEntry> IndexarEntradas(ZipArchive archivo)
    {
        if (archivo.Entries.Count is < 2 or > 3)
        {
            throw PaqueteInvalido("El respaldo contiene un número inesperado de componentes.");
        }

        var entradas = new Dictionary<string, ZipArchiveEntry>(StringComparer.Ordinal);
        foreach (var entrada in archivo.Entries)
        {
            if (string.IsNullOrWhiteSpace(entrada.FullName)
                || entrada.FullName.EndsWith('/', StringComparison.Ordinal)
                || entrada.FullName.Contains('\\', StringComparison.Ordinal)
                || entrada.FullName.StartsWith('/', StringComparison.Ordinal)
                || entrada.FullName.Split('/').Any(segmento => segmento is "." or ".." or ""))
            {
                throw PaqueteInvalido("El respaldo contiene una ruta de archivo no segura.");
            }

            if (!entradas.TryAdd(entrada.FullName, entrada))
            {
                throw PaqueteInvalido("El respaldo contiene componentes duplicados.");
            }
        }
        return entradas;
    }

    private void ValidarManifiesto(
        BackupManifest manifiesto,
        Dictionary<string, ZipArchiveEntry> entradas)
    {
        if (!string.Equals(manifiesto.Format, IdentificadorFormato, StringComparison.Ordinal))
        {
            throw PaqueteInvalido("El archivo no pertenece al formato de respaldo de Sistema Docente NEM.");
        }

        if (manifiesto.FormatVersion != VersionFormato)
        {
            throw new RecuperacionLocalException(
                CategoriaErrorRecuperacionLocal.PaqueteIncompatible,
                $"La versión de respaldo {manifiesto.FormatVersion} no es compatible con esta aplicación.");
        }

        if (string.IsNullOrWhiteSpace(manifiesto.ApplicationVersion))
        {
            throw PaqueteInvalido("El respaldo no identifica la versión de aplicación que lo creó.");
        }

        var modoOrigen = ModoDesdeTexto(manifiesto.SourceMode);
        if (modoOrigen != ModoActual)
        {
            throw new RecuperacionLocalException(
                CategoriaErrorRecuperacionLocal.PaqueteIncompatible,
                "El respaldo pertenece a un modo de almacenamiento diferente (Producción/Demo)." );
        }

        if (!string.Equals(
                manifiesto.Database.Path,
                RutaBaseDatosPaquete,
                StringComparison.Ordinal))
        {
            throw PaqueteInvalido("El manifiesto no referencia la base de datos esperada.");
        }

        var permitidas = manifiesto.ApplicationState.Included
            ? new[] { RutaManifiesto, RutaBaseDatosPaquete, RutaEstadoPaquete }
            : new[] { RutaManifiesto, RutaBaseDatosPaquete };
        if (entradas.Keys.Any(ruta => !permitidas.Contains(ruta, StringComparer.Ordinal)))
        {
            throw PaqueteInvalido("El respaldo contiene componentes inesperados para la versión 1.");
        }

        if (manifiesto.ApplicationState.Included)
        {
            if (!string.Equals(
                    manifiesto.ApplicationState.Path,
                    RutaEstadoPaquete,
                    StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(manifiesto.ApplicationState.Sha256))
            {
                throw PaqueteInvalido("El manifiesto del estado de aplicación es inválido.");
            }
        }
        else if (manifiesto.ApplicationState.Path is not null
                 || manifiesto.ApplicationState.SizeBytes != 0
                 || manifiesto.ApplicationState.Sha256 is not null)
        {
            throw PaqueteInvalido("El manifiesto declara un estado de aplicación inconsistente.");
        }
    }

    private static ZipArchiveEntry ObtenerEntradaRequerida(
        Dictionary<string, ZipArchiveEntry> entradas,
        string ruta)
    {
        return entradas.TryGetValue(ruta, out var entrada)
            ? entrada
            : throw PaqueteInvalido($"Falta el componente requerido '{ruta}'.");
    }

    private static void ValidarTamanoEntrada(
        ZipArchiveEntry entrada,
        long tamanoManifiesto,
        long maximo,
        string nombre)
    {
        if (tamanoManifiesto < 0
            || tamanoManifiesto > maximo
            || entrada.Length != tamanoManifiesto)
        {
            throw PaqueteInvalido($"El tamaño declarado para {nombre} no coincide con el paquete.");
        }
    }

    private static byte[] LeerEntradaLimitada(
        ZipArchiveEntry entrada,
        long maximo,
        string nombre)
    {
        if (entrada.Length < 0 || entrada.Length > maximo)
        {
            throw PaqueteInvalido($"El componente {nombre} excede el tamaño permitido.");
        }

        using var origen = entrada.Open();
        using var memoria = new MemoryStream();
        CopiarLimitado(origen, memoria, maximo, nombre, hash: null);
        return memoria.ToArray();
    }

    private static string CopiarEntradaConHash(
        ZipArchiveEntry entrada,
        string destino,
        long maximo)
    {
        using var origen = entrada.Open();
        using var salida = new FileStream(
            destino,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None);
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        CopiarLimitado(origen, salida, maximo, entrada.FullName, hash);
        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }

    private static void CopiarLimitado(
        Stream origen,
        Stream destino,
        long maximo,
        string nombre,
        IncrementalHash? hash)
    {
        var buffer = new byte[81920];
        long total = 0;
        int leidos;
        while ((leidos = origen.Read(buffer, 0, buffer.Length)) > 0)
        {
            total += leidos;
            if (total > maximo)
            {
                throw PaqueteInvalido($"El componente {nombre} excede el tamaño permitido.");
            }
            destino.Write(buffer, 0, leidos);
            hash?.AppendData(buffer, 0, leidos);
        }
    }

    private static void ValidarHash(
        string esperado,
        string actual,
        string nombre)
    {
        try
        {
            var esperadoBytes = Convert.FromHexString(esperado);
            var actualBytes = Convert.FromHexString(actual);
            if (esperadoBytes.Length != 32
                || actualBytes.Length != 32
                || !CryptographicOperations.FixedTimeEquals(esperadoBytes, actualBytes))
            {
                throw PaqueteInvalido($"La verificación SHA-256 de {nombre} no coincide.");
            }
        }
        catch (FormatException exception)
        {
            throw new RecuperacionLocalException(
                CategoriaErrorRecuperacionLocal.PaqueteInvalido,
                $"El manifiesto contiene un SHA-256 inválido para {nombre}.",
                exception);
        }
    }

    private static void CrearInstantaneaSqlite(string origen, string destino)
    {
        var cadenaOrigen = new SqliteConnectionStringBuilder
        {
            DataSource = origen,
            Mode = SqliteOpenMode.ReadOnly,
            DefaultTimeout = 15,
            Pooling = false,
        }.ToString();
        var cadenaDestino = new SqliteConnectionStringBuilder
        {
            DataSource = destino,
            Mode = SqliteOpenMode.ReadWriteCreate,
            DefaultTimeout = 15,
            Pooling = false,
        }.ToString();

        using var conexionOrigen = new SqliteConnection(cadenaOrigen);
        using var conexionDestino = new SqliteConnection(cadenaDestino);
        conexionOrigen.Open();
        conexionDestino.Open();
        conexionOrigen.BackupDatabase(conexionDestino);
    }

    private static void ValidarIntegridadSqlite(string ruta)
    {
        var cadena = new SqliteConnectionStringBuilder
        {
            DataSource = ruta,
            Mode = SqliteOpenMode.ReadOnly,
            DefaultTimeout = 15,
            Pooling = false,
        }.ToString();
        using var conexion = new SqliteConnection(cadena);
        conexion.Open();

        using (var integridad = conexion.CreateCommand())
        {
            integridad.CommandText = "PRAGMA integrity_check;";
            using var lector = integridad.ExecuteReader();
            if (!lector.Read()
                || !string.Equals(lector.GetString(0), "ok", StringComparison.OrdinalIgnoreCase)
                || lector.Read())
            {
                throw new RecuperacionLocalException(
                    CategoriaErrorRecuperacionLocal.IntegridadBaseDatos,
                    "La base SQLite del respaldo no supera la verificación de integridad.");
            }
        }

        using (var llaves = conexion.CreateCommand())
        {
            llaves.CommandText = "PRAGMA foreign_key_check;";
            using var lector = llaves.ExecuteReader();
            if (lector.Read())
            {
                throw new RecuperacionLocalException(
                    CategoriaErrorRecuperacionLocal.IntegridadBaseDatos,
                    "La base SQLite del respaldo contiene relaciones inválidas.");
            }
        }
    }

    private static void PrepararCompatibilidadSqlite(string ruta)
    {
        var cadena = new SqliteConnectionStringBuilder
        {
            DataSource = ruta,
            Mode = SqliteOpenMode.ReadWrite,
            DefaultTimeout = 15,
            Pooling = false,
        }.ToString();
        using var conexion = new SqliteConnection(cadena);
        conexion.Open();
        using (var configuracion = conexion.CreateCommand())
        {
            configuracion.CommandText = "PRAGMA foreign_keys = ON; PRAGMA busy_timeout = 5000;";
            configuracion.ExecuteNonQuery();
        }

        EsquemaSqlite.Inicializar(conexion);
        EsquemaNemMultigradoSqlite.Inicializar(conexion);
        EsquemaReportesSqlite.Inicializar(conexion);
        EsquemaPlaneacionNemSqlite.Inicializar(conexion);
    }

    private static int LeerVersionBaseDatos(string ruta)
    {
        var cadena = new SqliteConnectionStringBuilder
        {
            DataSource = ruta,
            Mode = SqliteOpenMode.ReadOnly,
            Pooling = false,
        }.ToString();
        using var conexion = new SqliteConnection(cadena);
        conexion.Open();
        using var comando = conexion.CreateCommand();
        comando.CommandText = "PRAGMA user_version;";
        return checked((int)(long)(comando.ExecuteScalar() ?? 0L));
    }

    private byte[]? LeerEstadoAplicacionValido(List<string> advertencias)
    {
        if (!File.Exists(_rutaEstadoAplicacion))
        {
            return null;
        }

        try
        {
            var bytes = File.ReadAllBytes(_rutaEstadoAplicacion);
            if (bytes.LongLength > MaximoEstadoBytes)
            {
                advertencias.Add("El estado de aplicación es demasiado grande y fue omitido del respaldo.");
                return null;
            }
            ValidarJson(bytes);
            return bytes;
        }
        catch (Exception exception) when (
            exception is JsonException or IOException or UnauthorizedAccessException)
        {
            advertencias.Add("El estado de aplicación no pudo validarse y fue omitido del respaldo.");
            return null;
        }
    }

    private static void ValidarJson(byte[] contenido)
    {
        using var _ = JsonDocument.Parse(contenido);
    }

    private static ComponenteRespaldoLocal CrearComponenteArchivo(
        string nombre,
        string ruta,
        bool requerido)
    {
        return new ComponenteRespaldoLocal(
            nombre,
            new FileInfo(ruta).Length,
            CalcularSha256(ruta),
            requerido);
    }

    private static ComponenteRespaldoLocal CrearComponenteBytes(
        string nombre,
        byte[] contenido,
        bool requerido)
    {
        var hash = SHA256.HashData(contenido);
        return new ComponenteRespaldoLocal(
            nombre,
            contenido.LongLength,
            Convert.ToHexString(hash).ToLowerInvariant(),
            requerido);
    }

    private static string CalcularSha256(string ruta)
    {
        using var flujo = File.OpenRead(ruta);
        return Convert.ToHexString(SHA256.HashData(flujo)).ToLowerInvariant();
    }

    private static void EscribirPaquete(
        string rutaTemporal,
        BackupManifest manifiesto,
        string rutaBaseDatos,
        byte[]? estadoBytes)
    {
        using var flujo = new FileStream(
            rutaTemporal,
            FileMode.CreateNew,
            FileAccess.ReadWrite,
            FileShare.None);
        using var archivo = new ZipArchive(flujo, ZipArchiveMode.Create, leaveOpen: false);

        EscribirArchivoEnEntrada(
            archivo,
            RutaBaseDatosPaquete,
            rutaBaseDatos,
            CompressionLevel.Optimal);

        if (estadoBytes is not null)
        {
            var entradaEstado = archivo.CreateEntry(RutaEstadoPaquete, CompressionLevel.Optimal);
            using var salidaEstado = entradaEstado.Open();
            salidaEstado.Write(estadoBytes, 0, estadoBytes.Length);
        }

        var entradaManifiesto = archivo.CreateEntry(RutaManifiesto, CompressionLevel.Optimal);
        using var salidaManifiesto = entradaManifiesto.Open();
        JsonSerializer.Serialize(salidaManifiesto, manifiesto, OpcionesJson);
    }

    private static void EscribirArchivoEnEntrada(
        ZipArchive archivo,
        string nombreEntrada,
        string rutaArchivo,
        CompressionLevel compresion)
    {
        var entrada = archivo.CreateEntry(nombreEntrada, compresion);
        using var origen = File.OpenRead(rutaArchivo);
        using var destino = entrada.Open();
        origen.CopyTo(destino);
    }

    private static void PublicarTemporal(string temporal, string destino)
    {
        try
        {
            File.Move(temporal, destino, overwrite: true);
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException)
        {
            throw new RecuperacionLocalException(
                CategoriaErrorRecuperacionLocal.Publicacion,
                "El respaldo se generó, pero no fue posible publicar el archivo de destino.",
                exception);
        }
    }

    private static IReadOnlyList<ComponenteRespaldoLocal> CrearComponentes(
        BackupManifest manifiesto)
    {
        var componentes = new List<ComponenteRespaldoLocal>
        {
            new(
                "Base de datos SQLite",
                manifiesto.Database.SizeBytes,
                manifiesto.Database.Sha256,
                Requerido: true),
        };
        if (manifiesto.ApplicationState.Included)
        {
            componentes.Add(new(
                "Estado de aplicación",
                manifiesto.ApplicationState.SizeBytes,
                manifiesto.ApplicationState.Sha256!,
                Requerido: false));
        }
        return componentes;
    }

    private static ModoAlmacenamientoLocal ModoDesdeTexto(string modo)
    {
        return modo switch
        {
            "Production" => ModoAlmacenamientoLocal.Produccion,
            "Demo" => ModoAlmacenamientoLocal.Demostracion,
            _ => throw PaqueteInvalido("El manifiesto contiene un modo de almacenamiento desconocido."),
        };
    }

    private static string TextoModo(ModoAlmacenamientoLocal modo)
    {
        return modo switch
        {
            ModoAlmacenamientoLocal.Produccion => "Production",
            ModoAlmacenamientoLocal.Demostracion => "Demo",
            _ => throw new ArgumentOutOfRangeException(nameof(modo)),
        };
    }

    private static void MoverSiExiste(string origen, string destino)
    {
        if (File.Exists(origen))
        {
            File.Move(origen, destino);
        }
    }

    private static string CrearDirectorioTemporal(string proposito)
    {
        var ruta = Path.Combine(
            Path.GetTempPath(),
            $"SistemaDocenteNEM-{proposito}-{Guid.NewGuid():N}");
        Directory.CreateDirectory(ruta);
        return ruta;
    }

    private static void EliminarArchivoSilencioso(string ruta)
    {
        try
        {
            if (File.Exists(ruta))
            {
                File.Delete(ruta);
            }
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException)
        {
            // Best effort cleanup only.
        }
    }

    private static void EliminarDirectorioSilencioso(string ruta)
    {
        try
        {
            if (Directory.Exists(ruta))
            {
                Directory.Delete(ruta, recursive: true);
            }
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException)
        {
            // Best effort cleanup only.
        }
    }

    private static RecuperacionLocalException PaqueteInvalido(string mensaje)
    {
        return new RecuperacionLocalException(
            CategoriaErrorRecuperacionLocal.PaqueteInvalido,
            mensaje);
    }

    private sealed record BackupManifest(
        string Format,
        int FormatVersion,
        DateTimeOffset CreatedUtc,
        string ApplicationVersion,
        string SourceMode,
        BackupDatabaseComponent Database,
        BackupStateComponent ApplicationState);

    private sealed record BackupDatabaseComponent(
        string Path,
        int UserVersion,
        long SizeBytes,
        string Sha256);

    private sealed record BackupStateComponent(
        bool Included,
        string? Path,
        long SizeBytes,
        string? Sha256);

    private sealed record PreparacionRestauracion(
        string RutaOrigen,
        string DirectorioTemporal,
        string RutaBasePreparada,
        string? RutaEstadoPreparado,
        BackupManifest Manifiesto,
        long TamanoPaqueteBytes,
        IReadOnlyList<string> Advertencias);
}