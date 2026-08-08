using System.IO;
using System.Threading;
using System.Windows;

using SistemaDocente.Application;
using SistemaDocente.Data;
using SistemaDocente.Presentation;

namespace SistemaDocente.App.Wpf.Tests;

public sealed class ImportacionEstudiantesWindowConstructionTests
{
    [Fact]
    public void AsistentePuedeConstruirseConRecursosWpfCargados()
    {
        Exception? capturada = null;

        var thread = new Thread(() =>
        {
            try
            {
                if (System.Windows.Application.Current is null)
                {
                    var app = new App();
                    app.InitializeComponent();
                }

                var directorio = Path.Combine(
                    Path.GetTempPath(),
                    "SistemaDocenteNEM-ImportConstruction-" + Guid.NewGuid().ToString("N"));
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
                ventana.Close();
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
    }
}