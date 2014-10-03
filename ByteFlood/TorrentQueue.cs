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
     */

    public class TorrentQueue
    {
        private Amib.Threading.SmartThreadPool slots = null;

        private State AppState = null;

        private Dictionary<string, QueueCake> queue_info_store = new Dictionary<string, QueueCake>();

        //used to check if the queue was initially disabled, and then it was enabled later
        private bool? last_queue_enable_setting = null;

        public TorrentQueue(State st)
        {
            this.AppState = st;

            this.slots = new Amib.Threading.SmartThreadPool();
            this.set_queue_size(App.Settings.QueueSize);
            this.last_queue_enable_setting = App.Settings.EnableQueue;
        }

        public void ReloadSettings()
        {
            if (App.Settings.EnableQueue)
            {
                #region Queue Size Change handling
                if (App.Settings.QueueSize > this.slots.MinThreads)
                {
                    this.set_queue_size(App.Settings.QueueSize);
                }
                else if (this.slots.MinThreads < App.Settings.QueueSize)
                {
                    this.set_queue_size(App.Settings.QueueSize);

                    //then we need to stop x amount of torrents
                    int how_much_to_stop = App.Settings.QueueSize - this.slots.MaxThreads;

                    int stopped_count = 0;

                    if (how_much_to_stop > queue_info_store.Count)
                    {
                        //count backwards from [n-1] element to [0]
                        for (int i = queue_info_store.Count - 1; i >= 0; i--)
                        {
                            try
                            {
                                var kvp = queue_info_store.ElementAt(i);

                                //check if the torrent is active so we don't "stop" already stopped/inactive torrents
                                if (!is_inactive(kvp.Value.Torrent))
                                {  //stop the torrent
                                    kvp.Value.Torrent.Stop();
                                    //requeue it, but since the queue is full a "Queued" status will show up
                                    this.QueueTorrent(kvp.Value.Torrent);
                                    stopped_count++;
                                    if (stopped_count == how_much_to_stop)
                                    {
                                        break;
                                    }
                                }
                            }
                            catch (System.IndexOutOfRangeException)
                            {
                                break;
                            }
                            catch (System.Exception) { }
                        }
                    }
                }
                #endregion

                if (this.last_queue_enable_setting == false)
                {
                    this.last_queue_enable_setting = App.Settings.EnableQueue;
                    for (int i = 0; i < this.AppState.Torrents.Count; i++)
                    {
                        try
                        {
                            TorrentInfo ti = this.AppState.Torrents[i];

                            //ignore forced torrents
                            if (ti.QueueState == QueueState.Queued)
                            {
                                this.QueueTorrent(ti);
                            }
                        }
                        catch (System.IndexOutOfRangeException)
                        {
                            break;
                        }
                        catch (System.Exception) { }
                    }
                }
            }
            else //queue was disabled
            {
                //force all queued items to be processed
                this.set_queue_size(this.AppState.Torrents.Count);

                string[] keys = queue_info_store.Keys.ToArray();
                foreach (string k in keys)
                {
                    QueueCake cake = queue_info_store[k];

                    //this will only kill the uncessary queue thread.
                    //it won't change the torrent state (downloading, paused, etc)
                    cake.Dequeue();
                    queue_info_store.Remove(k);
                }
            }
        }

        private void set_queue_size(int size)
        {
            if (size <= 0)
            {
                this.slots.MinThreads = 0;
                this.slots.MaxThreads = 1;
            }
            else
            {
                this.slots.MaxThreads = Int32.MaxValue;
                this.slots.MinThreads = size;
                this.slots.MaxThreads = size;
            }
        }

        public void QueueTorrent(TorrentInfo ti)
        {
            if (!App.Settings.SeedingTorrentsAreActive && ti.IsComplete)
            {
                //simply start it
                ti.QueueState = QueueState.Queued;
                ti.Torrent.Start();
                return;
            }

            if (App.Settings.EnableQueue)
            {
                if (!queue_info_store.ContainsKey(ti.InfoHash))
                {
                    QueueCake cake = new QueueCake();
                    cake.Torrent = ti;
                    ti.QueueState = QueueState.Queued;
                    ti.Torrent.Stop();

                    cake.WorkerThread = new Amib.Threading.Action(() =>
                    {
                        cake.ThreadBGRunning = true;

                        ti.Torrent.Start();
                        ti.is_going_to_start = false;
                        ti.UpdateList("Status");
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

                        cake.ThreadBGRunning = false;
                        //if we get here, then either the torrent was dequeued, or it is inactive
                        //if (ti.IsComplete) { return; }
                    });

                    ti.is_going_to_start = true;
                    cake.ThreadBG = this.slots.QueueWorkItem(cake.WorkerThread);

                    this.queue_info_store.Add(ti.InfoHash, cake);
                    ti.UpdateList("Status");
                }
                else
                {
                    //torrent is inside the queue store
                    var cake = queue_info_store[ti.InfoHash];

                    if (cake.ThreadBGRunning)
                    {
                        //this happen if the torrent was {downloading} and it was {paused}.
                        //in this case, we start it.
                        //in other words, the torrent still have a slot inside the queue, so we can start it
                        ti.Torrent.Start();
                    }
                    else
                    {
                        //let's seen how can this happen:
                        //- if the user has stopped the torrent, the TorrentInfo class will dequeue the torrent, so the queue_info_store doesn't
                        //  contain this torrent, so this should never happen.
                        //- if the user hasn't stopped the torrent but an error has been occured, the torrent queue thread will be stopped, but
                        //  the torrent cake will remain inside the queue_info_store. 
                        //- if the torrent entered seeding state, and seeding is configured as inactive.

                        //tl;dr torrent either has an error or it is seeding
                        if (ti.Torrent.State == MonoTorrent.Common.TorrentState.Error)
                        {
                            //Attempt to start it again
                            this.queue_info_store.Remove(ti.InfoHash);
                            this.QueueTorrent(ti);
                        }

                        //if (ti.Torrent.State == MonoTorrent.Common.TorrentState.Seeding) 
                        //{
                        //    //the torrent is seeding and the user clicked start, so do nothing
                        //    //except maybe remove the torrent cake, but I am not sure yet.
                        //}
                    }
                }
            }
            else { ti.Torrent.Start(); }
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
            if (App.Settings.EnableQueue && queue_info_store.ContainsKey(ti.InfoHash))
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
                     !App.Settings.SeedingTorrentsAreActive && ti.Torrent.State == MonoTorrent.Common.TorrentState.Seeding;
            }
            return false;
        }

    }

    public class QueueCake
    {
        public TorrentInfo Torrent { get; set; }
        public Amib.Threading.Action WorkerThread { get; set; }

        public Amib.Threading.IWorkItemResult ThreadBG { get; set; }
        public bool ThreadBGRunning { get; set; }

        public void Dequeue()
        {
            this.Torrent.QueueState = QueueState.NotQueued;
            this.ThreadBG.Cancel(true);
            this.ThreadBGRunning = false;
        }
    }
}
