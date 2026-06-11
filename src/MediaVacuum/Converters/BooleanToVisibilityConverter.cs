using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MediaVacuum.Converters;

public class BooleanToVisibilityConverter : IValueConverter
{
    public static readonly BooleanToVisibilityConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            bool invert = parameter?.ToString() == "invert";
            return boolValue ^ invert ? Visibility.Visible : Visibility.Collapsed;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            bool invert = parameter?.ToString() == "invert";
            return (visibility == Visibility.Visible) ^ invert;
        }

        return false;
    }
}
