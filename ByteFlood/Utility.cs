using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace ByteFlood
{
    public static class Utility
    {

        public static Dictionary<string, BitmapImage> IconCache = new Dictionary<string, BitmapImage>();

        const double K = 1024;
        const double M = 1048576;
        const double G = 1073741824;
        const double T = 1099511627776;

        static string Extension = ".torrent";
        static string OpenWith = Assembly.GetCallingAssembly().Location;
        static string KeyName = "ByteFlood";

        public static string PrettifyAmount(double amount)
        {
            if (amount > T)
                return (amount / T).ToString("0.00") + " TB";
            if (amount > G)
                return (amount / G).ToString("0.00") + " GB";
            if (amount > M)
                return (amount / M).ToString("0.00") + " MB";
            if (amount > K)
                return (amount / K).ToString("0.00") + " KB";
            return amount.ToString("0.00") + " B";
        }

        //public static string PrettifyAmount(long amount)
        //{
        //    return PrettifyAmount((ulong)amount);
        //}

        public static string PrettifySpeed(long speed)
        {
            return PrettifyAmount((ulong)speed) + "/s";
        }

        public static Label GenerateLabel(string text, Thickness margin)
        {
            Label l = new Label();
            l.Content = text;
            l.Width = double.NaN; // equivalent of Width="Auto"
            l.Height = double.NaN;

            l.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            l.Margin = margin;
            return l;
        }

        public static Color ToWPFColor(this System.Drawing.Color color)
        {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        public static System.Drawing.Color ToWinFormColor(this Color color)
        {
            return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        public static Line GenerateLine(double x1, double y1, double x2, double y2, Brush color, int thickness = 2, int zindex = 1)
        {
            Line line = new Line();
            line.X1 = x1;
            line.Y1 = y1;
            line.X2 = x2;
            line.Y2 = y2;
            line.StrokeThickness = thickness;
            line.Stroke = color;
            return line;
        }

        public static Thickness SizeToMargin(Thickness orig)
        {
            return new Thickness(orig.Left, orig.Top, orig.Right - orig.Left, orig.Bottom - orig.Top);
        }
        public static void SetIfLowerThan(ref double orig, double newval)
        {
            if (newval < orig)
                orig = newval;
        }
        public static void SetIfHigherThan(ref double orig, double newval)
        {
            if (newval > orig)
                orig = newval;
        }
        public static double CalculateLocation(double spp, double data)
        {
            return data * spp;
        }

        public static bool IsWindows8OrNewer
        {
            get
            {
                var os = Environment.OSVersion;
                return os.Platform == PlatformID.Win32NT &&
                       (os.Version.Major > 6 || (os.Version.Major == 6 && os.Version.Minor >= 2));
            }
        }

        public static void Serialize<T>(T t, string path)
        {
            try
            {
                XmlWriter xw = XmlWriter.Create(path, new XmlWriterSettings()
                {
                    Indent = true
                });
                new XmlSerializer(typeof(T)).Serialize(xw, t);
                xw.Flush();
            }
            catch
            {
                // ignore silently
            }
        }

        public static object CloneObject(object source)
        {
            Type type = source.GetType();
            object target;
            if (type == Settings.DefaultSettings.GetType())
                target = Settings.DefaultSettings;
            else
                target = Activator.CreateInstance(type);

            PropertyInfo[] prop_info = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (PropertyInfo item in prop_info)
            {
                if (item.CanWrite)
                {
                    if (item.PropertyType.IsValueType || item.PropertyType.IsEnum || item.PropertyType.Equals(typeof(System.String)))
                    {
                        item.SetValue(target, item.GetValue(source, null), null);
                    }
                    else
                    {
                        object prop_val = item.GetValue(source, null);
                        if (prop_val == null)
                        {
                            item.SetValue(target, null, null);
                        }
                        else
                        {
                            item.SetValue(target, CloneObject(prop_val), null);
                        }
                    }
                }
            }
            return target;
        }
        [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);
        public static void SetAssociation()
        {
            RegistryKey BaseKey;
            RegistryKey OpenMethod;
            RegistryKey Shell;
            RegistryKey CurrentUser;

            BaseKey = Registry.CurrentUser.OpenSubKey("Software\\Classes", true).CreateSubKey(Extension);
            BaseKey.SetValue("", KeyName);

            OpenMethod = Registry.CurrentUser.OpenSubKey("Software\\Classes", true).CreateSubKey(KeyName);
            OpenMethod.SetValue("", "TORRENT File");
            OpenMethod.CreateSubKey("DefaultIcon").SetValue("", "\"" + OpenWith + "\",0");
            Shell = OpenMethod.CreateSubKey("Shell");
            Shell.CreateSubKey("open").CreateSubKey("command").SetValue("", "\"" + OpenWith + "\"" + " \"%1\"");
            BaseKey.Close();
            OpenMethod.Close();
            Shell.Close();

            CurrentUser = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\.torrent", true);
            CurrentUser.DeleteSubKey("UserChoice", false);
            CurrentUser.Close();

            SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
        }

        public static bool Associated()
        {
            RegistryKey BaseKey;
            RegistryKey OpenMethod;
            RegistryKey Shell;

            BaseKey = Registry.CurrentUser.OpenSubKey("Software", true).OpenSubKey("Classes", true);
            if (!BaseKey.GetSubKeyNames().Contains(Extension))
                return false;
            BaseKey = BaseKey.OpenSubKey(Extension);

            OpenMethod = Registry.CurrentUser.OpenSubKey("Software", true).OpenSubKey("Classes", true);
            if (!OpenMethod.GetSubKeyNames().Contains(KeyName))
                return false;
            OpenMethod = OpenMethod.OpenSubKey(KeyName);
            Shell = OpenMethod.OpenSubKey("Shell");
            if (!Shell.GetSubKeyNames().Contains("open"))
                return false;
            BaseKey.Close();
            OpenMethod.Close();
            Shell.Close();
            return true;
        }
        public static T Deserialize<T>(string path)
        {
            string s = File.ReadAllText(path);
            XmlReader x = XmlReader.Create(new StringReader(s));
            return (T)new XmlSerializer(typeof(T)).Deserialize(x);
        }

        public static byte[] DecompressGzip(byte[] gzip_data)
        {
            using (var stream = new System.IO.Compression.GZipStream(new MemoryStream(gzip_data), System.IO.Compression.CompressionMode.Decompress))
            {
                using (MemoryStream memory = new MemoryStream())
                {
                    stream.CopyTo(memory);
                    return memory.ToArray();
                }
            }
        }
    }
}
