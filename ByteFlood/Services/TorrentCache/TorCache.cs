using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace ByteFlood.Services.TorrentCache
{
    public class TorCache : ITorrentCache
    {
        public string Name { get { return "TorCache"; } }

        public string Url { get { return "http://torcache.net/"; } }

        public byte[] Fetch(MonoTorrent.MagnetLink magnet)
        {
            string url = string.Format("http://torcache.net/torrent/{0}.torrent", magnet.InfoHash.ToHex());

            using (WebClient nc = new WebClient())
            {
                nc.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/18.0 (compatible; MSIE 10.0; Windows NT 5.2; .NET CLR 3.5.3705;)");

                byte[] gzip_data = null;

                try
                {
                    gzip_data = nc.DownloadData(url);
                }
                catch (WebException wex)
                {
                    if (wex.Message.Contains("404"))
                    {
                        return null;
                    }
                }
                catch (Exception) { return null; }

                byte[] data = Utility.DecompressGzip(gzip_data);

                return data;
            }
        }
    }
}
