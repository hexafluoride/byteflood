using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace ByteFlood.Formatters
{
    class SettingsGetter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Settings s = App.Settings;
            string param = parameter.ToString();
            return typeof(Settings).GetProperty(param).GetValue(s, null);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return 0;
        }
    }
}
