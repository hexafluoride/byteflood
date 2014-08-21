using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Reflection;

namespace ByteFlood.Formatters
{
    public class PeerSoftwareToIcon : IValueConverter
    {
        string[] Resources = null;

        public PeerSoftwareToIcon()
        {
            //http://stackoverflow.com/questions/16870698/how-to-check-if-a-wpf-resource-exists/16870970#16870970
            var assembly = Assembly.GetExecutingAssembly();
            string resName = assembly.GetName().Name + ".g.resources";
            using (var stream = assembly.GetManifestResourceStream(resName))
            {
                using (var reader = new System.Resources.ResourceReader(stream))
                {
                    this.Resources = reader.Cast<DictionaryEntry>().Select(entry =>
                             (string)entry.Key).ToArray();
                }
            }
            return;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string ClientName = value.ToString();

            string url = string.Format("Graphics/ClientIcons/{0}.png", ClientName);

            if (Resources.Contains(url.ToLower()))
            {
                return "/ByteFlood;component/" + url;
            }
            else { return null; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (int)value + 5;
        }
    }
}
