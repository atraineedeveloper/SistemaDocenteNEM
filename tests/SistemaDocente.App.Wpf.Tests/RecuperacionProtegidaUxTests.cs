using System.IO;

namespace SistemaDocente.App.Wpf.Tests;

public sealed class RecuperacionProtegidaUxTests
{
    [Fact]
    public void VentanaMantieneContrasenasEnPasswordBoxSinBindingAViewModel()
    {
        var raiz = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        var xaml = File.ReadAllText(Path.Combine(
            raiz,
            "src",
            "SistemaDocente.App.Wpf",
            "RecuperacionLocalWindow.xaml"));
        var code = File.ReadAllText(Path.Combine(
            raiz,
            "src",
            "SistemaDocente.App.Wpf",
            "RecuperacionLocalWindow.xaml.cs"));

        Assert.Contains("Proteger con contraseña (v2)", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"CreatePasswordBox\"", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"ConfirmCreatePasswordBox\"", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"RestorePasswordBox\"", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Password=\"{Binding", xaml, StringComparison.Ordinal);
        Assert.Contains("Desbloquear e inspeccionar", xaml, StringComparison.Ordinal);
        Assert.Contains("Array.Clear(contrasena", code, StringComparison.Ordinal);
        Assert.Contains("RestorePasswordBox.Clear()", code, StringComparison.Ordinal);
    }

    [Fact]
    public void ComposicionEnvuelveRecuperacionV1SinReemplazarSuImplementacion()
    {
        var raiz = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        var app = File.ReadAllText(Path.Combine(
            raiz,
            "src",
            "SistemaDocente.App.Wpf",
            "App.xaml.cs"));

        Assert.Contains("new ServicioRecuperacionLocalSqlite(", app, StringComparison.Ordinal);
        Assert.Contains("new ServicioRecuperacionLocalProtegida(servicioRecuperacionV1)", app, StringComparison.Ordinal);
    }
}
