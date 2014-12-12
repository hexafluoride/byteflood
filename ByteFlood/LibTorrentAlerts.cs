using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ragnar;

namespace ByteFlood
{
    public class LibTorrentAlertsWatcher
    {
        private System.Threading.Thread watcher_thread;

        private System.Windows.Threading.Dispatcher main_thread_dispatcher;

        private Ragnar.Session ses;
        private Ragnar.IAlertFactory alerts;
        public LibTorrentAlertsWatcher(Ragnar.Session session)
        {
            ses = session;
            alerts = ses.Alerts;

            main_thread_dispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;

            watcher_thread = new System.Threading.Thread(monitor);
            watcher_thread.Priority = System.Threading.ThreadPriority.AboveNormal;
            watcher_thread.Start();
        }

        private bool running = true;

        public void StopWatching()
        {
            running = false;
        }

        public bool IsRunning { get { return this.watcher_thread.IsAlive; } }

        private void monitor()
        {
            var timeout = TimeSpan.FromSeconds(0.5);
            var lastPost = DateTime.Now;

            while (true)
            {
                if ((DateTime.Now - lastPost).TotalSeconds > 1)
                {
                    ses.PostTorrentUpdates();

                    lastPost = DateTime.Now;
                }

                var foundAlerts = ses.Alerts.PeekWait(timeout);
                if (!foundAlerts)
                {
                    if (running)
                        continue;
                    else
                        return;
                }

                var alerts = ses.Alerts.PopAll();

                foreach (var alert in alerts)
                {
                    Type alert_type = alert.GetType();

                    if (alert_type == typeof(SaveResumeDataAlert))
                    {
                        SaveResumeDataAlert srda = (SaveResumeDataAlert)alert;
                        main_thread_dispatcher.Invoke(() => ResumeDataArrived(srda.Handle, srda.ResumeData));
						continue;
                    }

                    if (alert_type == typeof(TorrentAddedAlert))
                    {
                        TorrentAddedAlert taa = (TorrentAddedAlert)alert;
                        main_thread_dispatcher.Invoke(() => TorrentAdded(taa.Handle));
                        continue;
                    }

                    if (alert_type == typeof(StateChangedAlert))
                    {
                        StateChangedAlert taa = (StateChangedAlert)alert;
                        main_thread_dispatcher.Invoke(() => TorrentStateChanged(taa.Handle, taa.PreviousState, taa.State));
                        continue;
                    }

                    if (alert_type == typeof(StateUpdateAlert))
                    {
                        StateUpdateAlert sua = (StateUpdateAlert)alert;
                        foreach (var s in sua.Statuses)
                        {
                            main_thread_dispatcher.Invoke(() => TorrentStatsUpdated(s));
                        }
						continue;
                    }

                    if (alert_type == typeof(TorrentFinishedAlert))
                    {
                        TorrentFinishedAlert tfa = (TorrentFinishedAlert)alert;
                        main_thread_dispatcher.Invoke(() => TorrentFinished(tfa.Handle));
                        continue;
                    }


                    if (alert_type == typeof(MetadataReceivedAlert))
                    {
                        MetadataReceivedAlert mra = (MetadataReceivedAlert)alert;
                        main_thread_dispatcher.Invoke(() => MetadataReceived(mra.Handle));
                        continue;
                    }

                    /*
                        case typeof(Ragnar.FileCompletedAlert):
                        case typeof(Ragnar.FileRenamedAlert):

                        case typeof(Ragnar.PeerAlert):
                        case typeof(Ragnar.PeerBanAlert):
                        case typeof(Ragnar.PeerConnectAlert):
                        case typeof(Ragnar.PeerUnsnubbedAlert):
                        case typeof(Ragnar.PieceFinishedAlert):

                        case typeof(Ragnar.ScrapeReplyAlert):
                        case typeof(Ragnar.StorageMovedAlert):

                        case typeof(Ragnar.TorrentCheckedAlert):
                        case typeof(Ragnar.TorrentErrorAlert):

                        case typeof(Ragnar.TorrentPausedAlert):
                        case typeof(Ragnar.TorrentRemovedAlert):
                        case typeof(Ragnar.TorrentResumedAlert):
                     * 
                        case typeof(Ragnar.UnwantedBlockAlert):
                    */
                    System.Diagnostics.Debug.WriteLine(alert.Message);
                }

                System.Threading.Thread.Sleep(150);
            }
        }

        public delegate void ResumeDataEvent(TorrentHandle handle, byte[] data);
        public event ResumeDataEvent ResumeDataArrived;

        public delegate void TorrentAddedEvent(TorrentHandle handle);
        public event TorrentAddedEvent TorrentAdded;

        public delegate void TorrentStateChangedEvent(TorrentHandle handle, TorrentState oldstate, TorrentState newstate);
        public event TorrentStateChangedEvent TorrentStateChanged;

        public delegate void TorrentStatsUpdatedEvent(TorrentStatus status);
        public event TorrentStatsUpdatedEvent TorrentStatsUpdated;

        //public delegate void TorrentNetStatsUpdatedEvent(StatsAlert sa);
        //public event TorrentNetStatsUpdatedEvent TorrentNetworkStatisticsUpdated;

        public delegate void TorrentFinishedEvent(TorrentHandle handle);
        public event TorrentFinishedEvent TorrentFinished;


        public delegate void TorrentMetadataReceivedEvent(TorrentHandle handle);
        /// <summary>
        /// This event only fire when torrent metadata has been completly received
        /// </summary>
        public event TorrentMetadataReceivedEvent MetadataReceived;

    }
}
