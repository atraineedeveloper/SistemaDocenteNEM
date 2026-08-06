using System;
using System.Linq;
using System.Windows;
using System.Windows.Resources;

namespace SistemaDocente.App.Wpf.Services
{
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
        /// Cambia el tema de la aplicación.
        /// </summary>
        /// <param name="themeName">Light, Dark o HighContrast.</param>
        public static void ApplyTheme(string themeName)
        {
            if (themeName != Light && themeName != Dark && themeName != HighContrast)
                throw new ArgumentException($"Tema no soportado: {themeName}", nameof(themeName));

            var app = System.Windows.Application.Current;
            if (app == null) return;

            var dictionaries = app.Resources.MergedDictionaries;
            var themeUri = new Uri($"pack://application:,,,/Themes/{themeName}.xaml", UriKind.Absolute);

            // Buscar si ya existe un diccionario de tema en la lista.
            var existing = dictionaries
                .FirstOrDefault(d => d.Source != null && d.Source.ToString().Contains("/Themes/"));

            var newDictionary = new ResourceDictionary { Source = themeUri };

            if (existing != null)
            {
                int index = dictionaries.IndexOf(existing);
                dictionaries.Remove(existing);
                dictionaries.Insert(index, newDictionary);
            }
            else
            {
                // Insertar antes de DesignTokens para que sobreescriba los valores por defecto.
                dictionaries.Insert(0, newDictionary);
            }

            CurrentTheme = themeName;
            ThemeChanged?.Invoke(null, themeName);
        }
    }
}
