using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.Net;
using System.Web;

namespace ByteFlood.Services.WebSearch
{
    public static class TorrentzEu
    {
        public static SearchResult[] Search(string query)
        {
            List<SearchResult> results = new List<SearchResult>();
            using (WebClient nc = new WebClient())
            {
                string f_query = HttpUtility.UrlEncode(query);

                nc.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 6.1; rv:31.0) Firefox/31.0");
                nc.Headers.Add(HttpRequestHeader.Referer, "https://torrentz.eu/");

                string data = nc.DownloadString(string.Format("https://torrentz.eu/search?f={0}", f_query));

                if (!string.IsNullOrWhiteSpace(data))
                {
                    HtmlDocument doc = new HtmlDocument(); doc.LoadHtml(data);

                    List<HtmlNode> dl_nodes = new List<HtmlNode>();

                    #region Nodes Finder

                    foreach (HtmlNode html in doc.DocumentNode.ChildNodes)
                    {
                        if (html.Name == "html")
                        {
                            foreach (HtmlNode body in html.ChildNodes)
                            {
                                if (body.Name == "body")
                                {
                                    foreach (HtmlNode resuls_div in body.ChildNodes)
                                    {
                                        if (resuls_div.GetAttributeValue("class", "").Contains("results"))
                                        {
                                            foreach (HtmlNode dl_n in resuls_div.ChildNodes)
                                            {
                                                if (dl_n.Name == "dl") { dl_nodes.Add(dl_n); }
                                            }
                                            break;
                                        }
                                    }
                                    break;
                                }
                            }
                            break;
                        }
                    }

                    #endregion

                    #region Node parser

                    foreach (HtmlNode dl in dl_nodes)
                    {
                        try
                        {
                            SearchResult r = new SearchResult();

                            r.InfoHash = dl.ChildNodes[0].ChildNodes[0].GetAttributeValue("href", " ").Replace("/", "").ToUpper();
                            r.Name = HttpUtility.HtmlDecode(dl.ChildNodes[0].ChildNodes[0].InnerText);

                            r.Categories = HttpUtility.HtmlDecode(dl.ChildNodes[0].ChildNodes[1].InnerText);

                            foreach (HtmlNode n in dl.ChildNodes[1].ChildNodes)
                            {
                                switch (n.GetAttributeValue("class", ""))
                                {
                                    case "v":
                                        r.Votes = HttpUtility.HtmlDecode(n.InnerText);
                                        break;
                                    case "a":
                                        r.UploadDate = n.ChildNodes[0].GetAttributeValue("title", "");
                                        r.AddedSince = n.ChildNodes[0].InnerText;
                                        break;
                                    case "s":
                                        r.Size = n.InnerText; break;
                                    case "u":
                                        r.SeederCount = n.InnerText; break;
                                    case "d":
                                        r.LeechersCount = n.InnerText; break;
                                    default:
                                        break;
                                }
                            }

                            results.Add(r);
                        }
                        catch
                        {
                        }
                    }

                    #endregion

                }// data check
            } //webclient using

            return results.ToArray();
        }

        public static string ToString(this SearchOptions.ArchiveSite site)
        {
            if (site == SearchOptions.ArchiveSite.All) { return null; }
            if (site == SearchOptions.ArchiveSite._1337x_To) { return "1337x.to"; }
            if (site == SearchOptions.ArchiveSite.BtChat) { return "bt-chat.com"; }

            string[] s = site.ToString().ToLower().Split('_');

            return s[0] + "." + s[1];
        }
    }

    public class SearchOptions
    {
        public static readonly DateTime NODATE = new DateTime(0);

        #region Properties

        public bool ExactTerm { get; set; }
        public List<string> ExcludedTerms { get; set; }

        /// <summary>
        /// search for Query added more then x time ago
        /// </summary>
        public DateTime AddedSince { get; set; }

        /// <summary>
        /// search for Query added less then x time ago
        /// </summary>
        public DateTime AddedBefore { get; set; }

        public long MinimumSize { get; set; }
        public long MaximumSize { get; set; }

        public int MinimumSeeds { get; set; }
        public int MinimumPeers { get; set; }

        public ArchiveSite Site { get; set; }

        public SearchMode Mode { get; set; }
        #endregion

        public SearchOptions()
        {
            this.ExactTerm = false;
            this.ExcludedTerms = new List<string>();
            this.AddedSince = NODATE;
            this.AddedBefore = NODATE;

            this.MinimumSeeds = 1;
            this.MinimumPeers = 1;
            this.MinimumSize = 0;
            this.MaximumSize = -1;

            this.Site = ArchiveSite.All;
            this.Mode = SearchMode.CombineKeyWords;
        }

        public enum ArchiveSite
        {
            All,
            Eztv_It,
            PublicHD_Se,
            BtChat,
            Demonoid_Ph,
            KickAss_To,
            Newtorrents_Info,
            Rargb_Com,
            Archive_Org,
            ThePirateBay_Org,
            TorLock_Org,
            Monova_Org,
            TorrentReactor_Net,
            SpeedPeer_Me,
            TorrentDownloads_Me,
            Torrents_Net,
            LimeTorrents_CC,
            TorrentCrazy_Com,
            Take_Fm,
            Coda_Fm,
            _1337x_To,
            LinuxTracker_Org,
            ExtraTorrent_CC,
            Vector_Com,
            TorrentHound_Com,
            YourBitTorrent_Com,
            h33t_To,
            Fulldls_Com,
            TorrentZap_Com,
            TorrentBit_Net,
            TorrentFunk_Com,
            BitSnoop_Com
        }

        public enum SearchMode
        {
            /// <summary>
            /// Default Mode. Search for X AND Y
            /// </summary>
            CombineKeyWords,
            /// <summary>
            /// Search for "X Y"
            /// </summary>
            ExactKeyWords,
            /// <summary>
            /// Search for items that start with X
            /// </summary>
            StartWith,
            /// <summary>
            /// Search for any keyword that start with X
            /// </summary>
            PrefixedWith
        }
    }

    public struct SearchResult
    {
        public string Name { get; set; }
        public string InfoHash { get; set; }
        public string Categories { get; set; }

        public string UploadDate { get; set; }
        public string AddedSince { get; set; }
        public string Size { get; set; }
        public string Votes { get; set; }
        public string SeederCount { get; set; }
        public string LeechersCount { get; set; }

        public DateTime TimeAdded
        {
            get { return DateTime.Parse(this.UploadDate); }
        }

        private long _sb;

        public void SearchResults()
        {
            _sb = -1;
        }

        public long SizeBytes
        {
            get
            {
                if (_sb > 0) { return _sb; }

                string siz = this.Size.Trim().ToLower();

                long mult = 1;

                if (siz.Contains("kb")) { mult = 1024; siz = siz.Replace("kb", ""); }
                if (siz.Contains("mb")) { mult = 1024 * 1024; siz = siz.Replace("mb", ""); }
                if (siz.Contains("gb")) { mult = 1024 * 1024 * 1024; siz = siz.Replace("gb", ""); }

                long result = -1;

                long.TryParse(siz.Trim(), out result);

                if (result >= 0)
                {
                    _sb = result * mult;
                    return _sb;
                }
                else
                {
                    _sb = 0;
                    return 0;
                }
            }

        }
    }
}
