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
using MonoTorrent.Common;

namespace ByteFlood
{
    public class State : INotifyPropertyChanged
    {
        public ObservableCollection<TorrentInfo> Torrents = new ObservableCollection<TorrentInfo>();

        public event PropertyChangedEventHandler PropertyChanged;
        [XmlIgnore]
        public MainWindow window = (MainWindow)App.Current.MainWindow;

        public int DownloadingTorrentCount { get { return Torrents.Count(window.Downloading); } set { } }
        public int SeedingTorrentCount { get { return Torrents.Count(window.Seeding); } set { } }
        public int ActiveTorrentCount { get { return Torrents.Count(window.Active); } set { } }
        public int InactiveTorrentCount { get { return TorrentCount - ActiveTorrentCount; } set { } }
        public int FinishedTorrentCount { get { return Torrents.Count(window.Finished); } set { } }
        public int TorrentCount { get { return Torrents.Count; } set { } }

        public State()
        {

        }

        public static void Save(State s, string path)
        {
            Utility.Serialize<State>(s, path);
        }

        public static State Load(string path)
        {
            try
            {
                if (!File.Exists(path))
                    return new State();
                return Utility.Deserialize<State>(path);
            }
            catch
            {
                MessageBox.Show("An error occurred while loading the program state. You may need to re-add your torrents.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return new State();
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
