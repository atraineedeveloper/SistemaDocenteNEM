using System.IO;
using System.Threading;
using System.Windows;

using SistemaDocente.Application;
using SistemaDocente.Core;
using SistemaDocente.Data;
using SistemaDocente.Interchange;
using SistemaDocente.Presentation;

namespace SistemaDocente.App.Wpf.Tests;

public sealed class ExportacionGrupoWindowConstructionTests
{
    [Fact]
    public void AsistenteExportacionPuedeConstruirseYEmpiezaEnContenido()
    {
        Exception? capturada = null;
        Visibility? contenido = null;
        Visibility? alcance = null;
        Visibility? archivo = null;
        Visibility? resultado = null;

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
                    "SistemaDocenteNEM-ExportConstruction-" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(directorio);
                var baseSqlite = Path.Combine(directorio, "sistema-docente.db");
                var grupos = new PersistenciaGrupoSqlite(baseSqlite);
                var asistencias = new PersistenciaAsistenciaSqlite(baseSqlite);
                var proyectos = new PersistenciaProyectosSqlite(baseSqlite);
                var expedientes = new PersistenciaExpedienteSqlite(baseSqlite);
                var contextos = new PersistenciaContextoGrupoSqlite(baseSqlite);

                var grupo = Grupo.Crear("4.º A");
                grupos.Guardar(grupo);
                contextos.Guardar(ContextoGrupo.Crear(
                    grupo.Id,
                    cicloEscolar: "2026-2027",
                    grupo: "A",
                    gradosAtendidos: [GradoPrimaria.Cuarto]));

                var viewModel = new ExportacionGrupoViewModel(
                    new ExportacionGrupoCasosUso(
                        grupos,
                        asistencias,
                        proyectos,
                        proyectos,
                        expedientes,
                        contextos,
                        new ExportadorTabularArchivo()),
                    new ConsultaExportacionGrupoCasosUso(proyectos));
                viewModel.Inicializar(grupo.Id, new DateOnly(2026, 8, 8));

                var ventana = new ExportacionGrupoWindow(viewModel);
                ventana.Measure(new Size(940, 720));
                ventana.Arrange(new Rect(0, 0, 940, 720));
                ventana.UpdateLayout();

                contenido = ((FrameworkElement)ventana.FindName("PasoContenidoPanel")!).Visibility;
                alcance = ((FrameworkElement)ventana.FindName("PasoAlcancePanel")!).Visibility;
                archivo = ((FrameworkElement)ventana.FindName("PasoArchivoPanel")!).Visibility;
                resultado = ((FrameworkElement)ventana.FindName("PasoResultadoPanel")!).Visibility;
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
        Assert.Equal(Visibility.Visible, contenido);
        Assert.Equal(Visibility.Collapsed, alcance);
        Assert.Equal(Visibility.Collapsed, archivo);
        Assert.Equal(Visibility.Collapsed, resultado);
    }
}