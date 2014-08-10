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
            string path = value.ToString();

            var fi = new System.IO.FileInfo(path);

            if (Utility.IconCache.ContainsKey(fi.Extension))
            {
                return Utility.IconCache[fi.Extension];
            }
            else
            {
                string temp_file = "";

                if (!fi.Exists)
                {
                    string temp_dir = Environment.GetFolderPath(Environment.SpecialFolder.InternetCache);
                    temp_file = Path.Combine(temp_dir, "temp" + fi.Extension);
                    File.WriteAllText(temp_file, "");
                    path = temp_file;
                }

                Icon i = Icon.ExtractAssociatedIcon(path);

                if (File.Exists(temp_file)) { File.Delete(temp_file); }

                MemoryStream m = new MemoryStream();

                i.Save(m);
                i.Dispose();

                BitmapImage bi = new BitmapImage();

                bi.BeginInit();
                bi.CacheOption = BitmapCacheOption.Default;
                bi.StreamSource = m;
                bi.EndInit();

                Utility.IconCache.Add(fi.Extension, bi);

                return bi;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return "";
        }
    }
}
