using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SistemaDocente.App.Wpf.Converters;

/// <summary>
/// Devuelve <see cref="Visibility.Visible"/> cuando el conteo es mayor que cero;
/// <see cref="Visibility.Collapsed"/> cuando es cero.
/// Complemento de <see cref="CountToVisibilityConverter"/>.
/// </summary>
[ValueConversion(typeof(int), typeof(Visibility))]
public sealed class InverseCountToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var count = value as int? ?? 0;
        return count > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
