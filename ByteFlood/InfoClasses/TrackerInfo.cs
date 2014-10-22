using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using MonoTorrent.Common;
using MonoTorrent.Client.Tracker;
using ByteFlood.Formatters;
namespace ByteFlood
{
    public class TrackerInfo : INotifyPropertyChanged
    {
        public string URL
        {
            get { return this.Tracker.Uri.ToString(); }
        }

        public Tracker Tracker { get; private set; }

        public string UpdateInString
        {
            get
            {
                if (this.Parent.Torrent.State == TorrentState.Downloading ||
                    this.Parent.Torrent.State == TorrentState.Seeding ||
                    this.Parent.Torrent.State == TorrentState.Paused)
                {
                    if (this.Tracker.IsUpdating)
                    {
                        return "Updating...";
                    }
                    else
                    {
                        TimeSpan ts = this.NextAnnounce - DateTime.Now;
                        if (ts.TotalSeconds <= 0)
                        {
                            return "Update Queued";
                        }
                        else
                        {
                            return HMSFormatter.GetReadableTimespan(ts);
                        }
                    }
                }
                else { return ""; }
            }
        }

        public int PeersCount
        {
            get;
            private set;
        }

        public TrackerState State
        {
            get
            {
                return this.Tracker.Status;
            }
        }

        public string FailureMessage
        {
            get { return this.Tracker.FailureMessage; }
        }

        public TorrentInfo Parent { get; set; }

        public TrackerInfo(Tracker t, TorrentInfo parent)
        {
            this.Tracker = t;
            this.Parent = parent;

            t.AnnounceComplete += t_AnnounceComplete;
        }

        private DateTime LastAnnounce = DateTime.Now;
        private DateTime NextAnnounce = DateTime.Now;

        void t_AnnounceComplete(object sender, AnnounceResponseEventArgs e)
        {
            this.LastAnnounce = DateTime.Now;
            this.NextAnnounce = DateTime.Now + this.Tracker.UpdateInterval;
            if (e.Successful)
            {
                this.PeersCount = e.Peers.Count;
            }
            else
            {
                return;
            }
            //Insert here things that updates after an announce
            UpdateList("PeersCount", "State");
        }

        public void Update()
        {
            //Insert here things that need constant updating
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("UpdateInString"));
            }
        }

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        public void UpdateList(params string[] columns)
        {
            if (PropertyChanged == null)
                return;
            foreach (string str in columns)
                PropertyChanged(this, new PropertyChangedEventArgs(str));
        }
        #endregion
    }
}
