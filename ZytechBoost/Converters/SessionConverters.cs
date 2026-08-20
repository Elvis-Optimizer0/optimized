using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using ZytechBoost.Models;

namespace ZytechBoost.Models;

/// <summary>
/// Converts session running state to a background color.
/// </summary>
public class SessionStatusConverter : IValueConverter
{
    public static readonly SessionStatusConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isRunning && isRunning)
            return Color.FromArgb(40, 59, 130, 246); // Blue tint for running

        return Color.FromArgb(30, 34, 197, 94); // Green tint for completed
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// Converts session running state to a status icon.
/// </summary>
public class SessionIconConverter : IValueConverter
{
    public static readonly SessionIconConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isRunning && isRunning)
            return "🔄";

        return "✅";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
