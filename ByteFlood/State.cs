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
using MonoTorrent.Client.Connections;
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
            UpdateConnectionSettings();
            IPV4Connection.ExceptionThrown += Utility.LogException;
            //IPV4Connection.LocalAddress = new IPAddress(new byte[] { 127,0,0,1 });
            IPV4Connection.LocalAddress = IPAddress.Any;
            ce = new ClientEngine(new EngineSettings());
            dhtl = new DhtListener(new IPEndPoint(IPAddress.Any, App.Settings.ListeningPort));
            DhtEngine dht = new DhtEngine(dhtl);
            ce.Settings.Force = App.Settings.EncryptionType;
            ce.RegisterDht(dht);
            ce.DhtEngine.Start();

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

        public void UpdateConnectionSettings()
        {
            IPV4Connection.UseRandomPorts = App.Settings.OutgoingPortsRandom;
            if (!IPV4Connection.UseRandomPorts)
                IPV4Connection.LocalPorts = Enumerable.Range(App.Settings.OutgoingPortsStart, App.Settings.OutgoingPortsEnd - App.Settings.OutgoingPortsStart).ToArray();
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

        public void AddTorrentsByPath(string[] paths)
        {
            foreach (string str in paths)
            {
                AddTorrentByPath(str);
            }
        }

        public void AddTorrentByPath(string path)
        {
            try
            {
                //Torrent t = Torrent.Load(path);
                //string newfile = t.InfoHash.ToHex() + ".torrent";
                //string newpath = System.IO.Path.Combine(App.Settings.TorrentFileSavePath, newfile);
                //if (new DirectoryInfo(newpath).FullName != new DirectoryInfo(path).FullName)
                //    File.Copy(path, newpath, true);
                path = BackupTorrent(path);
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
                AddTorrentDialog atd = new AddTorrentDialog(path) { Owner = App.Current.MainWindow, Icon = App.Current.MainWindow.Icon };
                atd.ShowDialog();
                if (atd.UserOK)
                {
                    TorrentInfo ti = CreateTorrentInfo(atd.tm);
                    ti.Name = atd.TorrentName;
                    if (atd.AutoStartTorrent)
                    { ti.Start(); }
                    ti.RatioLimit = atd.RatioLimit;
                    TorrentProperties.Apply(ti.Torrent, App.Settings.DefaultTorrentProperties);
                    ti.Torrent.Settings.InitialSeedingEnabled = atd.initial.IsChecked == true;
                    Torrents.Add(ti);
                }
            }, null);
        }

        /// <summary>
        /// Copies a torrent to ./Torrents(or whatever Settings.TorrentFileSavePath is set to).
        /// </summary>
        /// <param name="path">The path of the torrent file to be copied.</param>
        /// <returns>The new path of the torrent file.</returns>
        public string BackupTorrent(string path)
        {
            Torrent t = Torrent.Load(path);
            string newfile = t.InfoHash.ToHex() + ".torrent";
            string newpath = System.IO.Path.Combine(App.Settings.TorrentFileSavePath, newfile);
            if (new DirectoryInfo(newpath).FullName != new DirectoryInfo(path).FullName)
                File.Copy(path, newpath, true);
            return newpath;
        }

        /// <summary>
        /// Like AddTorrentByPath, but uses a provided AddTorrentDialog
        /// </summary>
        public void AddTorrentByPath(string path, AddTorrentDialog atd)
        {
            uiContext.Send(x =>
            {
                App.Current.MainWindow.Activate();
                atd.Load(path);
                atd.Closed += (e, s) =>
                {
                    if (atd.UserOK)
                    {
                        TorrentInfo ti = CreateTorrentInfo(atd.tm);
                        ti.Name = atd.TorrentName;
                        if (atd.AutoStartTorrent)
                        { ti.Start(); }
                        ti.RatioLimit = atd.RatioLimit;
                        TorrentProperties.Apply(ti.Torrent, App.Settings.DefaultTorrentProperties);
                        ti.Torrent.Settings.InitialSeedingEnabled = atd.initial.IsChecked == true;
                        Torrents.Add(ti);
                    }
                };
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
                { ti.Start(); }
                Torrents.Add(ti);

            }, null);
            return success;
        }

        public void AddTorrentByMagnet(string magnet)
        {
            /*
             * MagnetLink mg = null;

            try { mg = new MagnetLink(magnet); }
            catch { MessageBox.Show("Invalid magnet link", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }

            if (!Directory.Exists(App.Settings.TorrentFileSavePath))
                Directory.CreateDirectory(App.Settings.TorrentFileSavePath);

            string path = System.IO.Path.Combine(App.Settings.TorrentFileSavePath, mg.InfoHash.ToHex().Replace("-", "") + ".torrent");

            AddTorrentDialog atd = new AddTorrentDialog("");
            atd.Show();

            byte[] data = GetMagnetFromCache(mg);
            if (data != null)
            {
                File.WriteAllBytes(path, data);
                this.AddTorrentByPath(path, atd);
                return;
            }

            ThreadPool.QueueUserWorkItem(delegate
            {
                uiContext.Send(x =>
                {
                    TorrentManager tm = new TorrentManager(mg, "./", new TorrentSettings(), path);

                    ce.Register(tm);
                    tm.Start();

                    ce.DhtEngine.GetPeers(mg.InfoHash);
                    int i = 0;

                    while (tm.State == TorrentState.Stopped)
                        Thread.Sleep(100);
                    while (tm.State == TorrentState.Metadata)
                    {
                        Thread.Sleep(100);
                        if((i++) % 100 == 0)
                            ce.DhtEngine.GetPeers(mg.InfoHash);
                    }

                    tm.Stop();
                    tm.Dispose();

                    this.AddTorrentByPath(path, atd);
                }, null);
            });
            return;
             * */
            MagnetLink mg = null;

            try { mg = new MagnetLink(magnet); }
            catch { MessageBox.Show("Invalid magnet link", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }

            if (!Directory.Exists(App.Settings.TorrentFileSavePath))
                Directory.CreateDirectory(App.Settings.TorrentFileSavePath);

            string path = System.IO.Path.Combine(App.Settings.TorrentFileSavePath, mg.InfoHash.ToHex() + ".torrent");

            AddTorrentDialog atd = new AddTorrentDialog("") { Icon = App.Current.MainWindow.Icon };
            atd.Show();

            ThreadPool.QueueUserWorkItem(delegate
            {
                byte[] data = GetMagnetFromCache(mg);
                if (data != null)
                {
                    File.WriteAllBytes(path, data);
                    this.AddTorrentByPath(path, atd);
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
                        if (atd.WindowClosed) //user cancelled the adding
                        {
                            tm.Stop();
                            while (tm.State == TorrentState.Stopping)
                                Thread.Sleep(10);
                            ce.Unregister(tm);
                            return;
                        }
                    }

                    tm.Stop();
                    tm.Dispose();
                    ce.Unregister(tm);

                    App.Current.Dispatcher.Invoke(new Action(() => { this.AddTorrentByPath(path, atd); }));
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
            string path = System.IO.Path.Combine(App.Settings.TorrentFileSavePath, hash + ".torrent");
            string temp_save = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache), hash);
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
