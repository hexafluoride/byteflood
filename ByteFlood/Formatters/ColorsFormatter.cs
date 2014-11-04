using System;
using System.Windows.Data;
using System.Windows.Media;

namespace ByteFlood.Formatters
{
    public class ForegroundColorProvider : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Color c = (Color)value;
            double luminance = 0.2126 * c.ScR + 0.7152 * c.ScG + 0.0722 * c.ScB;
            if (luminance < 0.5)
            {
                return Brushes.White;
            }
            else
            {
                return Brushes.Black;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
