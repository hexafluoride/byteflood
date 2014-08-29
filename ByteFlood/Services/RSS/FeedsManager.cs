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
using System.Collections.ObjectModel;
namespace ByteFlood.Services.RSS
{
    /// <summary>
    /// This class handles all RSS-related activities and logic.
    /// </summary>
    public static class FeedsManager
    {
        private static Dictionary<string, RssUrlEntry> entries = new Dictionary<string, RssUrlEntry>();

        public static ObservableCollection<RssUrlEntry> EntriesList { get; set; }

        private static List<string> url_404 = new List<string>();

        private static State AppState
        {
            get
            {
                return ((MainWindow)App.Current.MainWindow).state;
            }
        }

        public static string RssTorrentsStorageDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(App.Settings.RssTorrentsStorageDirectory))
                {
                    return System.IO.Path.Combine(App.Settings.DefaultDownloadPath, "RSS");
                }
                else
                {
                    return App.Settings.RssTorrentsStorageDirectory;
                }
            }
        }

        private static string EntriesSavePath
        {
            get { return "./rss-items.xml"; }
        }

        static FeedsManager()
        {
            if (!Directory.Exists(RssTorrentsStorageDirectory))
            {
                Directory.CreateDirectory(RssTorrentsStorageDirectory);
            }
            EntriesList = new ObservableCollection<RssUrlEntry>();

            Load();

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
                        RssTorrent[] rt = a.Update();
                        if (rt != null)
                        {
                            Process_NewRssItems(a, rt);
                        }
                        a.NotifyUpdate();
                    }
                }
                catch (Exception ex)
                {
#if DEBUG
                    throw;
#endif
                }
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
                    EntriesList.Add(entry);
                    Save();
                }
            }
        }

        public static void Remove(RssUrlEntry entry)
        {
            if (entries.ContainsKey(entry.Url))
            {
                entries.Remove(entry.Url);
                EntriesList.Remove(entry);
                Save();
            }
        }

        public static void ForceUpdate(RssUrlEntry entry)
        {
            entry.ForceUpdate();
        }

        public static void Save()
        {
            RssUrlEntry[] items = entries.Values.ToArray();
            Utility.Serialize<RssUrlEntry[]>(items, EntriesSavePath);
        }

        public static void Load()
        {
            if (File.Exists(EntriesSavePath))
            {
                RssUrlEntry[] it = Utility.Deserialize<RssUrlEntry[]>(EntriesSavePath);
                foreach (var i in it) { Add(i); }
            }
        }

        private static void Process_NewRssItems(RssUrlEntry entry, RssTorrent[] new_items)
        {
            foreach (var nitem in new_items)
            {
                if (url_404.Contains(nitem.TorrentFileUrl)) { continue; }

                string save_path = Path.Combine(RssTorrentsStorageDirectory, Utility.CleanFileName(nitem.Name) + ".torrent");
                nitem.TorrentFilePath = save_path;

                if (File.Exists(save_path))
                {
                    //re-load from cache
                    App.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        nitem.Success = AppState.AddTorrentRss(save_path, entry.DefaultSettings, entry.AutoDownload);
                    }));
                    continue;
                }

                var res = download(nitem.TorrentFileUrl);

                if (res.Type == DownloadRssResponse.ResonseType.OK)
                {
                    byte[] data = res.Data;
                    if (data.Length > 0)
                    {
                        File.WriteAllBytes(save_path, data);
                        App.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            nitem.Success = AppState.AddTorrentRss(save_path, entry.DefaultSettings, entry.AutoDownload);
                        }));
                    }
                    else
                    {
                        //What should we do?
                        continue;
                    }
                }
                else if (res.Type == DownloadRssResponse.ResonseType.NotFound)
                {
                    if (!url_404.Contains(nitem.TorrentFileUrl))
                    {
                        url_404.Add(nitem.TorrentFileUrl);
                        Debug.WriteLine("[Rssdownloader]: URL '{0}' not found, therefore banned.", nitem.TorrentFileUrl, "");
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
                    foreach (string header in nc.ResponseHeaders.AllKeys)
                    {
                        if (header == "Content-Encoding")
                            if (nc.ResponseHeaders[header] == "gzip")
                                data = Utility.DecompressGzip(data);
                    }
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

        public static RssTorrent ToTorrent(this SyndicationItem i)
        {
            var rt = new RssTorrent();
            try
            {
                rt.Name = i.Title.Text;

                if (i.Links.Count > 0)
                {
                    var results = i.Links.Where(t => t.RelationshipType == "enclosure");
                    if (results.Count() == 0)
                        rt.TorrentFileUrl = i.Links[0].Uri.ToString();
                    else
                        rt.TorrentFileUrl = results.First().Uri.ToString();
                }

                rt.TimePublished = i.PublishDate.DateTime;

                rt.Summary = i.Summary.Text;

                return rt;
            }
            catch (NullReferenceException ex)
            {
                if (rt != null) // try to recover as much info as we can
                    return rt;
                throw;
            }
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
