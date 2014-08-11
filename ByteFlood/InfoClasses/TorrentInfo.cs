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
using Microsoft.Win32;
using System.Threading;

namespace ByteFlood
{
    public class TorrentInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public TorrentManager Torrent { get; set; }
        public string Name { get; set; }
        public int Progress { get; set; }
        public long Size { get; set; }
        public int DownloadSpeed { get; set; }
        public int UploadSpeed { get; set; }

        public int Seeders { get; set; }
        public int Leechers { get; set; }

        public long Downloaded { get; set; }

        public string Status { get; set; }

        public int PeerCount { get; set; }

        public string Ratio { get { return RawRatio.ToString("0.000"); } }

        public float RawRatio { get; set; }

        public float RatioLimit { get; set; }

        public TimeSpan ETA { get; private set; }

        public ObservableCollection<PeerInfo> Peers = new ObservableCollection<PeerInfo>();
        public ObservableCollection<FileInfo> Files = new ObservableCollection<FileInfo>();
        public ObservableCollection<PieceInfo> Pieces = new ObservableCollection<PieceInfo>();
        public ObservableCollection<TrackerInfo> Trackers = new ObservableCollection<TrackerInfo>();
        private bool hooked_pieces = false;
        private SynchronizationContext context;
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
        private List<float> upspeeds = new List<float>();
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
        private List<float> downspeeds = new List<float>();
        public TorrentInfo(SynchronizationContext c)
        {
            context = c;
            Name = "";
        }

        private void TryHookPieceHandler()
        {
            if (hooked_pieces)
                return;
            try
            {
                Torrent.PieceHashed += new EventHandler<PieceHashedEventArgs>(PieceHashed);
                hooked_pieces = true;
            }
            catch { }
        }
        public void Stop()
        {
            Torrent.Stop();
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
            // TorrentSettings ts = new TorrentSettings();

            Torrent.Pause();
        }
        void PieceHashed(object sender, PieceHashedEventArgs e)
        {
            if (e.HashPassed)
            {
                var results = Pieces.Where(t => t.ID == e.PieceIndex);
                if (results.Count() != 0)
                {
                    int index = Pieces.IndexOf(results.ToList()[0]);
                    context.Send(x => Pieces[index].Finished = true, null);
                    return;
                }
            }
            PieceInfo pi = new PieceInfo();
            pi.ID = e.PieceIndex;
            pi.Finished = e.HashPassed;
            context.Send(x => Pieces.Add(pi), null);
        }
        public void OffMyself() // Dispose
        {
            if (Torrent.State != TorrentState.Stopped)
                Torrent.Stop();
            Torrent.Dispose();
        }

        public long GetDownloadedBytes() // I have to use this because Torrent.Monitor only shows bytes downloaded in this session
        {
            long ret = 0;
            foreach (TorrentFile file in Torrent.Torrent.Files)
            {
                ret += file.BytesDownloaded;
            }
            return ret;
        }

        public void Update()
        {
            // TODO: Break this up into small pieces
            TryHookPieceHandler();
            try // I hate having to do this
            {
                this.Status = Torrent.State.ToString();
                
                this.Size = this.Torrent.Torrent.Size;

                this.Progress = Convert.ToInt32(Torrent.Progress);

                this.DownloadSpeed = Torrent.Monitor.DownloadSpeed;

                var seconds = 0;
                if (this.DownloadSpeed > 0)
                {
                    seconds = Convert.ToInt32(this.Size / this.DownloadSpeed);
                }
                this.ETA = new TimeSpan(0, 0, seconds);

                this.UploadSpeed = Torrent.Monitor.UploadSpeed;
                this.Seeders = Torrent.Peers.Seeds;
                this.Leechers = Torrent.Peers.Leechs;

                this.Downloaded = Torrent.Monitor.DataBytesDownloaded;

                this.PeerCount = Seeders + Leechers;
                if (!this.Torrent.Complete)
                    this.RawRatio = ((float)Torrent.Monitor.DataBytesUploaded / (float)Torrent.Monitor.DataBytesDownloaded);
                else
                    this.RawRatio = ((float)Torrent.Monitor.DataBytesUploaded / (float)GetDownloadedBytes()); // sad :(
                if (this.RawRatio >= this.RatioLimit && this.RatioLimit != 0)
                {
                    this.Torrent.Settings.UploadSlots = 0;
                }



                //context.Send(x => Peers.Clear(), null);
                var peerlist = Torrent.GetPeers();
                foreach (PeerId peer in this.Torrent.GetPeers())
                {
                    var results = Peers.Where(t => t.IP == peer.Uri.ToString());
                    int index = -1;
                    if (results.Count() != 0)
                        index = Peers.IndexOf(results.ToList()[0]);
                    PeerInfo pi = new PeerInfo();
                    pi.IP = peer.Uri.ToString();
                    pi.PieceInfo = peer.PiecesReceived + "/" + peer.PiecesSent;
                    pi.Client = peer.ClientApp.Client.ToString();
                    if (index == -1)
                        context.Send(x => Peers.Add(pi), null);
                    else
                        context.Send(x => Peers[index].SetSelf(pi), null);
                }
                for (int i = 0; i < peerlist.Count; i++)
                {
                    PeerInfo peer = Peers[i];
                    var results = peerlist.Where(t => t.Uri.ToString() == peer.IP);
                    if (results.Count() == 0)
                    {
                        context.Send(x => Peers.Remove(peer), null);
                    }
                }
                foreach (TorrentFile file in Torrent.Torrent.Files)
                {
                    var results = Files.Where(t => t.Name == file.FullPath);
                    int index = -1;
                    if (results.Count() != 0)
                        index = Files.IndexOf(results.ToList()[0]);
                    FileInfo fi = new FileInfo();
                    fi.Name = file.FullPath;
                    fi.Priority = (file.Priority.ToString() == "DoNotDownload" ? "Don't download" : file.Priority.ToString());
                    fi.Progress = (int)(((float)file.BytesDownloaded / (float)file.Length) * 100);
                    fi.RawSize = (uint)file.Length;
                    if (index == -1)
                        context.Send(x => Files.Add(fi), null);
                    else
                        context.Send(x => Files[index].SetSelf(fi), null);
                }
                foreach (var tracker in Torrent.Torrent.AnnounceUrls)
                {
                    foreach (string str in tracker)
                    {
                        var results = Trackers.Where(t => t.URL == str);
                        int index = -1;
                        if (results.Count() != 0)
                            index = Trackers.IndexOf(results.ToList()[0]);
                        TrackerInfo ti = new TrackerInfo();
                        ti.URL = str;
                        if (index == -1)
                            context.Send(x => Trackers.Add(ti), null);
                        else
                            context.Send(x => Trackers[index].SetSelf(ti), null);
                    }
                }
                if (PropertyChanged != null)
                {
                    UpdateList("DownloadSpeed",
                        "UploadSpeed",
                        "PeerCount",
                        "Seeders",
                        "Leechers",
                        "Downloaded",
                        "Progress",
                        "Status",
                        "Ratio", "ETA", "Size");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine(ex.StackTrace);
            }
        }
        public void UpdateList(params string[] columns)
        {
            foreach (string str in columns)
                PropertyChanged(this, new PropertyChangedEventArgs(str));
        }
    }
}
