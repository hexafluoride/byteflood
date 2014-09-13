using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace ByteFlood.Services.RSS
{
    public class RssTorrent : INotifyPropertyChanged
    {
        public RssTorrent(string id) { this.Id = id; }
        public string Id { get; private set; }
        public string Name { get; set; }
        public string TorrentFileUrl { get; set; }
        public string TorrentMagnetUrl { get; set; }
        public bool IsMagnetOnly
        {
            get
            {
                return string.IsNullOrWhiteSpace(this.TorrentFileUrl) && !string.IsNullOrWhiteSpace(this.TorrentMagnetUrl);
            }
        }
        public string Summary { get; set; }
        public bool ContainDownloadLinks
        {
            get { return !string.IsNullOrEmpty(this.TorrentFileUrl) || !string.IsNullOrEmpty(this.TorrentMagnetUrl); }
        }
        public string TorrentFilePath { get; set; }
        public DateTime TimePublished { get; set; }

        public bool IsAllowed { get; set; }

        private bool _success = false;
        public bool Success
        {
            get { return _success; }
            set 
            {
                if (value != _success) 
                {
                    _success = value;
                    NotifyPropertyChanged("Success");
                }
            }
        }

        public FeedsManager.DownloadRssResponse LastResponse 
        {
            set
            {
                this.LastResponseMessage = value.Error == null ? "OK" : value.Error.Message;
                this.LastResponseType = value.Type;
            }
        }

        private FeedsManager.DownloadRssResponse.ResonseType _lrt;
        public FeedsManager.DownloadRssResponse.ResonseType LastResponseType 
        {
            get { return this._lrt; }
            set
            {
                if (value != _lrt)
                {
                    _lrt = value;
                    NotifyPropertyChanged("LastResponseType");
                }
            }
        }

        private string _lrm = null;
        public string LastResponseMessage 
        {
            get { return this._lrm; }
            set
            {
                if (value != _lrm)
                {
                    _lrm = value;
                    NotifyPropertyChanged("LastResponseMessage");
                }
            }
        }

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propname)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propname));
            }
        }

        #endregion
    }
}
