using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace ByteFlood.Formatters
{
    public class SpeedFormatter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double size = System.Convert.ToDouble(value);

            if (size == 0) { return "0 B/s"; }

            return string.Format("{0}/s", Utility.PrettifyAmount(size));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return 0;
        }
    }
}
