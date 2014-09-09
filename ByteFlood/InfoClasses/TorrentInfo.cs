using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using MonoTorrent;
using MonoTorrent.Client;
using MonoTorrent.Dht;
using MonoTorrent.Dht.Listeners;
using MonoTorrent.Common;
using MonoTorrent.Client.Encryption;
using Microsoft.Win32;
using System.Threading;
using System.Xml.Serialization;
using System.Xml;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Web;

namespace ByteFlood
{
    public class TorrentInfo : INotifyPropertyChanged
    {
        #region Properties and variables
        public event PropertyChangedEventHandler PropertyChanged;
        [XmlIgnore]
        public TorrentManager Torrent { get; private set; }
        [XmlIgnore]
        public string Ratio { get { return RawRatio.ToString("0.000"); } }
        [XmlIgnore]
        public TimeSpan ETA { get; private set; }
        public string Path = "";
        public string SavePath = "";
        public TorrentSettings TorrentSettings { get; set; }
        public string Name { get; set; }
        public double Progress { get { return Torrent.Progress; } set { } }
        public long Size { get { return Torrent.Torrent.Size; } }
        public int DownloadSpeed { get { return Torrent.Monitor.DownloadSpeed; } }
        public int MaxDownloadSpeed { get { return Torrent.Settings.MaxDownloadSpeed; } }
        public int MaxUploadSpeed { get { return Torrent.Settings.MaxUploadSpeed; } }
        public int UploadSpeed { get { return Torrent.Monitor.UploadSpeed; } }
        public TimeSpan Elapsed { get { return DateTime.Now.Subtract(StartTime); } }
        public DateTime StartTime { get; set; }
        public long PieceLength { get { return Torrent.Torrent.PieceLength; } }
        public int HashFails { get { return Torrent.HashFails; } }
        public long WastedBytes { get { return PieceLength * HashFails; } }
        public int Seeders { get { return Torrent.Peers.Seeds; } }
        public int Leechers { get { return Torrent.Peers.Leechs; } }
        public long Downloaded { get { return GetDownloadedBytes(); } }
        public long Uploaded { get; set; }
        public string Status { get { return Torrent.State.ToString(); } }
        public TorrentState SavedTorrentState { get; set; }
        public int PeerCount { get { return Seeders + Leechers; } }
        public string CompletionCommand { get; set; }
        public long SizeToBeDownloaded { get { return Torrent.Torrent.Files.Select<TorrentFile, long>(t => t.Priority != Priority.Skip ? t.Length : 0).Sum(); } }
        public bool ShowOnList
        {
            get
            {
                if (Torrent == null)
                    return false;
                return Invisible || ((MainWindow)App.Current.MainWindow).itemselector(this);
            }
        }
        public bool Invisible { get; set; }
        public float RawRatio { get; set; }
        public float RatioLimit { get; set; }
        [XmlIgnore]
        public float AverageDownloadSpeed { get { return downspeeds.Count == 0 ? 0 : downspeeds.Average(); } set { } }
        [XmlIgnore]
        public float AverageUploadSpeed { get { return upspeeds.Count == 0 ? 0 : upspeeds.Average(); } set { } }
        [XmlIgnore]
        public ObservableDictionary<string, PeerInfo> Peers = new ObservableDictionary<string, PeerInfo>();
        [XmlIgnore]
        //public PieceInfo[] Pieces { get; set; }
        public ObservableCollection<PieceInfo> Pieces = new ObservableCollection<PieceInfo>();

        [XmlIgnore]
        public List<FileInfo> FileInfoList = new List<FileInfo>();

        //Because dictionary is not xml serializable
        public List<FilePriority> FilesPriorities = new List<FilePriority>();

        [XmlIgnore]
        public ObservableCollection<TrackerInfo> Trackers = new ObservableCollection<TrackerInfo>();

        [XmlIgnore]
        public DirectoryKey FilesTree { get; private set; }
        private SynchronizationContext context;
        [XmlIgnore]
        public List<float> DownSpeeds
        {
            get
            {
                var t = downspeeds.Skip(downspeeds.Count - 50);
                if (t.Count() < 50)
                {
                    int count = 50 - t.Count();
                    var f = Enumerable.Repeat<float>(0f, count);
                    t = f.Concat(t);
                }
                return t.ToList();
            }
        }
        [XmlIgnore]
        public List<float> upspeeds = new List<float>();
        [XmlIgnore]
        public List<float> UpSpeeds
        {
            get
            {
                var t = upspeeds.Skip(downspeeds.Count - 50);
                if (t.Count() < 50)
                {
                    int count = 50 - t.Count();
                    var f = Enumerable.Repeat<float>(0f, count);
                    t = f.Concat(t);
                }
                return t.ToList();
            }
        }

        private ParallelOptions parallel = new ParallelOptions()
        {
            MaxDegreeOfParallelism = 8 // 8 seems like a good ground, even for single core machines.
        };
        [XmlIgnore]
        public List<float> downspeeds = new List<float>();
        private long up_previous = 0;
        #endregion

        public TorrentInfo() // this is reserved for the XML deserializer.
        {
        }

        public TorrentInfo(SynchronizationContext c, TorrentManager tm)
        {
            context = c;
            Name = "";
            StartTime = DateTime.Now;
            this.Torrent = tm;
            this.Torrent.TorrentStateChanged += new EventHandler<TorrentStateChangedEventArgs>(Torrent_TorrentStateChanged);
            //this.Pieces = new PieceInfo[this.Torrent.Torrent.Pieces.Count]; 
        }

        #region Event Handling

        [XmlIgnore]
        private bool events_hooked = false;

        private void TryHookEvents()
        {
            if (events_hooked)
                return;
            try
            {
                if (this.Torrent == null) { return; }

                Torrent.PieceHashed += new EventHandler<PieceHashedEventArgs>(PieceHashed);

                Torrent.PeerConnected += new EventHandler<PeerConnectionEventArgs>(Torrent_PeerConnected);
                Torrent.PeerDisconnected += new EventHandler<PeerConnectionEventArgs>(Torrent_PeerDisconnected);

                Torrent.TorrentStateChanged += new EventHandler<TorrentStateChangedEventArgs>(Torrent_TorrentStateChanged);

                events_hooked = true;
            }
            catch { }
        }

        private void Torrent_TorrentStateChanged(object sender, TorrentStateChangedEventArgs e)
        {
            this.SavedTorrentState = e.NewState;
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (e.NewState != TorrentState.Downloading)
                {
                    this.ETA = new TimeSpan(0, 0, 0);
                }
                if (e.TorrentManager.Complete && !string.IsNullOrWhiteSpace(CompletionCommand))
                {
                    try
                    {
                        (App.Current.MainWindow as MainWindow).NotifyIcon.ShowBalloonTip(
                            "ByteFlood", string.Format("'{0}' has been completed.", this.Name), Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
                        string command = CompletionCommand.Replace("%s", this.Name)
                                                          .Replace("%p", this.Path)
                                                          .Replace("%d", this.SavePath);
                        ProcessStartInfo psi = Utility.ParseCommandLine(command);
                        Process.Start(psi);
                    }
                    catch
                    {
                        // Let's keep this secret to our graves
                    }
                }
                if (PropertyChanged != null)
                {
                    UpdateList("ETA", "Status");
                }
            }));
        }

        private void Torrent_PeerConnected(object sender, PeerConnectionEventArgs e)
        {
            if (!Peers.ContainsKey(e.PeerID.PeerID))
            {
                PeerInfo pi = new PeerInfo()
                {
                    AddressBytes = e.PeerID.AddressBytes,
                    Client = e.PeerID.ClientApp.Client == Client.Unknown ? e.PeerID.ClientApp.ShortId : e.PeerID.ClientApp.Client.ToString(),
                    IP = e.PeerID.Uri.ToString(),
                    PieceInfo = string.Format("{0}/{1}", e.PeerID.PiecesReceived, e.PeerID.PiecesSent),
                    Encryption = (e.PeerID.Encryptor is PlainTextEncryption ? "None" : "RC4")
                };
                this.context.Send(x => { Peers.Add(e.PeerID.PeerID, pi); }, null);
            }
        }

        private void Torrent_PeerDisconnected(object sender, PeerConnectionEventArgs e)
        {
            this.context.Send(x =>
            {
                if (this.Peers.ContainsKey(e.PeerID.PeerID))
                {
                    this.Peers.Remove(e.PeerID.PeerID);
                }
            }, null);
        }

        private void PieceHashed(object sender, PieceHashedEventArgs e)
        {
            //if (this.Pieces[e.PieceIndex] != null)
            //{
            //    PieceInfo p = this.Pieces[e.PieceIndex];
            //    p.Finished = e.HashPassed;
            //}
            //else 
            //{
            //    this.Pieces[e.PieceIndex] = new PieceInfo() { Finished = e.HashPassed, ID = e.PieceIndex };
            //}
            if (e.HashPassed)
            {
                try
                {
                    var results = Pieces.Where(t => t.ID == e.PieceIndex);
                    if (results.Count() != 0)
                    {
                        int index = Pieces.IndexOf(results.ToList()[0]);
                        context.Send(x => Pieces[index].Finished = true, null);
                        return;
                    }
                }
                catch (InvalidOperationException) { }
            }
            PieceInfo pi = new PieceInfo();
            pi.ID = e.PieceIndex;
            pi.Finished = e.HashPassed;
            context.Send(x => Pieces.Add(pi), null);
        }

        #endregion

        public void Stop()
        {
            Torrent.Stop();
            this.Peers.Clear();
        }
        public void Start()
        {
            Torrent.Start();
        }
        public void UpdateGraphData()
        {
            downspeeds.Add(Torrent.Monitor.DownloadSpeed);
            upspeeds.Add(Torrent.Monitor.UploadSpeed);
        }

        public void Pause()
        {
            Torrent.Pause();
        }


        public void OffMyself() // Dispose
        {
            if (Torrent.State != TorrentState.Stopped)
                Torrent.Stop();
            Torrent.Dispose();
        }

        public string GetMagnetLink()
        {
            string str = "magnet:?xt=urn:btih:";
            str += this.Torrent.Torrent.InfoHash.ToHex().Replace("-", "");
            str += "&dn=" + HttpUtility.UrlEncode(this.Name);
            foreach (TrackerInfo tracker in this.Trackers)
                str += "&tr=" + HttpUtility.UrlPathEncode(tracker.URL);
            return str;
        }

        public long GetDownloadedBytes() // I have to use this because Torrent.Monitor only shows bytes downloaded in this session
        {
            long ret = 0;
            foreach (TorrentFile file in Torrent.Torrent.Files)
            {
                ret += file.CheckedBytes;
            }
            ret += Torrent.Monitor.DataBytesDownloaded;
            return ret;
        }

        public void Update()
        {
            if (this.Torrent == null)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    MainWindow mw = Application.Current.MainWindow as MainWindow;
                    this.context = mw.uiContext;

                    this.Torrent = new TorrentManager(MonoTorrent.Common.Torrent.Load(this.Path), SavePath, TorrentSettings, false);
                    mw.state.ce.Register(this.Torrent);

                    TorrentState[] StoppedStates = { TorrentState.Stopped, TorrentState.Stopping, TorrentState.Error };
                    if (!StoppedStates.Contains(this.SavedTorrentState))
                    {
                        this.Start();
                    }
                }));
            }

            TryHookEvents();
            try // I hate having to do this
            {
                UpdateProperties();
                PopulateFileList();
                PopulateTrackerList();
                //context.Send(x => Peers.Clear(), null);
                ThreadPool.QueueUserWorkItem(new WaitCallback(UpdateFileList));
                ThreadPool.QueueUserWorkItem(new WaitCallback(UpdatePeerList));
                ThreadPool.QueueUserWorkItem(new WaitCallback(UpdateTrackerList));
                UpdateList("DownloadSpeed",
                    "UploadSpeed",
                    "PeerCount",
                    "Seeders",
                    "Leechers",
                    "Downloaded",
                    "Uploaded",
                    "Progress",
                    "Ratio",
                    "ETA",
                    "Size",
                    "Elapsed",
                    "TorrentSettings",
                    "WastedBytes",
                    "HashFails",
                    "AverageDownloadSpeed",
                    "AverageUploadSpeed",
                    "MaxDownloadSpeed",
                    "MaxUploadSpeed",
                    "ShowOnList");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine(ex.StackTrace);
            }
        }

        private void PopulateFileList()
        {
            if (this.FilesTree == null)
            {
                if (this.Torrent != null)
                {
                    TorrentFile[] files = this.Torrent.Torrent.Files;

                    DirectoryKey base_dir = new DirectoryKey("/");

                    foreach (var file in files)
                    {
                        DirectoryKey.ProcessFile(file.Path, base_dir, this, file);
                    }

                    this.FilesTree = base_dir;
                    UpdateList("FilesTree");
                }
            }
        }
        [XmlIgnore]
        private bool trackers_populated = false;
        private void PopulateTrackerList()
        {
            if (!trackers_populated)
            {
                if (this.Torrent != null)
                {
                    var tm = this.Torrent.TrackerManager;

                    foreach (MonoTorrent.Client.Tracker.TrackerTier tier in tm)
                    {
                        foreach (MonoTorrent.Client.Tracker.Tracker t in tier)
                        {
                            this.Trackers.Add(new TrackerInfo(t, this));
                        }
                    }
                    trackers_populated = true;
                }
            }
        }
        private void UpdateTrackerList(object obj)
        {
            foreach (TrackerInfo ti in this.Trackers)
            {
                ti.Update();
            }
        }

        private void UpdatePeerList(object obj)
        {
            var peerlist = Torrent.GetPeers();
            Parallel.ForEach(peerlist, parallel, peer =>
            {
                if (this.Peers.ContainsKey(peer.PeerID))
                {
                    PeerInfo pi = this.Peers[peer.PeerID];
                    pi.PieceInfo = string.Format("{0}/{1}", peer.PiecesReceived, peer.PiecesSent);
                    pi.Client = peer.ClientApp.Client == Client.Unknown ? peer.ClientApp.ShortId : peer.ClientApp.Client.ToString();
                    pi.Encryption = (peer.Encryptor is PlainTextEncryption ? "None" : "RC4");
                }
            });
        }
        private void UpdateFileList(object obj)
        {
            if (this.FilesTree != null)
            {
                Parallel.ForEach(this.FileInfoList, parallel, file =>
                {
                    file.Update();
                });
            }
        }
        #region SavedFilePriority

        public MonoTorrent.Common.Priority GetSavedFilePriority(FileInfo fi)
        {
            var results = FilesPriorities.Where(x => x.Key == fi.File.Path);
            if (results.Count() > 0)
            {
                return results.First().Value;
            }

            return fi.File.Priority;
        }

        public void SetSavedFilePriority(FileInfo fi, MonoTorrent.Common.Priority pr)
        {
            var results = FilesPriorities.Where(x => x.Key == fi.File.Path);
            if (results.Count() > 0)
            {
                this.FilesPriorities.Remove(results.First());
                this.FilesPriorities.Add(new FilePriority { Key = fi.File.Path, Value = pr });
            }
            else
            {
                this.FilesPriorities.Add(new FilePriority { Key = fi.File.Path, Value = pr });
            }
        }

        #endregion
        public void UpdateList(params string[] columns)
        {
            if (PropertyChanged == null)
                return;
            foreach (string str in columns)
                PropertyChanged(this, new PropertyChangedEventArgs(str));
        }

        private void UpdateProperties()
        {
            this.Path = Torrent.Torrent.TorrentPath;
            this.SavePath = Torrent.SavePath;
            this.TorrentSettings = Torrent.Settings;

            if (this.Torrent.State == TorrentState.Downloading)
            {
                var seconds = 0;
                if (this.DownloadSpeed > 0)
                {
                    seconds = Convert.ToInt32((this.Size - this.Downloaded) / this.DownloadSpeed);
                }
                this.ETA = new TimeSpan(0, 0, seconds);
            }

            Uploaded += Torrent.Monitor.DataBytesUploaded - up_previous;
            up_previous = Torrent.Monitor.DataBytesUploaded;
            this.RawRatio = ((float)Uploaded / (float)Downloaded);

            //if (!this.Torrent.Complete)
            //    this.RawRatio = ((float)Uploaded / (float)Torrent.Monitor.DataBytesDownloaded);
            //else
            //    this.RawRatio = ((float)Torrent.Monitor.DataBytesUploaded / (float)GetDownloadedBytes()); // sad :(
            if (this.RawRatio >= this.RatioLimit && this.RatioLimit != 0)
            {
                this.Torrent.Settings.UploadSlots = 0;
            }
        }
    }
}
