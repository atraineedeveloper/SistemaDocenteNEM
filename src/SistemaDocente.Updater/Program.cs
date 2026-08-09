using System.Diagnostics;
using System.Security.Cryptography;
using System.Windows;

namespace SistemaDocente.Updater;

internal static class Program
{
    [STAThread]
    private static async Task<int> Main(string[] args)
    {
        try
        {
            var opciones = OpcionesActualizador.Parsear(args);
            await VerificarSha256Async(opciones.RutaInstalador, opciones.Sha256).ConfigureAwait(false);
            await EsperarProcesoAsync(opciones.ProcesoPadreId).ConfigureAwait(false);

            var instalador = Process.Start(new ProcessStartInfo
            {
                FileName = opciones.RutaInstalador,
                UseShellExecute = false,
                CreateNoWindow = true,
                ArgumentList =
                {
                    "/VERYSILENT",
                    "/SUPPRESSMSGBOXES",
                    "/NORESTART",
                    "/SP-",
                },
            }) ?? throw new InvalidOperationException("No se pudo iniciar el instalador.");

            await instalador.WaitForExitAsync().ConfigureAwait(false);
            if (instalador.ExitCode != 0)
                throw new InvalidOperationException("El instalador no finalizó correctamente.");

            if (!File.Exists(opciones.RutaAplicacion))
                throw new FileNotFoundException("No se encontró AulaRaíz después de la actualización.");

            var inicio = new ProcessStartInfo
            {
                FileName = opciones.RutaAplicacion,
                UseShellExecute = true,
            };
            if (opciones.ModoDemo) inicio.ArgumentList.Add("--demo");
            inicio.ArgumentList.Add("--updated-to");
            inicio.ArgumentList.Add(opciones.VersionObjetivo);
            Process.Start(inicio);
            return 0;
        }
        catch
        {
            MessageBox.Show(
                "No fue posible completar la actualización. AulaRaíz no modificará tus datos; puedes abrir la aplicación nuevamente e intentarlo más tarde.",
                "Actualización de AulaRaíz",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return 1;
        }
    }

    internal static async Task VerificarSha256Async(string rutaInstalador, string esperado)
    {
        if (!File.Exists(rutaInstalador)) throw new FileNotFoundException();
        if (esperado.Length != 64 || esperado.Any(c => !Uri.IsHexDigit(c)))
            throw new ArgumentException("SHA-256 inválido.");

        await using var stream = new FileStream(
            rutaInstalador,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            64 * 1024,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        var hash = await SHA256.HashDataAsync(stream).ConfigureAwait(false);
        var actual = Convert.ToHexStringLower(hash);
        if (!string.Equals(actual, esperado, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("SHA-256 no coincide.");
    }

    private static async Task EsperarProcesoAsync(int procesoId)
    {
        try
        {
            using var proceso = Process.GetProcessById(procesoId);
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            await proceso.WaitForExitAsync(timeout.Token).ConfigureAwait(false);
        }
        catch (ArgumentException)
        {
            // El proceso ya terminó antes de que el helper comenzara a esperar.
        }
    }
}

internal sealed record OpcionesActualizador(
    int ProcesoPadreId,
    string RutaInstalador,
    string Sha256,
    string RutaAplicacion,
    string VersionObjetivo,
    bool ModoDemo)
{
    public static OpcionesActualizador Parsear(string[] args)
    {
        var valores = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var modoDemo = false;

        for (var i = 0; i < args.Length; i++)
        {
            if (string.Equals(args[i], "--demo", StringComparison.OrdinalIgnoreCase))
            {
                modoDemo = true;
                continue;
            }

            if (!args[i].StartsWith("--", StringComparison.Ordinal) || i + 1 >= args.Length)
                throw new ArgumentException("Argumentos inválidos.");
            valores[args[i]] = args[++i];
        }

        if (!valores.TryGetValue("--wait-pid", out var pidTexto) || !int.TryParse(pidTexto, out var pid) || pid <= 0)
            throw new ArgumentException("PID inválido.");
        if (!valores.TryGetValue("--installer", out var instalador) || !Path.IsPathFullyQualified(instalador))
            throw new ArgumentException("Instalador inválido.");
        if (!valores.TryGetValue("--sha256", out var sha256))
            throw new ArgumentException("SHA-256 faltante.");
        if (!valores.TryGetValue("--app", out var aplicacion) || !Path.IsPathFullyQualified(aplicacion))
            throw new ArgumentException("Aplicación inválida.");
        if (!valores.TryGetValue("--target-version", out var version))
            throw new ArgumentException("Versión faltante.");

        var partesVersion = version.Split('.');
        if (partesVersion.Length != 3
            || partesVersion.Any(p => !int.TryParse(p, out var numero) || numero < 0)
            || !System.Version.TryParse(version, out var versionParseada))
        {
            throw new ArgumentException("Versión inválida.");
        }

        return new OpcionesActualizador(
            pid,
            Path.GetFullPath(instalador),
            sha256.ToLowerInvariant(),
            Path.GetFullPath(aplicacion),
            versionParseada.ToString(3),
            modoDemo);
    }
}
