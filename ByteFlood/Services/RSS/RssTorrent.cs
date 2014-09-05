using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ByteFlood.Services.RSS
{
    public class RssTorrent
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
        public bool Success { get; set; }
        public DateTime TimePublished { get; set; }
    }
}
