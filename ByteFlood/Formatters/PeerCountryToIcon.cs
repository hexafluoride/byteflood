using System;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ByteFlood.Formatters
{
    public class PeerCountryToIcon : IValueConverter
    {
        Services.GeoIPCountry goe = null;

        static string[] flags = 
        {
            "ad", "ae", "af", "ag", "ai", "al", "am", "an", "ao", "ar", "as", "at", "au", 
            "aw", "az", "ba", "bb", "bd", "be", "bf", "bg", "bh", "bi", "bj", "bm", "bn",
            "bo", "br", "bs", "bt", "bw", "by", "bz", "ca", "cc", "cd", "cf", "cg", "ch", 
            "ci", "ck", "cl", "cm", "cn", "co", "cr", "cs", "cu", "cv", "cx", "cy", "cz", 
            "de", "dj", "dm", "do", "dz", "ec", "ee", "eg", "eh", "er", "es", "et", "eu", 
            "fj", "fk", "fm", "fr", "ga", "gb", "gd", "ge", "gf", "gh", "gi", "gl", "gm", 
            "gn", "gp", "gq", "gr", "gs", "gt", "gu", "gw", "gy", "hk", "hm", "hn", "hr", 
            "ht", "hu", "id", "ie", "il", "in", "io", "iq", "ir", "it", "jm", "jo", "jp", 
            "ke", "kg", "kh", "ki", "km", "kn", "kp", "kw", "ky", "kz", "la", "lb", "lc", 
            "li", "lk", "lr", "ls", "lt", "lu", "lv", "ly", "ma", "mc", "md", "me", "mg", 
            "mh", "mk", "ml", "mm", "mn", "mo", "mp", "mq", "mr", "ms", "mt", "mu", "mv", 
            "mw", "mx", "my", "mz", "na", "nc", "ne", "nf", "ng", "ni", "nl", "np", "nr", 
            "nu", "nz", "om", "pa", "pe", "pf", "pg", "ph", "pk", "pl", "pm", "pn", "pr", 
            "ps", "pt", "pw", "py", "qa", "re", "ro", "rs", "ru", "rw", "sa", "sb", "sc", 
            "sd", "sg", "sh", "si", "sk", "sl", "sm", "sn", "so", "sr", "st", "sv", "sy", 
            "sz", "tc", "td", "tf", "tg", "th", "tj", "tk", "tl", "tm", "tn", "to", "tr", 
            "tt", "tv", "tw", "tz", "ua", "ug", "um", "us", "uy", "uz", "va", "vc", "ve", 
            "vg", "vi", "vn", "vu", "wf", "ws", "ye", "yt", "za", "zm", "zw"
        };

        public PeerCountryToIcon()
        {
            try
            {
                this.goe = new Services.GeoIPCountry(System.IO.Path.Combine("./Assets", "GeoIP.dat"));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[PeerCountryToIcon]: Unable to load GeoIP.dat file. Exception message: " + ex.Message);
            }
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (this.goe == null) { return null; }

            try
            {
                byte[] ip = value as byte[];

                if (ip != null)
                {
                    string CountryCode = this.goe.GetCountryCode(ip).ToLower();

                    string url = string.Format("Graphics/CountryFlags/{0}.png", CountryCode);

                    if (flags.Contains(CountryCode))
                    {
                        return new BitmapImage(new Uri("/ByteFlood;component/" + url, UriKind.Relative));
                    }
                }
            }
            catch { }

            return new BitmapImage(new Uri("/ByteFlood;component/Graphics/CountryFlags/zz.png", UriKind.Relative));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
