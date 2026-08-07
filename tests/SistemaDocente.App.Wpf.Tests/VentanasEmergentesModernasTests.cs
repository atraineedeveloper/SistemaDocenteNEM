using System.IO;
using System.Text.RegularExpressions;

namespace SistemaDocente.App.Wpf.Tests;

public sealed class VentanasEmergentesModernasTests
{
    private static readonly string[] Ventanas =
    [
        "src/SistemaDocente.App.Wpf/EditorEstudianteWindow.xaml",
        "src/SistemaDocente.App.Wpf/DetalleProyectoWindow.xaml",
        "src/SistemaDocente.App.Wpf/DetalleActividadWindow.xaml",
        "src/SistemaDocente.App.Wpf/ExpedienteEstudianteWindow.xaml",
        "src/SistemaDocente.App.Wpf/EditarEvaluacionCeldaWindow.xaml",
        "src/SistemaDocente.App.Wpf/DialogoMensajeWindow.xaml",
    ];

    private static string ObtenerRaiz() => Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static string Leer(string rutaRelativa) => File.ReadAllText(Path.Combine(
        ObtenerRaiz(), rutaRelativa.Replace('/', Path.DirectorySeparatorChar)));

    [Fact]
    public void FormFieldConservaChromeYExponeContenidoPropio()
    {
        var xaml = Leer("src/SistemaDocente.App.Wpf/Controls/FormField.xaml");
        var code = Leer("src/SistemaDocente.App.Wpf/Controls/FormField.xaml.cs");

        Assert.Contains("HeaderText", xaml, StringComparison.Ordinal);
        Assert.Contains("FieldContentPresenter", xaml, StringComparison.Ordinal);
        Assert.Contains("Content=\"{Binding FieldContent, ElementName=RootFormField}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("[ContentProperty(nameof(FieldContent))]", code, StringComparison.Ordinal);
        Assert.Contains("FieldContentProperty", code, StringComparison.Ordinal);
    }

    [Theory]
    [MemberData(nameof(ObtenerVentanas))]
    public void VentanasCompartenEstilosYRecursosSemanticos(string ruta)
    {
        var xaml = Leer(ruta);

        Assert.Contains("Styles/PopupStyles.xaml", xaml, StringComparison.Ordinal);
        Assert.Contains("WindowStartupLocation=\"CenterOwner\"", xaml, StringComparison.Ordinal);
        Assert.Contains("ShowInTaskbar=\"False\"", xaml, StringComparison.Ordinal);
        Assert.False(
            Regex.IsMatch(xaml, "#[0-9A-Fa-f]{6,8}", RegexOptions.CultureInvariant),
            $"{ruta} no debe contener colores hexadecimales locales.");
    }

    [Fact]
    public void EditorEstudianteMuestraLabelsVisiblesYAtajoDeGuardado()
    {
        var xaml = Leer("src/SistemaDocente.App.Wpf/EditorEstudianteWindow.xaml");

        Assert.Contains("Header=\"Primer apellido *\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Header=\"Segundo apellido\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Header=\"Nombre(s) *\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Header=\"Núm. de lista *\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Header=\"Género *\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Header=\"Fecha de nacimiento *\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Style=\"{StaticResource PopupDatePicker}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Modifiers=\"Control\" Key=\"S\"", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("<TextBox.InputBindings>", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void ProyectoYActividadSeparanGuardarDeAccionesDeCicloDeVida()
    {
        var proyecto = Leer("src/SistemaDocente.App.Wpf/DetalleProyectoWindow.xaml");
        var actividad = Leer("src/SistemaDocente.App.Wpf/DetalleActividadWindow.xaml");

        Assert.Contains("Content=\"Guardar cambios\"", proyecto, StringComparison.Ordinal);
        Assert.Contains("Content=\"Eliminar borrador\"", proyecto, StringComparison.Ordinal);
        Assert.Contains("PopupDestructiveButton", proyecto, StringComparison.Ordinal);
        Assert.Contains("Content=\"Guardar actividad\"", actividad, StringComparison.Ordinal);
        Assert.Contains("Content=\"Eliminar sin seguimiento\"", actividad, StringComparison.Ordinal);
        Assert.Contains("PopupDestructiveButton", actividad, StringComparison.Ordinal);
    }

    [Fact]
    public void ExpedienteEtiquetaCapturasYUsaTabsCompartidos()
    {
        var xaml = Leer("src/SistemaDocente.App.Wpf/ExpedienteEstudianteWindow.xaml");

        Assert.Contains("ItemContainerStyle=\"{StaticResource PopupTabItem}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Header=\"Nueva fortaleza\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Header=\"Nueva dificultad\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Header=\"Nuevo apoyo aplicado\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Header=\"Nueva observación\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Header=\"Motivo de la reunión *\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Header=\"Acuerdo o compromiso convenido *\"", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void DialogosPequenosTienenAccionPrimariaClara()
    {
        var mensaje = Leer("src/SistemaDocente.App.Wpf/DialogoMensajeWindow.xaml");
        var evaluacion = Leer("src/SistemaDocente.App.Wpf/EditarEvaluacionCeldaWindow.xaml");

        Assert.Contains("x:Name=\"BtnAfirmativo\"", mensaje, StringComparison.Ordinal);
        Assert.Contains("Style=\"{StaticResource PrimaryButton}\"", mensaje, StringComparison.Ordinal);
        Assert.Contains("Content=\"Aplicar a la matriz\"", evaluacion, StringComparison.Ordinal);
        Assert.Contains("Header=\"Nivel de logro *\"", evaluacion, StringComparison.Ordinal);
        Assert.Contains("Header=\"Observación pedagógica\"", evaluacion, StringComparison.Ordinal);
    }

    public static TheoryData<string> ObtenerVentanas()
    {
        var data = new TheoryData<string>();
        foreach (var ruta in Ventanas)
        {
            data.Add(ruta);
        }

        return data;
    }
}
