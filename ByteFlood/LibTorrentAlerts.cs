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

        private Ragnar.Session ses;
        private Ragnar.IAlertFactory alerts;
        public LibTorrentAlertsWatcher(Ragnar.Session session)
        {
            ses = session;
            alerts = ses.Alerts;

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
                        ResumeDataArrived(srda.Handle, srda.ResumeData);
                    }

                    if (alert_type == typeof(TorrentAddedAlert))
                    {
                        TorrentAddedAlert taa = (TorrentAddedAlert)alert;
                        TorrentAdded(taa.Handle);
                    }

                    if (alert_type == typeof(StateChangedAlert))
                    {
                        StateChangedAlert taa = (StateChangedAlert)alert;
                        TorrentStateChanged(taa.Handle, taa.PreviousState, taa.State);
                    }

                    if (alert_type == typeof(StateUpdateAlert))
                    {
                        StateUpdateAlert sua = (StateUpdateAlert)alert;
                        foreach (var s in sua.Statuses)
                        {
                            TorrentStatsUpdated(s);
                        }
                    }

                    if (alert_type == typeof(TorrentFinishedAlert))
                    {
                        TorrentFinishedAlert tfa = (TorrentFinishedAlert)alert;
                        TorrentFinished(tfa.Handle);
                    }

                    /*
                        case typeof(Ragnar.FileCompletedAlert):
                        case typeof(Ragnar.FileRenamedAlert):
                        case typeof(Ragnar.MetadataReceivedAlert):
                        case typeof(Ragnar.PeerAlert):
                        case typeof(Ragnar.PeerBanAlert):
                        case typeof(Ragnar.PeerConnectAlert):
                        case typeof(Ragnar.PeerUnsnubbedAlert):
                        case typeof(Ragnar.PieceFinishedAlert):

                        case typeof(Ragnar.ScrapeReplyAlert):

                        case typeof(Ragnar.StateUpdateAlert):
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

        public delegate void TorrentFinishedEvent(TorrentHandle handle);
        public event TorrentFinishedEvent TorrentFinished;
    

    }
}
