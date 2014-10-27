using System;
using System.Linq;
using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using MonoTorrent;
using MonoTorrent.Client;
using System.Xml.Serialization;
using MonoTorrent.Common;
using MonoTorrent.Client.Connections;
using System.Threading;
using System.Net;
using System.IO;

namespace ByteFlood
{
    public class State : INotifyPropertyChanged
    {
        public ObservableCollection<TorrentInfo> Torrents = new ObservableCollection<TorrentInfo>();

        public event PropertyChangedEventHandler PropertyChanged;
        [XmlIgnore]
        public MainWindow window = (MainWindow)App.Current.MainWindow;

        [XmlIgnore]
        public SynchronizationContext uiContext;
        public int DownloadingTorrentCount { get { return Torrents.Count(window.Downloading); } set { } }
        public int SeedingTorrentCount { get { return Torrents.Count(window.Seeding); } set { } }
        public int ActiveTorrentCount { get { return Torrents.Count(window.Active); } set { } }
        public int InactiveTorrentCount { get { return TorrentCount - ActiveTorrentCount; } set { } }
        public int FinishedTorrentCount { get { return Torrents.Count(window.Finished); } set { } }

        [XmlIgnore]
        public int TorrentCount { get { return Torrents.Count; } }

        [XmlIgnore]
        public Thread mainthread;

        [XmlIgnore]
        public Listener listener;

        [XmlIgnore]
        public int DHTPeers
        {
            get
            {
                if (this.LibtorrentSession.IsDhtRunning)
                {
                    return this.LibtorrentSession.QueryStatus().DhtNodes;
                }
                return 0;
            }
        }

        public int GlobalMaxDownloadSpeed
        {
            get { return this.LibtorrentSession.QuerySettings().DownloadRateLimit; }
            set
            {
                var settings = this.LibtorrentSession.QuerySettings();
                settings.DownloadRateLimit = value;
                this.LibtorrentSession.SetSettings(settings);
            }
        }

        public int GlobalMaxUploadSpeed
        {
            get { return this.LibtorrentSession.QuerySettings().UploadRateLimit; }
            set
            {
                var settings = this.LibtorrentSession.QuerySettings();
                settings.UploadRateLimit = value;
                this.LibtorrentSession.SetSettings(settings);
            }
        }

        public State()
        {
            this.Torrents.CollectionChanged += Torrents_CollectionChanged;
            this.Initialize();
        }

        public Ragnar.Session LibtorrentSession { get; private set; }

        public void Initialize()
        {
            UpdateConnectionSettings();
            IPV4Connection.ExceptionThrown += Utility.LogException;
            IPV4Connection.LocalAddress = IPAddress.Any;

            this.LibtorrentSession = new Ragnar.Session();

            this.LibtorrentSession.ListenOn(App.Settings.ListeningPort, App.Settings.ListeningPort);

            this.LibtorrentSession.StartDht();
            this.LibtorrentSession.StartLsd();
            this.LibtorrentSession.StartNatPmp();
            this.LibtorrentSession.StartUpnp();

            if (File.Exists(LtSessionFilePath))
            {
                this.LibtorrentSession.LoadState(File.ReadAllBytes(LtSessionFilePath));

                var torrents = this.LibtorrentSession.GetTorrents();

                foreach (var torrent in torrents) 
                {
                    this.Torrents.Add(new TorrentInfo(torrent));
                }
            }


            if (!App.Settings.AssociationAsked)
            {
                bool assoc = Utility.Associated();
                if (!assoc)
                {
                    if (MessageBox.Show("Do you want to associate ByteFlood with .torrent files?",
                             "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        Utility.SetAssociation();
                        App.Settings.AssociationAsked = true;
                    }
                    else if (MessageBox.Show("Do you want to be reminded about associations again?",
                        "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                        App.Settings.AssociationAsked = true;
                    else
                        App.Settings.AssociationAsked = false;
                }
            }

            listener = new Listener(this);
            listener.State = this;
        }


        public void ChangeNetworkInterface()
        {
            throw new NotImplementedException();
            // var new_iface = Utility.GetNetworkInterface(App.Settings.NetworkInterfaceID);
        }

        private void Torrents_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NotifySinglePropertyChanged("TorrentCount");
        }

        public void UpdateConnectionSettings()
        {
            IPV4Connection.UseRandomPorts = App.Settings.OutgoingPortsRandom;
            if (!IPV4Connection.UseRandomPorts)
                IPV4Connection.LocalPorts = Enumerable.Range(App.Settings.OutgoingPortsStart, App.Settings.OutgoingPortsEnd - App.Settings.OutgoingPortsStart).ToArray();
        }

        public void Shutdown()
        {
            SaveSettings();
            SaveState();
            mainthread.Abort();

            this.LibtorrentSession.StopDht();
            this.LibtorrentSession.Dispose();

            listener.Shutdown();
        }

        public void SaveSettings()
        {
            Settings.Save(App.Settings, "./config.xml");
        }

        private string StateSaveDirectory
        {
            get { return Path.Combine(".", "state"); }
        }

        private string LtSessionFilePath 
        {
            get 
            {
                return Path.Combine(this.StateSaveDirectory, "ltsession.bin");
            }
        }

        public void SaveState()
        {
            Directory.CreateDirectory(this.StateSaveDirectory);

            File.WriteAllBytes(LtSessionFilePath, this.LibtorrentSession.SaveState());

            var torrents = this.LibtorrentSession.GetTorrents();

            foreach (var torrent in torrents)
            {
                if (torrent.NeedSaveResumeData())
                {
                    torrent.SaveResumeData();
                }
            }
        }

        public void AddTorrentsByPath(string[] paths)
        {
            foreach (string str in paths)
            {
                AddTorrentByPath(str, false);
            }
        }

        public void AddTorrentByPath(string path, bool notifyIfAdded = true)
        {
            try
            {
                Torrent t = Torrent.Load(path);

                if (this.ContainTorrent(t.InfoHash.ToHex()))
                {
                    if (notifyIfAdded)
                    {
                        MessageBox.Show("This torrent is already added.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    return;
                }
                path = BackupTorrent(path, t);
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

            var handle = this.LibtorrentSession.AddTorrent(new Ragnar.AddTorrentParams() 
            {
                TorrentInfo = new Ragnar.TorrentInfo(File.ReadAllBytes(path)),
                SavePath = App.Settings.DefaultDownloadPath,
            });

            handle.AutoManaged = false;
            handle.Pause();

            TorrentInfo ti = new TorrentInfo(handle);

            uiContext.Send(x =>
            {
                App.Current.MainWindow.Activate();
                AddTorrentDialog atd = new AddTorrentDialog(ti) { Owner = App.Current.MainWindow, Icon = App.Current.MainWindow.Icon };
                atd.ShowDialog();
                if (atd.UserOK)
                {
                    handle.AutoManaged = true;
                    ti.Name = atd.TorrentName;
                    if (atd.AutoStartTorrent)
                    { ti.Start(); }
                    ti.RatioLimit = atd.RatioLimit;
                    Torrents.Add(ti);
                }
                else 
                {
                    ti.OffMyself();
                    this.LibtorrentSession.RemoveTorrent(handle);
                    handle.Dispose();
                }
            }, null);
        }

        public bool ContainTorrent(string infoHash)
        {
            return false;
            //this.LibtorrentSession.FindTorrent(infoHash).
        }

        /// <summary>
        /// Copies a torrent to ./torrents-bkp
        /// </summary>
        /// <param name="path">The path of the torrent file to be copied.</param>
        /// <returns>The new path of the torrent file.</returns>
        public string BackupTorrent(string path, Torrent t)
        {
            string directory = "./torrents-bkp";
            Directory.CreateDirectory(directory);
            string newpath = System.IO.Path.Combine(directory, t.InfoHash.ToHex() + ".torrent");
            if (new DirectoryInfo(newpath).FullName != new DirectoryInfo(path).FullName)
                File.Copy(path, newpath, true);
            return newpath;
        }

        public bool AddTorrentRss(string path, Services.RSS.RssUrlEntry entry)
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
                if (this.ContainTorrent(t.InfoHash.ToHex()))
                {
                    success = false;
                    return;
                }
                Directory.CreateDirectory(entry.DownloadDirectory);

                var handle = this.LibtorrentSession.AddTorrent(new Ragnar.AddTorrentParams() 
                {
                    SavePath = entry.DownloadDirectory,
                    TorrentInfo = new Ragnar.TorrentInfo(File.ReadAllBytes(path))
                });
                Torrents.Add(new TorrentInfo(handle));

            }, null);
            return success;
        }

        public void AddTorrentByMagnet(string magnet, bool notifyIfAdded = true)
        {
            MessageBox.Show("Not supported", "Info");
            return;

            //MagnetLink mg = null;

            //try { mg = new MagnetLink(magnet); }
            //catch { MessageBox.Show("Invalid magnet link", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }

            //if (this.ContainTorrent(mg.InfoHash.ToHex()))
            //{
            //    if (notifyIfAdded)
            //    {
            //        MessageBox.Show("This torrent is already added.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //    }
            //    return;
            //}

            //if (!Directory.Exists(App.Settings.TorrentFileSavePath))
            //    Directory.CreateDirectory(App.Settings.TorrentFileSavePath);

            //string path = System.IO.Path.Combine(App.Settings.TorrentFileSavePath, mg.InfoHash.ToHex() + ".torrent");

            //AddTorrentDialog atd = new AddTorrentDialog("") { Icon = App.Current.MainWindow.Icon };
            //atd.Show();
            //bool use_cache_services = false;
            //ThreadPool.QueueUserWorkItem(delegate
            //{

            //    if (use_cache_services)
            //    {
            //        byte[] data = GetMagnetFromCache(mg);
            //        if (data != null)
            //        {
            //            File.WriteAllBytes(path, data);
            //            this.AddTorrentByPath(path, atd);
            //            return;
            //        }
            //    }

            //    this.LibtorrentSession.AsyncAddTorrent(new Ragnar.AddTorrentParams() 
            //    {
            //        SavePath = App.Settings.DefaultDownloadPath,
            //        Url = magnet
            //    });

           
            //});
        }

        //public byte[] MagnetLinkTorrentFile(string magnet)
        //{
        //    return null;

        //    MagnetLink mg = null;

        //    try { mg = new MagnetLink(magnet); }
        //    catch
        //    {
        //        return null;
        //    }

        //    string hash = mg.InfoHash.ToHex();
        //    string path = System.IO.Path.Combine(App.Settings.TorrentFileSavePath, hash + ".torrent");
        //    string temp_save = System.IO.Path.Combine(System.IO.Path.GetTempPath(), hash);
        //    Directory.CreateDirectory(temp_save);
        //    TorrentManager tm = new TorrentManager(mg, temp_save, new TorrentSettings(), path);

           
        //    tm.Start();

          
        //    int i = 0;

        //    tm.Stop();
        //    tm.Dispose();
           

        //    byte[] data = null;

        //    if (File.Exists(path))
        //    {
        //        data = File.ReadAllBytes(path);
        //        File.Delete(path);
        //    }

        //    Directory.Delete(temp_save, true);
        //    return data;
        //}

        #region Magnets From Cache Websites

        [XmlIgnore]
        private static Services.TorrentCache.ITorrentCache[] TorrentCaches = new Services.TorrentCache.ITorrentCache[] 
        {
            new Services.TorrentCache.TorCache(),
            new Services.TorrentCache.ZoinkIT(),     
            new Services.TorrentCache.Torrage()
        };

        public static byte[] GetMagnetFromCache(MagnetLink mg)
        {
            for (int i = 0; i < TorrentCaches.Length; i++)
            {
                byte[] res = TorrentCaches[i].Fetch(mg);

                if (res != null)
                    return res;
            }

            return null;
        }

        public static byte[] GetMagnetFromCache(string uri)
        {
            MagnetLink mg = null;

            try { mg = new MagnetLink(uri); }
            catch { return null; }

            return GetMagnetFromCache(mg);
        }

        #endregion

        public void NotifyChanged(params string[] props)
        {
            if (PropertyChanged == null)
                return;
            foreach (string str in props)
                PropertyChanged(this, new PropertyChangedEventArgs(str));
        }

        public void NotifySinglePropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
