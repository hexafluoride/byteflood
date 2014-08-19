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
using System.Collections.ObjectModel;

namespace ByteFlood
{
    /// <summary>
    /// Interaction logic for Preferences.xaml
    /// </summary>
    public partial class Preferences : Window
    {
        Settings local;

        public Theme[] theme_list = new Theme[] 
        {
            Theme.Aero,
            Theme.Aero2,
            Theme.Classic,
            Theme.Luna,
            Theme.Royale
        };

        public Preferences()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            local = (Settings)Utility.CloneObject(App.Settings);
            this.DataContext = local;
            this.themeCombox.ItemsSource = theme_list;
            this.themeCombox.SelectedItem = local.Theme;
        }

        private void UpdateDataContext(Settings s)
        {
            this.DataContext = s;
            this.themeCombox.SelectedItem = s == null ? Theme.Aero2 : s.Theme;
        }

        private void SelectDownloadColor(object sender, RoutedEventArgs e)
        {
            local.DownloadColor = GetNewColor(local.DownloadColor);
            downcolor.GetBindingExpression(Button.BackgroundProperty).UpdateTarget();
        }

        public Color GetNewColor(Color current)
        {
            ColorDialog cd = new ColorDialog();
            cd.Color = current.ToWinFormColor();
            cd.AllowFullOpen = true;
            cd.FullOpen = true;
            cd.SolidColorOnly = true;
            cd.ShowDialog();
            return cd.Color.ToWPFColor();
        }

        private void SelectUploadColor(object sender, RoutedEventArgs e)
        {
            local.UploadColor = GetNewColor(local.UploadColor);
            upcolor.GetBindingExpression(Button.BackgroundProperty).UpdateTarget();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            var fd = new System.Windows.Forms.FolderBrowserDialog();
            fd.ShowNewFolderButton = true;
            fd.ShowDialog();
            local.DefaultDownloadPath = fd.SelectedPath;
            downpath.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
        }

        private void ReloadTheme(object sender, SelectionChangedEventArgs e)
        {
            var t = (Theme)themeCombox.SelectedItem;
            var app = (ByteFlood.App)App.Current;
            app.LoadTheme(t);
        }

        private void ChangeDefaultSettings(object sender, RoutedEventArgs e)
        {
            TorrentPropertiesForm tpf = new TorrentPropertiesForm(local.DefaultTorrentProperties);
            tpf.ShowDialog();
            if (tpf.success)
                local.DefaultTorrentProperties = tpf.tp;
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            App.Settings = (Settings)Utility.CloneObject(local);
            this.Close();
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            local = (Settings)Utility.CloneObject(Settings.DefaultSettings);
            UpdateDataContext(null);
            UpdateDataContext(local);
        }

    }
}
