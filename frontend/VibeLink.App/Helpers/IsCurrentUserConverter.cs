using System.Globalization;

namespace VibeLink.App.Helpers;

/// <summary>
/// Converter que compara el UserId del mensaje con el userId del usuario actual.
/// Se usa para alinear las burbujas del chat a la derecha (enviado) o izquierda (recibido).
/// ConverterParameter recibe el userId actual.
/// </summary>
public class IsCurrentUserConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int messageUserId && parameter is int currentUserId)
            return messageUserId == currentUserId;
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
