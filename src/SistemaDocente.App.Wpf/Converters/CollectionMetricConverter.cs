using System.Globalization;
using System.Windows.Data;

using SistemaDocente.Application;
using SistemaDocente.Presentation;
using SistemaDocente.Core;

namespace SistemaDocente.App.Wpf.Converters;

public sealed class CollectionMetricConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var criterio = parameter?.ToString() ?? "Total";

        if (value is IEnumerable<EstudianteVisual> estudiantes)
        {
            var snapshot = estudiantes.ToArray();
            return criterio switch
            {
                "Activos" => snapshot.Count(x => x.EstaActivo),
                "Inactivos" => snapshot.Count(x => !x.EstaActivo),
                _ => snapshot.Length,
            };
        }

        if (value is IEnumerable<ProyectoResumen> proyectos)
        {
            var snapshot = proyectos.ToArray();
            return criterio switch
            {
                "EnCurso" => snapshot.Count(x => x.Estado == EstadoProyecto.EnCurso),
                "Borrador" => snapshot.Count(x => x.Estado == EstadoProyecto.Borrador),
                "Finalizado" => snapshot.Count(x => x.Estado == EstadoProyecto.Finalizado),
                _ => snapshot.Length,
            };
        }

        return 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        Binding.DoNothing;
}