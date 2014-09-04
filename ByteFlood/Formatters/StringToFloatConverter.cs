using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace ByteFlood.Formatters
{
    public class StringToFloatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //value is float, we need a string (float is ok too)
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string s = System.Convert.ToString(value);

            if (string.IsNullOrEmpty(s))
            {
                return 0f;
            }
            else
            {
                try
                {
                    s = s.Replace(',', '.');
                    float vae = float.Parse(s);
                    if (vae < 0)
                    {
                        vae = 0;
                    }
                    return vae;
                }
                catch 
                {
                    return "invalid"; // trigget the data error
                }
            }
        }
    }
}
