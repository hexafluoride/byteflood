using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.IO;

namespace ByteFlood.Formatters
{
    public class FileNameToIcon : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!App.Settings.ShowFileIcons)
                return null;

            string path = value.ToString();

            int dot = path.LastIndexOf('.');

            string ext = "";

            if (dot >= 0)
            {
                ext = path.Substring(dot).ToLower();
            }

            if (Utility.IconCache.ContainsKey(ext))
            {
                return Utility.IconCache[ext];
            }
            else
            {
                string temp_file = Path.Combine(Path.GetTempPath(), "temp" + ext);

                File.Open(temp_file, FileMode.OpenOrCreate).Close();
                
                path = temp_file;

                Icon i = Icon.ExtractAssociatedIcon(path);

                File.Delete(temp_file);

                MemoryStream m = new MemoryStream();

                i.Save(m);
                i.Dispose();

                BitmapImage bi = new BitmapImage();

                bi.BeginInit();
                bi.CacheOption = BitmapCacheOption.Default;
                bi.StreamSource = m;
                bi.EndInit();
                bi.Freeze();

                Utility.IconCache.Add(ext, bi);

                return bi;
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return "";
        }
    }
}
