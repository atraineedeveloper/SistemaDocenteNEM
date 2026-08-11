using System.Buffers;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;

using SistemaDocente.Application;

namespace SistemaDocente.Interchange;

public sealed partial class ServicioActualizacionesGitHub : IServicioActualizacionesAplicacion, IDisposable
{
    private const string Repositorio = "atraineedeveloper/SistemaDocenteNEM";
    private const string NombreChecksums = "SHA256SUMS.txt";
    private const int MaximoBytesChecksums = 64 * 1024;
    private const long MaximoBytesInstalador = 512L * 1024 * 1024;
    private readonly HttpClient _httpClient;
    private readonly bool _poseeHttpClient;
    private readonly string _directorioActualizaciones;

    public ServicioActualizacionesGitHub(string localApplicationData, string versionActual)
        : this(CrearHttpClient(versionActual), localApplicationData, poseeHttpClient: true)
    {
    }

    internal ServicioActualizacionesGitHub(
        HttpClient httpClient,
        string localApplicationData,
        bool poseeHttpClient = false)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _poseeHttpClient = poseeHttpClient;
        if (string.IsNullOrWhiteSpace(localApplicationData))
            throw new ArgumentException("La ruta local es obligatoria.", nameof(localApplicationData));

        _directorioActualizaciones = Path.Combine(localApplicationData, "AulaRaiz", "Updates");
    }

    public async Task<ActualizacionDisponible?> BuscarAsync(
        string versionActual,
        CanalActualizacion canal,
        CancellationToken cancellationToken = default)
    {
        var actual = ParsearVersion(versionActual)
            ?? throw new ErrorActualizacionException("current_version_invalid");

        try
        {
            using var respuesta = await _httpClient.GetAsync(
                $"https://api.github.com/repos/{Repositorio}/releases?per_page=30",
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken).ConfigureAwait(false);
            respuesta.EnsureSuccessStatusCode();

            await using var stream = await respuesta.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            using var documento = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (documento.RootElement.ValueKind != JsonValueKind.Array)
                throw new ErrorActualizacionException("release_payload_invalid");

            ActualizacionDisponible? mejor = null;
            Version? mejorVersion = null;

            foreach (var release in documento.RootElement.EnumerateArray())
            {
                if (EsVerdadero(release, "draft")) continue;
                var prerelease = EsVerdadero(release, "prerelease");
                if (canal == CanalActualizacion.Estable && prerelease) continue;

                var etiqueta = ObtenerString(release, "tag_name");
                var candidata = ParsearEtiqueta(etiqueta);
                if (candidata is null || candidata <= actual) continue;
                if (mejorVersion is not null && candidata <= mejorVersion) continue;

                var versionTexto = candidata.ToString(3);
                var nombreInstalador = $"AulaRaiz-Setup-{versionTexto}-win-x64.exe";
                Uri? urlInstalador = null;
                Uri? urlChecksums = null;

                if (!release.TryGetProperty("assets", out var assets) || assets.ValueKind != JsonValueKind.Array)
                    continue;

                foreach (var asset in assets.EnumerateArray())
                {
                    var nombre = ObtenerString(asset, "name");
                    var urlTexto = ObtenerString(asset, "browser_download_url");
                    if (!Uri.TryCreate(urlTexto, UriKind.Absolute, out var url)) continue;

                    if (string.Equals(nombre, nombreInstalador, StringComparison.Ordinal)
                        && EsUrlAssetReleaseValida(url, etiqueta, nombreInstalador))
                    {
                        urlInstalador = url;
                    }
                    else if (string.Equals(nombre, NombreChecksums, StringComparison.Ordinal)
                        && EsUrlAssetReleaseValida(url, etiqueta, NombreChecksums))
                    {
                        urlChecksums = url;
                    }
                }

                if (urlInstalador is null || urlChecksums is null) continue;

                DateTimeOffset? publicadaEn = null;
                var publicadaTexto = ObtenerString(release, "published_at");
                if (DateTimeOffset.TryParse(publicadaTexto, out var publicada)) publicadaEn = publicada;

                mejorVersion = candidata;
                mejor = new ActualizacionDisponible(
                    versionTexto,
                    etiqueta,
                    ObtenerString(release, "body"),
                    urlInstalador,
                    urlChecksums,
                    publicadaEn);
            }

            return mejor;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (ErrorActualizacionException)
        {
            throw;
        }
        catch (Exception exception) when (exception is HttpRequestException or IOException or JsonException)
        {
            throw new ErrorActualizacionException("release_discovery_failed", exception);
        }
    }

    public async Task<ActualizacionVerificada> DescargarYVerificarAsync(
        ActualizacionDisponible actualizacion,
        IProgress<ProgresoDescargaActualizacion>? progreso = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actualizacion);
        var version = ParsearVersion(actualizacion.Version)
            ?? throw new ErrorActualizacionException("target_version_invalid");
        var versionTexto = version.ToString(3);
        var nombreInstalador = $"AulaRaiz-Setup-{versionTexto}-win-x64.exe";
        if (!EsUrlAssetReleaseValida(actualizacion.UrlInstalador, actualizacion.Etiqueta, nombreInstalador)
            || !EsUrlAssetReleaseValida(actualizacion.UrlChecksums, actualizacion.Etiqueta, NombreChecksums))
        {
            throw new ErrorActualizacionException("asset_url_invalid");
        }

        var directorioVersion = Path.Combine(_directorioActualizaciones, versionTexto);
        Directory.CreateDirectory(directorioVersion);
        var rutaFinal = Path.Combine(directorioVersion, nombreInstalador);
        var rutaTemporal = rutaFinal + ".download";

        try
        {
            var checksums = await DescargarTextoAcotadoAsync(actualizacion.UrlChecksums, cancellationToken).ConfigureAwait(false);
            var esperado = ObtenerSha256(checksums, nombreInstalador)
                ?? throw new ErrorActualizacionException("checksum_missing");

            await DescargarArchivoAsync(actualizacion.UrlInstalador, rutaTemporal, progreso, cancellationToken).ConfigureAwait(false);
            var actual = await CalcularSha256Async(rutaTemporal, cancellationToken).ConfigureAwait(false);
            if (!string.Equals(esperado, actual, StringComparison.OrdinalIgnoreCase))
                throw new ErrorActualizacionException("checksum_mismatch");

            File.Move(rutaTemporal, rutaFinal, overwrite: true);
            return new ActualizacionVerificada(versionTexto, rutaFinal, esperado.ToLowerInvariant());
        }
        catch
        {
            TryDelete(rutaTemporal);
            throw;
        }
    }

    internal static Version? ParsearEtiqueta(string etiqueta)
    {
        var coincidencia = EtiquetaVersionRegex().Match(etiqueta ?? string.Empty);
        return coincidencia.Success && Version.TryParse(coincidencia.Groups[1].Value, out var version)
            ? version
            : null;
    }

    internal static string? ObtenerSha256(string contenido, string nombreArchivo)
    {
        foreach (var linea in contenido.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var partes = linea.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (partes.Length < 2) continue;
            if (!string.Equals(partes[^1], nombreArchivo, StringComparison.Ordinal)) continue;
            if (Sha256Regex().IsMatch(partes[0])) return partes[0].ToLowerInvariant();
        }

        return null;
    }

    public void Dispose()
    {
        if (_poseeHttpClient) _httpClient.Dispose();
    }

    private static HttpClient CrearHttpClient(string versionActual)
    {
        var cliente = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30),
        };
        cliente.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("AulaRaiz", NormalizarVersionUserAgent(versionActual)));
        cliente.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        cliente.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
        return cliente;
    }

    private static string NormalizarVersionUserAgent(string version) =>
        ParsearVersion(version)?.ToString(3) ?? "0.0.0";

    private async Task<string> DescargarTextoAcotadoAsync(Uri url, CancellationToken cancellationToken)
    {
        try
        {
            using var respuesta = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            respuesta.EnsureSuccessStatusCode();
            if (respuesta.Content.Headers.ContentLength is > MaximoBytesChecksums)
                throw new ErrorActualizacionException("checksum_too_large");

            await using var stream = await respuesta.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            using var memoria = new MemoryStream();
            var buffer = ArrayPool<byte>.Shared.Rent(8192);
            try
            {
                while (true)
                {
                    var leidos = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken).ConfigureAwait(false);
                    if (leidos == 0) break;
                    if (memoria.Length + leidos > MaximoBytesChecksums)
                        throw new ErrorActualizacionException("checksum_too_large");
                    await memoria.WriteAsync(buffer.AsMemory(0, leidos), cancellationToken).ConfigureAwait(false);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            return System.Text.Encoding.ASCII.GetString(memoria.ToArray());
        }
        catch (ErrorActualizacionException)
        {
            throw;
        }
        catch (Exception exception) when (exception is HttpRequestException or IOException)
        {
            throw new ErrorActualizacionException("checksum_download_failed", exception);
        }
    }

    private async Task DescargarArchivoAsync(
        Uri url,
        string rutaTemporal,
        IProgress<ProgresoDescargaActualizacion>? progreso,
        CancellationToken cancellationToken)
    {
        try
        {
            using var respuesta = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            respuesta.EnsureSuccessStatusCode();
            var total = respuesta.Content.Headers.ContentLength;
            if (total is > MaximoBytesInstalador)
                throw new ErrorActualizacionException("installer_too_large");
            await using var origen = await respuesta.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            await using var destino = new FileStream(
                rutaTemporal,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                64 * 1024,
                FileOptions.Asynchronous | FileOptions.SequentialScan);

            var buffer = ArrayPool<byte>.Shared.Rent(64 * 1024);
            long recibidos = 0;
            try
            {
                while (true)
                {
                    var leidos = await origen.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken).ConfigureAwait(false);
                    if (leidos == 0) break;
                    if (recibidos + leidos > MaximoBytesInstalador)
                        throw new ErrorActualizacionException("installer_too_large");
                    await destino.WriteAsync(buffer.AsMemory(0, leidos), cancellationToken).ConfigureAwait(false);
                    recibidos += leidos;
                    progreso?.Report(new ProgresoDescargaActualizacion(recibidos, total));
                }

                await destino.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
        catch (Exception exception) when (exception is HttpRequestException or IOException)
        {
            throw new ErrorActualizacionException("installer_download_failed", exception);
        }
    }

    private static async Task<string> CalcularSha256Async(string ruta, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(
            ruta,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            64 * 1024,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        var hash = await SHA256.HashDataAsync(stream, cancellationToken).ConfigureAwait(false);
        return Convert.ToHexStringLower(hash);
    }

    private static Version? ParsearVersion(string version) =>
        VersionTextoRegex().IsMatch(version ?? string.Empty) && Version.TryParse(version, out var parsed)
            ? parsed
            : null;

    private static bool EsVerdadero(JsonElement elemento, string nombre) =>
        elemento.TryGetProperty(nombre, out var valor) && valor.ValueKind == JsonValueKind.True;

    private static string ObtenerString(JsonElement elemento, string nombre) =>
        elemento.TryGetProperty(nombre, out var valor) && valor.ValueKind == JsonValueKind.String
            ? valor.GetString() ?? string.Empty
            : string.Empty;

    private static bool EsUrlAssetReleaseValida(Uri url, string etiqueta, string nombreArchivo)
    {
        if (!string.Equals(url.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(url.Host, "github.com", StringComparison.OrdinalIgnoreCase)
            || !string.IsNullOrEmpty(url.Query)
            || !string.IsNullOrEmpty(url.Fragment))
        {
            return false;
        }

        var rutaEsperada = $"/{Repositorio}/releases/download/{Uri.EscapeDataString(etiqueta)}/{Uri.EscapeDataString(nombreArchivo)}";
        return string.Equals(url.AbsolutePath, rutaEsperada, StringComparison.Ordinal);
    }

    private static void TryDelete(string ruta)
    {
        try
        {
            if (File.Exists(ruta)) File.Delete(ruta);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    [GeneratedRegex("^v(\\d+\\.\\d+\\.\\d+)$", RegexOptions.CultureInvariant)]
    private static partial Regex EtiquetaVersionRegex();

    [GeneratedRegex("^\\d+\\.\\d+\\.\\d+$", RegexOptions.CultureInvariant)]
    private static partial Regex VersionTextoRegex();

    [GeneratedRegex("^[0-9a-fA-F]{64}$", RegexOptions.CultureInvariant)]
    private static partial Regex Sha256Regex();
}
