using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ColorDialog = System.Windows.Forms.ColorDialog;

namespace ByteFlood
{
    /// <summary>
    /// Interaction logic for Preferences.xaml
    /// </summary>
    public partial class Preferences : Window
    {
        public Preferences()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = App.Settings;
        }

        private void SelectDownloadColor(object sender, RoutedEventArgs e)
        {
            App.Settings.DownloadColor = GetNewColor(App.Settings.DownloadColor);
            downcolor.GetBindingExpression(Button.BackgroundProperty).UpdateTarget();
        }

        public Color GetNewColor(Color current)
        {
            ColorDialog cd = new ColorDialog();
            cd.Color = Utility.WPFColorToWindowsColor(current);
            cd.AllowFullOpen = true;
            cd.FullOpen = true;
            cd.SolidColorOnly = true;
            cd.ShowDialog();
            return Utility.WindowsColorToWPFColor(cd.Color);
        }

        private void SelectUploadColor(object sender, RoutedEventArgs e)
        {
            App.Settings.UploadColor = GetNewColor(App.Settings.UploadColor);
            upcolor.GetBindingExpression(Button.BackgroundProperty).UpdateTarget();
        }
    }
}
