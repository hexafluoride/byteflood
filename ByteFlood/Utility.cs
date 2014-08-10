using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Shapes;

namespace ByteFlood
{
    public static class Utility
    {

        public static Dictionary<string, BitmapImage> IconCache = new Dictionary<string, BitmapImage>();

        const double K = 1024;
        const double M = 1048576;
        const double G = 1073741824;
        const double T = 1099511627776;

        public static string PrettifyAmount(double amount)
        {
            if (amount > T)
                return (amount / T) + " TB";
            if (amount > G)
                return (amount / G).ToString("0.00") + " GB";
            if (amount > M)
                return (amount / M).ToString("0.00") + " MB";
            if (amount > K)
                return (amount / K).ToString("0.00") + " KB";
            return amount.ToString() + " B";
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
    }
}
