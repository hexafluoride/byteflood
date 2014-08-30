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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using MonoTorrent;
using MonoTorrent.Client;
using System.Xml;
using System.Xml.Serialization;
using MonoTorrent.Dht;
using MonoTorrent.Dht.Listeners;
using MonoTorrent.Common;
using Microsoft.Win32;
using System.Threading;
using System.Net;
using System.IO;
using Jayrock;
using Jayrock.JsonRpc;

namespace ByteFlood
{
    public class State : INotifyPropertyChanged
    {
        public ObservableCollection<TorrentInfo> Torrents = new ObservableCollection<TorrentInfo>();

        public event PropertyChangedEventHandler PropertyChanged;
        [XmlIgnore]
        public MainWindow window = (MainWindow)App.Current.MainWindow;
        [XmlIgnore]
        public ClientEngine ce;
        [XmlIgnore]
        public SynchronizationContext uiContext;
        public int DownloadingTorrentCount { get { return Torrents.Count(window.Downloading); } set { } }
        public int SeedingTorrentCount { get { return Torrents.Count(window.Seeding); } set { } }
        public int ActiveTorrentCount { get { return Torrents.Count(window.Active); } set { } }
        public int InactiveTorrentCount { get { return TorrentCount - ActiveTorrentCount; } set { } }
        public int FinishedTorrentCount { get { return Torrents.Count(window.Finished); } set { } }
        public int TorrentCount { get { return Torrents.Count; } set { } }
        [XmlIgnore]
        public Thread mainthread;
        [XmlIgnore]
        public DhtListener dhtl;
        [XmlIgnore]
        public Listener listener;

        public State()
        {
            this.Initialize();
        }

        public void Initialize()
        {
            ce = new ClientEngine(new EngineSettings());
            dhtl = new DhtListener(new IPEndPoint(IPAddress.Any, App.Settings.ListeningPort));
            DhtEngine dht = new DhtEngine(dhtl);

            ce.RegisterDht(dht);
            ce.DhtEngine.Start();

            if (!App.Settings.AssociationAsked) 
            { 
                bool assoc = Utility.Associated();
                if (!assoc &&
                         MessageBox.Show("Do you want to associate ByteFlood with .torrent files?", 
                         "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    Utility.SetAssociation();
                App.Settings.AssociationAsked = true;
            }
            //MessageBox.Show(assoc.ToString());
            listener = new Listener(this);
            listener.State = this;
        }

        public static void Save(State s, string path)
        {
            Utility.Serialize<State>(s, path);
        }

        public void Shutdown()
        {
            SaveSettings();
            SaveState();
            mainthread.Abort();
            ce.DiskManager.Flush();
            ce.PauseAll();
            listener.Shutdown();
        }

        public void SaveSettings()
        {
            Settings.Save(App.Settings, "./config.xml");
        }

        public void SaveState()
        {
            State.Save(this, "./state.xml");
        }

        public void AddTorrentByPath(string path)
        {
            try
            {
                Torrent t = Torrent.Load(path);
                string newfile = t.InfoHash.ToHex() + ".torrent";
                string newpath = System.IO.Path.Combine(App.Settings.TorrentFileSavePath, newfile);
                File.Copy(path, newpath);
                path = newpath;
            }
            catch (TorrentException)
            {
                MessageBox.Show(string.Format("Invalid torrent file {0}", path), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            catch (Exception) 
            {
                MessageBox.Show(string.Format("Could not load torrent {0}", path), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            uiContext.Send(x =>
            {
                App.Current.MainWindow.Activate();
                AddTorrentDialog atd = new AddTorrentDialog(path);
                atd.ShowDialog();
                if (atd.userselected)
                {
                    TorrentInfo ti = CreateTorrentInfo(atd.tm);
                    ti.Name = atd.torrentname;
                    if (!atd.start)
                        ti.Stop();
                    ti.RatioLimit = atd.limit;
                    TorrentProperties.Apply(ti.Torrent, App.Settings.DefaultTorrentProperties);
                    ti.Torrent.Settings.InitialSeedingEnabled = atd.initial.IsChecked == true;
                    Torrents.Add(ti);
                }
            }, null);
        }

        public bool AddTorrentRss(string path, TorrentSettings ts, bool autostart) 
        {
            Torrent t = null;
            try
            {
               t = Torrent.Load(path);
            }
            catch { return false; }

            bool success = true;
            uiContext.Send(x =>
            {
                if (Torrents.Any(tinf => tinf.Torrent.Torrent.InfoHash == t.InfoHash))
                {
                    success = false;
                    return;
                }
                TorrentManager tm = new TorrentManager(t, App.Settings.DefaultDownloadPath, ts);
                TorrentInfo ti = CreateTorrentInfo(tm);
                ti.Name = t.Name;
                if (autostart) 
                {
                    ti.Start();
                }
                else
                {
                    ti.Stop();
                }
                Torrents.Add(ti);
            }, null);
            return success;
        }

        public void AddTorrentByMagnet(string magnet) 
        {
            MagnetLink mg = null;

            try { mg = new MagnetLink(magnet); }
            catch { MessageBox.Show("Invalid magnet link", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }

            byte[] file_data = this.GetMagnetFromCache(mg);

            if (file_data != null)
            {
                string path = System.IO.Path.Combine(App.Settings.DefaultDownloadPath, mg.InfoHash.ToHex() + ".torrent");

                if (!Directory.Exists(App.Settings.DefaultDownloadPath)) 
                {
                    Directory.CreateDirectory(App.Settings.DefaultDownloadPath);
                }

                File.WriteAllBytes(path, file_data);
                this.AddTorrentByPath(path);
            }
            else 
            {
                MessageBox.Show("Could not find a cached copy of this magnet link.", "Loading failed", MessageBoxButton.OK, MessageBoxImage.Error); 
                return;
            }
        }

        [XmlIgnore]
        private Services.TorrentCache.ITorrentCache[] TorrentCaches = new Services.TorrentCache.ITorrentCache[] 
        {
            new Services.TorrentCache.TorCache(),
            new Services.TorrentCache.Torrage(),
            new Services.TorrentCache.ZoinkIT()
        };

        private byte[] GetMagnetFromCache(MagnetLink mg) 
        {
            for (int i = 0; i < TorrentCaches.Length; i++) 
            {
                byte[] res = TorrentCaches[i].Fetch(mg);

                if (res != null)
                    return res;
            }

            return null;
        }
       
        public TorrentInfo CreateTorrentInfo(TorrentManager tm)
        {
            ce.Register(tm);
            tm.Start();
            TorrentInfo t = new TorrentInfo(uiContext, tm);
            t.Update();
            return t;
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
