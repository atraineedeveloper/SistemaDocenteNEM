using System.IO;

using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

using SistemaDocente.App.Wpf.Demo;
using SistemaDocente.Application;
using SistemaDocente.Data;
using SistemaDocente.Interchange;

namespace SistemaDocente.App.Wpf.Tests;

public sealed class ExportacionGrupoDemoIntegrationTests
{
    [Fact]
    public void DemoGeneraWorkbookCompletoYCsvAlumnosLegibles()
    {
        var directorio = Path.Combine(
            Path.GetTempPath(),
            "SistemaDocenteNEM-ExportDemo-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directorio);
        var baseSqlite = Path.Combine(directorio, "sistema-docente.db");
        var grupos = new PersistenciaGrupoSqlite(baseSqlite);
        var asistencias = new PersistenciaAsistenciaSqlite(baseSqlite);
        var proyectos = new PersistenciaProyectosSqlite(baseSqlite);
        var expedientes = new PersistenciaExpedienteSqlite(baseSqlite);
        var contextos = new PersistenciaContextoGrupoSqlite(baseSqlite);

        var grupoId = DemoDataSeeder.AsegurarDatos(grupos, asistencias, proyectos, expedientes);
        DemoContextSeeder.AsegurarContexto(contextos, grupoId);
        var casosUso = new ExportacionGrupoCasosUso(
            grupos,
            asistencias,
            proyectos,
            proyectos,
            expedientes,
            contextos,
            new ExportadorTabularArchivo());

        var solicitudXlsx = new SolicitudExportacionGrupo(
            grupoId,
            FormatoExportacionTabular.Xlsx,
            [
                ConjuntoExportacionGrupo.Contexto,
                ConjuntoExportacionGrupo.Alumnos,
                ConjuntoExportacionGrupo.Asistencia,
                ConjuntoExportacionGrupo.Proyectos,
                ConjuntoExportacionGrupo.Actividades,
                ConjuntoExportacionGrupo.Evaluacion,
            ],
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 8, 31));
        var planXlsx = casosUso.Preparar(solicitudXlsx, new DateOnly(2026, 8, 8));
        var rutaXlsx = Path.Combine(directorio, planXlsx.NombreArchivoSugerido);

        var resultadoXlsx = casosUso.Exportar(planXlsx, rutaXlsx);

        Assert.True(File.Exists(resultadoXlsx.RutaArchivo));
        Assert.Equal(6, resultadoXlsx.Conjuntos.Count);
        Assert.Equal(31, resultadoXlsx.Conjuntos.Single(x => x.Conjunto == ConjuntoExportacionGrupo.Alumnos).Filas);
        Assert.True(resultadoXlsx.Conjuntos.Single(x => x.Conjunto == ConjuntoExportacionGrupo.Asistencia).Filas > 100);
        Assert.Equal(3, resultadoXlsx.Conjuntos.Single(x => x.Conjunto == ConjuntoExportacionGrupo.Proyectos).Filas);
        Assert.True(resultadoXlsx.Conjuntos.Single(x => x.Conjunto == ConjuntoExportacionGrupo.Actividades).Filas >= 15);
        Assert.True(resultadoXlsx.Conjuntos.Single(x => x.Conjunto == ConjuntoExportacionGrupo.Evaluacion).Filas > 300);

        using (var spreadsheet = SpreadsheetDocument.Open(rutaXlsx, false))
        {
            var workbookPart = Assert.IsType<WorkbookPart>(spreadsheet.WorkbookPart);
            var workbook = Assert.IsType<Workbook>(workbookPart.Workbook);
            var sheets = Assert.IsType<Sheets>(workbook.Sheets);
            var nombres = sheets
                .Elements<Sheet>()
                .Select(sheet => Assert.IsType<string>(sheet.Name?.Value))
                .ToArray();
            Assert.Equal(
                ["Contexto", "Alumnos", "Asistencia", "Proyectos", "Actividades", "Evaluacion"],
                nombres);
            Assert.Empty(workbookPart.WorksheetParts.SelectMany(
                part => Assert.IsType<Worksheet>(part.Worksheet).Descendants<CellFormula>()));
        }

        var solicitudCsv = new SolicitudExportacionGrupo(
            grupoId,
            FormatoExportacionTabular.Csv,
            [ConjuntoExportacionGrupo.Alumnos]);
        var planCsv = casosUso.Preparar(solicitudCsv, new DateOnly(2026, 8, 8));
        var rutaCsv = Path.Combine(directorio, planCsv.NombreArchivoSugerido);

        casosUso.Exportar(planCsv, rutaCsv);

        var releido = new LectorCsvTabular().Leer(rutaCsv);
        var hoja = Assert.Single(releido.Hojas);
        Assert.Equal(31, hoja.Filas.Count);
        Assert.Equal("Número de lista", hoja.Encabezados[0].Texto);
        Assert.Equal("Nombre", hoja.Encabezados[1].Texto);
        var ximena = Assert.Single(hoja.Filas, fila => fila.Celdas[0].Texto == "31");
        Assert.Equal("Torres", ximena.Celdas[2].Texto);
        Assert.Equal("Vidal", ximena.Celdas[3].Texto);
        Assert.Equal("Ximena", ximena.Celdas[4].Texto);
    }
}