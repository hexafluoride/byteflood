/* 
    ByteFlood - A BitTorrent client.
    Copyright (C) 2014 Burak Öztunç

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
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

namespace ftorrent
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ClientEngine ce;
        Thread thr;
        bool updategraph = false;
        SynchronizationContext uiContext = SynchronizationContext.Current;
        GraphDrawer graph;
        public MainWindow()
        {
            InitializeComponent();
            mainlist.ItemsSource = list_data;
            ce = new ClientEngine(new EngineSettings());
            thr = new Thread(new ThreadStart(Update));
            thr.Start();
            DhtListener dhtl = new DhtListener(new IPEndPoint(IPAddress.Any, 22239)); // TODO: Implement global settings, so this can(along with other options) can be changed
            DhtEngine dht = new DhtEngine(dhtl);
            
            ce.RegisterDht(dht);
            ce.DhtEngine.Start();

            graph = new GraphDrawer(graph_canvas);
        }
        public ObservableCollection<TorrentInfo> list_data = new ObservableCollection<TorrentInfo>();
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
                AddTorrentByPath(str);
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
            graph.DrawGrid(size.Left, size.Top, size.Right, size.Bottom);
        }

        public void AddTorrentByPath(string path)
        {
            AddTorrentDialog atd = new AddTorrentDialog(path);
            atd.ShowDialog();
            if (atd.userselected)
            {
                TorrentInfo ti = CreateTorrentInfo(atd.tm);
                ti.Name = atd.torrentname;
                if (!atd.start)
                    ti.Stop();
                ti.RatioLimit = atd.limit;
                ti.Torrent.Settings.InitialSeedingEnabled = atd.initial.IsChecked == true;
                list_data.Add(ti);
            }
        }

        public TorrentInfo CreateTorrentInfo(TorrentManager tm)
        {
            ce.Register(tm);
            tm.Start();
            TorrentInfo t = new TorrentInfo(uiContext);
            t.Torrent = tm;
            t.Update();
            return t;
        }
        public void Update()
        {
            while (true)
            {
                try
                {
                    foreach (TorrentInfo ti in list_data)
                        ti.Update();
                    uiContext.Send(x =>
                    {
                        if (updategraph)
                        {
                            if (updategraph)
                                ReDrawGraph();
                            foreach (TorrentInfo ti in list_data)
                                if(ti.Torrent.State != TorrentState.Paused)
                                    ti.UpdateGraphData();
                        }
                        updategraph = !updategraph;
                    }, null);
                    GC.Collect();
                }
                catch (Exception ex)
                {
                }
                System.Threading.Thread.Sleep(500);
            }
        }
        #region Event Handlers
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            thr.Abort();
            ce.DiskManager.Flush();
            ce.PauseAll();
        }

        private void mainlist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;
            peers_list.ItemsSource = ((TorrentInfo)e.AddedItems[0]).Peers;
            files_list.ItemsSource = ((TorrentInfo)e.AddedItems[0]).Files;
            pieces_list.ItemsSource = ((TorrentInfo)e.AddedItems[0]).Pieces;
            trackers_list.ItemsSource = ((TorrentInfo)e.AddedItems[0]).Trackers;
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            string toppest = (string)((DataObject)e.Data).GetFileDropList()[0];
            AddTorrentByPath(toppest);
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
            TorrentProperties tp = new TorrentProperties(t.Torrent);
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
            ce.Unregister(t.Torrent);
            list_data.Remove(t);
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
            ti = list_data[mainlist.SelectedIndex];
            return true;
        }
        #endregion

        private void graphtab_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ReDrawGraph();
        }
    }
}
