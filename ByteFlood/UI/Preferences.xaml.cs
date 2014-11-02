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
using MonoTorrent.Client;

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

        public string[] TrayIconBehaviorsReadable = new string[] 
        { 
            "Show/Hide ByteFlood",
            "Show context menu",
            "Do nothing"
        };

        public TrayIconBehavior[] TrayIconBehaviors = (TrayIconBehavior[])Enum.GetValues(typeof(TrayIconBehavior));

        public string[] WindowBehaviorsReadable = new string[]
        {
            "Minimize to tray",
            "Minimize to taskbar",
            "Exit"
        };

        public WindowBehavior[] WindowBehaviors = (WindowBehavior[])Enum.GetValues(typeof(WindowBehavior));

        public ComboBox[] TrayIconComboBoxes;
        public ComboBox[] WindowComboBoxes;

        public EncryptionForceType[] EncryptionTypes = (EncryptionForceType[])Enum.GetValues(typeof(EncryptionForceType));

        public string[] EncryptionTypesReadable = new string[]
        {
            "Forced",
            "Preferred",
            "Doesn't matter"
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
            TrayIconComboBoxes = new ComboBox[] { tcb, tdcb, trcb };
            WindowComboBoxes = new ComboBox[] { mb, cb };
            Utility.SetItemsSource<ComboBox>(TrayIconComboBoxes, TrayIconBehaviorsReadable);
            Utility.SetItemsSource<ComboBox>(WindowComboBoxes, WindowBehaviorsReadable);
            enctype.ItemsSource = EncryptionTypesReadable;
            LoadNetworkInterfaces();
            styleCombox.SelectedIndex = local.ApplicationStyle;
            styleCombox.SelectionChanged += this.ReloadStyle;
        }

        private void LoadNetworkInterfaces()
        {
            foreach (var iface in Utility.GetValidNetworkInterfaces())
            {
                ComboBoxItem bi = new ComboBoxItem();
                bi.Content = iface.Name;
                bi.Tag = iface;
                interfaces.Items.Add(bi);
                if (iface.Id == local.NetworkInterfaceID)
                {
                    interfaces.SelectedItem = bi;
                }
            }
            if (interfaces.SelectedIndex == -1)
            {
                interfaces.SelectedIndex = 0;
            }
            interfaces.SelectionChanged += interfaces_SelectionChanged;
        }

        void interfaces_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem bi = interfaces.SelectedItem as ComboBoxItem;
            var iface = bi.Tag as System.Net.NetworkInformation.NetworkInterface;
            local.NetworkInterfaceID = iface.Id;
            if (iface.OperationalStatus != System.Net.NetworkInformation.OperationalStatus.Up)
            {
                iface_error.Visibility = Visibility.Visible;
            }
            else
            {
                iface_error.Visibility = Visibility.Collapsed;
            }
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

        private void PickPath(object sender, RoutedEventArgs e)
        {
            string new_path = Utility.PromptFolderSelection("Choose default download path", local.DefaultDownloadPath, this);
            if (new_path != null)
            {
                local.DefaultDownloadPath = new_path;
                downpath.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
            }
        }

        private void ImportTorrents(object sender, RoutedEventArgs e)
        {
            MainWindow mw = (App.Current.MainWindow as MainWindow);
            if (!mw.ImportTorrents())
            {
                MessageBox.Show("resume.dat not found! You either have no torrents or have not installed uTorrent.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReloadTheme(object sender, SelectionChangedEventArgs e)
        {
            var t = (Theme)themeCombox.SelectedItem;
            Utility.ReloadTheme(t);
        }

        private void ChangeDefaultSettings(object sender, RoutedEventArgs e)
        {
            TorrentPropertiesForm tpf = new TorrentPropertiesForm(local.DefaultTorrentProperties) { Owner = this, Icon = this.Icon };
            tpf.ShowDialog();
            if (tpf.success)
                local.DefaultTorrentProperties = tpf.tp;
        }

        private void SaveSettings(object sender, RoutedEventArgs e)
        {
            local.TrayIconClickBehavior = TrayIconBehaviors[tcb.SelectedIndex];
            local.TrayIconRightClickBehavior = TrayIconBehaviors[trcb.SelectedIndex];
            local.TrayIconDoubleClickBehavior = TrayIconBehaviors[tdcb.SelectedIndex];
            local.MinimizeBehavior = WindowBehaviors[mb.SelectedIndex];
            local.ExitBehavior = WindowBehaviors[cb.SelectedIndex];
            local.EncryptionType = EncryptionTypes[enctype.SelectedIndex];
            local.SeedingTorrentsAreActive = this.seedingTorrentsInac.IsChecked == true;
            MainWindow mw = (App.Current.MainWindow as MainWindow);
           
            //mw.state.ce.Settings.Force = local.EncryptionType;
            
            local.Theme = (Theme)themeCombox.SelectedItem;

            //bool iface_changed = App.Settings.NetworkInterfaceID != local.NetworkInterfaceID;
           
            App.Settings = (Settings)Utility.CloneObject(local);
            //if (iface_changed)
            //{
            //    mw.state.ChangeNetworkInterface();
            //}
            this.Close();
        }

        private void DiscardSettings(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ResetToDefaultSettings(object sender, RoutedEventArgs e)
        {
            local = (Settings)Utility.CloneObject(Settings.DefaultSettings);
            UpdateDataContext(null);
            UpdateDataContext(local);
        }

        private void AssociateFiles(object sender, RoutedEventArgs e)
        {
            Utility.FileAssociate();
            Utility.MagnetAssociate();
        }

        private void RefreshNetworkInterfaces(object sender, RoutedEventArgs e)
        {
            interfaces.SelectionChanged -= interfaces_SelectionChanged;
            interfaces.Items.Clear();
            LoadNetworkInterfaces();
        }

        private void ReloadStyle(object sender, SelectionChangedEventArgs e)
        {
            ComboBox s = (sender as ComboBox);
            if (s != null)
            {
                (App.Current.MainWindow as MainWindow).UpdateAppStyle(s.SelectedIndex);
                local.ApplicationStyle = s.SelectedIndex;
            }
        }

    }
}
