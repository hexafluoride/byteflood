using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.Serialization;

namespace ByteFlood
{
    public enum Theme
    {
        Aero2, Aero, Classic, Luna, Royale
    }
    public class Settings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public Theme Theme { get; set; }
        public bool DrawGrid { get; set; }
        public Color DownloadColor { get; set; }
        public Color UploadColor { get; set; }
        public string DefaultDownloadPath { get; set; }
        public bool PreferEncryption { get; set; }
        public int ListeningPort { get; set; }
        public string FileRegex { get; set; }
        public bool EnableFileRegex { get; set; }
        public bool DownloadAllRSS { get; set; }
        public string RSSRegex { get; set; }
        public bool RSSCheckForDuplicates { get; set; }
        public bool MetroStyleHover { get; set; }
        public TorrentProperties DefaultTorrentProperties { get; set; }
        [XmlIgnore]
        public Visibility TreeViewVisibility { get { return TreeViewVisible ? Visibility.Visible : Visibility.Collapsed; } }
        [XmlIgnore]
        public Visibility BottomCanvasVisibility { get { return BottomCanvasVisible ? Visibility.Visible : Visibility.Collapsed; } }
        public bool TreeViewVisible { get; set; }
        public bool BottomCanvasVisible { get; set; }
        public bool ShowFileIcons { get; set; }
        public bool ShowClientIcons { get; set; }
        [XmlIgnore]
        public Visibility FileIconVisibility { get { return ShowFileIcons ? Visibility.Visible : Visibility.Collapsed; } }
        [XmlIgnore]
        public Visibility ClientIconVisibility { get { return ShowClientIcons ? Visibility.Visible : Visibility.Collapsed; } }
        public string Path;
        [XmlIgnore]
        public Brush DownloadBrush { get { return new SolidColorBrush(DownloadColor); } }
        [XmlIgnore]
        public Brush UploadBrush { get { return new SolidColorBrush(UploadColor); } }

        public static Settings DefaultSettings
        {
            get
            {
                return new Settings()
                {
                    Theme = Theme.Aero2,
                    DrawGrid = true,
                    DownloadColor = Colors.Green,
                    UploadColor = Colors.Red,
                    DefaultDownloadPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Downloads"),
                    PreferEncryption = true,
                    ListeningPort = 1025,
                    FileRegex = "",
                    EnableFileRegex = false,
                    DownloadAllRSS = false,
                    RSSRegex = "",
                    RSSCheckForDuplicates = false,
                    MetroStyleHover = false,
                    BottomCanvasVisible = true,
                    TreeViewVisible = true,
                    ShowClientIcons = true,
                    ShowFileIcons = true,
                    DefaultTorrentProperties = TorrentProperties.DefaultTorrentProperties
                };
            }
        }

        public Settings()
        {
        }
        public static void Save(Settings s, string path)
        {
            Utility.Serialize<Settings>(s, path);
        }

        public static Settings Load(string path)
        {
            try
            {
                if (!File.Exists(path))
                    return Settings.DefaultSettings;
                return Utility.Deserialize<Settings>(path);
            }
            catch
            {
                MessageBox.Show("An error occurred while loading configuration file. Falling back to default settings.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return Settings.DefaultSettings;
            }
        }
        public void NotifyChanged(params string[] props)
        {
            if (PropertyChanged == null)
                return;
            foreach (string str in props)
                PropertyChanged(this, new PropertyChangedEventArgs(str));
        }
    }
}
