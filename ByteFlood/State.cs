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

namespace ByteFlood
{
    public class State
    {
        public ObservableCollection<TorrentInfo> Torrents = new ObservableCollection<TorrentInfo>();
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
    }
}
