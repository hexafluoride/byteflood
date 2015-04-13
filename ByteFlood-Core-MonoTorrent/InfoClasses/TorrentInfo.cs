using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using MonoTorrent.Client;
using MonoTorrent.Common;
using MonoTorrent.Client.Encryption;
using MonoTorrent.BEncoding;
using System.Threading;
using System.Xml.Serialization;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Web;
using Jayrock.Json.Conversion;
//using System.Windows.Media;
using ByteFlood.Services.MoviesDatabases;

namespace ByteFlood
{
    // A few notes on this:
    // A paused/resumed torrent has the "Unprocessed" state for the queue manager to ignore it.
    // All "Unprocessed" torrents are turned into "Queued" ones in the next queue manager tick.
    // A force pause/force resume means that the queue manager should completely ignore this torrent.
    //
    // diantahoc: I suggest a new terminology:
    // Queued = The queue control the torrent.
    // NotQueed = The TorrentInfo control itself.
    public enum QueueState
    {
        Queued, NotQueued//, Forced, Unprocessed
    }
    public class TorrentInfo : INotifyPropertyChanged
    {
        #region Properties and variables

        [XmlIgnore]
        [JsonIgnore]
        public TorrentManager Torrent { get; private set; }
        [XmlIgnore]
        public string Ratio { get { return RawRatio.ToString("0.000"); } }
        [XmlIgnore]
        public TimeSpan ETA { get; private set; }
        //[XmlIgnore]
        //public Brush ProgressBarColor
        //{
        //    get
        //    {
        //        return Utility.GetBrushFromTorrentState(Torrent.State, Torrent.Complete);
        //    }
        //}
        public bool RanCommand { get; set; }
        public string Path = "";
        public string SavePath = "";
        public TorrentSettings TorrentSettings { get; set; }
        public string Name { get; set; }
        public double Progress { get { return Torrent.Progress; } set { } }
        [JsonIgnore]
        public string FastResumeData { get { return Convert.ToBase64String(Torrent.SaveFastResume().Encode().Encode()); } set { fastResumeData = value; } }
        [XmlIgnore]
        [JsonIgnore]
        private string fastResumeData;
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
        public long Downloaded { get { return this.Torrent.Monitor.DataBytesDownloaded; } }
        public long Uploaded { get; set; }
        public QueueState QueueState { get; set; }

        /*[XmlIgnore]
        public string QueueNumber 
        {
            get 
            {
                try
                {
                    return this.QueueManager.GetTorrentIndex(this).ToString();
                }
                catch { }
                return null;
            }
        } // set by the state
        */

        [XmlIgnore]
        public bool is_going_to_start = false;

        [XmlIgnore]
        public string Status
        {
            get
            {
                if (Program.Settings.EnableQueue)
                {
                    if (this.QueueState == ByteFlood.QueueState.Queued)
                    {
                        if (is_going_to_start)
                        {
                            return "Queued";
                        }
                        else
                        {
                            return this.Torrent.State.ToString();
                        }
                    }
                    else
                    {
                        //only in paused we show the [F] signe
                        if (this.Torrent.State == TorrentState.Paused || this.Torrent.State == TorrentState.Downloading)
                        {
                            return string.Format("[F] {0}", this.Torrent.State.ToString());
                        }
                        else
                        {
                            return this.Torrent.State.ToString();
                        }
                    }
                }
                else
                {
                    return this.Torrent.State.ToString();
                }
            }
        }

        [JsonIgnore]
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
                return Invisible /*||  this.MainAppWindow.itemselector(this) */;
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
        public Dictionary<string, PeerInfo> Peers = new Dictionary<string, PeerInfo>();
        [XmlIgnore]
        //public PieceInfo[] Pieces { get; set; }
        public ObservableCollection<PieceInfo> Pieces = new ObservableCollection<PieceInfo>();

        [XmlIgnore]
        public List<FileInfo> FileInfoList = new List<FileInfo>();

        //Because dictionary is not xml serializable
        public List<FilePriority> FilesPriorities = new List<FilePriority>();

        [XmlIgnore]
        public ObservableCollection<TrackerInfo> Trackers = new ObservableCollection<TrackerInfo>();

        //[XmlIgnore]
        //public DirectoryKey FilesTree { get; private set; }
        [JsonIgnore]
        [XmlIgnore]
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

        [JsonIgnore]
        [XmlIgnore]
        private ParallelOptions parallel = new ParallelOptions()
        {
            MaxDegreeOfParallelism = 8 // 8 seems like a good ground, even for single core machines.
        };
        [XmlIgnore]
        public List<float> downspeeds = new List<float>();
        private long up_previous = 0;
        //[XmlIgnore]
        //private MainWindow MainAppWindow { get; set; }

        public IMDBSRSerializeable<IMovieDBSearchResult> PickedMovieData { get; set; }

        [XmlIgnore]
        public bool IsComplete
        {
            get
            {
                if (this.Torrent != null)
                {
                    return this.Torrent.ActuallyComplete;
                }
                return false;
            }
        }

        [XmlIgnore]
        public string InfoHash
        {
            get
            {
                if (this.Torrent != null)
                {
                    return this.Torrent.Torrent.InfoHash.ToHex();
                }
                return null;
            }
        }

        //[XmlIgnore]
        //private TorrentQueue QueueManager = null;

        #endregion

        public TorrentInfo() // this is reserved for the XML deserializer.
        {
            //this.MainAppWindow = (App.Current.MainWindow as MainWindow);
            //this.context = this.MainAppWindow.uiContext;
            this.PickedMovieData = new IMDBSRSerializeable<IMovieDBSearchResult>();

            InitUpdateThread();
        }

        public TorrentInfo(SynchronizationContext c, TorrentManager tm)
        {
            context = c;
            Name = "";
            RanCommand = false;
            StartTime = DateTime.Now;
            this.Torrent = tm;
            //this.MainAppWindow = (App.Current.MainWindow as MainWindow);
            this.PickedMovieData = new IMDBSRSerializeable<IMovieDBSearchResult>();
            //this.QueueManager = this.MainAppWindow.state.tq;

            TryHookEvents();
            PopulateFileList();
            PopulateTrackerList();

            this.Path = Torrent.Torrent.TorrentPath;
            this.SavePath = Torrent.SavePath;
            this.TorrentSettings = Torrent.Settings;

            InitUpdateThread();
        }

        Thread bg = null;
        Thread graph_updater = null;
        private void InitUpdateThread()
        {
            bg = new Thread(() =>
            {
                while (true)
                {
                    this.Update();

                    Thread.Sleep(650);
                }
            });
            bg.IsBackground = true;
            bg.Priority = ThreadPriority.BelowNormal;
            bg.Start();

            //start a new thread that refresh each 1 second
            //for the graph data of course
            graph_updater = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        if (this.Torrent.State != TorrentState.Paused)
                        {
                            this.UpdateGraphData();
                        }
                    }
                    catch { }
                    Thread.Sleep(1000);
                }
            });
            graph_updater.IsBackground = true;
            graph_updater.Priority = ThreadPriority.Lowest;
            graph_updater.Start();
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
#if DEBUG
            catch (Exception ex)
            {
                //breakpoint here
                throw ex;
            }
#else
            catch { }
#endif

        }

        private void Torrent_TorrentStateChanged(object sender, TorrentStateChangedEventArgs e)
        {
            this.SavedTorrentState = e.NewState;
            ThreadPool.QueueUserWorkItem(delegate
            {
                if (e.NewState != TorrentState.Downloading)
                {
                    this.ETA = new TimeSpan(0, 0, 0);
                }
                if (e.TorrentManager.Complete && !string.IsNullOrWhiteSpace(CompletionCommand) && !RanCommand)
                {
                    try
                    {
                        //this.MainAppWindow.NotifyIcon.ShowBalloonTip(
                        //    "ByteFlood", string.Format("'{0}' has been completed.", this.Name), Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
                        string command = CompletionCommand.Replace("%s", this.Name)
                                                          .Replace("%p", System.IO.Path.GetFullPath(this.Path))
                                                          .Replace("%d", System.IO.Path.GetFullPath(this.SavePath));
                        ProcessStartInfo psi = Utility.ParseCommandLine(command);
                        Process.Start(psi);
                        RanCommand = true;
                    }
                    catch
                    {
                        // Let's keep this secret to our graves
                    }
                }
                UpdateList("ETA", "Status");
            });
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
            pi.Tooltip = get_pieceinfo_tooltip(e.PieceIndex);
            context.Send(x => Pieces.Add(pi), null);
        }

        private string get_pieceinfo_tooltip(int pieceindex)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Piece#: {0}\n", pieceindex);
            TorrentFile[] files = get_piece_files(pieceindex);
            if (files.Length > 1)
            {
                sb.Append("Files:\n");
                foreach (var f in files)
                {
                    sb.AppendFormat("- {0} ({1})\n", System.IO.Path.GetFileName(f.FullPath), Utility.PrettifyAmount(f.Length));
                }
            }
            else
            {
                sb.AppendFormat("File: {0} ({1})", System.IO.Path.GetFileName(files[0].FullPath), Utility.PrettifyAmount(files[0].Length));
            }
            return sb.ToString();
        }

        private TorrentFile[] get_piece_files(int pieceindex)
        {
            List<TorrentFile> files = new List<TorrentFile>();
            foreach (TorrentFile file in this.Torrent.Torrent.Files)
            {
                // startIndex <= pieceIndex <= endIndex
                if (pieceindex >= file.StartPieceIndex
                    && pieceindex <= file.EndPieceIndex)
                {
                    files.Add(file);
                }
            }
            return files.ToArray();
        }

        #endregion

        public void Stop()
        {
            //this.QueueManager.DeQueueTorrent(this);
            this.Torrent.Stop();
            this.Peers.Clear();
            UpdateList("Status");
        }

        public void ForcePause()
        {
            //QueueManager.DeQueueTorrent(this);
            this.Torrent.Pause();
        }

        public void ForceStart()
        {
            //QueueManager.DeQueueTorrent(this);
            this.Torrent.Start();
        }

        /// <summary>
        /// Queue-start
        /// </summary>
        public void Start()
        {
            //queuing the torrent WILL start it.
            //this.QueueManager.QueueTorrent(this);
        }

        public void Pause()
        {
            this.Torrent.Pause();
        }

        public void UpdateGraphData()
        {
            downspeeds.Add(Torrent.Monitor.DownloadSpeed);
            upspeeds.Add(Torrent.Monitor.UploadSpeed);
        }

        private bool _is_rechecking = false;
        public void Recheck()
        {
            if (_is_rechecking || this.Torrent.State == TorrentState.Hashing) { return; }
            Task.Factory.StartNew(new Action(() =>
            {
                _is_rechecking = true;
                if (this.Torrent != null)
                {
                    this.Torrent.Monitor.PreviousDataBytesDownloaded = 0;
                    bool autostart = this.Torrent.State == TorrentState.Downloading || this.Torrent.State == TorrentState.Seeding;
                    this.Torrent.Stop();
                    while (this.Torrent.State != TorrentState.Stopped)
                    {
                        this.Torrent.Stop();
                        Thread.Sleep(100);
                    }
                    this.Torrent.HashCheck(autostart);
                }
                _is_rechecking = false;
            }));
        }

        public void OffMyself() // Dispose
        {
            if (Torrent.State != TorrentState.Stopped)
                Torrent.Stop();
            Torrent.Dispose();
            bg.Abort();
            graph_updater.Abort();
        }

        public string GetMagnetLink()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("magnet:?xt=urn:btih:");
            sb.Append(this.Torrent.Torrent.InfoHash.ToHex());
            sb.AppendFormat("&dn={0}", HttpUtility.UrlEncode(this.Name));

            foreach (TrackerInfo tracker in this.Trackers)
                sb.AppendFormat("&tr={0}", HttpUtility.UrlPathEncode(tracker.URL));

            return sb.ToString();
        }

        [XmlIgnore]
        private bool TorrentNotLoaded = true;

        public void Update()
        {
            if (this.TorrentNotLoaded && this.Torrent == null)
            {
                this.TorrentNotLoaded = false;
                State t = null;

                //Setup some stuffs
                //App.Current.Dispatcher.Invoke(new Action(() =>
                //{
                //    t = this.MainAppWindow.state;
                //    this.QueueManager = t.tq;
                //}));

                //Load torrrent in the background, while not locking up the UI thread
                Task.Factory.StartNew(new Action(() =>
                {
                    this.Torrent = new TorrentManager(MonoTorrent.Common.Torrent.Load(this.Path), SavePath, TorrentSettings, false);
                    t.ce.Register(this.Torrent);
                    if (!string.IsNullOrWhiteSpace(fastResumeData))
                    {
                        byte[] fastresume = Convert.FromBase64String(fastResumeData);
                        this.Torrent.LoadFastResume(new FastResume((BEncodedDictionary)BEncodedDictionary.Decode(fastresume)));
                    }

                    TorrentState[] StoppedStates = { TorrentState.Stopped, TorrentState.Stopping, TorrentState.Error };
                    if (!StoppedStates.Contains(this.SavedTorrentState))
                    {
                        this.Start();
                    }

                    TryHookEvents();
                    PopulateFileList();
                    PopulateTrackerList();

                    this.Path = Torrent.Torrent.TorrentPath;
                    this.SavePath = Torrent.SavePath;
                    this.TorrentSettings = Torrent.Settings;

                    this.LoadMovieDataIntoFolder();
                }));
            }

            try // I hate having to do this
            {
                UpdateProperties();
                if (this.Torrent.State == TorrentState.Downloading)
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(UpdateFileList));
                }
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
                    "ShowOnList",
                    "ProgressBarColor");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine(ex.StackTrace);
            }
        }

        [XmlIgnore]
        private bool is_lmdif_loading_data = false;
        public void LoadMovieDataIntoFolder()
        {
            if (this.PickedMovieData != null && this.PickedMovieData.Value != null)
            {
                if (is_lmdif_loading_data) { return; }
                string save_path = System.IO.Path.Combine(this.SavePath, "folder.jpg");
                if (!System.IO.File.Exists(save_path))
                {
                    Task.Factory.StartNew(new Action(() =>
                    {
                        is_lmdif_loading_data = true;
                        int retry_count = 0;
                        while (true)
                        {
                            try
                            {
                                using (System.Net.WebClient nc = new System.Net.WebClient())
                                {
                                    byte[] data = nc.DownloadData(this.PickedMovieData.Value.PosterImageUri);
                                    System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(save_path));
                                    System.IO.File.WriteAllBytes(save_path, data);
                                }
                            }
                            catch (System.Net.WebException wex)
                            {
                                if (wex.Status == System.Net.WebExceptionStatus.NameResolutionFailure)
                                {
                                    break;
                                }
                                else
                                {
                                    if (retry_count > 5) { break; }
                                    retry_count++;
                                }
                            }
                            catch (Exception)
                            {
                                break;
                            }
                            Thread.Sleep(5000);
                        }
                        is_lmdif_loading_data = false;
                    }));
                }
                return;
            }
        }

        private void PopulateFileList()
        {
            //if (this.FilesTree == null)
            //{
            //    if (this.Torrent != null)
            //    {
            //        TorrentFile[] files = this.Torrent.Torrent.Files;

            //        DirectoryKey base_dir = new DirectoryKey("/", this);

            //        foreach (var file in files)
            //        {
            //            DirectoryKey.ProcessFile(file.Path, base_dir, this, file);
            //        }

            //        this.FilesTree = base_dir;
            //        UpdateList("FilesTree");
            //    }
            //}
        }

        private void PopulateTrackerList()
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
            }
        }

        #region ListsUpdaters

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
            //if (this.FilesTree != null)
            //{
            //    Parallel.ForEach(this.FileInfoList, parallel, file =>
            //    {
            //        file.Update();
            //    });
            //}
        }

        #endregion

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

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void UpdateList(params string[] columns)
        {
            if (PropertyChanged == null)
                return;
            foreach (string str in columns)
                PropertyChanged(this, new PropertyChangedEventArgs(str));
        }

        #endregion

        private void UpdateProperties()
        {
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
