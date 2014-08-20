using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using MonoTorrent;
using MonoTorrent.Client;
using MonoTorrent.Dht;
using MonoTorrent.Dht.Listeners;
using MonoTorrent.Common;
using Microsoft.Win32;
using System.Threading;
using System.Net;

namespace ByteFlood
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool gripped = false;
        Thread thr;
        bool updategraph = false;
        public SynchronizationContext uiContext = SynchronizationContext.Current;
        public Func<TorrentInfo, bool> itemselector;
        public Func<TorrentInfo, bool> ShowAll = new Func<TorrentInfo, bool>((t) => { return true; });
        public Func<TorrentInfo, bool> Downloading = new Func<TorrentInfo, bool>((t) => { return t.Torrent == null ? false : t.Torrent.State == TorrentState.Downloading; });
        public Func<TorrentInfo, bool> Seeding = new Func<TorrentInfo, bool>((t) => { return t.Torrent == null ? false : t.Torrent.State == TorrentState.Seeding; });
        public Func<TorrentInfo, bool> Active = new Func<TorrentInfo, bool>((t) => { return t.Torrent == null ? false : (t.Torrent.State == TorrentState.Seeding || t.Torrent.State == TorrentState.Downloading) || t.Torrent.State == TorrentState.Hashing; });
        public Func<TorrentInfo, bool> Inactive = new Func<TorrentInfo, bool>((t) => { return t.Torrent == null ? false : (t.Torrent.State != TorrentState.Seeding && t.Torrent.State != TorrentState.Downloading) && t.Torrent.State != TorrentState.Hashing; });
        public Func<TorrentInfo, bool> Finished = new Func<TorrentInfo, bool>((t) => { return t.Torrent == null ? false : t.Torrent.Progress == 100; });
        GraphDrawer graph;
        public State state;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Open a torrent...";
            ofd.Filter = "Torrent files|*.torrent";
            ofd.DefaultExt = "*.torrent";
            ofd.InitialDirectory = Environment.CurrentDirectory;
            ofd.CheckFileExists = true;
            ofd.Multiselect = true;
            ofd.ShowDialog();
            foreach (string str in ofd.FileNames)
            {
                state.AddTorrentByPath(str);
            }
        }
        public void ReDrawGraph()
        {
            if (mainlist.SelectedIndex == -1)
                return;
            TorrentInfo ti = ((TorrentInfo)mainlist.Items[mainlist.SelectedIndex]);
            graph.Clear();
            bool drawdown = false;
            bool drawup = false;
            int index = graph_selector.SelectedIndex;
            switch (index)
            {
                case 0:
                    drawdown = drawup = true;
                    break;
                case 1:
                    drawdown = true;
                    drawup = false;
                    break;
                case 2:
                    drawdown = false;
                    drawup = true;
                    break;
                default:
                    drawdown = drawup = true;
                    break;
            }
            graph.Draw(ti.DownSpeeds.ToArray(), ti.UpSpeeds.ToArray(), drawdown, drawup);
            Thickness size = Utility.SizeToMargin(graph.GetSize());
            if (App.Settings.DrawGrid)
                graph.DrawGrid(size.Left, size.Top, size.Right, size.Bottom);
        }

        

        
        public void Update()
        {
            while (true)
            {
                try
                {
                    foreach (TorrentInfo ti in state.Torrents)
                        ti.Update();

                    uiContext.Send(x =>
                    {
                        if (mainlist.SelectedIndex == -1)
                            ResetDataContext();
                        if (updategraph)
                        {
                            if (updategraph)
                                ReDrawGraph();
                            foreach (TorrentInfo ti in state.Torrents)
                                if (ti.Torrent.State != TorrentState.Paused)
                                    ti.UpdateGraphData();
                        }
                        updategraph = !updategraph;
                    }, null);

                    string[] torrentstates = new string[] { 
                        "Downloading",
                        "Seeding",
                        "Inactive",
                        "Active",
                        "Finished"
                    };
                    foreach (string str in torrentstates)
                    {
                        state.NotifyChanged(str + "Torrents", str + "TorrentCount");
                    }
                    state.NotifyChanged("TorrentCount");
                }
                catch
                {
                }
                System.Threading.Thread.Sleep(500);
            }
        }
        #region Event Handlers
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            state.Shutdown();
        }

        private void mainlist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;
            SetDataContext((TorrentInfo)e.AddedItems[0]);
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            string toppest = (string)((DataObject)e.Data).GetFileDropList()[0];
            state.AddTorrentByPath(toppest);
        }
        #endregion

        #region ContextMenu Handlers
        // I'd like to have a better way to handle these
        public void StopSelectedTorrent(object sender, RoutedEventArgs e)
        {
            TorrentInfo t;
            if (!GetSelectedTorrent(out t))
                return;
            t.Stop();
        }
        public void StartSelectedTorrent(object sender, RoutedEventArgs e)
        {
            TorrentInfo t;
            if (!GetSelectedTorrent(out t))
                return;
            t.Start();
        }
        public void PauseSelectedTorrent(object sender, RoutedEventArgs e)
        {
            TorrentInfo t;
            if (!GetSelectedTorrent(out t))
                return;
            t.Pause();
        }
        public void OpenTorrentProperties(object sender, RoutedEventArgs e)
        {
            TorrentInfo t;
            if (!GetSelectedTorrent(out t))
                return;
            TorrentPropertiesForm tp = new TorrentPropertiesForm(t.Torrent);
            tp.Show();
        }

        public void HighPriority(object sender, RoutedEventArgs e)
        {
            TorrentInfo t;
            if (!GetSelectedTorrent(out t))
                return;
            t.Torrent.Torrent.Files[files_list.SelectedIndex].Priority = Priority.High;
        }
        public void NormalPriority(object sender, RoutedEventArgs e)
        {
            TorrentInfo t;
            if (!GetSelectedTorrent(out t))
                return;
            t.Torrent.Torrent.Files[files_list.SelectedIndex].Priority = Priority.Normal;
        }
        public void RemoveSelectedTorrent(object sender, RoutedEventArgs e)
        {
            TorrentInfo t;
            if (!GetSelectedTorrent(out t))
                return;
            t.Torrent.Stop();
            while (t.Torrent.State != TorrentState.Stopped) ;
            state.ce.Unregister(t.Torrent);
            state.Torrents.Remove(t);
        }
        public void LowPriority(object sender, RoutedEventArgs e)
        {
            TorrentInfo t;
            if (!GetSelectedTorrent(out t))
                return;
            t.Torrent.Torrent.Files[files_list.SelectedIndex].Priority = Priority.Low;
        }
        public void NoPriority(object sender, RoutedEventArgs e)
        {
            TorrentInfo t;
            if (!GetSelectedTorrent(out t))
                return;
            t.Torrent.Torrent.Files[files_list.SelectedIndex].Priority = Priority.DoNotDownload;
        }
        public bool GetSelectedTorrent(out TorrentInfo ti)
        {
            ti = null;
            if (mainlist.SelectedIndex == -1)
                return false;
            ti = state.Torrents[mainlist.SelectedIndex];
            return true;
        }
        #endregion

        private void graphtab_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ReDrawGraph();
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OpenPreferences(object sender, RoutedEventArgs e)
        {

        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            Preferences pref = new Preferences();
            pref.Show();
            state.SaveSettings();
        }

        private void SetDataContext(TorrentInfo ti)
        {
            peers_list.ItemsSource = ti.Peers;
            files_list.ItemsSource = ti.Files;
            pieces_list.ItemsSource = ti.Pieces;
            trackers_list.ItemsSource = ti.Trackers;
            overview_canvas.DataContext = ti;
        }

        private void ResetDataContext()
        {
            peers_list.ItemsSource = null;
            files_list.ItemsSource = null;
            pieces_list.ItemsSource = null;
            trackers_list.ItemsSource = null;
            overview_canvas.DataContext = null;
        }

        private void mainlist_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            mainlist.UnselectAll();
            ResetDataContext();
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            thr = new Thread(new ThreadStart(Update));
            state = State.Load("./state.xml");
            state.uiContext = uiContext;
            state.mainthread = thr;
            thr.Start();
            mainlist.ItemsSource = state.Torrents;
            torrents_treeview.DataContext = state;
            itemselector = ShowAll;
            graph = new GraphDrawer(graph_canvas);
            
            foreach (string str in App.to_add)
            {
                state.AddTorrentByPath(str);
            }
        }

        private void ResizeInfoAreaStart(object sender, MouseButtonEventArgs e)
        {
            gripped = true;
        }
        private void ResizeInfoAreaEnd(object sender, MouseButtonEventArgs e)
        {
            gripped = false;
        }

        private void ResizeInfoAreaMove(object sender, MouseEventArgs e)
        {
            if (gripped)
            {
                Point p = e.MouseDevice.GetPosition(this);
                Point position = info_canvas.TransformToAncestor(this).Transform(new Point(0, 0));
                double ypos = position.Y;
                double left = info_canvas.Margin.Left;
                if (p.Y > this.ActualHeight - 120 || p.Y < 90)
                    return;
                info_canvas.Margin = new Thickness { Left = left, Top = p.Y };
                info_canvas.Height += ypos - info_canvas.Margin.Top;
                double mainlist_top = mainlist.Margin.Top;
                double mainlist_left = mainlist.Margin.Left;
                mainlist.Height = mainlist.ActualHeight - (ypos - info_canvas.Margin.Top);
                mainlist.Margin = new Thickness() { Left = mainlist_left, Top=mainlist_top };
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            mainlist.Height = this.Height - info_canvas.ActualHeight - mainlist.Margin.Top;
        }

        private void grip_MouseEnter(object sender, MouseEventArgs e)
        {
            //this.Cursor = Cursors.SizeNS;
        }

        private void grip_MouseLeave(object sender, MouseEventArgs e)
        {
            //if(!gripped)
            //    this.Cursor = Cursors.Arrow;
        }

        private void SwitchTorrentDisplay(object sender, RoutedEventArgs e)
        {
            string tag = (string)((TreeViewItem)e.Source).Tag;
            switch (tag)
            {
                case "downloading":
                    itemselector = Downloading;
                    break;
                case "seeding":
                    itemselector = Seeding;
                    break;
                case "active":
                    itemselector = Active;
                    break;
                case "inactive":
                    itemselector = Inactive;
                    break;
                case "finished":
                    itemselector = Finished;
                    break;
                case "showall":
                    itemselector = ShowAll;
                    break;
                default:
                    itemselector = ShowAll;
                    break;
            }
            foreach (TorrentInfo ti in state.Torrents)
            {
                ti.UpdateList("ShowOnList");
            }
        }

    }
}
