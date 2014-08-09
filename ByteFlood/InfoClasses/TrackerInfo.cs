using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace ByteFlood
{
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
