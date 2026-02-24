using System.Globalization;

namespace VibeLink.App.Helpers;

/// <summary>
/// Convierte la categoría seleccionada a un color para los filtros.
/// Activo = color de la categoría, inactivo = color de Surface.
/// </summary>
public class CategoryColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var selectedCategory = value?.ToString() ?? "All";
        var buttonCategory = parameter?.ToString() ?? "All";

        bool isActive = selectedCategory == buttonCategory;

        if (isActive)
        {
            return buttonCategory switch
            {
                "Movies" => Color.FromArgb("#e94560"),
                "Series" => Color.FromArgb("#00d4ff"),
                "Games" => Color.FromArgb("#2ed573"),
                _ => Color.FromArgb("#e94560") // All = Primary
            };
        }

        return Color.FromArgb("#1e2a45"); // Surface / Inactive
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
