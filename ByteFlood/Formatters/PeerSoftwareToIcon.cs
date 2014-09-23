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
        static string[] icons = new string[] 
        {
            "ABC","Ares","Artemis","Artic","Azureus","BTG","BitBuddy",
            "BitComet","BitPump","BitRocket","BitSpirit","BitTornado","BitTorrent",
            "BitsOnWheels","DelugeTorrent","ElectricSheep","KTorrent","Lphant","MLDonkey",
            "MooPolice","MoonlightTorrent","Opera","Shareaza","Transmission","Tribler",
            "Vuze","XBTClient","XanTorrent","ZipTorrent","qBittorrent","uTorrent"
        };

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!App.Settings.ShowClientIcons)
                return null;

            try
            {
                string ClientName = value.ToString();

                string url = string.Format("Graphics/ClientIcons/{0}.png", ClientName);

                if (icons.Contains(ClientName))
                {
                    return new BitmapImage(new Uri("/ByteFlood;component/" + url, UriKind.Relative));
                }
                else { return null; }
            }
            catch
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
