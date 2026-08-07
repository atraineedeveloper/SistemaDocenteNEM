using System.IO;

using SistemaDocente.App.Wpf;

namespace SistemaDocente.App.Wpf.Tests;

public sealed class ModernizacionUiEvaluacionDemoTests
{
    private static string ObtenerRaiz() => Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static string Leer(string rutaRelativa) => File.ReadAllText(Path.Combine(
        ObtenerRaiz(), rutaRelativa.Replace('/', Path.DirectorySeparatorChar)));

    [Fact]
    public void EvaluacionUsaMatrizYEliminaSelectorDeActividad()
    {
        var xaml = Leer("src/SistemaDocente.App.Wpf/Views/EvaluacionView.xaml");
        var code = Leer("src/SistemaDocente.App.Wpf/Views/EvaluacionView.xaml.cs");

        Assert.Contains("GrillaEvaluacionMatriz", xaml, StringComparison.Ordinal);
        Assert.Contains("FrozenColumnCount=\"2\"", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("SelectedItem=\"{Binding ActividadSeleccionada", xaml, StringComparison.Ordinal);
        Assert.Contains("ColumnasActividades", code, StringComparison.Ordinal);
        Assert.Contains("Celdas[{indice}].EtiquetaNivel", code, StringComparison.Ordinal);
        Assert.Contains("ToolTip = actividad.DescripcionAccesible", code, StringComparison.Ordinal);
    }

    [Fact]
    public void EvaluacionMantieneAtajosContextualesYSeparaEntregaDeLogro()
    {
        var xaml = Leer("src/SistemaDocente.App.Wpf/Views/EvaluacionView.xaml");
        var code = Leer("src/SistemaDocente.App.Wpf/Views/EvaluacionView.xaml.cs");
        var editor = Leer("src/SistemaDocente.App.Wpf/EditarEvaluacionCeldaWindow.xaml");

        Assert.Contains("GrillaEvaluacionMatriz.IsAncestorOf(foco)", code, StringComparison.Ordinal);
        Assert.Contains("Keyboard.FocusedElement is TextBoxBase", code, StringComparison.Ordinal);
        Assert.Contains("Key.D", code, StringComparison.Ordinal);
        Assert.Contains("Key.S", code, StringComparison.Ordinal);
        Assert.Contains("Key.E", code, StringComparison.Ordinal);
        Assert.Contains("Key.R", code, StringComparison.Ordinal);
        Assert.Contains("Key.T", code, StringComparison.Ordinal);
        Assert.Contains("Key.N", code, StringComparison.Ordinal);
        Assert.Contains("Key.P", code, StringComparison.Ordinal);
        Assert.Contains("Key.Enter", code, StringComparison.Ordinal);
        Assert.Contains("T/N/P = captura rápida", xaml, StringComparison.Ordinal);
        Assert.Contains("MarcarTodosEntregadaCommand", xaml, StringComparison.Ordinal);
        Assert.Contains("MarcarTodosNoEntregadaCommand", xaml, StringComparison.Ordinal);
        Assert.Contains("No entregó (N)", xaml, StringComparison.Ordinal);
        Assert.Contains("OpcionesResultado", editor, StringComparison.Ordinal);
        Assert.Contains("SelectedValue=\"{Binding Resultado", editor, StringComparison.Ordinal);
        Assert.Contains("actualiza automáticamente el estado de entrega", editor, StringComparison.Ordinal);
        Assert.DoesNotContain("SelectedValue=\"{Binding EstadoEntrega", editor, StringComparison.Ordinal);
        Assert.DoesNotContain("IsEnabled=\"{Binding PuedeEvaluarLogro}\"", editor, StringComparison.Ordinal);
        Assert.Contains("Más opciones…", code, StringComparison.Ordinal);
    }

    [Fact]
    public void GrupoProyectosYReportesCompartenConfiguracionContextual()
    {
        var main = Leer("src/SistemaDocente.App.Wpf/MainWindow.xaml");
        var grupo = Leer("src/SistemaDocente.App.Wpf/Views/GrupoView.xaml");
        var grupoCode = Leer("src/SistemaDocente.App.Wpf/Views/GrupoView.xaml.cs");
        var reportesCode = Leer("src/SistemaDocente.App.Wpf/Views/ReportesView.xaml.cs");
        const string binding = "Configuracion=\"{Binding ConfiguracionGrupo, ElementName=RootWindow}\"";

        Assert.Equal(3, main.Split(binding, StringSplitOptions.None).Length - 1);
        Assert.Contains("⚙  Configurar grupo", grupo, StringComparison.Ordinal);
        Assert.Contains("OnConfigurarGrupoClic", grupo, StringComparison.Ordinal);
        Assert.Contains("ConfiguracionGrupoWindow", grupoCode, StringComparison.Ordinal);
        Assert.Contains("ConfiguracionGrupoWindow", reportesCode, StringComparison.Ordinal);
    }

    [Fact]
    public void HeaderNoIntroduceSidebarYDistingueModoDemo()
    {
        var header = Leer("src/SistemaDocente.App.Wpf/Controls/MainNavigationHeader.xaml");
        var main = Leer("src/SistemaDocente.App.Wpf/MainWindow.xaml");

        Assert.Contains("ModoDemostracion", header, StringComparison.Ordinal);
        Assert.Contains("DEMO", header, StringComparison.Ordinal);
        Assert.Contains("CardBackgroundBrush", header, StringComparison.Ordinal);
        Assert.DoesNotContain("SideBar", main, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("NavigationView", main, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GrupoYProyectosTienenJerarquiaYAccionesPrimariasModernizadas()
    {
        var grupo = Leer("src/SistemaDocente.App.Wpf/Views/GrupoView.xaml");
        var proyectos = Leer("src/SistemaDocente.App.Wpf/Views/ProyectosView.xaml");

        Assert.Contains("LISTA DE ESTUDIANTES", grupo, StringComparison.Ordinal);
        Assert.Contains("Iniciales", grupo, StringComparison.Ordinal);
        Assert.Contains("＋  Agregar estudiante", grupo, StringComparison.Ordinal);
        Assert.Contains("CollectionMetric", grupo, StringComparison.Ordinal);

        Assert.Contains("PLANEACIÓN DIDÁCTICA", proyectos, StringComparison.Ordinal);
        Assert.Contains("Buscar proyecto", proyectos, StringComparison.Ordinal);
        Assert.Contains("Abrir proyecto", proyectos, StringComparison.Ordinal);
        Assert.DoesNotContain("Ver / Editar Detalle del Proyecto", proyectos, StringComparison.Ordinal);
    }

    [Fact]
    public void RutasDemoSonAisladasDeProduccion()
    {
        var baseTemporal = Path.Combine(Path.GetTempPath(), "SistemaDocenteNEM-Rutas-" + Guid.NewGuid().ToString("N"));
        var produccion = RutasAplicacion.DesdeLocalApplicationData(baseTemporal);
        var demo = RutasAplicacion.DesdeLocalApplicationData(baseTemporal, true);

        Assert.False(produccion.EsDemostracion);
        Assert.True(demo.EsDemostracion);
        Assert.NotEqual(produccion.BaseSqlite, demo.BaseSqlite);
        Assert.NotEqual(produccion.EstadoAplicacion, demo.EstadoAplicacion);
        Assert.Contains("SistemaDocenteNEM-Demo", demo.BaseSqlite, StringComparison.Ordinal);
        Assert.DoesNotContain("SistemaDocenteNEM-Demo", produccion.BaseSqlite, StringComparison.Ordinal);
    }

    [Fact]
    public void ReinicioDemoNoTocaArchivosDeProduccion()
    {
        var baseTemporal = Path.Combine(Path.GetTempPath(), "SistemaDocenteNEM-Reset-" + Guid.NewGuid().ToString("N"));
        var produccion = RutasAplicacion.DesdeLocalApplicationData(baseTemporal);
        var demo = RutasAplicacion.DesdeLocalApplicationData(baseTemporal, true);
        Directory.CreateDirectory(Path.GetDirectoryName(produccion.BaseSqlite)!);
        Directory.CreateDirectory(Path.GetDirectoryName(demo.BaseSqlite)!);
        File.WriteAllText(produccion.BaseSqlite, "produccion");
        File.WriteAllText(demo.BaseSqlite, "demo");
        File.WriteAllText(demo.BaseSqlite + "-wal", "wal");
        File.WriteAllText(demo.BaseSqlite + "-shm", "shm");
        File.WriteAllText(demo.EstadoAplicacion, "estado");

        demo.ReiniciarDemostracion();

        Assert.True(File.Exists(produccion.BaseSqlite));
        Assert.False(File.Exists(demo.BaseSqlite));
        Assert.False(File.Exists(demo.BaseSqlite + "-wal"));
        Assert.False(File.Exists(demo.BaseSqlite + "-shm"));
        Assert.False(File.Exists(demo.EstadoAplicacion));
    }

    [Fact]
    public void ArranqueReconoceDemoYDemoReset()
    {
        var app = Leer("src/SistemaDocente.App.Wpf/App.xaml.cs");
        var seeder = Leer("src/SistemaDocente.App.Wpf/Demo/DemoDataSeeder.cs");
        var contexto = Leer("src/SistemaDocente.App.Wpf/Demo/DemoContextSeeder.cs");

        Assert.Contains("--demo", app, StringComparison.Ordinal);
        Assert.Contains("--demo-reset", app, StringComparison.Ordinal);
        Assert.Contains("DemoDataSeeder.AsegurarDatos", app, StringComparison.Ordinal);
        Assert.Contains("DemoContextSeeder.AsegurarContexto", app, StringComparison.Ordinal);
        Assert.Contains("4.º A · Demostración", seeder, StringComparison.Ordinal);
        Assert.Contains("31", seeder, StringComparison.Ordinal);
        Assert.Contains("EstadoAsistencia.Falta", seeder, StringComparison.Ordinal);
        Assert.Contains("NivelLogro.RequiereApoyo", seeder, StringComparison.Ordinal);
        Assert.Contains("MetodologiaProyectoNem.ProyectosComunitarios", seeder, StringComparison.Ordinal);
        Assert.Contains("CampoFormativoNem.SaberesPensamientoCientifico", seeder, StringComparison.Ordinal);
        Assert.Contains("grado: GradoPrimaria.Quinto", seeder, StringComparison.Ordinal);
        Assert.Contains("GradoPrimaria.Cuarto", contexto, StringComparison.Ordinal);
        Assert.Contains("OrganizacionEscolar.Completa", contexto, StringComparison.Ordinal);
        Assert.Contains("EtapaDesarrolloCognoscitivo.NoEspecificada", contexto, StringComparison.Ordinal);
        Assert.DoesNotContain("OperacionesConcretas", contexto, StringComparison.Ordinal);
    }
}