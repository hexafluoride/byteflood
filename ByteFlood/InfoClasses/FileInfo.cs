using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace ByteFlood
{
    public class FileInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public string Name { get; set; }
        public int Progress { get; set; }
        public string Priority { get; set; }
        public bool DownloadFile { get; set; }

        public string Size { get { return Utility.PrettifyAmount(RawSize); } }

        public long RawSize { get; set; }
        public TorrentInfo Parent { get; set; }
        public FileInfo() { }
        public void SetSelf(FileInfo pi)
        {
            this.Name = pi.Name;
            this.Progress = pi.Progress;
            this.Priority = pi.Priority;
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
