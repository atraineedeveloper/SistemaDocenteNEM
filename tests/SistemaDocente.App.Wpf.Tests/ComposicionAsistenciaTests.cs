using SistemaDocente.App.Wpf;
using SistemaDocente.Data;
using SistemaDocente.Presentation;

namespace SistemaDocente.App.Wpf.Tests;

public sealed class ComposicionAsistenciaTests
{
    [Fact]
    public void AplicacionReferenciaCapasDeComposicionRequeridas()
    {
        var referencias = typeof(App).Assembly.GetReferencedAssemblies().Select(x => x.Name).ToArray();

        Assert.Contains("SistemaDocente.Application", referencias);
        Assert.Contains("SistemaDocente.Data", referencias);
        Assert.Contains("SistemaDocente.Presentation", referencias);
        Assert.NotNull(typeof(PersistenciaAsistenciaSqlite));
        Assert.NotNull(typeof(GestionAsistenciaViewModel));
    }

    [Fact]
    public void VistaIntegraNavegacionCapturaYNoContieneSql()
    {
        var raiz = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", ".."));
        var xaml = File.ReadAllText(Path.Combine(
            raiz,
            "src",
            "SistemaDocente.App.Wpf",
            "MainWindow.xaml"));
        var codeBehind = File.ReadAllText(Path.Combine(
            raiz,
            "src",
            "SistemaDocente.App.Wpf",
            "MainWindow.xaml.cs"));

        Assert.Contains("DatePicker", xaml, StringComparison.Ordinal);
        Assert.Contains("DataGrid", xaml, StringComparison.Ordinal);
        Assert.Contains("Falta justificada", xaml, StringComparison.Ordinal);
        Assert.Contains("Marcar todos presentes", xaml, StringComparison.Ordinal);
        Assert.Contains("Modifiers=\"Control\" Key=\"S\"", xaml, StringComparison.Ordinal);
        Assert.Contains("FrozenColumnCount=\"2\"", xaml, StringComparison.Ordinal);
        Assert.Contains("GrillaMensual", xaml, StringComparison.Ordinal);
        Assert.Contains("Guardar cambios (Ctrl+S)", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("No lectivo", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("No lectivo", codeBehind, StringComparison.Ordinal);
        Assert.Contains("EsCierreSemana", codeBehind, StringComparison.Ordinal);
        Assert.Contains("BorderThicknessProperty", codeBehind, StringComparison.Ordinal);
        Assert.Contains("CrearColumnasMensuales", codeBehind, StringComparison.Ordinal);
        Assert.Contains("Key.PageUp", codeBehind, StringComparison.Ordinal);
        Assert.DoesNotContain("GrupoId", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("EstudianteId", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("SELECT ", xaml, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("INSERT ", codeBehind, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Sqlite", codeBehind, StringComparison.OrdinalIgnoreCase);
    }
}