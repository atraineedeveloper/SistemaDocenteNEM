using System.IO;

namespace SistemaDocente.App.Wpf.Tests;

public sealed class VentanasEmergentesIntegracionTests
{
    private static string Raiz() => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static string Leer(string archivo) => File.ReadAllText(Path.Combine(Raiz(), "src", "SistemaDocente.App.Wpf", archivo));

    [Fact]
    public void FormFieldConservaEtiquetaYContenidoPropio()
    {
        var xaml = Leer(Path.Combine("Controls", "FormField.xaml"));
        var code = Leer(Path.Combine("Controls", "FormField.xaml.cs"));
        Assert.Contains("FieldContentPresenter", xaml, StringComparison.Ordinal);
        Assert.Contains("FieldContent, ElementName=RootFormField", xaml, StringComparison.Ordinal);
        Assert.Contains("ContentProperty(nameof(FieldContent))", code, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("EditorEstudianteWindow.xaml")]
    [InlineData("DetalleProyectoWindow.xaml")]
    [InlineData("DetalleActividadWindow.xaml")]
    [InlineData("EditarEvaluacionCeldaWindow.xaml")]
    [InlineData("DialogoMensajeWindow.xaml")]
    public void VentanasModernizadasUsanPopupStyles(string archivo)
    {
        Assert.Contains("Styles/PopupStyles.xaml", Leer(archivo), StringComparison.Ordinal);
    }

    [Fact]
    public void EditorEvaluacionExponeResultadoUnificadoYObservacion()
    {
        var xaml = Leer("EditarEvaluacionCeldaWindow.xaml");
        Assert.Contains("OpcionesResultado", xaml, StringComparison.Ordinal);
        Assert.Contains("SelectedValue=\"{Binding Resultado", xaml, StringComparison.Ordinal);
        Assert.Contains("Observación pedagógica", xaml, StringComparison.Ordinal);
        Assert.Contains("Aplicar a la matriz", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("OpcionesEstadoEntrega", xaml, StringComparison.Ordinal);
    }
}