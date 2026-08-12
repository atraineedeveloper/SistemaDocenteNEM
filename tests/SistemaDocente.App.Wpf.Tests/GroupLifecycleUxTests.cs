using System.IO;

namespace SistemaDocente.App.Wpf.Tests;

public sealed class GroupLifecycleUxTests
{
    [Fact]
    public void MisGruposExponeArchivarRestaurarYEliminacionSeparada()
    {
        var raiz = ObtenerRaizRepositorio();
        var vista = File.ReadAllText(Path.Combine(
            raiz,
            "src",
            "SistemaDocente.App.Wpf",
            "Views",
            "InicioGruposView.xaml"));

        Assert.Contains("Content=\"Archivar\"", vista, StringComparison.Ordinal);
        Assert.Contains("Text=\"Archivados\"", vista, StringComparison.Ordinal);
        Assert.Contains("Content=\"Restaurar\"", vista, StringComparison.Ordinal);
        Assert.Contains("Content=\"Eliminar…\"", vista, StringComparison.Ordinal);
        Assert.Contains("OnArchivarGrupoClic", vista, StringComparison.Ordinal);
        Assert.Contains("OnRestaurarGrupoClic", vista, StringComparison.Ordinal);
        Assert.Contains("OnEliminarGrupoClic", vista, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.Name", vista, StringComparison.Ordinal);
    }

    [Fact]
    public void EliminacionPobladaExigeNombreExactoYBotonIniciaDeshabilitado()
    {
        var raiz = ObtenerRaizRepositorio();
        var dialogo = File.ReadAllText(Path.Combine(
            raiz,
            "src",
            "SistemaDocente.App.Wpf",
            "ConfirmarEliminacionGrupoWindow.xaml"));
        var codigo = File.ReadAllText(Path.Combine(
            raiz,
            "src",
            "SistemaDocente.App.Wpf",
            "ConfirmarEliminacionGrupoWindow.xaml.cs"));

        Assert.Contains("x:Name=\"ConfirmacionTextBox\"", dialogo, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"EliminarButton\"", dialogo, StringComparison.Ordinal);
        Assert.Contains("IsEnabled=\"False\"", dialogo, StringComparison.Ordinal);
        Assert.Contains("Content=\"Eliminar definitivamente\"", dialogo, StringComparison.Ordinal);
        Assert.Contains("StringComparison.Ordinal", codigo, StringComparison.Ordinal);
        Assert.Contains("ConfirmacionTextBox.Text", codigo, StringComparison.Ordinal);
        Assert.DoesNotContain("PasswordBox", dialogo, StringComparison.Ordinal);
    }

    [Fact]
    public void ComposicionUsaExtensionDeCicloVidaYRespaldoPrevioAEliminacion()
    {
        var raiz = ObtenerRaizRepositorio();
        var app = File.ReadAllText(Path.Combine(
            raiz,
            "src",
            "SistemaDocente.App.Wpf",
            "App.xaml.cs"));
        var almacenamiento = File.ReadAllText(Path.Combine(
            raiz,
            "src",
            "SistemaDocente.Data",
            "AlmacenamientoGruposConRespaldoEliminacion.cs"));

        Assert.Contains("PersistenciaGrupoCicloVidaSqlite", app, StringComparison.Ordinal);
        Assert.Contains("AlmacenamientoGruposConRespaldoEliminacion", app, StringComparison.Ordinal);
        Assert.Contains("CicloVidaGruposWpf.Configurar", app, StringComparison.Ordinal);
        Assert.Contains("_recuperacion.CrearRespaldo", almacenamiento, StringComparison.Ordinal);
        Assert.Contains("if (resumen.TieneDatos)", almacenamiento, StringComparison.Ordinal);
    }

    private static string ObtenerRaizRepositorio() => Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory,
        "..",
        "..",
        "..",
        "..",
        ".."));
}
