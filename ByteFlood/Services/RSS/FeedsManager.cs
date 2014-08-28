using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Syndication;
using System.Threading;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace ByteFlood.Services.RSS
{
    /// <summary>
    /// This class handles all RSS-related activities and logic.
    /// </summary>
    public static class FeedsManager
    {
        private static Dictionary<string, RssUrlEntry> entries = new Dictionary<string, RssUrlEntry>();

        public static string RssTorrentsStorageDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(App.Settings.RssTorrentsStorageDirectory))
                {
                    return System.IO.Path.Combine(App.Settings.DefaultDownloadPath, "rss");
                }
                else
                {
                    return App.Settings.RssTorrentsStorageDirectory;
                }
            }
        }

        static FeedsManager()
        {
            if (!Directory.Exists(RssTorrentsStorageDirectory))
            {
                Directory.CreateDirectory(RssTorrentsStorageDirectory);
            }

            Thread work = new Thread(loop);
            work.IsBackground = true;
            work.Start();
        }

        private static void loop()
        {
            while (true)
            {
                try
                {
                    RssUrlEntry[] v = entries.Values.ToArray();
                    foreach (var a in v)
                    {
                        a.Update();
                    }
                }
                catch { }
                Thread.Sleep(1000);
            }
        }

        public static void Add(RssUrlEntry entry)
        {
            lock (entries)
            {
                if (!entries.ContainsKey(entry.Url))
                {
                    entries.Add(entry.Url, entry);
                    entry.NewItems += entry_NewItems;
                }
            }
        }

        private static List<string> url_404 = new List<string>();

        private static State AppState
        {
            get
            {
                return ((MainWindow)App.Current.MainWindow).state;
            }
        }

        static void entry_NewItems(RssUrlEntry entry, RssTorrent[] new_items)
        {
            foreach (var nitem in new_items)
            {
                if (url_404.Contains(nitem.TorrentFileUrl)) { continue; }

                string save_path = Path.Combine(RssTorrentsStorageDirectory, Utility.CleanFileName(nitem.Name));

                var res = download(nitem.TorrentFileUrl);

                if (res.Type == DownloadRssResponse.ResonseType.OK)
                {
                    byte[] data = res.Data;
                    if (data.Length > 0)
                    {
                        File.WriteAllBytes(save_path, data);
                        AppState.AddTorrentRss(save_path, entry.DefaultSettings, entry.AutoDownload);
                    }
                    else
                    {
                        //What should we do?
                        continue;
                    }
                }
                if (res.Type == DownloadRssResponse.ResonseType.NotFound)
                {
                    if (!url_404.Contains(nitem.TorrentFileUrl))
                    {
                        url_404.Add(nitem.TorrentFileUrl);
                        Debug.WriteLine("[Rssdownloader]: URL '{0}' not found, therefore banned.", nitem.TorrentFileUrl);
                    }
                }
                else
                {
                    //breakpoint time
                    continue;
                }
            }
        }

        private static DownloadRssResponse download(string url)
        {
            try
            {
                using (WebClient nc = new WebClient())
                {
                    byte[] data = nc.DownloadData(url);
                    return new DownloadRssResponse()
                    {
                        Data = data,
                        Type = DownloadRssResponse.ResonseType.OK
                    };
                }
            }
            catch (WebException wex)
            {
                if (wex.Message.Contains("404"))
                {
                    return new DownloadRssResponse()
                    {
                        Type = DownloadRssResponse.ResonseType.NotFound
                    };
                }
                else
                {
                    return new DownloadRssResponse()
                    {
                        Type = DownloadRssResponse.ResonseType.NetFail,
                        Error = wex
                    };
                }
            }
            catch (Exception ex)
            {
                return new DownloadRssResponse()
                {
                    Type = DownloadRssResponse.ResonseType.Fail,
                    Error = ex
                };
            }
        }

        public delegate void NewItemsEvent(RssUrlEntry entry, RssTorrent[] new_items);

        public static RssTorrent ToTorrent(this SyndicationItem i)
        {
            var rt = new RssTorrent();
            rt.Name = i.Title.Text;
            if (i.Links.Count > 0)
            {
                rt.TorrentFileUrl = i.Links[0].Uri.ToString();
            }

            rt.TimePublished = i.PublishDate.DateTime;

            rt.Summary = i.Summary.Text;

            return rt;
        }

        private struct DownloadRssResponse
        {
            public ResonseType Type;
            public byte[] Data;
            public Exception Error;
            public enum ResonseType { Fail, NetFail, NotFound, OK }
        }


    }
}
