using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Web;
using System.Windows.Media;
using ByteFlood.Services.MoviesDatabases;
using Ragnar;

namespace ByteFlood
{
    public class TorrentInfo : INotifyPropertyChanged, IEquatable<TorrentInfo>
    {
        #region Properties and variables

        public static LanguageEngine Language { get { return App.CurrentLanguage; } }

        public State AppState { get { return this.MainAppWindow.state; } }

        public TorrentHandle Torrent { get; private set; }

        private TorrentStatus StatusData = null;

        public string Ratio
        {
            get
            {
                return RawRatio.ToString("0.000");
            }
        }

        public float RawRatio
        {
            get
            {
                if (this.StatusData.AllTimeDownload > 0)
                {
                    return Convert.ToSingle(this.StatusData.AllTimeUpload) / Convert.ToSingle(this.StatusData.AllTimeDownload);
                }
                return 0;
            }
        }

        private static TimeSpan ZeroTimeSpan = TimeSpan.FromSeconds(0);

        public TimeSpan ETA
        {
            get
            {
                if (this.Torrent.IsPaused)
                {
                    return ZeroTimeSpan;
                }
                else
                {
                    if (this.StatusData.DownloadRate > 0)
                    {
                        return TimeSpan.FromSeconds(Convert.ToDouble(this.StatusData.TotalWanted - this.StatusData.TotalWantedDone) / this.StatusData.DownloadRate);
                    }
                    return ZeroTimeSpan;
                }
            }
        }

        public Brush ProgressBarColor
        {
            get
            {
                return Utility.GetBrushFromTorrentState(this.StatusData);
            }
        }

        public bool RanCommand { get; set; }

        public string SavePath
        {
            get { return this.StatusData.SavePath; }
        }

        public string RootDownloadDirectory
        {
            get
            {
                if (this.Torrent.TorrentFile.NumFiles > 1)
                {
                    return System.IO.Path.Combine(this.SavePath, this.Torrent.TorrentFile.Name);
                }
                else
                {
                    return this.SavePath;
                }
            }
        }

        private string _custom_name = null;
        public string Name
        {
            get
            {
                if (_custom_name == null)
                {
                    if (this.Torrent.HasMetadata)
                        return this.Torrent.TorrentFile.Name;
                    else
                        return "[metadata mode]";
                }
                else
                {
                    return _custom_name;
                }
            }
            set
            {
                _custom_name = value;
            }
        }

        public string TorrentFilePath
        {
            get
            {
                return System.IO.Path.Combine(State.TorrentsStateSaveDirectory, this.Torrent.InfoHash.ToHex() + ".torrent");
            }
        }


        private string _otfp = null;
        public string OriginalTorrentFilePath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_otfp))
                {
                    return this.TorrentFilePath;
                }
                return _otfp;
            }
            set
            {
                _otfp = value;
            }
        }

        public float Progress
        {
            get
            {
                return this.StatusData.Progress * 100;
            }
            set { }
        }

        public long Size
        {
            get
            {
                if (this.Torrent.HasMetadata)
                    return this.Torrent.TorrentFile.TotalSize;
                else
                    return -1;
            }
        }

        public int DownloadSpeed
        {
            get
            {
                return this.StatusData.DownloadRate;
            }
        }

        public int MaxDownloadSpeed
        {
            get
            {
                return this.Torrent.DownloadLimit;
            }
        }

        public int UploadSpeed
        {
            get
            {
                return this.StatusData.UploadRate;
            }
        }

        public int MaxUploadSpeed
        {
            get
            {
                return this.StatusData.UploadsLimit;
            }
        }

        public TimeSpan Elapsed
        {
            get
            {
                return this.StatusData.ActiveTime;
            }
        }

        public DateTime StartTime
        {
            get
            {
                return this.StatusData.AddedTime;
            }
        }

        public DateTime? CompletedOn
        {
            get
            {
                return this.StatusData.CompletedTime;
            }
        }

        public int PieceLength
        {
            get
            {
                if (this.Torrent.HasMetadata)
                    return this.Torrent.TorrentFile.PieceLength;
                else
                    return -1;
            }
        }

        public long WastedBytes { get { return this.StatusData.TotalFailedBytes; } }

        public long HashFails { get { return this.StatusData.TotalFailedBytes / this.PieceLength; } }

        public int Seeders { get { return this.StatusData.NumSeeds; } }

        public int Leechers { get { return this.StatusData.NumPeers; } }

        public long Downloaded { get { return this.StatusData.AllTimeDownload; } }

        public long Uploaded { get { return this.StatusData.AllTimeUpload; } }

        public long WantedBytesDone { get { return this.StatusData.TotalWantedDone; } }

        public long WantedBytes { get { return this.StatusData.TotalWanted; } }

        public string QueueNumber
        {
            get
            {
                if (this.Torrent.QueuePosition != -1)
                {
                    return this.Torrent.QueuePosition.ToString();
                }
                return string.Empty;
            }
        }

        public string Status
        {
            get
            {
                if (this.Torrent.IsPaused)
                {
                    if (!string.IsNullOrEmpty(this.StatusData.Error))
                    {
                        return this.StatusData.Error;
                    }

                    if (this.IsStopped)
                    {
                        return "Stopped";
                    }
                    else
                    {
                        return "Paused";
                    }
                }
                else
                {
                    switch (this.StatusData.State)
                    {
                        case TorrentState.Downloading:
                            return "Downloading";

                        case TorrentState.DownloadingMetadata:
                            return "Metadata";

                        case TorrentState.CheckingFiles:
                        case TorrentState.CheckingResumeData:
                            return "Checking files";

                        case TorrentState.Allocating:
                            return "Allocating files";

                        case TorrentState.Finished:
                            return "Finished";

                        case TorrentState.QueuedForChecking:
                            return "Queued for files check";

                        case TorrentState.Seeding:
                            return "Seeding";

                        default:
                            return this.StatusData.State.ToString();
                    }
                }
            }
        }

        public int PeerCount
        {
            get
            {
                return this.Seeders + this.Leechers;
            }
        }

        public string CompletionCommand { get; set; }

        public bool HasMetadata { get { return this.Torrent.HasMetadata; } }

        public bool ShowOnList
        {
            get
            {
                if (Invisible || Torrent == null)
                    return false;
                
                if (this.MainAppWindow.state.LabelManager.Can_I_ShowUP(this))
                    return this.MainAppWindow.itemselector(this);

                return false;
            }
        }

        public bool Invisible { get; set; }

        public float RatioLimit { get; set; }

        public float AverageDownloadSpeed { get { return downspeeds.Count == 0 ? 0 : downspeeds.Average(); } }

        public float AverageUploadSpeed { get { return upspeeds.Count == 0 ? 0 : upspeeds.Average(); } }

        //public ObservableDictionary<string, PeerInfo> Peers = new ObservableDictionary<string, PeerInfo>();

        public List<FileInfo> FileInfoList = new List<FileInfo>();

        public ObservableCollection<TrackerInfo> Trackers { get; private set; }

        public DirectoryKey FilesTree { get; private set; }

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

        List<float> upspeeds = new List<float>();

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

        List<float> downspeeds = new List<float>();

        private MainWindow MainAppWindow { get; set; }

        public IMDBSRSerializeable<IMovieDBSearchResult> PickedMovieData { get; set; }

        public bool IsComplete
        {
            get
            {
                return this.Torrent.IsFinished;
            }
        }

        public string InfoHash
        {
            get
            {
                return this.Torrent.InfoHash.ToHex();

                /*if (true) // TODO: Find a way to make Ragnar work as a project/implement IsValid
                {
                    return this.Torrent.InfoHash.ToHex();
                }
                return null;*/
            }
        }

        public string Label 
        {
            get 
            {
                return AppState.LabelManager.GetFirstLabelForTorrent(this);
            }
        }

        #endregion

        public TorrentInfo(TorrentHandle t)
        {
            RanCommand = false;
            this.Torrent = t;
            this.StatusData = t.QueryStatus();

            this.MainAppWindow = (App.Current.MainWindow as MainWindow);
            this.PickedMovieData = new IMDBSRSerializeable<IMovieDBSearchResult>();
            this.Trackers = new ObservableCollection<TrackerInfo>();

            if (t.HasMetadata)
            {
                SetupTorrent();
            }

            PopulateTrackerList();
        }

        private bool _torrent_initialized = false;

        private void SetupTorrent()
        {
            if (this._torrent_initialized) { return; }
            PopulateFileList();
            LoadMiscSettings();
            this._torrent_initialized = true;
        }

        private void LoadMiscSettings()
        {
            string TJSON_FILE = System.IO.Path.Combine(State.TorrentsStateSaveDirectory, this.InfoHash + ".tjson");

            if (System.IO.File.Exists(TJSON_FILE))
            {
                using (var reader = System.IO.File.OpenText(TJSON_FILE))
                {
                    var jo = Jayrock.Json.Conversion.JsonConvert.Import<Jayrock.Json.JsonObject>(reader);

                    this.RatioLimit = Convert.ToSingle(jo["RatioLimit"]);
                    this.RanCommand = Convert.ToBoolean(jo["RanCommand"]);
                    this.CompletionCommand = Convert.ToString(jo["CompletionCommand"]);
                    this.Name = Convert.ToString(jo["CustomName"]);
                    this._otfp = Convert.ToString(jo["OriginalTorrentFilePath"]);
                    this.IsStopped = Convert.ToBoolean(jo["IsStopped"]);

                    Jayrock.Json.JsonArray labels = jo["Labels"] as Jayrock.Json.JsonArray;
                    if (labels != null)
                    {
                        foreach (var a in labels)
                        {
                            this.MainAppWindow.state.LabelManager.AddLabelForTorrent(this, a.ToString());
                        }
                    }
                }
            }
            else
            {
                this.RatioLimit = 0;
                this.RanCommand = false;
                this.CompletionCommand = null;
                this._custom_name = null;
                this._otfp = null;
                this.IsStopped = false;
            }

            this.TorrentSettings = new TorrentProperties()
            {
                EnablePeerExchange = true,
                UploadSlots = this.Torrent.MaxUploads,
                UseDHT = true,
                MaxConnections = this.Torrent.MaxConnections,
                MaxDownloadSpeed = this.Torrent.DownloadLimit,
                MaxUploadSpeed = this.Torrent.UploadLimit,
                OnFinish = this.CompletionCommand,
                RatioLimit = this.RatioLimit
            };
        }

        #region Event Handling

        public void DoStateChanged(Ragnar.TorrentState oldstate, Ragnar.TorrentState newstate)
        {
            UpdateList("Status");
            if (this.Torrent.NeedSaveResumeData())
            {
                this.Torrent.SaveResumeData();
            }
        }

        public void DoTorrentComplete()
        {
            if (this.Torrent.IsFinished && !string.IsNullOrWhiteSpace(this.CompletionCommand) && !RanCommand)
            {
                try
                {
                    this.MainAppWindow.NotifyIcon.ShowBalloonTip(
                        "ByteFlood", string.Format("'{0}' has been completed.", this.Name), Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
                    string command = this.CompletionCommand.Replace("%s", this.Name)
                                                      .Replace("%p", System.IO.Path.GetFullPath(this.TorrentFilePath))
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
        }

        public void DoStatsUpdate(Ragnar.TorrentStatus stats)
        {
            var previous = this.StatusData;
            this.StatusData = stats;
            previous.Dispose();
            Update();
            UpdateGraphData();
        }

        public void DoMetadataDownloadComplete()
        {
            SetupTorrent();
            // Notify WPF bindings that the following properties were changed.
            UpdateList("FilesTree", "Name", "PieceLength", "Size", "WantedBytes");
        }

        #endregion

        public TorrentProperties TorrentSettings { get; private set; }

        public void ApplyTorrentSettings(TorrentProperties props)
        {
            this.TorrentSettings = props;

            this.Torrent.MaxUploads = props.UploadSlots;
            this.Torrent.MaxConnections = props.MaxConnections;
            this.Torrent.DownloadLimit = props.MaxDownloadSpeed;
            this.Torrent.UploadLimit = props.MaxUploadSpeed;
            this.CompletionCommand = props.OnFinish;
            this.RatioLimit = props.RatioLimit;

            this.UpdateList("TorrentSettings", "MaxDownloadSpeed", "MaxUploadSpeed", "RatioLimit");
        }

        public void ChangeSavePath(string newpath, TorrentHandle.MoveFlags flags = TorrentHandle.MoveFlags.DontReplace)
        {
            this.Torrent.MoveStorage(newpath, flags);
            this.StatusData = this.Torrent.QueryStatus();
        }

        /*
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
        
        #endregion*/

        public bool IsStopped { get; private set; }

        public void Stop()
        {
            this.Torrent.AutoManaged = false;
            this.Torrent.Pause();
            this.IsStopped = true;
        }

        public void ForcePause()
        {
            this.Torrent.AutoManaged = false;
            this.Torrent.Pause();
            this.IsStopped = false;
        }

        public void ForceStart()
        {
            this.Torrent.AutoManaged = false;
            this.Torrent.Resume();
            this.IsStopped = false;
        }

        /// <summary>
        /// Queue-start
        /// </summary>
        public void Start()
        {
            this.Torrent.AutoManaged = true;
            this.Torrent.Resume();
            this.IsStopped = false;
        }

        public void Pause()
        {
            this.Torrent.AutoManaged = false;
            this.Torrent.Pause();
            this.IsStopped = false;
        }

        public void UpdateGraphData()
        {
            if (this.downspeeds.Count == 50)
            {
                this.downspeeds.RemoveAt(0);
            }
            this.downspeeds.Add(this.DownloadSpeed);

            if (this.upspeeds.Count == 50)
            {
                this.upspeeds.RemoveAt(0);
            }
            this.upspeeds.Add(this.UploadSpeed);
        }

        public void Recheck()
        {
            this.Torrent.ForceRecheck();
            this.IsStopped = false;
        }

        public void OffMyself() // Dispose
        {
            this.Torrent.Dispose();
        }

        public string GetMagnetLink()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("magnet:?xt=urn:btih:");
            sb.Append(this.Torrent.InfoHash.ToHex());
            sb.AppendFormat("&dn={0}", HttpUtility.UrlEncode(this.Name));

            foreach (TrackerInfo tracker in this.Trackers)
                sb.AppendFormat("&tr={0}", HttpUtility.UrlPathEncode(tracker.URL));

            return sb.ToString();
        }

        public void Update()
        {
            try // I hate having to do this
            {
                if (this.RawRatio >= this.RatioLimit && this.RatioLimit != 0)
                {
                    this.Pause();
                }

                if (this.Torrent.QueryStatus().State == TorrentState.Downloading)
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(UpdateFileList));
                }

                //ThreadPool.QueueUserWorkItem(new WaitCallback(UpdatePeerList));
                ThreadPool.QueueUserWorkItem(new WaitCallback(UpdateTrackerList));

                UpdateList(
                    "DownloadSpeed", "UploadSpeed",
                    "PeerCount", "Seeders",
                    "Leechers", "Downloaded",
                    "Uploaded", "Progress",
                    "Ratio", "ETA",
                    "Elapsed",
                    "WantedBytesDone",
                    "WastedBytes", "HashFails",
                    "AverageDownloadSpeed", "AverageUploadSpeed",
                    "ShowOnList", "ProgressBarColor",
                    "Status");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine(ex.StackTrace);
            }
        }

        private bool is_lmdif_loading_data = false;
        public void LoadMovieDataIntoFolder()
        {
            if (this.PickedMovieData != null && this.PickedMovieData.Value != null)
            {
                if (is_lmdif_loading_data) { return; }
                string save_path = System.IO.Path.Combine(this.RootDownloadDirectory, "folder.jpg");
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
                                    break;
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
            if (this.FilesTree == null)
            {
                if (this.Torrent != null)
                {
                    DirectoryKey base_dir = new DirectoryKey("/", this);

                    for (int i = 0; i < this.Torrent.TorrentFile.NumFiles; i++)
                    {
                        FileEntry file = this.Torrent.TorrentFile.FileAt(i);
                        DirectoryKey.ProcessFile(file.Path, base_dir, this, file, i);
                    }

                    this.FilesTree = base_dir;
                    UpdateList("FilesTree");
                    this.files_progresses = new long[this.FileInfoList.Count];
                }
            }
        }

        private void PopulateTrackerList()
        {
            var trackers = this.Torrent.GetTrackers();

            foreach (var tracker in trackers)
            {
                this.Trackers.Add(new TrackerInfo(tracker, this));
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

        /*private void UpdatePeerList(object obj)
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
        }*/

        long[] files_progresses = null;

        // This is only called when the torrent is in Downloading State
        private void UpdateFileList(object obj)
        {
            if (this.files_progresses != null)
            {
                this.files_progresses = this.Torrent.GetFileProgresses();

                for (int i = 0; i < this.files_progresses.Length; i++)
                {
                    this.FileInfoList[i].DownloadedBytes = this.files_progresses[i];
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void UpdateList(params string[] columns)
        {
            foreach (string str in columns)
                UpdateSingle(str);
        }

        public void UpdateSingle([CallerMemberName] string name = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(name));
        }

        #endregion

        public bool Equals(TorrentInfo other)
        {
            if (other != null)
            {
                return this.InfoHash == other.InfoHash;
            }
            return false;
        }
    }
}