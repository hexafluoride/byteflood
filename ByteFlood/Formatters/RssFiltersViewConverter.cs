using System;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ByteFlood.Formatters
{
    public class RssFiltersViewConverter : IValueConverter
    {
        SolidColorBrush VioletBrush = null;

        public RssFiltersViewConverter()
        {
            this.VioletBrush = new SolidColorBrush(new Color()
            {
                //FF 63 16 80
                A = 0xff,
                R = 0x63,
                G = 0x16,
                B = 0x80
            });
            this.VioletBrush.Freeze();
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var action = (ByteFlood.Services.RSS.RssFilter.FilterActionEnum)value;

            if (action == Services.RSS.RssFilter.FilterActionEnum.Download)
            {
                return this.VioletBrush;
            }
            else
            {
                return Brushes.Orange;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
