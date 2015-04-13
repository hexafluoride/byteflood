using System;
using System.Linq;
//using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using MonoTorrent;
using MonoTorrent.Client;
using System.Xml.Serialization;
using MonoTorrent.Dht;
using MonoTorrent.Dht.Listeners;
using MonoTorrent.Common;
using MonoTorrent.Client.Connections;
using System.Threading;
using System.Net;
using System.IO;

using Logger = ByteFlood.Core.MonoTorrent.Logger;

namespace ByteFlood
{
    public class State : INotifyPropertyChanged
    {
        public Func<TorrentInfo, bool> ShowAll = new Func<TorrentInfo, bool>((t) => { return true; });
        public Func<TorrentInfo, bool> Downloading = new Func<TorrentInfo, bool>((t) => { return t.Torrent == null ? false : t.Torrent.State == TorrentState.Downloading; });
        public Func<TorrentInfo, bool> Seeding = new Func<TorrentInfo, bool>((t) => { return t.Torrent == null ? false : t.Torrent.State == TorrentState.Seeding; });
        public Func<TorrentInfo, bool> Active = new Func<TorrentInfo, bool>((t) => { return t.Torrent == null ? false : (t.Torrent.State == TorrentState.Seeding || t.Torrent.State == TorrentState.Downloading) || t.Torrent.State == TorrentState.Hashing; });
        public Func<TorrentInfo, bool> Inactive = new Func<TorrentInfo, bool>((t) => { return t.Torrent == null ? false : (t.Torrent.State != TorrentState.Seeding && t.Torrent.State != TorrentState.Downloading) && t.Torrent.State != TorrentState.Hashing; });
        public Func<TorrentInfo, bool> Finished = new Func<TorrentInfo, bool>((t) => { return t.Torrent == null ? false : t.Torrent.Progress == 100; });

        public ObservableCollection<TorrentInfo> Torrents = new ObservableCollection<TorrentInfo>();

        public event PropertyChangedEventHandler PropertyChanged;
        //[XmlIgnore]
        //public MainWindow window = (MainWindow)App.Current.MainWindow;
        [XmlIgnore]
        public ClientEngine ce;
        [XmlIgnore]
        public SynchronizationContext uiContext;
        public int DownloadingTorrentCount { get { return Torrents.Count(Downloading); } set { } }
        public int SeedingTorrentCount { get { return Torrents.Count(Seeding); } set { } }
        public int ActiveTorrentCount { get { return Torrents.Count(Active); } set { } }
        public int InactiveTorrentCount { get { return TorrentCount - ActiveTorrentCount; } set { } }
        public int FinishedTorrentCount { get { return Torrents.Count(Finished); } set { } }
        [XmlIgnore]
        public int TorrentCount { get { return Torrents.Count; } }

        [XmlIgnore]
        public Thread mainthread;
        [XmlIgnore]
        public DhtListener dhtl;
        [XmlIgnore]
        public Listener listener;
        [XmlIgnore]
        private int _dht_peers_count = 0;
        [XmlIgnore]
        public int DHTPeers
        {
            get { return _dht_peers_count; }
            set
            {
                if (value != _dht_peers_count)
                {
                    Logger.Log("DHT peer count changed, new count: " + value.ToString(), "STATE", 3);
                    this._dht_peers_count = value;
                    NotifySinglePropertyChanged("DHTPeers");
                }
            }
        }

        public int GlobalMaxDownloadSpeed { get { return ce.Settings.GlobalMaxDownloadSpeed; } set { ce.Settings.GlobalMaxDownloadSpeed = value; } }
        public int GlobalMaxUploadSpeed { get { return ce.Settings.GlobalMaxUploadSpeed; } set { ce.Settings.GlobalMaxUploadSpeed = value; } }


        public State()
        {
            this.Torrents.CollectionChanged += Torrents_CollectionChanged;
        }
        //public TorrentQueue tq = null;
        public void Initialize()
        {
            Logger.Log("Initializing state.", "STATE-INIT");

            UpdateConnectionSettings();
            IPV4Connection.ExceptionThrown += Utility.LogException;
            IPV4Connection.LocalAddress = IPAddress.Any;

            var iface = Utility.GetNetworkInterface(Program.Settings.NetworkInterfaceID);
            ce = new ClientEngine(new EngineSettings());
            ce.ChangeListenEndpoint(new IPEndPoint(iface.GetIPv4(), ce.Listener.Endpoint.Port));
            ce.Settings.Force = Program.Settings.EncryptionType;

            Logger.Log(string.Format("Got network interface, local IP: {0}", iface.GetIPv4().ToString()), "STATE-INIT", 2);

            ce.RegisterDht(get_dht_engine(iface.GetIPv4()));
            ce.DhtEngine.Start();

            Logger.Log("Registered and started DHT engine.", "STATE-INIT");

            // OFFLOAD TO UI
            //if (!App.Settings.AssociationAsked)
            //{
            //    bool assoc = Utility.Associated();
            //    if (!assoc)
            //    {
            //        if (MessageBox.Show("Do you want to associate ByteFlood with .torrent files?",
            //                 "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            //        {
            //            Utility.SetAssociation();
            //            App.Settings.AssociationAsked = true;
            //        }
            //        else if (MessageBox.Show("Do you want to be reminded about associations again?",
            //            "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            //            App.Settings.AssociationAsked = true;
            //        else
            //            App.Settings.AssociationAsked = false;
            //    }
            //}
            listener = new Listener(this);
            listener.State = this;
            //tq = new TorrentQueue(this);
        }

        /// <summary>
        /// return a new dht engine with ByteFlood settings
        /// </summary>
        /// <returns></returns>
        private DhtEngine get_dht_engine(IPAddress ip)
        {
            dhtl = new DhtListener(new IPEndPoint(ip, Program.Settings.ListeningPort));
            DhtEngine dht = new DhtEngine(dhtl);
            dht.PeersFound += new EventHandler<PeersFoundEventArgs>(PeersFound);
            return dht;
        }

        public void ChangeNetworkInterface()
        {
            var new_iface = Utility.GetNetworkInterface(Program.Settings.NetworkInterfaceID);
            ce.ChangeListenEndpoint(new IPEndPoint(new_iface.GetIPv4(), ce.Listener.Endpoint.Port));

            //stop the current dht engine
            ce.DhtEngine.Stop();
            ce.DhtEngine.Dispose();
            ce.DhtEngine.PeersFound -= PeersFound;
            this.DHTPeers = 0;

            //registering a new dht engine will override the old one
            ce.RegisterDht(get_dht_engine(new_iface.GetIPv4()));
            ce.DhtEngine.Start();
        }

        private void Torrents_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NotifySinglePropertyChanged("TorrentCount");
        }

        #region DHT Engine Events

        void PeersFound(object sender, PeersFoundEventArgs e)
        {
            DHTPeers += e.Peers.Count;
        }

        #endregion

        public void UpdateConnectionSettings()
        {
            IPV4Connection.UseRandomPorts = Program.Settings.OutgoingPortsRandom;
            if (!IPV4Connection.UseRandomPorts)
            {
                IPV4Connection.LocalPorts = Enumerable.Range(Program.Settings.OutgoingPortsStart, Program.Settings.OutgoingPortsEnd - Program.Settings.OutgoingPortsStart).ToArray();
                Logger.Log("Not using random ports.", "STATE-CONN-SETTINGS", 3);
            }
            else
                Logger.Log("Using random ports.", "STATE-CONN-SETTINGS");
        }

        public static void Save(State s, string path)
        {
            Utility.Serialize<State>(s, path);
        }

        public void Shutdown()
        {
            Logger.Log("Shutting down.", "STATE-SHUTDOWN");
            SaveSettings();
            SaveState();
            //mainthread.Abort();
            Logger.Log("Killed main thread.", "STATE-SHUTDOWN", 3);

            //Shouldn't we pause before attempting to dispose everything?
            ce.PauseAll();
            Logger.Log("Paused all torrents.", "STATE-SHUTDOWN", 3);

            ce.DiskManager.Flush();
            ce.DiskManager.Dispose();
            Logger.Log("Flushed remaining pieces.", "STATE-SHUTDOWN", 3);

            ce.Dispose();

            listener.Shutdown();
            Logger.Log("Shut down listener.", "STATE-SHUTDOWN", 3);

            Logger.Log("Shut down successfully!", "STATE-SHUTDOWN");

            Environment.Exit(0);
        }

        public void SaveSettings()
        {
            Logger.Log("Saving settings to \"./config.xml\".", "STATE", 3);
            Settings.Save(Program.Settings, "./config.xml");
            Logger.Log("Saved settings.", "STATE", 3);
        }

        public void SaveState()
        {
            Logger.Log("Saving state to \"./state.xml\".", "STATE", 3);
            State.Save(this, "./state.xml");
            Logger.Log("Saved state.", "STATE", 3);
        }

        public void AddTorrentsByPath(string[] paths)
        {
            Logger.Log(string.Format("Adding {0} torrents by path.", paths.Length), "STATE");
            foreach (string str in paths)
            {
                AddTorrentByPath(str, false);
            }
        }

        public void AddTorrentByPath(string path, bool notifyIfAdded = true)
        {
            try
            {
                Logger.Log(string.Format("Loading torrent from \"{0}\"", path), "STATE");
                Torrent t = Torrent.Load(path);

                Logger.Log(string.Format("Loaded torrent {0} successfully", t.InfoHash.ToHex()), "STATE", 3);

                if (this.ce.Contains(t.InfoHash)) 
                {
                    if (notifyIfAdded) 
                    {
                        //MessageBox.Show("This torrent is already added.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    return;
                }
                path = BackupTorrent(path, t);
            }
            catch (TorrentException)
            {
                //MessageBox.Show(string.Format("Invalid torrent file {0}", path), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            catch (Exception)
            {
                //MessageBox.Show(string.Format("Could not load torrent {0}", path), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            TorrentInfo ti = CreateTorrentInfo(new TorrentManager(Torrent.Load(path), Program.Settings.DefaultDownloadPath, new TorrentSettings()));
            Torrents.Add(ti);

                // OFFLOAD TO UI
                //App.Current.MainWindow.Activate();
                //AddTorrentDialog atd = new AddTorrentDialog(path) { Owner = App.Current.MainWindow, Icon = App.Current.MainWindow.Icon };
                //atd.ShowDialog();
                //if (atd.UserOK)
                //{
                //    TorrentInfo ti = CreateTorrentInfo(new TorrentManager(atd.t, atd.TorrentSavePath, new TorrentSettings()));
                //    ti.Name = atd.TorrentName;
                //    if (atd.AutoStartTorrent)
                //    { ti.Start(); }
                //    ti.RatioLimit = atd.RatioLimit;
                //    TorrentProperties.Apply(ti.Torrent, App.Settings.DefaultTorrentProperties);
                //    ti.Torrent.Settings.InitialSeedingEnabled = atd.initial.IsChecked == true;
                //    Torrents.Add(ti);
                //}
        }

        /// <summary>
        /// Copies a torrent to ./Torrents(or whatever Settings.TorrentFileSavePath is set to).
        /// </summary>
        /// <param name="path">The path of the torrent file to be copied.</param>
        /// <returns>The new path of the torrent file.</returns>
        public string BackupTorrent(string path, Torrent t)
        {
            string newfile = t.InfoHash.ToHex() + ".torrent";
            string newpath = System.IO.Path.Combine(Program.Settings.TorrentFileSavePath, newfile);
            if (new DirectoryInfo(newpath).FullName != new DirectoryInfo(path).FullName)
                File.Copy(path, newpath, true);
            return newpath;
        }

        /// <summary>
        /// Like AddTorrentByPath, but uses a provided AddTorrentDialog
        /// </summary>
        //private void AddTorrentByPath(string path, AddTorrentDialog atd)
        //{
        //    uiContext.Send(x =>
        //    {
        //        App.Current.MainWindow.Activate();
        //        atd.Load(path);
        //        atd.Closed += (e, s) =>
        //        {
        //            if (atd.UserOK)
        //            {
        //                TorrentInfo ti = CreateTorrentInfo(new TorrentManager(atd.t, atd.TorrentSavePath, new TorrentSettings()));
        //                ti.Name = atd.TorrentName;
        //                if (atd.AutoStartTorrent)
        //                { ti.Start(); }
        //                ti.RatioLimit = atd.RatioLimit;
        //                TorrentProperties.Apply(ti.Torrent, App.Settings.DefaultTorrentProperties);
        //                ti.Torrent.Settings.InitialSeedingEnabled = atd.initial.IsChecked == true;
        //                Torrents.Add(ti);
        //            }
        //        };
        //    }, null);

        //}

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
                if (this.ce.Contains(t.InfoHash)) 
                {
                    success = false;
                    return;
                }
                Directory.CreateDirectory(entry.DownloadDirectory);
                TorrentManager tm = new TorrentManager(t, entry.DownloadDirectory, entry.DefaultSettings);
                TorrentInfo ti = CreateTorrentInfo(tm);
                ti.Name = t.Name;
                if (entry.AutoDownload)
                { ti.Start(); }
                Torrents.Add(ti);

            }, null);
            return success;
        }

        public void AddTorrentByMagnet(string magnet, bool notifyIfAdded = true)
        {
            MagnetLink mg = null;

            //try { mg = new MagnetLink(magnet); }
            //catch { MessageBox.Show("Invalid magnet link", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }

            if (this.ce.Contains(mg.InfoHash))
            {
                if (notifyIfAdded)
                {
                    //MessageBox.Show("This torrent is already added.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                return;
            }

            if (!Directory.Exists(Program.Settings.TorrentFileSavePath))
                Directory.CreateDirectory(Program.Settings.TorrentFileSavePath);

            string path = System.IO.Path.Combine(Program.Settings.TorrentFileSavePath, mg.InfoHash.ToHex() + ".torrent");

            //AddTorrentDialog atd = new AddTorrentDialog("") { Icon = App.Current.MainWindow.Icon };
            //atd.Show();

            ThreadPool.QueueUserWorkItem(delegate
            {
                byte[] data = GetMagnetFromCache(mg);
                if (data != null)
                {
                    File.WriteAllBytes(path, data);
                    //this.AddTorrentByPath(path, atd);
                    return;
                }

                TorrentManager tm = new TorrentManager(mg, "./", new TorrentSettings(), path);

                ce.Register(tm);
                tm.Start();

                System.Threading.Tasks.Task.Factory.StartNew(new Action(() =>
                {
                    ce.DhtEngine.GetPeers(mg.InfoHash);
                    int i = 0;

                    while (tm.State == TorrentState.Stopped)
                        Thread.Sleep(100);
                    while (tm.State == TorrentState.Metadata)
                    {
                        Thread.Sleep(10);
                        if ((i++) % 1000 == 0)
                            ce.DhtEngine.GetPeers(mg.InfoHash);
                        //if (atd.WindowClosed) //user cancelled the adding
                        //{
                        //    tm.Stop();
                        //    while (tm.State == TorrentState.Stopping)
                        //        Thread.Sleep(10);
                        //    ce.Unregister(tm);
                        //    return;
                        //}
                    }

                    tm.Stop();
                    tm.Dispose();
                    ce.Unregister(tm);

                    //App.Current.Dispatcher.Invoke(new Action(() => { this.AddTorrentByPath(path, atd); }));
                }));
            });
            // return; why?
        }

        public byte[] MagnetLinkTorrentFile(string magnet)
        {
            MagnetLink mg = null;

            try { mg = new MagnetLink(magnet); }
            catch
            {
                return null;
            }

            string hash = mg.InfoHash.ToHex();
            string path = System.IO.Path.Combine(Program.Settings.TorrentFileSavePath, hash + ".torrent");
            string temp_save = System.IO.Path.Combine(System.IO.Path.GetTempPath(), hash);
            Directory.CreateDirectory(temp_save);
            TorrentManager tm = new TorrentManager(mg, temp_save, new TorrentSettings(), path);

            ce.Register(tm);
            tm.Start();

            ce.DhtEngine.GetPeers(mg.InfoHash);
            int i = 0;

            while (tm.State == TorrentState.Stopped)
                Thread.Sleep(100);
            while (tm.State == TorrentState.Metadata)
            {
                Thread.Sleep(100);
                if ((i++) % 100 == 0)
                    ce.DhtEngine.GetPeers(mg.InfoHash);
            }

            tm.Stop();
            tm.Dispose();
            ce.Unregister(tm);

            byte[] data = null;

            if (File.Exists(path))
            {
                data = File.ReadAllBytes(path);
                File.Delete(path);
            }

            Directory.Delete(temp_save, true);
            return data;
        }

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

        public TorrentInfo CreateTorrentInfo(TorrentManager tm)
        {
            ce.Register(tm);
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
            catch (Exception ex)
            {
                //MessageBox.Show("An error occurred while loading the program state. You may need to re-add your torrents.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return new State();

            }
        }

        /*public void UpdateQueue()
        {
            try
            {
                var applicable = Torrents.Where(t => t.QueueState != QueueState.Forced);
                var active = applicable.Where(t => t.Torrent.State == TorrentState.Downloading);
                active = active.Reverse();
                var inactive = applicable.Where(t => t.Torrent.State != TorrentState.Downloading);
                if (active.Count() > App.Settings.QueueSize) // we need to reduce the number of active torrents
                {
                    while (active.Count() > App.Settings.QueueSize)
                    {
                        TorrentInfo ti = active.First(t => t.QueueState == QueueState.Queued);
                        ti.Pause();
                    }
                }
                else if (active.Count() < App.Settings.QueueSize) // we need to increase the number of active torrents
                {
                    while (active.Count() < App.Settings.QueueSize)
                    {
                        TorrentInfo ti = inactive.First(t => t.QueueState == QueueState.Queued);
                        ti.Start();
                    }
                }
            }
            catch
            {
            }

            Torrents.Where(t => t.QueueState == QueueState.Unprocessed).ToList().ForEach(t => t.QueueState = QueueState.Queued);

            int i = 0;
            foreach (TorrentInfo ti in Torrents)
            {
                if (ti.QueueState == QueueState.Forced || ti.QueueState == QueueState.NotQueued)
                    ti.QueueNumber = "-";
                else
                    ti.QueueNumber = (++i).ToString();
                ti.UpdateList("QueueNumber");
            }
        }*/

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
