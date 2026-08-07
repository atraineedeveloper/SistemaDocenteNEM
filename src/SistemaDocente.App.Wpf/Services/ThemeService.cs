using System.Windows;

namespace SistemaDocente.App.Wpf.Services;

/// <summary>
/// Permite cambiar entre los temas Light, Dark y HighContrast en caliente
/// sin reiniciar la aplicación.
/// </summary>
public static class ThemeService
{
    public const string Light = "Light";
    public const string Dark = "Dark";
    public const string HighContrast = "HighContrast";

    public static string CurrentTheme { get; private set; } = Light;

    public static event EventHandler<string>? ThemeChanged;

    /// <summary>
    /// Cambia el tema de la aplicación. Los tokens base permanecen cargados y el
    /// diccionario de tema se coloca al final para que sus claves tengan precedencia.
    /// </summary>
    public static void ApplyTheme(string themeName)
    {
        if (themeName != Light && themeName != Dark && themeName != HighContrast)
        {
            throw new ArgumentException($"Tema no soportado: {themeName}", nameof(themeName));
        }

        var app = System.Windows.Application.Current;
        if (app is null) return;

        var dictionaries = app.Resources.MergedDictionaries;
        var existingTheme = dictionaries.FirstOrDefault(IsThemeDictionary);
        if (existingTheme is not null)
        {
            dictionaries.Remove(existingTheme);
        }

        var themeUri = new Uri(
            $"pack://application:,,,/Themes/{themeName}.xaml",
            UriKind.Absolute);
        dictionaries.Add(new ResourceDictionary { Source = themeUri });

        CurrentTheme = themeName;
        ThemeChanged?.Invoke(null, themeName);
    }

    private static bool IsThemeDictionary(ResourceDictionary dictionary)
    {
        var source = dictionary.Source?.ToString();
        return source is not null
            && source.Contains("Themes/", StringComparison.OrdinalIgnoreCase)
            && !source.EndsWith("DesignTokens.xaml", StringComparison.OrdinalIgnoreCase);
    }
}