using System.Globalization;

using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;

using PdfSharp.Fonts;

using SistemaDocente.Application;
using SistemaDocente.Core;
using SistemaDocente.Reporting;

namespace SistemaDocente.Interchange;

public sealed class ExportadorReportesPdf : IExportadorReportesPdf
{
    private const string FamiliaFuente = "Segoe UI";
    private static readonly object SincronizacionFuentes = new();
    private static readonly CultureInfo CulturaEsMx = CultureInfo.GetCultureInfo("es-MX");
    private static bool _fuentesInicializadas;

    public void Exportar(ReporteIndividualAlumno reporte, string rutaArchivo)
    {
        ArgumentNullException.ThrowIfNull(reporte);
        ExportarDocumento(
            CrearDocumentoIndividual(reporte),
            rutaArchivo,
            $"Reporte individual - {reporte.Nombre}");
    }

    public void Exportar(ReporteGrupal reporte, string rutaArchivo)
    {
        ArgumentNullException.ThrowIfNull(reporte);
        ExportarDocumento(
            CrearDocumentoGrupal(reporte),
            rutaArchivo,
            $"Reporte grupal - {reporte.NombreGrupo}");
    }

    private static void ExportarDocumento(Document documento, string rutaArchivo, string titulo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rutaArchivo);
        AsegurarFuentes();

        var destino = Path.GetFullPath(rutaArchivo);
        var directorio = Path.GetDirectoryName(destino)
            ?? throw new ExportacionReportePdfException("No fue posible determinar la carpeta de destino del PDF.");
        Directory.CreateDirectory(directorio);
        var temporal = Path.Combine(
            directorio,
            $".{Path.GetFileName(destino)}.{Guid.NewGuid():N}.tmp.pdf");

        try
        {
            documento.Info.Title = titulo;
            documento.Info.Subject = IdentidadProducto.Subtitulo;
            documento.Info.Author = IdentidadProducto.Nombre;

            var renderer = new PdfDocumentRenderer
            {
                Document = documento,
            };
            renderer.RenderDocument();
            renderer.PdfDocument.Save(temporal);
            File.Move(temporal, destino, true);
        }
        catch (Exception exception) when (
            exception is IOException
                or UnauthorizedAccessException
                or InvalidOperationException
                or NotSupportedException
                or ArgumentException)
        {
            throw new ExportacionReportePdfException(
                "No fue posible generar el reporte PDF. Verifica la carpeta de destino e intenta nuevamente.",
                exception);
        }
        finally
        {
            if (File.Exists(temporal))
            {
                try
                {
                    File.Delete(temporal);
                }
                catch
                {
                    // La limpieza del temporal no debe ocultar el resultado principal.
                }
            }
        }
    }

    private static Document CrearDocumentoIndividual(ReporteIndividualAlumno reporte)
    {
        var documento = CrearDocumentoBase("Reporte individual");
        var seccion = documento.Sections[0];
        AgregarTitulo(seccion, "Reporte individual", reporte.Nombre);
        AgregarContexto(seccion, reporte.Contexto, reporte.NombreGrupo);

        var identidad = seccion.AddTable();
        ConfigurarTablaSinBordes(identidad, 6, 12);
        AgregarFilaClaveValor(identidad, "Estudiante", reporte.Nombre);
        AgregarFilaClaveValor(identidad, "Número de lista", reporte.NumeroLista.ToString(CultureInfo.InvariantCulture));
        AgregarFilaClaveValor(identidad, "Estado", reporte.EstaActivo ? "Activo" : "Inactivo");
        AgregarFilaClaveValor(identidad, "Edad", reporte.Edad?.ToString(CultureInfo.InvariantCulture) ?? "—");
        AgregarSeparacion(seccion, 8);

        AgregarMetricasIndividuales(seccion, reporte);
        AgregarAsistenciaMensual(seccion, reporte.AsistenciaMensual);
        AgregarDistribucionLogro(seccion, reporte.Logro);
        AgregarActividades(seccion, reporte.Actividades);
        AgregarLista(seccion, "Fortalezas", reporte.Fortalezas);
        AgregarLista(seccion, "Aspectos a fortalecer", reporte.Dificultades);
        AgregarLista(seccion, "Apoyos aplicados", reporte.Apoyos);
        AgregarLista(seccion, "Acuerdos con tutores", reporte.Acuerdos);
        AgregarLista(seccion, "Observaciones recientes", reporte.Observaciones);
        return documento;
    }

    private static Document CrearDocumentoGrupal(ReporteGrupal reporte)
    {
        var documento = CrearDocumentoBase("Reporte grupal");
        var seccion = documento.Sections[0];
        AgregarTitulo(seccion, "Reporte grupal", reporte.NombreGrupo);
        AgregarContexto(seccion, reporte.Contexto, reporte.NombreGrupo);
        AgregarMetricasGrupales(seccion, reporte);
        AgregarAsistenciaMensual(seccion, reporte.AsistenciaMensual);
        AgregarDistribucionLogro(seccion, reporte.Logro);
        AgregarSeguimientoGrupal(seccion, reporte.Seguimiento);
        return documento;
    }

    private static Document CrearDocumentoBase(string tipoReporte)
    {
        var documento = new Document();
        var normal = documento.Styles[StyleNames.Normal];
        normal.Font.Name = FamiliaFuente;
        normal.Font.Size = Unit.FromPoint(9.5);
        normal.ParagraphFormat.SpaceAfter = Unit.FromPoint(3);

        var heading1 = documento.Styles[StyleNames.Heading1];
        heading1.Font.Name = FamiliaFuente;
        heading1.Font.Size = Unit.FromPoint(18);
        heading1.Font.Bold = true;
        heading1.ParagraphFormat.SpaceAfter = Unit.FromPoint(4);

        var heading2 = documento.Styles[StyleNames.Heading2];
        heading2.Font.Name = FamiliaFuente;
        heading2.Font.Size = Unit.FromPoint(12);
        heading2.Font.Bold = true;
        heading2.ParagraphFormat.SpaceBefore = Unit.FromPoint(10);
        heading2.ParagraphFormat.SpaceAfter = Unit.FromPoint(6);

        var seccion = documento.AddSection();
        seccion.PageSetup.PageFormat = PageFormat.A4;
        seccion.PageSetup.TopMargin = Unit.FromCentimeter(1.5);
        seccion.PageSetup.BottomMargin = Unit.FromCentimeter(1.6);
        seccion.PageSetup.LeftMargin = Unit.FromCentimeter(1.5);
        seccion.PageSetup.RightMargin = Unit.FromCentimeter(1.5);

        var encabezado = seccion.Headers.Primary.AddParagraph();
        encabezado.Format.Font.Name = FamiliaFuente;
        encabezado.Format.Font.Size = Unit.FromPoint(8);
        encabezado.Format.Font.Bold = true;
        encabezado.AddText($"{IdentidadProducto.Nombre} · {tipoReporte}");

        var pie = seccion.Footers.Primary.AddParagraph();
        pie.Format.Font.Name = FamiliaFuente;
        pie.Format.Font.Size = Unit.FromPoint(7.5);
        pie.Format.Alignment = ParagraphAlignment.Center;
        pie.AddText($"Generado por {IdentidadProducto.Nombre} · ");
        pie.AddPageField();
        pie.AddText(" / ");
        pie.AddNumPagesField();

        return documento;
    }

    private static void AgregarTitulo(Section seccion, string tipo, string sujeto)
    {
        var marca = seccion.AddParagraph();
        marca.Format.Font.Size = Unit.FromPoint(10);
        marca.Format.Font.Bold = true;
        marca.AddText(IdentidadProducto.Nombre);

        var subtituloMarca = seccion.AddParagraph(IdentidadProducto.Subtitulo);
        subtituloMarca.Format.Font.Size = Unit.FromPoint(8.5);
        subtituloMarca.Format.SpaceAfter = Unit.FromPoint(8);

        seccion.AddParagraph(tipo, StyleNames.Heading1);
        var sujetoParrafo = seccion.AddParagraph(sujeto);
        sujetoParrafo.Format.Font.Size = Unit.FromPoint(11);
        sujetoParrafo.Format.Font.Bold = true;
        sujetoParrafo.Format.SpaceAfter = Unit.FromPoint(10);
    }

    private static void AgregarContexto(Section seccion, ContextoGrupo contexto, string nombreGrupo)
    {
        AgregarEncabezadoSeccion(seccion, "Contexto escolar");
        var tabla = seccion.AddTable();
        ConfigurarTablaSinBordes(tabla, 5.2, 12.8);
        AgregarFilaClaveValor(tabla, "Escuela", Valor(contexto.NombreEscuela));
        AgregarFilaClaveValor(tabla, "CCT", Valor(contexto.Cct));
        AgregarFilaClaveValor(tabla, "Ciclo escolar", Valor(contexto.CicloEscolar));
        AgregarFilaClaveValor(tabla, "Grupo", Valor(nombreGrupo));
        AgregarFilaClaveValor(tabla, "Grados atendidos", Valor(contexto.GradosTexto));
        AgregarFilaClaveValor(tabla, "Fase(s) NEM", Valor(contexto.FasesNemTexto));
        AgregarFilaClaveValor(tabla, "Turno", Valor(contexto.Turno));
        AgregarFilaClaveValor(tabla, "Docente responsable", Valor(contexto.DocenteResponsable));
        var ubicacion = string.Join(
            ", ",
            new[] { contexto.Localidad, contexto.Municipio, contexto.EntidadFederativa }
                .Where(x => !string.IsNullOrWhiteSpace(x)));
        AgregarFilaClaveValor(tabla, "Ubicación", Valor(ubicacion));
    }

    private static void AgregarMetricasIndividuales(Section seccion, ReporteIndividualAlumno reporte)
    {
        AgregarEncabezadoSeccion(seccion, "Resumen");
        var tabla = seccion.AddTable();
        ConfigurarTablaConBordes(tabla, 4.5, 4.5, 4.5, 4.5);
        var encabezado = tabla.AddRow();
        encabezado.HeadingFormat = true;
        encabezado.Format.Font.Bold = true;
        encabezado.Cells[0].AddParagraph("Asistencia");
        encabezado.Cells[1].AddParagraph("Cumplimiento");
        encabezado.Cells[2].AddParagraph("No entregadas");
        encabezado.Cells[3].AddParagraph("Pendientes");
        var valores = tabla.AddRow();
        valores.Cells[0].AddParagraph(Porcentaje(reporte.PorcentajeAsistencia));
        valores.Cells[1].AddParagraph(Porcentaje(reporte.Cumplimiento.PorcentajeCumplimiento));
        valores.Cells[2].AddParagraph(reporte.Cumplimiento.NoEntregadas.ToString(CultureInfo.InvariantCulture));
        valores.Cells[3].AddParagraph(reporte.Cumplimiento.Pendientes.ToString(CultureInfo.InvariantCulture));
    }

    private static void AgregarMetricasGrupales(Section seccion, ReporteGrupal reporte)
    {
        AgregarEncabezadoSeccion(seccion, "Resumen del grupo");
        var tabla = seccion.AddTable();
        ConfigurarTablaConBordes(tabla, 3.6, 3.6, 3.6, 3.6, 3.6);
        var encabezado = tabla.AddRow();
        encabezado.HeadingFormat = true;
        encabezado.Format.Font.Bold = true;
        encabezado.Cells[0].AddParagraph("Alumnos activos");
        encabezado.Cells[1].AddParagraph("Históricos");
        encabezado.Cells[2].AddParagraph("Asistencia");
        encabezado.Cells[3].AddParagraph("Cumplimiento");
        encabezado.Cells[4].AddParagraph("No entregadas");
        var valores = tabla.AddRow();
        valores.Cells[0].AddParagraph(reporte.AlumnosActivos.ToString(CultureInfo.InvariantCulture));
        valores.Cells[1].AddParagraph(reporte.AlumnosHistoricos.ToString(CultureInfo.InvariantCulture));
        valores.Cells[2].AddParagraph(Porcentaje(reporte.PorcentajeAsistencia));
        valores.Cells[3].AddParagraph(Porcentaje(reporte.Cumplimiento.PorcentajeCumplimiento));
        valores.Cells[4].AddParagraph(reporte.Cumplimiento.NoEntregadas.ToString(CultureInfo.InvariantCulture));
    }

    private static void AgregarAsistenciaMensual(Section seccion, IReadOnlyList<MesAsistenciaReporte> meses)
    {
        AgregarEncabezadoSeccion(seccion, "Asistencia mensual");
        if (meses.Count == 0)
        {
            AgregarSinDatos(seccion);
            return;
        }

        var tabla = seccion.AddTable();
        ConfigurarTablaConBordes(tabla, 3.8, 2.1, 2.1, 2.1, 2.1, 2.1, 3.7);
        AgregarEncabezadoTabla(tabla, "Mes", "Días", "Presentes", "Faltas", "Retardos", "Justificadas", "Asistencia");
        foreach (var mes in meses)
        {
            var fila = tabla.AddRow();
            fila.Cells[0].AddParagraph(new DateTime(mes.Anio, mes.Mes, 1).ToString("MMM yyyy", CulturaEsMx));
            fila.Cells[1].AddParagraph(mes.Dias.ToString(CultureInfo.InvariantCulture));
            fila.Cells[2].AddParagraph(mes.Presentes.ToString(CultureInfo.InvariantCulture));
            fila.Cells[3].AddParagraph(mes.Faltas.ToString(CultureInfo.InvariantCulture));
            fila.Cells[4].AddParagraph(mes.Retardos.ToString(CultureInfo.InvariantCulture));
            fila.Cells[5].AddParagraph(mes.Justificadas.ToString(CultureInfo.InvariantCulture));
            fila.Cells[6].AddParagraph(Porcentaje(mes.Porcentaje));
        }
    }

    private static void AgregarDistribucionLogro(Section seccion, DistribucionLogroReporte logro)
    {
        AgregarEncabezadoSeccion(seccion, "Evaluación formativa");
        var tabla = seccion.AddTable();
        ConfigurarTablaConBordes(tabla, 3.6, 3.6, 3.6, 3.6, 3.6);
        AgregarEncabezadoTabla(tabla, "Domina", "Suficiente", "En proceso", "Requiere apoyo", "Por evaluar");
        var fila = tabla.AddRow();
        fila.Cells[0].AddParagraph(logro.Domina.ToString(CultureInfo.InvariantCulture));
        fila.Cells[1].AddParagraph(logro.Suficiente.ToString(CultureInfo.InvariantCulture));
        fila.Cells[2].AddParagraph(logro.EnProceso.ToString(CultureInfo.InvariantCulture));
        fila.Cells[3].AddParagraph(logro.RequiereApoyo.ToString(CultureInfo.InvariantCulture));
        fila.Cells[4].AddParagraph(logro.Pendientes.ToString(CultureInfo.InvariantCulture));
    }

    private static void AgregarActividades(Section seccion, IReadOnlyList<ActividadReporteFuente> actividades)
    {
        AgregarEncabezadoSeccion(seccion, "Proyectos y actividades");
        if (actividades.Count == 0)
        {
            AgregarSinDatos(seccion);
            return;
        }

        var tabla = seccion.AddTable();
        ConfigurarTablaConBordes(tabla, 4.4, 4.4, 2.4, 3.0, 3.8);
        AgregarEncabezadoTabla(tabla, "Proyecto", "Actividad", "Fecha", "Entrega", "Resultado");
        foreach (var actividad in actividades)
        {
            var fila = tabla.AddRow();
            fila.Cells[0].AddParagraph(Valor(actividad.Proyecto));
            fila.Cells[1].AddParagraph(Valor(actividad.Actividad));
            fila.Cells[2].AddParagraph(actividad.Fecha.ToString("dd/MM/yyyy", CulturaEsMx));
            fila.Cells[3].AddParagraph(FormatearEntrega(actividad.EstadoEntrega));
            fila.Cells[4].AddParagraph(FormatearResultado(actividad.EstadoEntrega, actividad.NivelLogro));
        }
    }

    private static void AgregarSeguimientoGrupal(Section seccion, IReadOnlyList<SeguimientoAlumnoReporte> seguimiento)
    {
        AgregarEncabezadoSeccion(seccion, "Seguimiento del grupo");
        if (seguimiento.Count == 0)
        {
            AgregarSinDatos(seccion);
            return;
        }

        var tabla = seccion.AddTable();
        ConfigurarTablaConBordes(tabla, 1.6, 6.2, 2.5, 3.0, 3.0, 1.7);
        AgregarEncabezadoTabla(tabla, "Núm.", "Alumno", "Estado", "Asistencia", "Cumplimiento", "Apoyo");
        foreach (var alumno in seguimiento)
        {
            var fila = tabla.AddRow();
            fila.Cells[0].AddParagraph(alumno.NumeroLista.ToString(CultureInfo.InvariantCulture));
            fila.Cells[1].AddParagraph(Valor(alumno.Nombre));
            fila.Cells[2].AddParagraph(alumno.EstaActivo ? "Activo" : "Inactivo");
            fila.Cells[3].AddParagraph(Porcentaje(alumno.PorcentajeAsistencia));
            fila.Cells[4].AddParagraph(Porcentaje(alumno.Cumplimiento.PorcentajeCumplimiento));
            fila.Cells[5].AddParagraph(alumno.RequiereApoyo.ToString(CultureInfo.InvariantCulture));
        }
    }

    private static void AgregarLista(Section seccion, string titulo, IReadOnlyList<string> elementos)
    {
        AgregarEncabezadoSeccion(seccion, titulo);
        var filtrados = elementos.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
        if (filtrados.Length == 0)
        {
            AgregarSinDatos(seccion);
            return;
        }

        foreach (var elemento in filtrados)
        {
            var parrafo = seccion.AddParagraph();
            parrafo.Format.LeftIndent = Unit.FromCentimeter(0.3);
            parrafo.Format.FirstLineIndent = Unit.FromCentimeter(-0.2);
            parrafo.AddText($"- {elemento.Trim()}");
        }
    }

    private static void AgregarEncabezadoSeccion(Section seccion, string titulo) =>
        seccion.AddParagraph(titulo, StyleNames.Heading2);

    private static void AgregarSinDatos(Section seccion)
    {
        var parrafo = seccion.AddParagraph("Sin registros disponibles.");
        parrafo.Format.Font.Italic = true;
    }

    private static void AgregarSeparacion(Section seccion, double puntos)
    {
        var parrafo = seccion.AddParagraph();
        parrafo.Format.SpaceAfter = Unit.FromPoint(puntos);
    }

    private static void ConfigurarTablaSinBordes(Table tabla, params double[] anchosCm)
    {
        foreach (var ancho in anchosCm)
        {
            tabla.AddColumn(Unit.FromCentimeter(ancho));
        }
        tabla.Format.Font.Size = Unit.FromPoint(8.8);
    }

    private static void ConfigurarTablaConBordes(Table tabla, params double[] anchosCm)
    {
        ConfigurarTablaSinBordes(tabla, anchosCm);
        tabla.Borders.Width = Unit.FromPoint(0.5);
    }

    private static void AgregarEncabezadoTabla(Table tabla, params string[] encabezados)
    {
        var fila = tabla.AddRow();
        fila.HeadingFormat = true;
        fila.Format.Font.Bold = true;
        for (var indice = 0; indice < encabezados.Length; indice++)
        {
            fila.Cells[indice].AddParagraph(encabezados[indice]);
        }
    }

    private static void AgregarFilaClaveValor(Table tabla, string clave, string valor)
    {
        var fila = tabla.AddRow();
        fila.Cells[0].Format.Font.Bold = true;
        fila.Cells[0].AddParagraph(clave);
        fila.Cells[1].AddParagraph(valor);
    }

    private static string Porcentaje(double? valor) =>
        valor.HasValue
            ? valor.Value.ToString("0.0", CulturaEsMx) + "%"
            : "—";

    private static string Valor(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? "—" : valor.Trim();

    private static string FormatearEntrega(EstadoEntregaActividad estado) => estado switch
    {
        EstadoEntregaActividad.Pendiente => "Pendiente",
        EstadoEntregaActividad.Entregada => "Entregada",
        EstadoEntregaActividad.NoEntregada => "No entregada",
        _ => "—",
    };

    private static string FormatearResultado(EstadoEntregaActividad entrega, NivelLogro nivel)
    {
        if (entrega == EstadoEntregaActividad.NoEntregada) return "No entregada";
        if (entrega == EstadoEntregaActividad.Pendiente) return "Pendiente";
        return nivel switch
        {
            NivelLogro.Domina => "Domina",
            NivelLogro.Suficiente => "Suficiente",
            NivelLogro.EnProceso => "En proceso",
            NivelLogro.RequiereApoyo => "Requiere apoyo",
            _ => "Pendiente de evaluación",
        };
    }

    private static void AsegurarFuentes()
    {
        if (_fuentesInicializadas) return;
        lock (SincronizacionFuentes)
        {
            if (_fuentesInicializadas) return;
            GlobalFontSettings.FontResolver = new FuenteWindowsResolver();
            PredefinedFontsAndChars.ErrorFontName = FamiliaFuente;
            _fuentesInicializadas = true;
        }
    }

    private sealed class FuenteWindowsResolver : IFontResolver
    {
        private const string CaraRegular = "AulaRaizSegoeUIRegular";
        private const string CaraNegrita = "AulaRaizSegoeUIBold";
        private readonly byte[] _regular;
        private readonly byte[]? _negrita;

        public FuenteWindowsResolver()
        {
            var fuentes = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
            if (string.IsNullOrWhiteSpace(fuentes))
            {
                throw new ExportacionReportePdfException("No fue posible localizar las fuentes de Windows necesarias para generar el PDF.");
            }

            var rutaRegular = Path.Combine(fuentes, "segoeui.ttf");
            var rutaNegrita = Path.Combine(fuentes, "segoeuib.ttf");
            if (!File.Exists(rutaRegular))
            {
                throw new ExportacionReportePdfException("No fue posible localizar Segoe UI en Windows para generar el PDF.");
            }

            _regular = File.ReadAllBytes(rutaRegular);
            _negrita = File.Exists(rutaNegrita) ? File.ReadAllBytes(rutaNegrita) : null;
        }

        public FontResolverInfo? ResolveTypeface(string familyName, bool bold, bool italic)
        {
            if (bold && _negrita is not null)
            {
                return new FontResolverInfo(CaraNegrita, mustSimulateBold: false, mustSimulateItalic: italic);
            }

            return new FontResolverInfo(CaraRegular, mustSimulateBold: bold, mustSimulateItalic: italic);
        }

        public byte[]? GetFont(string faceName) => faceName switch
        {
            CaraRegular => _regular,
            CaraNegrita when _negrita is not null => _negrita,
            _ => null,
        };
    }
}