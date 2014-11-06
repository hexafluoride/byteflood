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
using System.Collections.ObjectModel;
using MonoTorrent.BEncoding;
using MonoTorrent.Client;
using MonoTorrent.Common;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

namespace ByteFlood
{
    /// <summary>
    /// Interaction logic for ImportTorrents.xaml
    /// </summary>
    public partial class ImportTorrents : Window
    {
        public static string[] uTorrentDirs = 
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "uTorrent"),  
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BitTorrent")
        };
        public ObservableCollection<TorrentListing> list = new ObservableCollection<TorrentListing>();
        public List<TorrentInfo> selected = new List<TorrentInfo>();
        public MainWindow MainWindow = (App.Current.MainWindow as MainWindow);
        public State AppState = (App.Current.MainWindow as MainWindow).state;

        public ImportTorrents()
        {
            InitializeComponent();
        }

        /*private static bool ResumeExist()
        {
            foreach (string dir in uTorrentDirs)
            {
                string p = Path.Combine(dir, "resume.dat");
                if (File.Exists(p))
                    return true;
            }

            return false;
        }*/

        /// <param name="fast_load">Indicate wither to stop loading at the first torrent found</param>
        public void Load(bool fast_load = false)
        {
            foreach (string dir in uTorrentDirs)
            {
                string p = Path.Combine(dir, "resume.dat");
                if (File.Exists(p))
                {
                    var fs = new FileStream(p, FileMode.Open);
                    var val = BEncodedDictionary.Decode(fs);
                    BEncodedDictionary dict = val as BEncodedDictionary;
                    foreach (var pair in dict)
                    {
                        try
                        {
                            string key = pair.Key.ToString();
                            if (key != ".fileguard") // special case
                            {
                                TorrentListing tl = new TorrentListing();
                                tl.Path = Path.Combine(dir, key);
                                BEncodedDictionary values = pair.Value as BEncodedDictionary;
                                tl.Name = values[new BEncodedString("caption")].ToString();
                                tl.SavePath = values[new BEncodedString("path")].ToString();
                                tl.Import = true;
                                list.Add(tl);
                                if (fast_load)
                                {
                                    fs.Close();
                                    return;
                                }
                            }
                        }
                        catch
                        { }
                    }
                }
            }
            torrents.Items.Refresh();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            torrents.ItemsSource = list;
            list.Clear();
            await Task.Run(() => Load());
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox s = sender as CheckBox;
            TorrentListing tl = s.Tag as TorrentListing;
            tl.Import = s.IsChecked == true;
        }

        private void Import(object sender, RoutedEventArgs e)
        {
            foreach (var listing in list)
            {
                try
                {
                    if (!listing.Import)
                        continue;
                    Torrent t = Torrent.Load(listing.Path);

                    string torrent_file_path = AppState.BackupTorrent(listing.Path, t);

                    string savepath = null;

                    if (t.Files.Length > 1)
                    {
                        if (listing.SavePath.EndsWith(t.Name))
                        {
                            // then we should download in the parent directory
                            DirectoryInfo di = new DirectoryInfo(listing.SavePath);
                            savepath = di.Parent.FullName;
                        }
                        else
                        {
                            savepath = listing.SavePath;
                        }
                    }
                    else if (t.Files.Length == 1)
                    {
                        savepath = Path.GetDirectoryName(listing.SavePath);
                    }
                    else
                    {
                        savepath = listing.SavePath;
                    }

                    Ragnar.AddTorrentParams param = new Ragnar.AddTorrentParams()
                    {
                        SavePath = savepath,
                        TorrentInfo = new Ragnar.TorrentInfo(File.ReadAllBytes(torrent_file_path))
                    };

                    var handle = AppState.LibtorrentSession.AddTorrent(param);

                    TorrentInfo ti = new TorrentInfo(handle);
                    ti.Name = listing.Name;

                    selected.Add(ti);
                }
                catch
                {
                }
            }
            App.Settings.ImportedTorrents = true;
            this.Close();
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            App.Settings.ImportedTorrents = true;
            this.Close();
        }
    }

    /// <summary>
    /// This is only used internally, so there's no need to copy this over to InfoClasses(I think)
    /// </summary>
    public class TorrentListing
    {
        public string Name { get; set; }
        public bool Import { get; set; }
        public string Path { get; set; }
        public string SavePath { get; set; }
        public TorrentListing()
        {
        }
    }
}
