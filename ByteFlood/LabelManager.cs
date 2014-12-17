using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ByteFlood
{
    public class LabelManager : INotifyPropertyChanged
    {
        public State AppState
        {
            get;
            private set;
        }

        public ObservableCollection
            <TorrentLabel> Labels { get; private set; }

        //This is used in the MainWindow -> "Add existing label" menu
        public System.Windows.Visibility CustomLabelsExists 
        {
            get 
            {
                if (Labels.Count > 1)
                    return System.Windows.Visibility.Visible;
                else
                    return System.Windows.Visibility.Collapsed;
            }
        }

        public LabelManager(State s)
        {
            this.AppState = s;

            this.Labels = new ObservableCollection<TorrentLabel>();

            this.Labels.Add(new TorrentLabel(null, null, this)); // This is for the "no label" case

            this.Labels.CollectionChanged += Labels_CollectionChanged;
        }

        void Labels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("CustomLabelsExists"));
        }

        private string _sl = null;

        public string SelectedLabel
        {
            get { return _sl; }
            set { _sl = value; }
        }

        private TorrentLabel check_label(string label)
        {
            var results = Labels.Where(t => t.Name == label);

            if (results.Count() == 1)
            {
                return results.First();
            }
            else
            {
                TorrentLabel t = new TorrentLabel(label, new ObservableCollection<TorrentInfo>(), this);
                Labels.Add(t);
                return t;
            }
        }

        public void AddLabelForTorrent(TorrentInfo ti, string label)
        {
            var t = check_label(label);
            t.RegisterTorrent(ti);
            Labels[0].RefreshCount();
        }

        public void AddLabelForTorrent(TorrentInfo ti, TorrentLabel label)
        {
            label.RegisterTorrent(ti);
            Labels[0].RefreshCount();
        }

        public void RemoveLabelForTorrent(TorrentInfo ti, string label)
        {
            var t = check_label(label);
            t.UnregisterTorrent(ti);
            Labels[0].RefreshCount();
        }

        public void RemoveLabel(TorrentLabel label)
        {
            if (label.Name != null)
            {
                label.RefreshCount();
                if (label.Count != 0)
                    label.UnregisterAll();
                this.Labels.Remove(label);

                Labels[0].RefreshCount();
            }
        }

        public bool TorrentHasLabel(TorrentInfo ti, string label)
        {
            return check_label(label).HasTorrent(ti);
        }

        public bool TorrentHasAnyLabel(TorrentInfo ti)
        {
            for (int i = 1 /*skip the null label*/; i < Labels.Count; i++)
            {
                if (Labels[i].HasTorrent(ti))
                    return true;
            }
            return false;
        }

        public string GetFirstLabelForTorrent(TorrentInfo ti) 
        {
            for (int i = 1 /*skip the null label*/; i < Labels.Count; i++)
            {
                if (Labels[i].HasTorrent(ti))
                    return Labels[i].Name;
            }
            return null;
        }

        public bool EnableFilter { get; set; }

        public bool Can_I_ShowUP(TorrentInfo ti)
        {
            if (this.EnableFilter)
            {
                if (_sl != null)
                {
                    return TorrentHasLabel(ti, SelectedLabel);
                }
                else
                {
                    return !TorrentHasAnyLabel(ti);
                }
            }
            return true;
        }

        public string[] GetLabelsForTorrent(TorrentInfo ti)
        {
            List<string> a = new List<string>();

            for (int i = 1 /*skip the null label*/; i < Labels.Count; i++)
            {
                TorrentLabel label = Labels[i];
                if (label.HasTorrent(ti))
                    a.Add(label.Name);
            }

            return a.ToArray();
        }

        public void ClearLabels(TorrentInfo ti)
        {
            for (int i = 1 /*skip the null label*/; i < Labels.Count; i++)
            {
                Labels[i].UnregisterTorrent(ti);
            }
            Labels[0].RegisterTorrent(ti); // the null label
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class TorrentLabel : INotifyPropertyChanged
    {
        public string Name { get; private set; }

        private int _c = 0;
        public int Count
        {
            get { return _c; }
            set
            {
                if (_c != value)
                {
                    this._c = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("Count"));
                }
            }
        }

        private ObservableCollection<TorrentInfo> my_torrent_list;

        private LabelManager lm;

        public TorrentLabel(string name, ObservableCollection<TorrentInfo> my_c, LabelManager LM)
        {
            this.lm = LM;

            this.Name = name;

            if (my_c == null)
            {
                // bind to AppState Torrent list
                this.lm.AppState.Torrents.CollectionChanged
                    += (s, e) =>
                    {
                        RefreshCount();
                    };
            }
            else
            {
                my_torrent_list = my_c;
                my_torrent_list.CollectionChanged += my_torrent_list_CollectionChanged;
            }
        }

        void my_torrent_list_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (my_torrent_list.Count == 0)
            {
                lm.RemoveLabel(this);
            }
            else
            {
                this.Count = my_torrent_list.Count;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RefreshCount()
        {
            if (this.Name == null)
            {
                try
                {
                    int c = 0;
                    foreach (var t in this.lm.AppState.Torrents)
                    {
                        if (!this.lm.TorrentHasAnyLabel(t))
                            c++;
                    }
                    this.Count = c;
                }
                catch { }
            }
            else
            {
                this.Count = my_torrent_list.Count;
            }
        }

        public bool HasTorrent(TorrentInfo ti)
        {
            if (this.Name == null)
            {
                return false;
            }
            else
            {
                return my_torrent_list.Contains(ti);
            }
        }

        public void RegisterTorrent(TorrentInfo ti)
        {
            if (this.Name != null)
            {
                if (!my_torrent_list.Contains(ti))
                {
                    my_torrent_list.Add(ti);
                }
            }
        }

        public void UnregisterTorrent(TorrentInfo ti)
        {
            if (this.Name != null)
            {
                my_torrent_list.Remove(ti);
            }
        }

        public void UnregisterAll()
        {
            if (this.Name != null)
            {
                my_torrent_list.CollectionChanged -= this.my_torrent_list_CollectionChanged;
                my_torrent_list.Clear();
            }
        }

    }
}
