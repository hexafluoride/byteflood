using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using MonoTorrent.BEncoding;
using System.IO;
using System.Threading.Tasks;

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
        public MainWindow MainWindow = (App.Current.MainWindow as MainWindow);
        public State AppState = (App.Current.MainWindow as MainWindow).state;

        public ImportTorrents()
        {
            InitializeComponent();
        }

        /// <param name="fast_load">Indicate wither to stop loading at the first torrent found</param>
        public void Load(bool fast_load = false)
        {
            foreach (string dir in uTorrentDirs)
            {
                string p = Path.Combine(dir, "resume.dat");
                if (File.Exists(p))
                {
                    using (var fs = new FileStream(p, FileMode.Open))
                    {
                        try
                        {
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
                        catch (BEncodingException)
                        {
                            //this exception may be thrown by the Decode function if the format is for some reason not recognized (maybe a later format?)
                            //no matter what, let's not crash because of it, and just exit out in a safe manner.
                        }
                    }
                }
            }
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                torrents.Items.Refresh();
            }));
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

                    Ragnar.TorrentInfo torrent = new Ragnar.TorrentInfo(File.ReadAllBytes(listing.Path));

                    if (!torrent.IsValid)
                    {
                        continue;
                    }

                    AppState.BackupTorrent(listing.Path, torrent);

                    string savepath = null;

                    if (torrent.NumFiles > 1)
                    {
                        if (listing.SavePath.EndsWith(torrent.Name))
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
                    else if (torrent.NumFiles == 1)
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
                        TorrentInfo = torrent,
                        Name = listing.Name
                    };


                    // calling LibtorrentSession.AsyncAddTorrent will fire the TorrentAddedEvent
                    var handle = AppState.LibtorrentSession.AddTorrent(param);
                    AppState.set_files_priorities(handle, 3);
                }
                catch
                { }
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
