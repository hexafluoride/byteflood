using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Syndication;
using System.Threading;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

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

        static void entry_NewItems(RssUrlEntry entry, RssTorrent[] new_items)
        {
            foreach (var nitem in new_items)
            {
                string save_path = Path.Combine(RssTorrentsStorageDirectory, Utility.CleanFileName(nitem.Name));

                byte[] data = download(nitem.TorrentFileUrl);

                if (data != null && data.Length > 0)
                {
                    File.WriteAllBytes(save_path, data);
                    if (entry.AutoDownload)
                    {
                        //((MainWindow)App.Current.MainWindow).state.AddTorrentByPath(save_path);
                    }
                }
                else 
                {
                    // TODO: Check why this failed, and retry later except when the error is 
                    // 404 not found
                }
            }
        }

        static byte[] download(string url)
        {
            try
            {
                using (WebClient nc = new WebClient())
                {
                    return nc.DownloadData(url);
                }
            }
            catch (WebException wex)
            {
                if (wex.Message.Contains("404"))
                {
                    return null;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
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
    }
}
