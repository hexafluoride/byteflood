/* 
    ByteFlood - A BitTorrent client.
    Copyright (C) 2014 Burak Öztunç

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
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
using MonoTorrent.Dht;
using MonoTorrent.Dht.Listeners;
using MonoTorrent.Common;
using Microsoft.Win32;
using System.Threading;
using System.Net;

namespace ByteFlood
{
    public class TorrentInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public TorrentManager Torrent { get; set; }
        public string Name { get; set; }
        public int Progress { get; set; }
        public string DownloadSpeed { get; set; } // can't be bothered with wrapper code to display kilobytes or mbytes etc.
        public string UploadSpeed { get; set; }
        public int Seeders { get; set; }
        public int Leechers { get; set; }
        public string Downloaded { get; set; }
        public string Status { get; set; }
        public int PeerCount { get; set; }
        public string Ratio { get { return RawRatio.ToString("0.000"); } }
        public float RawRatio { get; set; }
        public float RatioLimit { get; set; }
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
            catch {}
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
            TorrentSettings ts = new TorrentSettings();
            
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
                ret += file.BytesDownloaded;
            return ret;
        }
        public void Update()
        {
            // TODO: Break this up into small pieces
            TryHookPieceHandler();
            try // I hate having to do this
            {
                this.Status = Torrent.State.ToString();
                this.Progress = (int)Torrent.Progress;
                this.DownloadSpeed = Utility.PrettifySpeed(Torrent.Monitor.DownloadSpeed);
                this.UploadSpeed = Utility.PrettifySpeed(Torrent.Monitor.UploadSpeed);
                this.Seeders = Torrent.Peers.Seeds;
                this.Leechers = Torrent.Peers.Leechs;
                this.Downloaded = Utility.PrettifyAmount((uint)Torrent.Monitor.DataBytesDownloaded);
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
                    fi.Priority = file.Priority.ToString();
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
                        "Ratio");
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
            foreach(string str in columns)
                PropertyChanged(this, new PropertyChangedEventArgs(str));
        }
    }
    public class Utility
    {
        static ulong K = 1024;
        static ulong M = 1024 * 1024;
        static ulong G = 1024 * 1024 * 1024;
        static ulong T = 1024L * 1024L * 1024L * 1024L;
        public static string PrettifyAmount(ulong amount)
        {
            if (amount > T)
                return (amount / T) + " TB";
            if (amount > G)
                return ((float)amount / (G)).ToString("0.00") + " GB";
            if (amount > M)
                return ((float)amount / (M)).ToString("0.00") + " MB";
            if (amount > K)
                return ((float)amount / (K)).ToString("0.00") + " KB";
            return amount.ToString() + " B";
        }
        public static string PrettifyAmount(long amount)
        {
            return PrettifyAmount((ulong)amount);
        }
        public static string PrettifySpeed(long speed)
        {
            return PrettifyAmount((ulong)speed) + "/s";
        }
        public static Label GenerateLabel(string text, Thickness margin)
        {
            Label l = new Label();
            l.Content = text;
            l.Width = double.NaN; // equivalent of Width="Auto"
            l.Height = double.NaN;
            
            l.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            l.Margin = margin;
            return l;
        }
        public static Line GenerateLine(double x1, double y1, double x2, double y2, Brush color, int thickness = 2, int zindex = 1)
        {
            Line line = new Line();
            line.X1 = x1;
            line.Y1 = y1;
            line.X2 = x2;
            line.Y2 = y2;
            line.StrokeThickness = thickness;
            line.Stroke = color;
            return line;
        }

        public static Thickness SizeToMargin(Thickness orig)
        {
            return new Thickness(orig.Left, orig.Top, orig.Right - orig.Left, orig.Bottom - orig.Top);
        }
        public static void SetIfLowerThan(ref double orig, double newval)
        {
            if (newval < orig)
                orig = newval;
        }
        public static void SetIfHigherThan(ref double orig, double newval)
        {
            if (newval > orig)
                orig = newval;
        }
        public static double CalculateLocation(double spp, double data)
        {
            return data * spp;
        }
    }
    public class PeerInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public string IP { get; set; }
        public string Client { get; set; }
        public string PieceInfo { get; set; }
        public PeerInfo() { }
        public void SetSelf(PeerInfo pi)
        {
            this.IP = pi.IP;
            this.Client = pi.Client;
            this.PieceInfo = pi.PieceInfo;
            UpdateList("IP", "Client", "PieceInfo");
        }
        public void UpdateList(params string[] columns)
        {
            if (PropertyChanged == null)
                return;
            foreach (string str in columns)
                PropertyChanged(this, new PropertyChangedEventArgs(str));
        }
    }
    public class PieceInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public bool Finished { get; set; }
        public int ID { get; set; }
        public PieceInfo() { }
        public void SetSelf(PieceInfo pi)
        {
            this.Finished = pi.Finished;
            this.ID = pi.ID;
            UpdateList("Finished", "ID");
        }
        public void UpdateList(params string[] columns)
        {
            if (PropertyChanged == null)
                return;
            foreach (string str in columns)
                PropertyChanged(this, new PropertyChangedEventArgs(str));
        }
    }
    public class FileInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public string Name { get; set; }
        public int Progress { get; set; }
        public string Priority { get; set; }
        public string Size { get { return Utility.PrettifyAmount(RawSize); } }
        public long RawSize { get; set; }
        public TorrentInfo Parent { get; set; }
        public FileInfo() { }
        public void SetSelf(FileInfo pi)
        {
            this.Name = pi.Name;
            this.Progress = pi.Progress;
            this.Priority = pi.Priority;
            UpdateList("Name", "Progress", "Priority");
        }
        public void UpdateList(params string[] columns)
        {
            if (PropertyChanged == null)
                return;
            foreach (string str in columns)
                PropertyChanged(this, new PropertyChangedEventArgs(str));
        }
    }
    public class TrackerInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public string URL { get; set; }
        public TorrentInfo Parent { get; set; }
        public TrackerInfo() { }
        public void SetSelf(TrackerInfo pi)
        {
            this.URL = pi.URL;
            UpdateList("URL");
        }
        public void UpdateList(params string[] columns)
        {
            if (PropertyChanged == null)
                return;
            foreach (string str in columns)
                PropertyChanged(this, new PropertyChangedEventArgs(str));
        }
    }
}
