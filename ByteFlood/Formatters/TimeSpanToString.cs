using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace ByteFlood.Formatters
{
    public class TimeSpanToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var t = (TimeSpan)value;
            if (t.TotalSeconds == 0)
            {
                return "-";
            }
            else
            {
                return HMSFormatter.GetReadableTimespan(t);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
