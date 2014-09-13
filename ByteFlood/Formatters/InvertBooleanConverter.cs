using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using System.Text;

namespace ByteFlood.Formatters
{
    // http://stackoverflow.com/a/2580486/3188175
    public class InvertBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool original = (bool)value;
            return !original;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool original = (bool)value;
            return !original;
        }
    }
}
