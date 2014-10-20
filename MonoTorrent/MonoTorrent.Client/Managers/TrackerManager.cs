//
// TrackerManager.cs
//
// Authors:
//   Alan McGovern alan.mcgovern@gmail.com
//
// Copyright (C) 2006 Alan McGovern
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//


using System;
using System.Text;
using System.Net;
using System.IO;
using MonoTorrent.Common;
using System.Collections.ObjectModel;
using System.Threading;
using System.Web;
using System.Diagnostics;
using System.Collections.Generic;
using MonoTorrent.BEncoding;
using MonoTorrent.Client.Encryption;
using System.Collections;

namespace MonoTorrent.Client.Tracker
{
    /// <summary>
    /// Represents the connection to a tracker that an TorrentManager has
    /// </summary>
    public class TrackerManager : IEnumerable<TrackerTier>
    {
        #region Member Variables
        private TorrentManager manager;
        IList<TrackerTier> tierList;

        /// <summary>
        /// The infohash for the torrent
        /// </summary>
        private InfoHash infoHash;

        /// <summary>
        /// The trackers available
        /// </summary>
        public IList<TrackerTier> TrackerTiers
        {
            get { return tierList; }
        }
        List<TrackerTier> trackerTiers;

        #endregion

        /// <summary>
        /// List that hold all our torrents trackers
        /// </summary>
        private List<Tracker> _trackers = null;

        #region Constructors

        /// <summary>
        /// Creates a new TrackerConnection for the supplied torrent file
        /// </summary>
        /// <param name="manager">The TorrentManager to create the tracker connection for</param>
        public TrackerManager(TorrentManager manager, InfoHash infoHash, IList<RawTrackerTier> announces)
        {
            this.manager = manager;
            this.infoHash = infoHash;
            this._trackers = new List<Tracker>();

            // Check if this tracker supports scraping
            trackerTiers = new List<TrackerTier>();
            for (int i = 0; i < announces.Count; i++)
                trackerTiers.Add(new TrackerTier(announces[i]));

            trackerTiers.RemoveAll(delegate(TrackerTier t) { return t.Trackers.Count == 0; });
            foreach (TrackerTier tier in trackerTiers)
            {
                foreach (Tracker tracker in tier)
                {
                    tracker.AnnounceComplete += delegate(object o, AnnounceResponseEventArgs e)
                    {
                        ClientEngine.MainLoop.Queue(delegate { OnAnnounceComplete(o, e); });
                    };

                    tracker.ScrapeComplete += delegate(object o, ScrapeResponseEventArgs e)
                    {
                        ClientEngine.MainLoop.Queue(delegate { OnScrapeComplete(o, e); });
                    };

                    this._trackers.Add(tracker);
                }
            }

            tierList = new ReadOnlyCollection<TrackerTier>(trackerTiers);
        }

        #endregion

        #region Methods

        //Ideally, we should check tiers, instead of individual trackers.
        public void CheckAndAnnounceAll(TorrentEvent ev = TorrentEvent.None)
        {
            foreach (Tracker t in this._trackers)
            {
                if (CanAnnounce(t))
                {
                    _announce_t(t, ev);
                }
            }
        }

        public void AnnounceTracker(Tracker t, TorrentEvent ev = TorrentEvent.None)
        {
            if (CanAnnounce(t))
            {
                _announce_t(t, ev);
            }
        }

        // TODO: Make use of the Minimum Update Interval
        // TODO: Allow the user to bypass this (for manual announces) but ONLY if it's larger than the allowed minimum limit
        private bool CanAnnounce(Tracker t)
        {
            if (t.FailureCount >= 5)
            {
                return false;
            }

            bool a = DateTime.Now - t.LastUpdated >= t.UpdateInterval;
            return a;
        }

        private void _announce_t(Tracker t, TorrentEvent ev)
        {
            EncryptionTypes e = this.manager.Engine.Settings.AllowedEncryption;
            bool requireEncryption = !Toolbox.HasEncryption(e, EncryptionTypes.PlainText);
            bool supportsEncryption = Toolbox.HasEncryption(e, EncryptionTypes.RC4Full) || Toolbox.HasEncryption(e, EncryptionTypes.RC4Header);

            requireEncryption = requireEncryption && ClientEngine.SupportsEncryption;
            supportsEncryption = supportsEncryption && ClientEngine.SupportsEncryption;

            //The IP address to report to the tracker, if any.
            IPEndPoint reportedAddress = this.manager.Engine.Settings.ReportedAddress;

            var parameters = new AnnounceParameters()
            {
                BytesDownloaded = this.manager.Monitor.DataBytesDownloaded,
                BytesUploaded = this.manager.Monitor.DataBytesUploaded,
                BytesLeft = get_remaining_bytes(),
                ClientEvent = ev,
                InfoHash = this.infoHash,
                RequireEncryption = requireEncryption,
                SupportsEncryption = supportsEncryption,
                PeerId = this.manager.Engine.PeerId,
            };

            if (this.manager.Engine.Settings.ReportedAddress != null)
            {
                parameters.Ipaddress = reportedAddress.Address.ToString();
                parameters.Port = reportedAddress.Port;
            }
            else
            {
                parameters.Ipaddress = null;
                parameters.Port = this.manager.Engine.Listener.Endpoint.Port;
            }

            var connection_id = new TrackerConnectionID(t, false, ev, null);

            t.Announce(parameters, connection_id);
        }

        private long get_remaining_bytes()
        {
            if (this.manager.HasMetadata)
            {
                return Convert.ToInt64((1 - this.manager.Bitfield.PerOneComplete) * this.manager.Torrent.Size);
            }
            else
            {
                /*
                 * In metadata mode we need to pretend we need to download data otherwise
                 * tracker optimisations might result in no peers being sent back.
                 */
                return 1000L;
            }
        }

        /*
        public WaitHandle Announce()
        {
            if (CurrentTracker == null)
                return new ManualResetEvent(true);

            return Announce(trackerTiers[0].SentStartedEvent ? TorrentEvent.None : TorrentEvent.Started);
        }


        public WaitHandle Announce(Tracker tracker)
        {
            Check.Tracker(tracker);
            TrackerTier tier = trackerTiers.Find(delegate(TrackerTier t) { return t.Trackers.Contains(tracker); });
            if (tier == null)
                throw new ArgumentException("Tracker has not been registered with the manager", "tracker");

            TorrentEvent tevent = tier.SentStartedEvent ? TorrentEvent.None : TorrentEvent.Started;
            return Announce(tracker, tevent, false, new ManualResetEvent(false));
        }
        
        internal WaitHandle Announce(TorrentEvent clientEvent)
        {
            if (CurrentTracker == null)
                return new ManualResetEvent(true);
            return Announce(CurrentTracker, clientEvent, true, new ManualResetEvent(false));
        }

        private WaitHandle Announce(Tracker tracker, TorrentEvent clientEvent, bool trySubsequent, ManualResetEvent waitHandle)
        {
            ClientEngine engine = manager.Engine;

            // If the engine is null, we have been unregistered
            if (engine == null)
            {
                waitHandle.Set();
                return waitHandle;
            }

            this.updateSucceeded = true;
            this.lastUpdated = DateTime.Now;

            EncryptionTypes e = engine.Settings.AllowedEncryption;
            bool requireEncryption = !Toolbox.HasEncryption(e, EncryptionTypes.PlainText);
            bool supportsEncryption = Toolbox.HasEncryption(e, EncryptionTypes.RC4Full) || Toolbox.HasEncryption(e, EncryptionTypes.RC4Header);

            requireEncryption = requireEncryption && ClientEngine.SupportsEncryption;
            supportsEncryption = supportsEncryption && ClientEngine.SupportsEncryption;

            IPEndPoint reportedAddress = engine.Settings.ReportedAddress;
            string ip = reportedAddress == null ? null : reportedAddress.Address.ToString();
            int port = reportedAddress == null ? engine.Listener.Endpoint.Port : reportedAddress.Port;

            // FIXME: In metadata mode we need to pretend we need to download data otherwise
            // tracker optimisations might result in no peers being sent back.
            long bytesLeft = 1000;
            if (manager.HasMetadata)
                bytesLeft = (long)((1 - this.manager.Bitfield.PercentComplete / 100.0) * this.manager.Torrent.Size);
            AnnounceParameters p = new AnnounceParameters(this.manager.Monitor.DataBytesDownloaded,
                                                this.manager.Monitor.DataBytesUploaded,
                                                bytesLeft,
                                                clientEvent, this.infoHash, requireEncryption, manager.Engine.PeerId,
                                                ip, port);
            p.SupportsEncryption = supportsEncryption;
            TrackerConnectionID id = new TrackerConnectionID(tracker, trySubsequent, clientEvent, waitHandle);
            tracker.Announce(p, id);
            return waitHandle;
        }

        private bool GetNextTracker(Tracker tracker, out TrackerTier trackerTier, out Tracker trackerReturn)
        {
            for (int i = 0; i < this.trackerTiers.Count; i++)
            {
                for (int j = 0; j < this.trackerTiers[i].Trackers.Count; j++)
                {
                    if (this.trackerTiers[i].Trackers[j] != tracker)
                        continue;

                    // If we are on the last tracker of this tier, check to see if there are more tiers
                    if (j == (this.trackerTiers[i].Trackers.Count - 1))
                    {
                        if (i == (this.trackerTiers.Count - 1))
                        {
                            trackerTier = null;
                            trackerReturn = null;
                            return false;
                        }

                        trackerTier = this.trackerTiers[i + 1];
                        trackerReturn = trackerTier.Trackers[0];
                        return true;
                    }

                    trackerTier = this.trackerTiers[i];
                    trackerReturn = trackerTier.Trackers[j + 1];
                    return true;
                }
            }

            trackerTier = null;
            trackerReturn = null;
            return false;
        }*/

        private void OnScrapeComplete(object sender, ScrapeResponseEventArgs e)
        {
            e.Id.WaitHandle.Set();
        }

        private void OnAnnounceComplete(object sender, AnnounceResponseEventArgs e)
        {
            //add tracker.lastupdated field

            e.Tracker.LastUpdated = DateTime.Now;

            if (e.Successful)
            {
                manager.Peers.BusyPeers.Clear();
                int count = manager.AddPeersCore(e.Peers);
                manager.RaisePeersFound(new TrackerPeersAdded(manager, count, e.Peers.Count, e.Tracker));
                
                // If the announce was successful, the server should have provided the appropriate update interval,
                // the value of e.Tracker.UpdateInterval is then set by the corresponding Tracker class

                //e.Tracker.UpdateInterval = TimeSpan.FromMinutes(20);
            }
            else
            {
                e.Tracker.UpdateInterval = TimeSpan.FromMinutes(3);

                /* TODO: if it is because of {connection time out, connection error, transport failed, DNS lookup error}, 
                 * update in 2 mins, otherwise increment the failure count.
                 */
            }

            /*
            this.updateSucceeded = e.Successful;
            if (manager.Engine == null)
            {
                e.Id.WaitHandle.Set();
                return;
            }

            if (e.Successful)
            {
                manager.Peers.BusyPeers.Clear();
                int count = manager.AddPeersCore(e.Peers);
                manager.RaisePeersFound(new TrackerPeersAdded(manager, count, e.Peers.Count, e.Tracker));

                TrackerTier tier = trackerTiers.Find(delegate(TrackerTier t) { return t.Trackers.Contains(e.Tracker); });
                if (tier != null)
                {
                    Toolbox.Switch<Tracker>(tier.Trackers, 0, tier.IndexOf(e.Tracker));
                    Toolbox.Switch<TrackerTier>(trackerTiers, 0, trackerTiers.IndexOf(tier));
                }
                e.Id.WaitHandle.Set();
            }
            else
            {
                TrackerTier tier;
                Tracker tracker;

                if (!e.Id.TrySubsequent || !GetNextTracker(e.Tracker, out tier, out tracker))
                    e.Id.WaitHandle.Set();
                else
                    Announce(tracker, e.Id.TorrentEvent, true, e.Id.WaitHandle);
            }*/
        }

        //public WaitHandle Scrape()
        //{
        //    if (CurrentTracker == null)
        //        return new ManualResetEvent(true);
        //    return Scrape(CurrentTracker, false);
        //}

        public WaitHandle Scrape(Tracker tracker)
        {
            TrackerTier tier = trackerTiers.Find(delegate(TrackerTier t) { return t.Trackers.Contains(tracker); });
            if (tier == null)
                return new ManualResetEvent(true);

            return Scrape(tracker, false);
        }

        private WaitHandle Scrape(Tracker tracker, bool trySubsequent)
        {
            if (tracker == null)
                throw new ArgumentNullException("tracker");

            if (!tracker.CanScrape)
                throw new TorrentException("This tracker does not support scraping");

            TrackerConnectionID id = new TrackerConnectionID(tracker, trySubsequent, TorrentEvent.None, new ManualResetEvent(false));
            tracker.Scrape(new ScrapeParameters(this.infoHash), id);
            return id.WaitHandle;
        }

        #endregion

        public IEnumerator<TrackerTier> GetEnumerator()
        {
            return trackerTiers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
