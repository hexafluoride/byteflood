using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonoTorrent.Common;
using System.ComponentModel;
using System.Xml.Serialization;

namespace ByteFlood
{
    public class FileInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public string Name { get; set; }
        public int Progress { get; set; }
        public string Priority { get { return ActualPriority == MonoTorrent.Common.Priority.DoNotDownload ? "Don't download" : ActualPriority.ToString(); } }
        public Priority ActualPriority { get; set; }
        public bool DownloadFile { get; set; }

        public string Size { get { return Utility.PrettifyAmount(RawSize); } }

        public long RawSize { get; set; }
        [XmlIgnore]
        public TorrentInfo Parent { get; set; }
        public FileInfo()
        {
        }

        public FileInfo(TorrentInfo owner) 
        {
            Parent = owner;
        }

        public void SetSelf(FileInfo pi)
        {
            this.Name = pi.Name;
            this.Progress = pi.Progress;
            this.ActualPriority = pi.ActualPriority;
            this.DownloadFile = pi.DownloadFile;
            UpdateList("Name", "Progress", "Priority", "DownloadFile");
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
