using System.IO;
using System.Threading;
using System.Windows;

using SistemaDocente.Application;
using SistemaDocente.Presentation;

namespace SistemaDocente.App.Wpf.Tests;

public sealed class RecuperacionLocalWindowTests
{
    [Fact]
    public void VentanaRecuperacionPuedeConstruirseConRecursosReales()
    {
        Exception? capturada = null;
        Visibility? backupVisibility = null;
        Visibility? restoreVisibility = null;
        bool? puedeRestaurar = null;

        var thread = new Thread(() =>
        {
            try
            {
                if (System.Windows.Application.Current is null)
                {
                    var app = new App();
                    app.InitializeComponent();
                }

                var viewModel = new RecuperacionLocalViewModel(
                    new GestionRespaldoCasosUso(new ServicioRecuperacionFalso()));
                var ventana = new RecuperacionLocalWindow(viewModel);
                ventana.Measure(new System.Windows.Size(960, 760));
                ventana.Arrange(new System.Windows.Rect(0, 0, 960, 760));
                ventana.UpdateLayout();

                backupVisibility = ventana.BackupPanel.Visibility;
                restoreVisibility = ventana.RestorePanel.Visibility;
                puedeRestaurar = viewModel.PuedeRestaurar;
            }
            catch (Exception ex)
            {
                capturada = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(capturada);
        Assert.Equal(Visibility.Visible, backupVisibility);
        Assert.Equal(Visibility.Visible, restoreVisibility);
        Assert.False(puedeRestaurar);
    }

    [Fact]
    public void ShellExponeRecuperacionComoAccionGlobalSeparadaDelGrupo()
    {
        var raiz = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        var header = File.ReadAllText(Path.Combine(
            raiz,
            "src",
            "SistemaDocente.App.Wpf",
            "Controls",
            "MainNavigationHeader.xaml"));
        var headerCode = File.ReadAllText(Path.Combine(
            raiz,
            "src",
            "SistemaDocente.App.Wpf",
            "Controls",
            "MainNavigationHeader.xaml.cs"));
        var mainWindow = File.ReadAllText(Path.Combine(
            raiz,
            "src",
            "SistemaDocente.App.Wpf",
            "MainWindow.xaml"));
        var mainCode = File.ReadAllText(Path.Combine(
            raiz,
            "src",
            "SistemaDocente.App.Wpf",
            "MainWindow.xaml.cs"));

        Assert.Contains("Respaldo", header, StringComparison.Ordinal);
        Assert.Contains("RespaldoRestauracion_Click", header, StringComparison.Ordinal);
        Assert.Contains("RecuperacionSolicitada", headerCode, StringComparison.Ordinal);
        Assert.Contains("RecuperacionSolicitada=\"OnRecuperacionSolicitada\"", mainWindow, StringComparison.Ordinal);
        Assert.Contains(nameof(RecuperacionLocalWindow), mainCode, StringComparison.Ordinal);
    }

    private sealed class ServicioRecuperacionFalso : IServicioRecuperacionLocal
    {
        public ModoAlmacenamientoLocal ModoActual => ModoAlmacenamientoLocal.Demostracion;

        public ResultadoRespaldoLocal CrearRespaldo(
            string rutaDestino,
            DateTimeOffset ahoraUtc,
            string versionAplicacion) => throw new NotSupportedException();

        public InspeccionRespaldoLocal Inspeccionar(string rutaRespaldo) =>
            throw new NotSupportedException();

        public ResultadoRestauracionLocal Restaurar(
            string rutaRespaldo,
            DateTimeOffset ahoraUtc,
            string versionAplicacion) => throw new NotSupportedException();
    }
}