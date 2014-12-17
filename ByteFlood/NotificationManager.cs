using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ByteFlood
{
    public static class NotificationManager
    {
        private static List<string> dismissed_notifications = new List<string>();

        public static void Notify(Notification i)
        {
            if (dismissed_notifications.Contains(i.ID))
            {
                return;
            }

            // for now, this is how will I handle notifications.
            // I intend to get visualstudio-like notification pane.
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                (App.Current.MainWindow as MainWindow).NotifyIcon.ShowBalloonTip(i.Title, i.Message, get_icon(i.Type));
            }));
        }

        public static void DismissNotification(Notification i)
        {
            dismissed_notifications.Add(i.ID);
        }

        private static Hardcodet.Wpf.TaskbarNotification.BalloonIcon get_icon(NotificationType t)
        {
            switch (t)
            {
                case NotificationType.Error:
                    return Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Error;
                case NotificationType.Info:
                    return Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info;
                case NotificationType.Warning:
                    return Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Warning;
                default:
                    return Hardcodet.Wpf.TaskbarNotification.BalloonIcon.None;
            }
        }

    }

    public class Notification
    {
        public Notification()
        {
        }

        public Notification(NotificationType type, string title, string message)
        {
            this.Message = message;
            this.Type = type;

            this.ID = this.Type.ToString() + this.Title + this.Message;
        }

        public string ID
        {
            get;
            internal set;
        }

        public string Title { get; internal set; }

        public string Message { get; internal set; }

        public NotificationType Type { get; internal set; }

        public void Dismiss()
        {
            NotificationManager.DismissNotification(this);
        }
    }

    public class TorrentAlreadyAddedNotification : Notification 
    {
        public TorrentAlreadyAddedNotification(string name, string infohash) 
        {
            this.Title = "Torrent already added";

            this.Message = string.Format("The torrent \"{0}\" has been already added to the torrent list.", name);

            this.ID = "torrent_added_" + infohash;
        }
    }

    public class MagnetLinkNotification : Notification
    {
        private string infohash = null;
        private string mg_name = null;

        public MagnetLinkNotification(EventType type, MonoTorrent.MagnetLink m)
            : this(type, m.Name, m.InfoHash.ToHex()) { }

        public MagnetLinkNotification(EventType type, Ragnar.TorrentHandle h)
            : this(type, h.TorrentFile.Name, h.TorrentFile.InfoHash) { }

        public MagnetLinkNotification(EventType type, string mg_name, string infohash)
        {
            this.mg_name = mg_name;
            this.infohash = infohash;

            this.ID = string.Format("magnet_nofitication_" + type.ToString() + infohash);

            this.Type = map_et_to_nt(type);

            this.Title = string.Format("Maget link: {0}", mg_name);

            this.Message = map_et_to_msg(type);
        }

        private string map_et_to_msg(EventType et)
        {
            switch (et)
            {
                case EventType.MetadataDownloadComplete:
                    return string.Format("Metadata information has been successfully retrieved for \"{0}\"", mg_name);
                case EventType.MetadataDownloadStarted:
                    return string.Format("Metadata download has been started. The torrent will be added to the list after a while.");
                case EventType.MetadataDownloadFailed:
                    return string.Format("Unable to start this magnet link: {0}", mg_name);
                default:
                    return null;
            }
        }

        private NotificationType map_et_to_nt(EventType et)
        {
            switch (et)
            {
                case EventType.MetadataDownloadComplete:
                case EventType.MetadataDownloadStarted:
                    return NotificationType.Info;
                case EventType.MetadataDownloadFailed:
                    return NotificationType.Error;

                default:
                    return NotificationType.NOTYPE;
            }
        }

        public enum EventType
        {
            MetadataDownloadStarted,
            MetadataDownloadComplete,
            MetadataDownloadFailed
        }
    }

    /*
    public class MagnetLinkComplete : Notification { }

    public class TorrentComplete : Notification { }

    public class TorrentError : Notification { } 
    */
    public enum NotificationType
    {
        NOTYPE,
        Info,
        Error,
        Warning
    }

}
