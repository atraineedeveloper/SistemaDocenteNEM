using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

using SistemaDocente.App.Wpf;
using SistemaDocente.Application;
using SistemaDocente.Data;
using SistemaDocente.Presentation;

namespace SistemaDocente.App.Wpf.Tests;

public sealed class ImportacionEstudiantesWindowRuntimeTests
{
    [Fact]
    public void EstadoInicialMuestraSoloSeleccionDeArchivoYAccionesValidas()
    {
        Exception? capturada = null;
        Visibility? archivo = null;
        Visibility? resultado = null;
        Visibility? crearPrevia = null;
        Visibility? continuar = null;
        Visibility? importar = null;
        bool? volverHabilitado = null;
        bool? cerrarHabilitado = null;

        var thread = new Thread(() =>
        {
            try
            {
                if (Application.Current is null)
                {
                    var app = new App();
                    app.InitializeComponent();
                }

                var directorio = Path.Combine(
                    Path.GetTempPath(),
                    "SistemaDocenteNEM-ImportWindow-" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(directorio);
                var baseSqlite = Path.Combine(directorio, "sistema-docente.db");
                var grupos = new PersistenciaGrupoSqlite(baseSqlite);
                var contextos = new PersistenciaContextoGrupoSqlite(baseSqlite);
                var viewModel = new ImportacionEstudiantesViewModel(
                    new SistemaDocente.Interchange.LectorImportacionTabular(),
                    new ImportacionEstudiantesCasosUso(grupos, contextos));

                var ventana = new ImportacionEstudiantesWindow(viewModel);
                ventana.Measure(new Size(1080, 780));
                ventana.Arrange(new Rect(0, 0, 1080, 780));
                ventana.UpdateLayout();

                archivo = ((Border)ventana.FindName("PasoArchivoPanel")).Visibility;
                resultado = ((Border)ventana.FindName("PasoResultadoPanel")).Visibility;
                crearPrevia = ((Button)ventana.FindName("CrearPreviaButton")).Visibility;
                continuar = ((Button)ventana.FindName("ContinuarButton")).Visibility;
                importar = ((Button)ventana.FindName("ImportarButton")).Visibility;
                volverHabilitado = ((Button)ventana.FindName("VolverButton")).IsEnabled;
                cerrarHabilitado = ((Button)ventana.FindName("CerrarButton")).IsEnabled;
            }
            catch (Exception exception)
            {
                capturada = exception;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(capturada);
        Assert.Equal(Visibility.Visible, archivo);
        Assert.Equal(Visibility.Collapsed, resultado);
        Assert.Equal(Visibility.Collapsed, crearPrevia);
        Assert.Equal(Visibility.Collapsed, continuar);
        Assert.Equal(Visibility.Collapsed, importar);
        Assert.False(volverHabilitado);
        Assert.True(cerrarHabilitado);
    }
}