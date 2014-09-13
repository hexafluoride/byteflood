using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using Jayrock.Json;
using Jayrock.Json.Conversion;

namespace ByteFlood.Services
{
    public static class AutoUpdater
    {
        public static string UpdateURL = "https://api.github.com/repos/hexafluoride/byteflood/releases";

        static Thread bg;

        public static void StartMonitoring()
        {
            bg = new Thread(loop);
            bg.IsBackground = true;
            bg.Start();
        }

        public static void StopMonitoring()
        {
            if (bg != null && bg.IsAlive)
            {
                bg.Abort();
            }
        }

        private static void loop()
        {
            while (true)
            {
                CheckforUpdates();
                Thread.Sleep(3600000); //1 hour
            }
        }

        public static NewUpdateInfo CheckforUpdates(bool fire_events = true)
        {
            try
            {
                using (WebClient nc = new WebClient())
                {
                    nc.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 6.1; rv:31.0) Firefox/31.0");
                    nc.Headers.Add(HttpRequestHeader.IfNoneMatch, App.Settings.UpdateSourceEtag);

                    string data = nc.DownloadString(UpdateURL);

                    App.Settings.UpdateSourceEtag = nc.ResponseHeaders[HttpResponseHeader.ETag];

                    JsonArray releases_list = JsonConvert.Import<JsonArray>(data);

                    JsonObject latest_release = (JsonObject)releases_list[0];

                    bool prerelease = Convert.ToBoolean(latest_release["prerelease"]);
                    if (prerelease) { return null; }

                    string tag = Convert.ToString(latest_release["tag_name"]);

                    int version = 0;
                    string a = tag.Remove(0, 1).Replace(".", "");

                    Int32.TryParse(a, out version);

                    if (version > Utility.ByteFloodVersion)
                    {
                        NewUpdateInfo ifo = new NewUpdateInfo();
                        ifo.Tag = tag;
                        ifo.Version = version;

                        ifo.Link = Convert.ToString(latest_release["html_url"]);
                        ifo.Title = Convert.ToString(latest_release["name"]);

                        JsonArray downloads = (JsonArray)latest_release["assets"];

                        foreach (JsonObject release in downloads)
                        {
                            string u = Convert.ToString(release["browser_download_url"]);
                            if (u.EndsWith(".zip"))
                            {
                                ifo.DownloadUrl = u;
                                break;
                            }
                        }

                        ifo.ChangeLog = Convert.ToString(latest_release["body"]);

                        if (NewUpdate != null && fire_events) { NewUpdate(ifo); }
                        return ifo;
                    }
                }
            }
            catch (WebException wex) 
            {
                //{"The remote server returned an error: (304) Not Modified."}
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
            return null;
        }

        public delegate void NewUpdateEvent(NewUpdateInfo info);
        public static event NewUpdateEvent NewUpdate;
    }

    public class NewUpdateInfo
    {
        public string Title { get; set; }
        public int Version { get; set; }
        public string Tag { get; set; }
        public string DownloadUrl { get; set; }
        public string Link { get; set; }
        public string ChangeLog { get; set; }
    }
}
