using SistemaDocente.App.Wpf;
using SistemaDocente.Data;
using SistemaDocente.Presentation;

using System.Text.RegularExpressions;
using System.Xml.Linq;

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
        Assert.NotNull(typeof(PersistenciaProyectosSqlite));
        Assert.NotNull(typeof(GestionProyectosViewModel));
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
        Assert.Contains("Marcar _todos presentes", xaml, StringComparison.Ordinal);
        Assert.Contains("Modifiers=\"Control\" Key=\"S\"", xaml, StringComparison.Ordinal);
        Assert.Contains("FrozenColumnCount=\"2\"", xaml, StringComparison.Ordinal);
        Assert.Contains("GrillaMensual", xaml, StringComparison.Ordinal);
        Assert.Contains("G_uardar cambios (Ctrl+S)", xaml, StringComparison.Ordinal);
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
        Assert.Contains("Content=\"_Proyectos\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Content=\"_Evaluación\"", xaml, StringComparison.Ordinal);
        Assert.Contains("ProyectosVisibles", xaml, StringComparison.Ordinal);
        Assert.Contains("EntregasVisibles", xaml, StringComparison.Ordinal);
        Assert.Contains("D Domina", xaml, StringComparison.Ordinal);
        Assert.Contains("S Suficiente", xaml, StringComparison.Ordinal);
        Assert.Contains("P Pendiente", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void ConteosDeSoloLecturaUsanBindingOneWay()
    {
        var xaml = LeerArchivoAplicacion("MainWindow.xaml");

        _ = XDocument.Parse(xaml);
        foreach (var propiedad in new[] { "Total", "Pendientes", "Domina", "Suficiente", "EnProceso", "RequiereApoyo", "NoEntrego" })
        {
            Assert.Contains($"{{Binding {propiedad}, Mode=OneWay}}", xaml, StringComparison.Ordinal);
            Assert.DoesNotMatch(
                $"Binding\\s+{propiedad}[^}}]*Mode=(?:TwoWay|OneWayToSource)",
                xaml);
        }
    }

    [Fact]
    public void AtajosEnpSonContextualesYCtrlSPermaneceGlobal()
    {
        var xaml = LeerArchivoAplicacion("MainWindow.xaml");
        var codeBehind = LeerArchivoAplicacion("MainWindow.xaml.cs");

        Assert.DoesNotMatch("<KeyBinding\\s+Key=\"[ENP]\"", xaml);
        Assert.Contains("Modifiers=\"Control\" Key=\"S\"", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"GrillaEntregasEvaluacion\"", xaml, StringComparison.Ordinal);
        Assert.Contains("PreviewKeyDown=\"OnGrillaEntregasEvaluacionPreviewKeyDown\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Keyboard.FocusedElement is TextBoxBase", codeBehind, StringComparison.Ordinal);
        Assert.Contains("GrillaEntregasEvaluacion.IsAncestorOf(foco)", codeBehind, StringComparison.Ordinal);
        Assert.Contains("MarcarDominaCommand", codeBehind, StringComparison.Ordinal);
        Assert.Contains("MarcarSuficienteCommand", codeBehind, StringComparison.Ordinal);
        Assert.Contains("MarcarEnProcesoCommand", codeBehind, StringComparison.Ordinal);
        Assert.Contains("MarcarRequiereApoyoCommand", codeBehind, StringComparison.Ordinal);
        Assert.Contains("MarcarNoEntregoCommand", codeBehind, StringComparison.Ordinal);
        Assert.Contains("MarcarPendienteCommand", codeBehind, StringComparison.Ordinal);
        Assert.DoesNotContain("NivelLogro.", codeBehind, StringComparison.Ordinal);
        Assert.DoesNotContain("EstadoEntrega.", codeBehind, StringComparison.Ordinal);
    }

    [Fact]
    public void NavegacionYCierreDeleganConfirmacionAlModuloProyectos()
    {
        var mainWindowViewModel = LeerArchivoPresentacion("MainWindowViewModel.cs");

        Assert.Contains("Proyectos?.SolicitarSalir()", mainWindowViewModel, StringComparison.Ordinal);
        Assert.True(Regex.Count(mainWindowViewModel, "Proyectos\\?\\.SolicitarSalir\\(\\)") >= 3);
    }

    private static string LeerArchivoAplicacion(string nombre) => File.ReadAllText(Path.Combine(
        ObtenerRaiz(), "src", "SistemaDocente.App.Wpf", nombre));

    private static string LeerArchivoPresentacion(string nombre) => File.ReadAllText(Path.Combine(
        ObtenerRaiz(), "src", "SistemaDocente.Presentation", nombre));

    private static string ObtenerRaiz() => Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
}
