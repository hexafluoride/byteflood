using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.IO;
using System.Net;
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

        public static State AppState
        {
            get
            {
                State s = null;
                App.Current.Dispatcher.Invoke(new Action(() => { s = ((MainWindow)App.Current.MainWindow).state; }));
                return s;
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
                    }
                }
#if DEBUG
                catch (Exception ex)
                {
                    throw;
                }
#else
                catch {}
#endif
                Thread.Sleep(1000);
            }
        }

        #region RssUrlEntry Functions

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
                try
                {
                    RssUrlEntry[] it = Utility.Deserialize<RssUrlEntry[]>(EntriesSavePath);
                    foreach (var i in it) { Add(i); }
                }
                catch (Exception ex) { Debug.WriteLine("[FeedManager]: Could not load rss items '{0}' @ '{1}'", ex.Message, ex.StackTrace); }
            }
        }

        #endregion

        private static List<RssTorrent> QueuedItems = new List<RssTorrent>();

        private static bool IsQueued(RssTorrent rt)
        {
            return QueuedItems.Contains(rt);
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
                    //re-load from downloaded file
                    App.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        nitem.LastResponseMessage = "Data was already saved";
                        nitem.LastResponseType = DownloadRssResponse.ResonseType.OK;
                        nitem.Success = AppState.AddTorrentRss(save_path, entry);
                    }));
                    continue;
                }

                if (!IsQueued(nitem))
                {
                    QueuedItems.Add(nitem);

                    System.Threading.Tasks.Task.Factory.StartNew(new Action(() =>
                    {
                        Debug.WriteLine("[Rssdownloader-TQ]: Work Item '{0}' has been started", nitem.Name, "");

                        var res = download(nitem.IsMagnetOnly ? nitem.TorrentMagnetUrl : nitem.TorrentFileUrl);
                        nitem.LastResponse = res;

                        Debug.WriteLine("[Rssdownloader-TQ]: Work Item '{0}' resp type is {1}", nitem.Name, res.Type);

                        if (res.Type == DownloadRssResponse.ResonseType.OK || res.Type == DownloadRssResponse.ResonseType.MagnetLink)
                        {
                            byte[] data = res.Data;
                            if (data.Length > 0)
                            {
                                File.WriteAllBytes(save_path, data);
                                Debug.WriteLine("[Rssdownloader-TQ]: Work Item '{0}' succesfully downloaded", nitem.Name, "");

                                App.Current.Dispatcher.Invoke(new Action(() =>
                                {
                                    nitem.Success = AppState.AddTorrentRss(save_path, entry);
                                }));
                            }
                            else
                            {
                                if (res.Type == DownloadRssResponse.ResonseType.MagnetLink)
                                {
                                    Debug.WriteLine("[Rssdownloader-TQ]: Cannot add torrent ({0}) magnet ('{1}')",
                                        nitem.Name, nitem.TorrentMagnetUrl);
                                }
                                //What should we do?
                            }
                        }
                        else if (res.Type == DownloadRssResponse.ResonseType.NotFound)
                        {
                            if (!url_404.Contains(nitem.TorrentFileUrl))
                            {
                                url_404.Add(nitem.TorrentFileUrl);
                                Debug.WriteLine("[Rssdownloader-TQ]: URL '{0}' not found, therefore banned.", nitem.TorrentFileUrl, "");
                            }
                        }

                        QueuedItems.Remove(nitem);
                        return;
                    }));
                }
            }
        }

        private static DownloadRssResponse download(string url)
        {
            if (Utility.IsMagnetLink(url))
            {
                try
                {
                    // First, we try to load the magnet from cache, since it's faster.
                    // If the cache fail, we load magnets traditionally

                    byte[] data = State.GetMagnetFromCache(url);

                    if (data == null)
                    {
                        data = AppState.MagnetLinkTorrentFile(url);
                    }

                    return new DownloadRssResponse()
                    {
                        Type = DownloadRssResponse.ResonseType.MagnetLink,
                        Data = data == null ? new byte[0] : data
                    };
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

        //(t => t.RelationshipType == "enclosure");
        //(t => t.RelationshipType == "alternate");

        public static RssTorrent ToTorrent(this Rss.RssItem item)
        {
            RssTorrent rt = new RssTorrent(item.Guid.Name)
            {
                Name = item.Title,
                Summary = item.Description,
                TimePublished = item.PubDate,
            };

            if (item.Enclosure != null)
            {
                if (item.Enclosure.Type == "application/x-bittorrent")
                {
                    rt.TorrentFileUrl = item.Enclosure.Url.ToString();
                }
            }
            else
            {
                string url = item.Link.ToString();
                if (Utility.IsMagnetLink(url))
                {
                    rt.TorrentMagnetUrl = url;
                }
                else
                {
                    // Warning: This URL might not be the .torrent file. (In case of https://animetosho.org)
                    rt.TorrentFileUrl = url;
                }
            }

            return rt;

        }

        public struct DownloadRssResponse
        {
            public ResonseType Type;
            public byte[] Data;
            public Exception Error;
            public enum ResonseType { Fail, NetFail, NotFound, OK, MagnetLink }
        }


    }
}
