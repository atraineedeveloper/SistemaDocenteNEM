using System.Net;
using System.Security.Cryptography;
using System.Text;

using SistemaDocente.Application;
using SistemaDocente.Interchange;

namespace SistemaDocente.Application.Tests;

public sealed class ActualizacionesGitHubTests
{
    [Fact]
    public async Task PreviewSeleccionaLaVersionSemanticaMayorConAssetsEsperados()
    {
        var json = """
        [
          {
            "tag_name":"v0.2.4","draft":false,"prerelease":true,"body":"old","published_at":"2026-08-01T00:00:00Z",
            "assets":[
              {"name":"AulaRaiz-Setup-0.2.4-win-x64.exe","browser_download_url":"https://github.com/atraineedeveloper/SistemaDocenteNEM/releases/download/v0.2.4/AulaRaiz-Setup-0.2.4-win-x64.exe"},
              {"name":"SHA256SUMS.txt","browser_download_url":"https://github.com/atraineedeveloper/SistemaDocenteNEM/releases/download/v0.2.4/SHA256SUMS.txt"}
            ]
          },
          {
            "tag_name":"v0.2.7","draft":false,"prerelease":true,"body":"new","published_at":"2026-08-08T00:00:00Z",
            "assets":[
              {"name":"AulaRaiz-Setup-0.2.7-win-x64.exe","browser_download_url":"https://github.com/atraineedeveloper/SistemaDocenteNEM/releases/download/v0.2.7/AulaRaiz-Setup-0.2.7-win-x64.exe"},
              {"name":"SHA256SUMS.txt","browser_download_url":"https://github.com/atraineedeveloper/SistemaDocenteNEM/releases/download/v0.2.7/SHA256SUMS.txt"}
            ]
          },
          {
            "tag_name":"v9.0.0","draft":true,"prerelease":false,"body":"draft",
            "assets":[
              {"name":"AulaRaiz-Setup-9.0.0-win-x64.exe","browser_download_url":"https://github.com/atraineedeveloper/SistemaDocenteNEM/releases/download/v9.0.0/AulaRaiz-Setup-9.0.0-win-x64.exe"},
              {"name":"SHA256SUMS.txt","browser_download_url":"https://github.com/atraineedeveloper/SistemaDocenteNEM/releases/download/v9.0.0/SHA256SUMS.txt"}
            ]
          }
        ]
        """;
        using var cliente = new HttpClient(new RespuestaHandler(_ => Json(json)));
        using var servicio = new ServicioActualizacionesGitHub(cliente, Path.GetTempPath());

        var resultado = await servicio.BuscarAsync("0.2.5", CanalActualizacion.Preview);

        Assert.NotNull(resultado);
        Assert.Equal("0.2.7", resultado.Version);
        Assert.Equal("new", resultado.Notas);
    }

    [Fact]
    public void ChecksumSoloAceptaSha256ExactoDelInstaladorEsperado()
    {
        var valido = new string('a', 64);
        var contenido = $"{new string('b', 64)}  otro.exe\n{valido}  AulaRaiz-Setup-0.2.5-win-x64.exe\n";

        Assert.Equal(
            valido,
            ServicioActualizacionesGitHub.ObtenerSha256(contenido, "AulaRaiz-Setup-0.2.5-win-x64.exe"));
        Assert.Null(ServicioActualizacionesGitHub.ObtenerSha256("1234  AulaRaiz-Setup-0.2.5-win-x64.exe", "AulaRaiz-Setup-0.2.5-win-x64.exe"));
    }

    [Fact]
    public async Task DescargaPublicaElInstaladorSoloCuandoElHashCoincide()
    {
        var raiz = Path.Combine(Path.GetTempPath(), $"aularaiz-update-{Guid.NewGuid():N}");
        var bytes = Encoding.UTF8.GetBytes("instalador-ficticio");
        var sha = Convert.ToHexStringLower(SHA256.HashData(bytes));
        var nombre = "AulaRaiz-Setup-0.2.6-win-x64.exe";
        using var cliente = new HttpClient(new RespuestaHandler(request =>
            request.RequestUri!.AbsolutePath.EndsWith("SHA256SUMS.txt", StringComparison.Ordinal)
                ? Texto($"{sha}  {nombre}\n")
                : Binario(bytes)));
        using var servicio = new ServicioActualizacionesGitHub(cliente, raiz);
        var actualizacion = new ActualizacionDisponible(
            "0.2.6",
            "v0.2.6",
            string.Empty,
            new Uri($"https://github.com/atraineedeveloper/SistemaDocenteNEM/releases/download/v0.2.6/{nombre}"),
            new Uri("https://github.com/atraineedeveloper/SistemaDocenteNEM/releases/download/v0.2.6/SHA256SUMS.txt"),
            null);

        try
        {
            var resultado = await servicio.DescargarYVerificarAsync(actualizacion);

            Assert.Equal(sha, resultado.Sha256);
            Assert.True(File.Exists(resultado.RutaInstalador));
            Assert.Equal(bytes, await File.ReadAllBytesAsync(resultado.RutaInstalador));
            Assert.False(File.Exists(resultado.RutaInstalador + ".download"));
        }
        finally
        {
            if (Directory.Exists(raiz)) Directory.Delete(raiz, recursive: true);
        }
    }

    [Fact]
    public async Task HashDistintoRechazaYNoPublicaInstalador()
    {
        var raiz = Path.Combine(Path.GetTempPath(), $"aularaiz-update-{Guid.NewGuid():N}");
        var bytes = Encoding.UTF8.GetBytes("instalador-ficticio");
        var nombre = "AulaRaiz-Setup-0.2.6-win-x64.exe";
        using var cliente = new HttpClient(new RespuestaHandler(request =>
            request.RequestUri!.AbsolutePath.EndsWith("SHA256SUMS.txt", StringComparison.Ordinal)
                ? Texto($"{new string('0', 64)}  {nombre}\n")
                : Binario(bytes)));
        using var servicio = new ServicioActualizacionesGitHub(cliente, raiz);
        var actualizacion = new ActualizacionDisponible(
            "0.2.6",
            "v0.2.6",
            string.Empty,
            new Uri($"https://github.com/atraineedeveloper/SistemaDocenteNEM/releases/download/v0.2.6/{nombre}"),
            new Uri("https://github.com/atraineedeveloper/SistemaDocenteNEM/releases/download/v0.2.6/SHA256SUMS.txt"),
            null);

        try
        {
            var error = await Assert.ThrowsAsync<ErrorActualizacionException>(() =>
                servicio.DescargarYVerificarAsync(actualizacion));

            Assert.Equal("checksum_mismatch", error.Codigo);
            var final = Path.Combine(raiz, "AulaRaiz", "Updates", "0.2.6", nombre);
            Assert.False(File.Exists(final));
            Assert.False(File.Exists(final + ".download"));
        }
        finally
        {
            if (Directory.Exists(raiz)) Directory.Delete(raiz, recursive: true);
        }
    }

    [Fact]
    public async Task DescargaRechazaAssetsDeOtroRepositorioAntesDeSolicitarlos()
    {
        using var cliente = new HttpClient(new RespuestaHandler(_ =>
            throw new InvalidOperationException("La URL inválida no debe solicitarse.")));
        using var servicio = new ServicioActualizacionesGitHub(cliente, Path.GetTempPath());
        var actualizacion = new ActualizacionDisponible(
            "0.2.6",
            "v0.2.6",
            string.Empty,
            new Uri("https://github.com/otro/proyecto/releases/download/v0.2.6/AulaRaiz-Setup-0.2.6-win-x64.exe"),
            new Uri("https://github.com/otro/proyecto/releases/download/v0.2.6/SHA256SUMS.txt"),
            null);

        var error = await Assert.ThrowsAsync<ErrorActualizacionException>(() =>
            servicio.DescargarYVerificarAsync(actualizacion));

        Assert.Equal("asset_url_invalid", error.Codigo);
    }

    [Fact]
    public async Task DescargaRechazaInstaladorQueExcedeElLimiteDeclarado()
    {
        var raiz = Path.Combine(Path.GetTempPath(), $"aularaiz-update-{Guid.NewGuid():N}");
        var bytes = Encoding.UTF8.GetBytes("instalador-ficticio");
        var sha = Convert.ToHexStringLower(SHA256.HashData(bytes));
        var nombre = "AulaRaiz-Setup-0.2.6-win-x64.exe";
        using var cliente = new HttpClient(new RespuestaHandler(request =>
            request.RequestUri!.AbsolutePath.EndsWith("SHA256SUMS.txt", StringComparison.Ordinal)
                ? Texto($"{sha}  {nombre}\n")
                : Binario(bytes, 512L * 1024 * 1024 + 1)));
        using var servicio = new ServicioActualizacionesGitHub(cliente, raiz);
        var actualizacion = new ActualizacionDisponible(
            "0.2.6",
            "v0.2.6",
            string.Empty,
            new Uri($"https://github.com/atraineedeveloper/SistemaDocenteNEM/releases/download/v0.2.6/{nombre}"),
            new Uri("https://github.com/atraineedeveloper/SistemaDocenteNEM/releases/download/v0.2.6/SHA256SUMS.txt"),
            null);

        try
        {
            var error = await Assert.ThrowsAsync<ErrorActualizacionException>(() =>
                servicio.DescargarYVerificarAsync(actualizacion));

            Assert.Equal("installer_too_large", error.Codigo);
            var final = Path.Combine(raiz, "AulaRaiz", "Updates", "0.2.6", nombre);
            Assert.False(File.Exists(final));
            Assert.False(File.Exists(final + ".download"));
        }
        finally
        {
            if (Directory.Exists(raiz)) Directory.Delete(raiz, recursive: true);
        }
    }

    private static HttpResponseMessage Json(string contenido) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(contenido, Encoding.UTF8, "application/json"),
    };

    private static HttpResponseMessage Texto(string contenido) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(contenido, Encoding.ASCII, "text/plain"),
    };

    private static HttpResponseMessage Binario(byte[] contenido, long? longitudDeclarada = null)
    {
        var respuesta = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(contenido),
        };
        if (longitudDeclarada is not null)
            respuesta.Content.Headers.ContentLength = longitudDeclarada;
        return respuesta;
    }

    private sealed class RespuestaHandler(Func<HttpRequestMessage, HttpResponseMessage> respuesta) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(respuesta(request));
    }
}