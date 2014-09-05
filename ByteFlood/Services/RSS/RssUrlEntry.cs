using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.ServiceModel.Syndication;
using System.Net;
using System.Diagnostics;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using System.IO;

namespace ByteFlood.Services.RSS
{
    public class RssUrlEntry : INotifyPropertyChanged
    {
        public string Url { get; set; }

        public string Alias { get; set; }

        public bool CheckDuplicate { get; set; }

        public string FilterExpression { get; set; }

        public FilterActionEnum FilterAction { get; set; }

        public enum FilterActionEnum { Download, Skip }

        public bool AutoDownload { get; set; }

        public MonoTorrent.Client.TorrentSettings DefaultSettings { get; set; }

        [XmlIgnore]
        private TimeSpan UpdateInterval = new TimeSpan(0, 15, 0);

        [XmlIgnore]
        private int tick = 1000;

        private Dictionary<string, RssTorrent> items = new Dictionary<string, RssTorrent>();

        [XmlIgnore]
        //The feed title provided by the webserver
        private string feed_title = null;

        [XmlIgnore]
        public string Name
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.Alias))
                {
                    if (string.IsNullOrEmpty(feed_title))
                    {
                        try
                        {
                            return (new Uri(this.Url)).Host;
                        }
                        catch
                        {
                            return this.Url;
                        }
                    }
                    else
                    {
                        return this.feed_title;
                    }
                }
                else
                {
                    return this.Alias;
                }
            }
        }

        [XmlIgnore]
        public int Count
        {
            get
            {
                return items.Count(rt => FeedsManager.AppState.Torrents.Any(ti => ti.Path == rt.Value.TorrentFilePath));
            }
        }

        [XmlIgnore]
        public BitmapImage Icon
        {
            get;
            private set;
        }

        [XmlIgnore]
        private int icon_load_try_count = 0;

        /// <summary>
        /// Return an array of RssTorrent when new items are found, otherwise return null 
        /// </summary>
        /// <returns></returns>
        public RssTorrent[] Update()
        {
            tick++;
            NotifyPropertyChanged("Count");

            if (tick >= UpdateInterval.TotalSeconds)
            {
                try
                {
                    Debug.WriteLine("[Feed '{0}']: update started.", this.Url, "");

                    DateTime start = DateTime.Now;

                    List<RssTorrent> new_item_list = new List<RssTorrent>();

                    double time_diff_sum = 0;

                    foreach (RssTorrent rt in RetriveFeed())
                    {
                        if (!items.ContainsKey(rt.Id))
                        {
                            time_diff_sum += (start - rt.TimePublished).TotalSeconds;
                            if (IsAllowed(rt))
                            {
                                items.Add(rt.Id, rt);
                                new_item_list.Add(rt);
                            }
                        }
                        else
                        {
                            time_diff_sum += (start - items[rt.Id].TimePublished).TotalSeconds;
                        }
                    }

                    TryLoadIcon();
                    NotifyPropertyChanged("Name");

                    this.UpdateInterval = new TimeSpan(0, 0, Convert.ToInt32(time_diff_sum / items.Count()));
                    Debug.WriteLine("[Feed '{0}']: Calculated update interval: {1} sec", this.Url, this.UpdateInterval.TotalSeconds);
                    tick = 0;

                    Debug.WriteLine("[Feed '{0}']: update terminated.", this.Url, "");

                    if (new_item_list.Count > 0)
                    {
                        Debug.WriteLine("[Feed '{0}']: {1} new item found.", this.Url, new_item_list.Count);
                        NotifyPropertyChanged("Count");
                        return new_item_list.ToArray();
                    }
                }
                catch (Exception e)
                {
                    this.tick = Convert.ToInt32(UpdateInterval.TotalSeconds / 2);
                    return null;
                }
            }

            return null;
        }

        private void TryLoadIcon()
        {
            if (this.Icon == null && icon_load_try_count < 15)
            {
                try
                {
                    using (WebClient nc = new WebClient())
                    {
                        Uri i = new Uri(this.Url);

                        string icon_file = string.Format("http://{0}/favicon.ico", i.Host);

                        byte[] icon_data = nc.DownloadData(icon_file);

                        if (icon_data != null && icon_data.Length > 0)
                        {
                            MemoryStream m = new MemoryStream(icon_data);

                            BitmapImage bi = new BitmapImage();

                            bi.BeginInit();
                            bi.CacheOption = BitmapCacheOption.Default;
                            bi.StreamSource = m;
                            bi.EndInit();
                            bi.Freeze();

                            this.Icon = bi;
                            NotifyPropertyChanged("Icon");
                        }
                    }
                }
                catch { }
            }
        }

        public void ForceUpdate()
        {
            tick = Convert.ToInt32(this.UpdateInterval.TotalSeconds + 1);
        }

        public bool Test()
        {
            // TODO: Add more tests to check feed validity
            try
            {
                Rss.RssFeed.Read(this.Url);
                return true;
            }
            catch (WebException wex)
            {
                if (wex.Message.Contains("404"))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private RssTorrent[] RetriveFeed()
        {
            Rss.RssFeed feed = Rss.RssFeed.Read(this.Url);

            // Note:
            // I don't know if more than 1 channel is allowed for torrents feeds
            // But I will stick to the first one only.
            // Idea: Probably multiple channels are for multiple languages.
            // In this case, we select the prefered one.

            Rss.RssChannel channel = feed.Channels[0];

            List<RssTorrent> torrents = new List<RssTorrent>(channel.Items.Count);

            foreach (Rss.RssItem item in channel.Items)
            {
                torrents.Add(item.ToTorrent());
            }

            this.feed_title = channel.Title;

            return torrents.ToArray();
        }

        [XmlIgnore]
        Regex m = null;

        private bool IsAllowed(RssTorrent t)
        {
            if (!string.IsNullOrWhiteSpace(this.FilterExpression))
            {
                if (m == null)
                {
                    m = new Regex(Regex.Escape(this.FilterExpression), RegexOptions.Compiled | RegexOptions.IgnoreCase);
                }

                bool regex_match = m.IsMatch(t.Name);

                if (this.FilterAction == FilterActionEnum.Download)
                {
                    return regex_match;
                }
                else //In Skip action, we don't want stuffs that match the filter
                {
                    return !regex_match;
                }
            }

            return true;
        }

        /// <summary>
        /// This method is only to be called when the RSS Entry is edited.
        /// </summary>
        public void NotifyUpdate()
        {
            this.m = null;
            NotifyPropertyChanged("Name", "Count");
        }

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(params string[] propnames)
        {
            if (PropertyChanged != null)
            {
                foreach (string propname in propnames)
                    PropertyChanged(this, new PropertyChangedEventArgs(propname));
            }
        }

        #endregion
    }
}
