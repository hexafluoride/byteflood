using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace ByteFlood
{
    /*
     * Notes:
     * All of the following torrent states {Paused,Downloading,Seeding*,Metadata,Hashing} are considered active.
     * Unless {Seeding} is not considered active.
     * 
     * Still todo: 
     * - Do appropriate action when App.Settings.QueueSize is changed
     * - The current implementation can seperate torrents from the queuing system, but this isn't implemented yet.
     */

    public class TorrentQueue
    {
        private Amib.Threading.SmartThreadPool slots = null;

        private State AppState = null;

        public bool SeedingTorrentsAreActive { get; set; }

        private Dictionary<string, QueueCake> queue_info_store = new Dictionary<string, QueueCake>();

        public TorrentQueue(State st)
        {
            this.SeedingTorrentsAreActive = false;

            this.AppState = st;

            this.slots = new Amib.Threading.SmartThreadPool();
            this.slots.MaxThreads = App.Settings.QueueSize;
        }

        public void QueueTorrent(TorrentInfo ti)
        {
            if (!queue_info_store.ContainsKey(ti.InfoHash))
            {
                QueueCake cake = new QueueCake();
                cake.Torrent = ti;
                ti.QueueState = QueueState.Queued;

                cake.WorkerThread = new Amib.Threading.Action(() =>
                {
                    ti.Torrent.Start();
                    ti.is_going_to_start = false;

                    while (ti.QueueState == QueueState.Queued)
                    {
                        if (is_inactive(ti))
                        {
                            // The torrent is either:
                            // stopped or has an error.
                            // or is complete
                            break;
                        }
                        else
                        {
                            //wait until something happen, while keeping the queue busy
                            Thread.Sleep(1000);
                        }
                    }
                    //if we get here, then either the torrent was dequeued, or it is inactive
                    //if (ti.IsComplete) { return; }
                });

                ti.is_going_to_start = true;
                cake.ThreadBG = this.slots.QueueWorkItem(cake.WorkerThread);
             
                this.queue_info_store.Add(ti.InfoHash, cake);
            }
            else
            {
                ti.Start();
            }
        }

        /// <summary>
        /// The caller should stop/start/whatever on the torrent, since it's no longer in the queue
        /// </summary>
        public void DeQueueTorrent(TorrentInfo ti)
        {
            if (queue_info_store.ContainsKey(ti.InfoHash))
            {
                var cake = queue_info_store[ti.InfoHash];
                cake.Dequeue();
                queue_info_store.Remove(ti.InfoHash);
            }
            else
            {
                ti.QueueState = QueueState.NotQueued;
            }
        }

        public int GetTorrentIndex(TorrentInfo ti) 
        {
            if (queue_info_store.ContainsKey(ti.InfoHash)) 
            {
               return Array.IndexOf(queue_info_store.Keys.ToArray(), ti.InfoHash);
            }
            return -1;
        }

        private bool can_be_queued(TorrentInfo ti)
        {
            return !(ti.IsComplete || ti.QueueState == QueueState.NotQueued);
        }

        private bool is_inactive(TorrentInfo ti)
        {
            if (ti.Torrent != null)
            {
                return
                     ti.Torrent.State == MonoTorrent.Common.TorrentState.Stopped ||
                     ti.Torrent.State == MonoTorrent.Common.TorrentState.Error ||
                     ti.Torrent.State == MonoTorrent.Common.TorrentState.Stopping ||
                     !this.SeedingTorrentsAreActive && ti.Torrent.State == MonoTorrent.Common.TorrentState.Seeding;
            }
            return false;
        }

    }

    public class QueueCake
    {
        public TorrentInfo Torrent { get; set; }
        public Amib.Threading.Action WorkerThread { get; set; }

        public Amib.Threading.IWorkItemResult ThreadBG { get; set; }

        public void Dequeue()
        {
            this.Torrent.QueueState = QueueState.NotQueued;
            ThreadBG.Cancel(true);
        }
    }
}
