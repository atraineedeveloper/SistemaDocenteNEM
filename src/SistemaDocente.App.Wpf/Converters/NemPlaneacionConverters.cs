using System.Globalization;
using System.Windows.Data;

using SistemaDocente.Core;

namespace SistemaDocente.App.Wpf.Converters;

public sealed class MetodologiaProyectoNemConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is MetodologiaProyectoNem metodologia
            ? CatalogoPlaneacionNem.FormatearMetodologia(metodologia)
            : "No especificada";

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        Binding.DoNothing;
}

public sealed class CampoFormativoNemConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is CampoFormativoNem campo
            ? CatalogoPlaneacionNem.FormatearCampo(campo)
            : "No especificado";

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        Binding.DoNothing;
}

public sealed class GradosObjetivoConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not IEnumerable<GradoPrimaria> grados)
        {
            return "Sin grados definidos";
        }

        var texto = CatalogoNemPrimaria.FormatearGrados(grados);
        return string.IsNullOrWhiteSpace(texto) ? "Sin grados definidos" : texto;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        Binding.DoNothing;
}