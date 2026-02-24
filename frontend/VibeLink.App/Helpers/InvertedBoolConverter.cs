using System.Globalization;

namespace VibeLink.App.Helpers;

/// <summary>
/// Convierte true a false y viceversa.
/// Lo usamos en XAML para: IsEnabled="{Binding IsBusy, Converter={StaticResource InvertedBoolConverter}}"
/// Cuando IsBusy=true, el botón se deshabilita (IsEnabled=false).
/// </summary>
public class InvertedBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool b ? !b : value!;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool b ? !b : value!;
    }
}
