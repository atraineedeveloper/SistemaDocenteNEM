using System.Buffers.Binary;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using SistemaDocente.Application;

namespace SistemaDocente.Data;

public sealed class ServicioRecuperacionLocalProtegida : IServicioRecuperacionLocal, IProteccionRespaldoLocal
{
    private readonly IServicioRecuperacionLocal _interno;
    private readonly object _sincronizacion = new();

    public ServicioRecuperacionLocalProtegida(IServicioRecuperacionLocal interno)
    {
        _interno = interno ?? throw new ArgumentNullException(nameof(interno));
    }

    public ModoAlmacenamientoLocal ModoActual => _interno.ModoActual;

    public ResultadoRespaldoLocal CrearRespaldo(
        string rutaDestino,
        DateTimeOffset ahoraUtc,
        string versionAplicacion)
    {
        lock (_sincronizacion)
        {
            return _interno.CrearRespaldo(rutaDestino, ahoraUtc, versionAplicacion);
        }
    }

    public InspeccionRespaldoLocal Inspeccionar(string rutaRespaldo)
    {
        lock (_sincronizacion)
        {
            if (PaqueteRespaldoProtegidoV2.DetectarProteccion(rutaRespaldo)
                == TipoProteccionRespaldoLocal.Contrasena)
            {
                throw new RecuperacionLocalException(
                    CategoriaErrorRecuperacionLocal.ContrasenaRequerida,
                    "El respaldo está protegido con contraseña.");
            }

            return _interno.Inspeccionar(rutaRespaldo);
        }
    }

    public ResultadoRestauracionLocal Restaurar(
        string rutaRespaldo,
        DateTimeOffset ahoraUtc,
        string versionAplicacion)
    {
        lock (_sincronizacion)
        {
            if (PaqueteRespaldoProtegidoV2.DetectarProteccion(rutaRespaldo)
                == TipoProteccionRespaldoLocal.Contrasena)
            {
                throw new RecuperacionLocalException(
                    CategoriaErrorRecuperacionLocal.ContrasenaRequerida,
                    "El respaldo está protegido con contraseña.");
            }

            return _interno.Restaurar(rutaRespaldo, ahoraUtc, versionAplicacion);
        }
    }

    public TipoProteccionRespaldoLocal DetectarProteccion(string rutaRespaldo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rutaRespaldo);
        lock (_sincronizacion)
        {
            return PaqueteRespaldoProtegidoV2.DetectarProteccion(rutaRespaldo);
        }
    }

    public ResultadoRespaldoLocal CrearRespaldoProtegido(
        string rutaDestino,
        DateTimeOffset ahoraUtc,
        string versionAplicacion,
        char[] contrasena)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rutaDestino);
        ArgumentException.ThrowIfNullOrWhiteSpace(versionAplicacion);
        ArgumentNullException.ThrowIfNull(contrasena);

        lock (_sincronizacion)
        {
            var destino = Path.GetFullPath(rutaDestino);
            var directorioDestino = Path.GetDirectoryName(destino)
                ?? throw new RecuperacionLocalException(
                    CategoriaErrorRecuperacionLocal.AccesoArchivo,
                    "No fue posible determinar la carpeta de destino del respaldo protegido.");
            Directory.CreateDirectory(directorioDestino);

            var directorioTemporal = CrearDirectorioTemporal("backup-v2");
            var rutaV1 = Path.Combine(directorioTemporal, "payload-v1.sdocbackup");
            var temporalV2 = Path.Combine(
                directorioDestino,
                $".{Path.GetFileName(destino)}.{Guid.NewGuid():N}.tmp");

            try
            {
                var resultadoV1 = _interno.CrearRespaldo(
                    rutaV1,
                    ahoraUtc,
                    versionAplicacion);
                PaqueteRespaldoProtegidoV2.Crear(rutaV1, temporalV2, contrasena);
                PublicarTemporal(temporalV2, destino);

                return resultadoV1 with
                {
                    RutaArchivo = destino,
                    TamanoBytes = new FileInfo(destino).Length,
                };
            }
            catch (RecuperacionLocalException)
            {
                throw;
            }
            catch (Exception exception) when (
                exception is IOException
                    or UnauthorizedAccessException
                    or InvalidDataException
                    or JsonException
                    or CryptographicException)
            {
                throw new RecuperacionLocalException(
                    CategoriaErrorRecuperacionLocal.AccesoArchivo,
                    "No fue posible crear el respaldo protegido.",
                    exception);
            }
            finally
            {
                Array.Clear(contrasena, 0, contrasena.Length);
                EliminarArchivoSilencioso(temporalV2);
                EliminarDirectorioSilencioso(directorioTemporal);
            }
        }
    }

    public InspeccionRespaldoLocal InspeccionarProtegido(
        string rutaRespaldo,
        char[] contrasena)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rutaRespaldo);
        ArgumentNullException.ThrowIfNull(contrasena);

        lock (_sincronizacion)
        {
            var ruta = Path.GetFullPath(rutaRespaldo);
            var directorioTemporal = CrearDirectorioTemporal("inspect-v2");
            var rutaV1 = Path.Combine(directorioTemporal, "payload-v1.sdocbackup");
            try
            {
                PaqueteRespaldoProtegidoV2.Desproteger(ruta, rutaV1, contrasena);
                var inspeccion = _interno.Inspeccionar(rutaV1);
                return inspeccion with
                {
                    RutaArchivo = ruta,
                    TamanoBytes = new FileInfo(ruta).Length,
                };
            }
            finally
            {
                Array.Clear(contrasena, 0, contrasena.Length);
                EliminarDirectorioSilencioso(directorioTemporal);
            }
        }
    }

    public ResultadoRestauracionLocal RestaurarProtegido(
        string rutaRespaldo,
        DateTimeOffset ahoraUtc,
        string versionAplicacion,
        char[] contrasena)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rutaRespaldo);
        ArgumentException.ThrowIfNullOrWhiteSpace(versionAplicacion);
        ArgumentNullException.ThrowIfNull(contrasena);

        lock (_sincronizacion)
        {
            var ruta = Path.GetFullPath(rutaRespaldo);
            var directorioTemporal = CrearDirectorioTemporal("restore-v2");
            var rutaV1 = Path.Combine(directorioTemporal, "payload-v1.sdocbackup");
            try
            {
                PaqueteRespaldoProtegidoV2.Desproteger(ruta, rutaV1, contrasena);
                var resultado = _interno.Restaurar(rutaV1, ahoraUtc, versionAplicacion);
                return resultado with { RutaArchivoOrigen = ruta };
            }
            finally
            {
                Array.Clear(contrasena, 0, contrasena.Length);
                EliminarDirectorioSilencioso(directorioTemporal);
            }
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
                "El respaldo protegido se generó, pero no fue posible publicar el archivo de destino.",
                exception);
        }
    }

    private static void EliminarArchivoSilencioso(string ruta)
    {
        try
        {
            if (File.Exists(ruta)) File.Delete(ruta);
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
            if (Directory.Exists(ruta)) Directory.Delete(ruta, recursive: true);
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException)
        {
            // Best effort cleanup only.
        }
    }
}

internal static class PaqueteRespaldoProtegidoV2
{
    internal const int IteracionesPbkdf2Escritura = 600_000;

    private const string IdentificadorFormato = "SistemaDocenteNEM.Backup";
    private const int VersionFormato = 2;
    private const string RutaProteccion = "protection.json";
    private const string RutaPayload = "payload.bin";
    private const string ModoProteccion = "Password";
    private const string Kdf = "PBKDF2-HMAC-SHA256";
    private const string Cifrado = "AES-256-GCM-CHUNKED";

    private const int LongitudMinimaContrasena = 12;
    private const int LongitudSal = 16;
    private const int LongitudClave = 32;
    private const int LongitudPrefijoNonce = 4;
    private const int LongitudNonce = 12;
    private const int LongitudTag = 16;
    private const int TamanoChunkEscritura = 1024 * 1024;
    private const int TamanoMinimoChunk = 64 * 1024;
    private const int TamanoMaximoChunk = 4 * 1024 * 1024;
    private const int IteracionesPbkdf2Minimas = 100_000;
    private const int IteracionesPbkdf2Maximas = 5_000_000;
    private const long MaximoEncabezadoBytes = 16 * 1024;
    private const long MaximoPayloadPlanoBytes = 3L * 1024 * 1024 * 1024;

    private static readonly JsonSerializerOptions OpcionesJson = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = false,
        WriteIndented = false,
    };

    internal static TipoProteccionRespaldoLocal DetectarProteccion(string rutaRespaldo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rutaRespaldo);
        var ruta = Path.GetFullPath(rutaRespaldo);
        if (!File.Exists(ruta))
        {
            throw new RecuperacionLocalException(
                CategoriaErrorRecuperacionLocal.AccesoArchivo,
                "El archivo de respaldo seleccionado no existe.");
        }

        try
        {
            using var flujo = File.Open(ruta, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var archivo = new ZipArchive(flujo, ZipArchiveMode.Read, leaveOpen: false);
            var contieneProteccion = archivo.Entries.Any(x =>
                string.Equals(x.FullName, RutaProteccion, StringComparison.Ordinal));
            var contienePayload = archivo.Entries.Any(x =>
                string.Equals(x.FullName, RutaPayload, StringComparison.Ordinal));

            if (!contieneProteccion && !contienePayload)
            {
                return TipoProteccionRespaldoLocal.Ninguna;
            }

            _ = LeerPerfil(archivo);
            return TipoProteccionRespaldoLocal.Contrasena;
        }
        catch (RecuperacionLocalException)
        {
            throw;
        }
        catch (Exception exception) when (
            exception is IOException
                or UnauthorizedAccessException
                or InvalidDataException
                or JsonException)
        {
            throw PaqueteInvalido(
                "El archivo seleccionado no es un respaldo protegido válido o está dañado.",
                exception);
        }
    }

    internal static void Crear(
        string rutaPayloadV1,
        string rutaDestinoV2,
        char[] contrasena)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rutaPayloadV1);
        ArgumentException.ThrowIfNullOrWhiteSpace(rutaDestinoV2);
        ValidarLongitudContrasena(contrasena);

        var origen = Path.GetFullPath(rutaPayloadV1);
        var destino = Path.GetFullPath(rutaDestinoV2);
        var tamanoPlano = new FileInfo(origen).Length;
        if (tamanoPlano <= 0 || tamanoPlano > MaximoPayloadPlanoBytes)
        {
            throw PaqueteInvalido("El contenido interno del respaldo tiene un tamaño no admitido.");
        }

        var chunks = checked((tamanoPlano + TamanoChunkEscritura - 1) / TamanoChunkEscritura);
        var sal = RandomNumberGenerator.GetBytes(LongitudSal);
        var prefijoNonce = RandomNumberGenerator.GetBytes(LongitudPrefijoNonce);
        var encabezado = new EncabezadoProteccion(
            IdentificadorFormato,
            VersionFormato,
            new DetalleProteccion(
                ModoProteccion,
                Kdf,
                IteracionesPbkdf2Escritura,
                Convert.ToBase64String(sal),
                Cifrado,
                TamanoChunkEscritura,
                Convert.ToBase64String(prefijoNonce),
                tamanoPlano,
                chunks));
        var encabezadoBytes = JsonSerializer.SerializeToUtf8Bytes(encabezado, OpcionesJson);
        if (encabezadoBytes.LongLength > MaximoEncabezadoBytes)
        {
            throw new InvalidOperationException("El encabezado de protección generado excede el límite interno.");
        }

        var clave = DerivarClave(contrasena, sal, IteracionesPbkdf2Escritura);
        try
        {
            using var flujoSalida = new FileStream(
                destino,
                FileMode.CreateNew,
                FileAccess.ReadWrite,
                FileShare.None);
            using var archivo = new ZipArchive(flujoSalida, ZipArchiveMode.Create, leaveOpen: false);

            var entradaProteccion = archivo.CreateEntry(RutaProteccion, CompressionLevel.Optimal);
            using (var salidaEncabezado = entradaProteccion.Open())
            {
                salidaEncabezado.Write(encabezadoBytes, 0, encabezadoBytes.Length);
            }

            var entradaPayload = archivo.CreateEntry(RutaPayload, CompressionLevel.NoCompression);
            using var salidaPayload = entradaPayload.Open();
            using var flujoOrigen = File.Open(origen, FileMode.Open, FileAccess.Read, FileShare.Read);
            CifrarPayload(
                flujoOrigen,
                salidaPayload,
                encabezadoBytes,
                prefijoNonce,
                clave,
                tamanoPlano,
                chunks,
                TamanoChunkEscritura);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(clave);
            CryptographicOperations.ZeroMemory(sal);
            CryptographicOperations.ZeroMemory(prefijoNonce);
        }
    }

    internal static void Desproteger(
        string rutaOrigenV2,
        string rutaDestinoV1,
        char[] contrasena)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rutaOrigenV2);
        ArgumentException.ThrowIfNullOrWhiteSpace(rutaDestinoV1);
        ValidarLongitudContrasena(contrasena);

        var origen = Path.GetFullPath(rutaOrigenV2);
        var destino = Path.GetFullPath(rutaDestinoV1);
        byte[]? clave = null;
        try
        {
            using var flujo = File.Open(origen, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var archivo = new ZipArchive(flujo, ZipArchiveMode.Read, leaveOpen: false);
            var perfil = LeerPerfil(archivo);
            clave = DerivarClave(contrasena, perfil.Sal, perfil.Encabezado.Protection.Iterations);

            using var salida = new FileStream(
                destino,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None);
            using var entradaPayload = perfil.EntradaPayload.Open();
            DescifrarPayload(
                entradaPayload,
                salida,
                perfil.EncabezadoBytes,
                perfil.PrefijoNonce,
                clave,
                perfil.Encabezado.Protection.PlaintextSizeBytes,
                perfil.Encabezado.Protection.ChunkCount,
                perfil.Encabezado.Protection.ChunkSizeBytes);
        }
        catch (RecuperacionLocalException)
        {
            EliminarArchivoSilencioso(destino);
            throw;
        }
        catch (CryptographicException exception)
        {
            EliminarArchivoSilencioso(destino);
            throw ErrorAutenticacion(exception);
        }
        catch (Exception exception) when (
            exception is IOException
                or UnauthorizedAccessException
                or InvalidDataException
                or JsonException)
        {
            EliminarArchivoSilencioso(destino);
            throw ErrorAutenticacion(exception);
        }
        finally
        {
            if (clave is not null) CryptographicOperations.ZeroMemory(clave);
        }
    }

    private static PerfilProteccion LeerPerfil(ZipArchive archivo)
    {
        if (archivo.Entries.Count != 2)
        {
            throw PaqueteInvalido("El respaldo protegido contiene un número inesperado de componentes.");
        }

        var entradas = new Dictionary<string, ZipArchiveEntry>(StringComparer.Ordinal);
        foreach (var entrada in archivo.Entries)
        {
            if (!EsRutaSegura(entrada.FullName) || !entradas.TryAdd(entrada.FullName, entrada))
            {
                throw PaqueteInvalido("El respaldo protegido contiene rutas inseguras o componentes duplicados.");
            }
        }

        if (!entradas.TryGetValue(RutaProteccion, out var entradaProteccion)
            || !entradas.TryGetValue(RutaPayload, out var entradaPayload))
        {
            throw PaqueteInvalido("El respaldo protegido no contiene todos los componentes esperados.");
        }

        if (entradaProteccion.Length <= 0 || entradaProteccion.Length > MaximoEncabezadoBytes)
        {
            throw PaqueteInvalido("El encabezado de protección tiene un tamaño no válido.");
        }

        var encabezadoBytes = LeerEntradaLimitada(entradaProteccion, MaximoEncabezadoBytes);
        var encabezado = JsonSerializer.Deserialize<EncabezadoProteccion>(encabezadoBytes, OpcionesJson)
            ?? throw PaqueteInvalido("El encabezado de protección está vacío.");
        var (sal, prefijoNonce) = ValidarEncabezado(encabezado, entradaPayload);
        return new PerfilProteccion(
            encabezado,
            encabezadoBytes,
            sal,
            prefijoNonce,
            entradaPayload);
    }

    private static (byte[] Sal, byte[] PrefijoNonce) ValidarEncabezado(
        EncabezadoProteccion encabezado,
        ZipArchiveEntry entradaPayload)
    {
        if (!string.Equals(encabezado.Format, IdentificadorFormato, StringComparison.Ordinal)
            || encabezado.FormatVersion != VersionFormato
            || encabezado.Protection is null
            || !string.Equals(encabezado.Protection.Mode, ModoProteccion, StringComparison.Ordinal)
            || !string.Equals(encabezado.Protection.Kdf, Kdf, StringComparison.Ordinal)
            || !string.Equals(encabezado.Protection.Cipher, Cifrado, StringComparison.Ordinal))
        {
            throw PaqueteInvalido("El encabezado de protección usa un perfil no compatible.");
        }

        var p = encabezado.Protection;
        if (p.Iterations < IteracionesPbkdf2Minimas || p.Iterations > IteracionesPbkdf2Maximas)
        {
            throw PaqueteInvalido("El respaldo protegido declara un costo de derivación no permitido.");
        }
        if (p.ChunkSizeBytes < TamanoMinimoChunk || p.ChunkSizeBytes > TamanoMaximoChunk)
        {
            throw PaqueteInvalido("El respaldo protegido declara un tamaño de bloque no permitido.");
        }
        if (p.PlaintextSizeBytes <= 0 || p.PlaintextSizeBytes > MaximoPayloadPlanoBytes)
        {
            throw PaqueteInvalido("El respaldo protegido declara un tamaño interno no permitido.");
        }

        var chunkCountEsperado = checked(
            (p.PlaintextSizeBytes + p.ChunkSizeBytes - 1) / p.ChunkSizeBytes);
        if (p.ChunkCount != chunkCountEsperado || p.ChunkCount <= 0)
        {
            throw PaqueteInvalido("El respaldo protegido declara una cantidad de bloques inconsistente.");
        }

        var longitudPayloadEsperada = checked(
            p.PlaintextSizeBytes + (p.ChunkCount * (sizeof(int) + LongitudTag)));
        if (entradaPayload.Length != longitudPayloadEsperada)
        {
            throw PaqueteInvalido("El tamaño del contenido protegido no coincide con su encabezado.");
        }

        byte[] sal;
        byte[] prefijoNonce;
        try
        {
            sal = Convert.FromBase64String(p.Salt);
            prefijoNonce = Convert.FromBase64String(p.NoncePrefix);
        }
        catch (FormatException exception)
        {
            throw PaqueteInvalido("El encabezado de protección contiene parámetros codificados inválidos.", exception);
        }

        if (sal.Length != LongitudSal || prefijoNonce.Length != LongitudPrefijoNonce)
        {
            CryptographicOperations.ZeroMemory(sal);
            CryptographicOperations.ZeroMemory(prefijoNonce);
            throw PaqueteInvalido("El encabezado de protección contiene parámetros de longitud inválida.");
        }

        return (sal, prefijoNonce);
    }

    private static void CifrarPayload(
        Stream origen,
        Stream destino,
        byte[] encabezadoBytes,
        byte[] prefijoNonce,
        byte[] clave,
        long tamanoPlano,
        long chunkCount,
        int chunkSize)
    {
        var plano = new byte[chunkSize];
        var cifrado = new byte[chunkSize];
        var tag = new byte[LongitudTag];
        var longitudBytes = new byte[sizeof(int)];
        var restante = tamanoPlano;

        try
        {
            using var aes = new AesGcm(clave, LongitudTag);
            for (long indice = 0; indice < chunkCount; indice++)
            {
                var longitud = checked((int)Math.Min(chunkSize, restante));
                LeerExactamente(origen, plano.AsSpan(0, longitud));
                var nonce = CrearNonce(prefijoNonce, indice);
                var aad = CrearDatosAsociados(encabezadoBytes, indice, longitud);
                try
                {
                    aes.Encrypt(
                        nonce,
                        plano.AsSpan(0, longitud),
                        cifrado.AsSpan(0, longitud),
                        tag,
                        aad);
                    BinaryPrimitives.WriteInt32BigEndian(longitudBytes, longitud);
                    destino.Write(longitudBytes, 0, longitudBytes.Length);
                    destino.Write(cifrado, 0, longitud);
                    destino.Write(tag, 0, tag.Length);
                }
                finally
                {
                    CryptographicOperations.ZeroMemory(nonce);
                    CryptographicOperations.ZeroMemory(aad);
                }
                restante -= longitud;
            }

            if (restante != 0 || origen.ReadByte() != -1)
            {
                throw new InvalidDataException("El contenido interno cambió mientras se protegía el respaldo.");
            }
        }
        finally
        {
            CryptographicOperations.ZeroMemory(plano);
            CryptographicOperations.ZeroMemory(cifrado);
            CryptographicOperations.ZeroMemory(tag);
            CryptographicOperations.ZeroMemory(longitudBytes);
        }
    }

    private static void DescifrarPayload(
        Stream origen,
        Stream destino,
        byte[] encabezadoBytes,
        byte[] prefijoNonce,
        byte[] clave,
        long tamanoPlano,
        long chunkCount,
        int chunkSize)
    {
        var cifrado = new byte[chunkSize];
        var plano = new byte[chunkSize];
        var tag = new byte[LongitudTag];
        var longitudBytes = new byte[sizeof(int)];
        var restante = tamanoPlano;

        try
        {
            using var aes = new AesGcm(clave, LongitudTag);
            for (long indice = 0; indice < chunkCount; indice++)
            {
                LeerExactamente(origen, longitudBytes);
                var longitud = BinaryPrimitives.ReadInt32BigEndian(longitudBytes);
                var esperada = checked((int)Math.Min(chunkSize, restante));
                if (longitud != esperada)
                {
                    throw new InvalidDataException("La secuencia de bloques protegidos es inconsistente.");
                }

                LeerExactamente(origen, cifrado.AsSpan(0, longitud));
                LeerExactamente(origen, tag);
                var nonce = CrearNonce(prefijoNonce, indice);
                var aad = CrearDatosAsociados(encabezadoBytes, indice, longitud);
                try
                {
                    aes.Decrypt(
                        nonce,
                        cifrado.AsSpan(0, longitud),
                        tag,
                        plano.AsSpan(0, longitud),
                        aad);
                    destino.Write(plano, 0, longitud);
                }
                finally
                {
                    CryptographicOperations.ZeroMemory(nonce);
                    CryptographicOperations.ZeroMemory(aad);
                }
                restante -= longitud;
            }

            if (restante != 0 || origen.ReadByte() != -1)
            {
                throw new InvalidDataException("El contenido protegido contiene datos inesperados.");
            }
        }
        catch (CryptographicException exception)
        {
            throw ErrorAutenticacion(exception);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(cifrado);
            CryptographicOperations.ZeroMemory(plano);
            CryptographicOperations.ZeroMemory(tag);
            CryptographicOperations.ZeroMemory(longitudBytes);
        }
    }

    private static byte[] DerivarClave(char[] contrasena, byte[] sal, int iteraciones)
    {
        ValidarLongitudContrasena(contrasena);
        var normalizada = new string(contrasena).Normalize(NormalizationForm.FormC);
        var bytesContrasena = Encoding.UTF8.GetBytes(normalizada);
        var clave = new byte[LongitudClave];
        try
        {
            Rfc2898DeriveBytes.Pbkdf2(
                bytesContrasena,
                sal,
                clave,
                iteraciones,
                HashAlgorithmName.SHA256);
            return clave;
        }
        catch
        {
            CryptographicOperations.ZeroMemory(clave);
            throw;
        }
        finally
        {
            CryptographicOperations.ZeroMemory(bytesContrasena);
        }
    }

    private static byte[] CrearNonce(byte[] prefijoNonce, long indice)
    {
        var nonce = new byte[LongitudNonce];
        prefijoNonce.CopyTo(nonce, 0);
        BinaryPrimitives.WriteUInt64BigEndian(nonce.AsSpan(LongitudPrefijoNonce), checked((ulong)indice));
        return nonce;
    }

    private static byte[] CrearDatosAsociados(
        byte[] encabezadoBytes,
        long indice,
        int longitud)
    {
        var aad = new byte[encabezadoBytes.Length + sizeof(long) + sizeof(int)];
        encabezadoBytes.CopyTo(aad, 0);
        BinaryPrimitives.WriteUInt64BigEndian(
            aad.AsSpan(encabezadoBytes.Length, sizeof(long)),
            checked((ulong)indice));
        BinaryPrimitives.WriteInt32BigEndian(
            aad.AsSpan(encabezadoBytes.Length + sizeof(long), sizeof(int)),
            longitud);
        return aad;
    }

    private static byte[] LeerEntradaLimitada(ZipArchiveEntry entrada, long maximo)
    {
        if (entrada.Length <= 0 || entrada.Length > maximo)
        {
            throw PaqueteInvalido("Un componente del respaldo protegido excede el tamaño permitido.");
        }
        using var origen = entrada.Open();
        using var memoria = new MemoryStream(checked((int)entrada.Length));
        var buffer = new byte[8192];
        long total = 0;
        int leidos;
        while ((leidos = origen.Read(buffer, 0, buffer.Length)) > 0)
        {
            total += leidos;
            if (total > maximo)
            {
                throw PaqueteInvalido("Un componente del respaldo protegido excede el tamaño permitido.");
            }
            memoria.Write(buffer, 0, leidos);
        }
        return memoria.ToArray();
    }

    private static void LeerExactamente(Stream origen, byte[] destino) =>
        LeerExactamente(origen, destino.AsSpan());

    private static void LeerExactamente(Stream origen, Span<byte> destino)
    {
        var total = 0;
        while (total < destino.Length)
        {
            var leidos = origen.Read(destino[total..]);
            if (leidos <= 0)
            {
                throw new InvalidDataException("El contenido protegido está truncado.");
            }
            total += leidos;
        }
    }

    private static bool EsRutaSegura(string ruta)
    {
        return !string.IsNullOrWhiteSpace(ruta)
            && !ruta.EndsWith('/')
            && !ruta.Contains('\\')
            && !ruta.StartsWith('/')
            && ruta.Split('/').All(segmento => segmento is not ("." or ".." or ""));
    }

    private static void ValidarLongitudContrasena(char[] contrasena)
    {
        ArgumentNullException.ThrowIfNull(contrasena);
        if (contrasena.Length < LongitudMinimaContrasena)
        {
            throw new InvalidOperationException(
                $"La contraseña del respaldo debe tener al menos {LongitudMinimaContrasena} caracteres.");
        }
    }

    private static RecuperacionLocalException ErrorAutenticacion(Exception exception)
    {
        return new RecuperacionLocalException(
            CategoriaErrorRecuperacionLocal.PaqueteInvalido,
            "La contraseña es incorrecta o el respaldo protegido está dañado.",
            exception);
    }

    private static RecuperacionLocalException PaqueteInvalido(
        string mensaje,
        Exception? exception = null)
    {
        return new RecuperacionLocalException(
            CategoriaErrorRecuperacionLocal.PaqueteInvalido,
            mensaje,
            exception);
    }

    private static void EliminarArchivoSilencioso(string ruta)
    {
        try
        {
            if (File.Exists(ruta)) File.Delete(ruta);
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException)
        {
            // Best effort cleanup only.
        }
    }

    private sealed record EncabezadoProteccion(
        string Format,
        int FormatVersion,
        DetalleProteccion Protection);

    private sealed record DetalleProteccion(
        string Mode,
        string Kdf,
        int Iterations,
        string Salt,
        string Cipher,
        int ChunkSizeBytes,
        string NoncePrefix,
        long PlaintextSizeBytes,
        long ChunkCount);

    private sealed record PerfilProteccion(
        EncabezadoProteccion Encabezado,
        byte[] EncabezadoBytes,
        byte[] Sal,
        byte[] PrefijoNonce,
        ZipArchiveEntry EntradaPayload);
}
