using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Threading.Tasks;
using Jayrock.Json;
using Jayrock.Json.Conversion;

namespace ByteFlood.Services
{
    public static class AutoUpdater
    {
        public const string UpdateURL = "https://api.github.com/repos/hexafluoride/byteflood/releases";

	    private static CancellationTokenSource _monitoringTokenSource;

        public static void StartMonitoring()
        {
			_monitoringTokenSource = new CancellationTokenSource();
	        Task.Run(() => Loop(_monitoringTokenSource.Token), _monitoringTokenSource.Token);
        }

        public static void StopMonitoring()
        {
			if (_monitoringTokenSource != null)
				_monitoringTokenSource.Cancel();
        }

        private async static Task Loop(CancellationToken token)
        {
            while (true)
            {
				if (token.IsCancellationRequested)
		            break;

                if (!App.Settings.CheckForUpdates)
                    break;

	            await Task.WhenAll(CheckforUpdatesAsync(token), Task.Delay(TimeSpan.FromHours(1), token));
            }
        }

        public static async Task<NewUpdateInfo> CheckforUpdatesAsync(CancellationToken token, bool fire_events = true)
        {
            try
            {
                using (WebClient nc = new WebClient())
                {
                    nc.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 6.1; rv:31.0) Firefox/31.0");
                    nc.Headers.Add(HttpRequestHeader.IfNoneMatch, App.Settings.UpdateSourceEtag);

                    var task = nc.DownloadStringTaskAsync(UpdateURL);
					token.Register(nc.CancelAsync);
	                var data = await task;

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
            catch (WebException) 
            {
                //{"The remote server returned an error: (304) Not Modified."}
                return null;
            }
            catch (Exception)
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
