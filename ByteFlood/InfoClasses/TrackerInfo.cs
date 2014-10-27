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
            get { return this.Tracker.Url; }
        }

        public Ragnar.AnnounceEntry Tracker { get; private set; }

        public string UpdateInString
        {
            get
            {
                if (this.Tracker.Updating)
                {
                    return "Updating...";
                }
                else
                {
                    return "";

                    //TimeSpan ts = this.NextAnnounce - DateTime.Now;
                    //if (ts.TotalSeconds <= 0)
                    //{
                    //    return "Update Queued";
                    //}
                    //else
                    //{
                    //    return HMSFormatter.GetReadableTimespan(ts);
                    //}
                }
            }
        }

        //public int PeersCount
        //{
        //    get 
        //    {
        //        return 0;
        //    }
        //}

        //public TrackerState State
        //{
        //    get
        //    {
        //        return this.Tracker.s;
        //    }
        //}

        public string FailureMessage
        {
            get { return this.Tracker.Message; }
        }

        public TorrentInfo Parent { get; set; }

        public TrackerInfo(Ragnar.AnnounceEntry t, TorrentInfo parent)
        {
            this.Tracker = t;
            this.Parent = parent;
        }

        //private DateTime LastAnnounce = DateTime.Now;
        //private DateTime NextAnnounce = DateTime.Now;

        //void t_AnnounceComplete(object sender, AnnounceResponseEventArgs e)
        //{
        //    this.LastAnnounce = DateTime.Now;
        //    this.NextAnnounce = DateTime.Now + this.Tracker.UpdateInterval;
        //    if (e.Successful)
        //    {
        //        this.PeersCount = e.Peers.Count;
        //    }
        //    else
        //    {
        //        return;
        //    }
        //    //Insert here things that updates after an announce
        //    UpdateList("PeersCount", "State");
        //}

        public void Update()
        {
            //Insert here things that need constant updating
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("UpdateInString"));
                PropertyChanged(this, new PropertyChangedEventArgs("FailureMessage"));
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
