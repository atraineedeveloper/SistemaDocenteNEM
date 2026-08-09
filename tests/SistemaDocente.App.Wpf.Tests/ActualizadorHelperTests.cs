using System.Security.Cryptography;
using System.Text;

using SistemaDocente.Updater;

namespace SistemaDocente.App.Wpf.Tests;

public sealed class ActualizadorHelperTests
{
    [Fact]
    public void ParseoConservaModoDemoYVersionExacta()
    {
        var raiz = Path.GetTempPath();
        var opciones = OpcionesActualizador.Parsear([
            "--wait-pid", "123",
            "--installer", Path.Combine(raiz, "setup.exe"),
            "--sha256", new string('a', 64),
            "--app", Path.Combine(raiz, "AulaRaiz.exe"),
            "--target-version", "0.2.5",
            "--demo",
        ]);

        Assert.Equal(123, opciones.ProcesoPadreId);
        Assert.Equal("0.2.5", opciones.VersionObjetivo);
        Assert.True(opciones.ModoDemo);
    }

    [Theory]
    [InlineData("0.2")]
    [InlineData("0.2.5.1")]
    [InlineData("v0.2.5")]
    public void ParseoRechazaVersionQueNoSeaMayorMenorParche(string version)
    {
        var raiz = Path.GetTempPath();

        Assert.Throws<ArgumentException>(() => OpcionesActualizador.Parsear([
            "--wait-pid", "123",
            "--installer", Path.Combine(raiz, "setup.exe"),
            "--sha256", new string('a', 64),
            "--app", Path.Combine(raiz, "AulaRaiz.exe"),
            "--target-version", version,
        ]));
    }

    [Fact]
    public async Task RevalidacionSha256AceptaArchivoIntactoYRechazaCambio()
    {
        var ruta = Path.Combine(Path.GetTempPath(), $"aularaiz-updater-{Guid.NewGuid():N}.exe");
        var contenido = Encoding.UTF8.GetBytes("fixture-update");
        await File.WriteAllBytesAsync(ruta, contenido);
        var sha = Convert.ToHexStringLower(SHA256.HashData(contenido));

        try
        {
            await Program.VerificarSha256Async(ruta, sha);
            await Assert.ThrowsAsync<InvalidDataException>(() =>
                Program.VerificarSha256Async(ruta, new string('0', 64)));
        }
        finally
        {
            File.Delete(ruta);
        }
    }
}
