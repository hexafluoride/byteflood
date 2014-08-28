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
        public string Name
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.Alias))
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
                    return this.Alias;
                }
            }
        }

        [XmlIgnore]
        public int Count
        {
            get
            {
                return items.Count;
            }
        }

        /// <summary>
        /// Return an array of RssTorrent when new items are found, otherwise return null 
        /// </summary>
        /// <returns></returns>
        public RssTorrent[] Update()
        {
            tick++;

            if (tick >= UpdateInterval.TotalSeconds)
            {
                Debug.WriteLine("[Feed '{0}']: update started.", this.Url, "");

                DateTime start = DateTime.Now;

                List<RssTorrent> new_item_list = new List<RssTorrent>();

                double time_diff_sum = 0;

                using (XmlReader r = XmlReader.Create(this.Url))
                {
                    SyndicationFeed feed = SyndicationFeed.Load(r);
                    foreach (SyndicationItem item in feed.Items)
                    {
                        if (!items.ContainsKey(item.Id))
                        {
                            RssTorrent rt = item.ToTorrent();
                            time_diff_sum += (start - rt.TimePublished).TotalSeconds;
                            if (IsAllowed(rt))
                            {
                                items.Add(item.Id, rt);
                                new_item_list.Add(rt);
                            }
                        }
                        else
                        {
                            time_diff_sum += (start - items[item.Id].TimePublished).TotalSeconds;
                        }
                    }
                }

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

            return null;
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
                using (WebClient nc = new WebClient())
                {
                    byte[] data = nc.DownloadData(this.Url);
                    // Test1 passed: valid URL and server response

                    using (XmlReader r = XmlReader.Create(new System.IO.MemoryStream(data)))
                    {
                        SyndicationFeed feed = SyndicationFeed.Load(r);
                    }
                    // Test2 passed: XML validity

                    return true;
                }
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

        [XmlIgnore]
        Regex m = null;

        private bool IsAllowed(RssTorrent t)
        {
            if (!string.IsNullOrWhiteSpace(this.FilterExpression))
            {
                if (m == null)
                {
                    m = new Regex(this.FilterExpression, RegexOptions.Compiled | RegexOptions.IgnoreCase);
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

        public void NotifyUpdate() 
        {
            this.m = null;
            NotifyPropertyChanged("Name");
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
