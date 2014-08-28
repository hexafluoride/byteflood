using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.ServiceModel.Syndication;
using System.Net;
using System.Diagnostics;
namespace ByteFlood.Services.RSS
{
    public class RssUrlEntry
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
                            items.Add(item.Id, rt);
                            new_item_list.Add(rt);
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
                    return new_item_list.ToArray();
                }
            }

            return null;
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

    }
}
