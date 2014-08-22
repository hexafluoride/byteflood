using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Reflection;

namespace ByteFlood.Formatters
{
    public class PeerCountryToIcon : IValueConverter
    {
        string[] Resources = null;

        Services.GeoIPCountry goe = new Services.GeoIPCountry();

        public PeerCountryToIcon()
        {
            //http://stackoverflow.com/questions/16870698/how-to-check-if-a-wpf-resource-exists/16870970#16870970
            var assembly = Assembly.GetExecutingAssembly();
            string resName = assembly.GetName().Name + ".g.resources";
            using (var stream = assembly.GetManifestResourceStream(resName))
            {
                using (var reader = new System.Resources.ResourceReader(stream))
                {
                    this.Resources = reader.Cast<DictionaryEntry>().Select(entry => (string)entry.Key).ToArray();
                }
            }
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            byte[] ip = (byte[])value;

            string CountryCode = this.goe.GetCountryCode(ip).ToLower();
            
            string url = string.Format("Graphics/CountryFlags/{0}.png", CountryCode);

            if (this.Resources.Contains(url.ToLower()))
            {
                return new BitmapImage(new Uri("/ByteFlood;component/"+url, UriKind.Relative));
            }
            else 
            {
                return new BitmapImage(new Uri("/ByteFlood;component/Graphics/CountryFlags/zz.png", UriKind.Relative));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
