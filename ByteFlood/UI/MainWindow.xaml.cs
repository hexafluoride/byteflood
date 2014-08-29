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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using MonoTorrent.Client;
using MonoTorrent.Common;
using Microsoft.Win32;
using System.Threading;
using System.Diagnostics;
using ByteFlood.Services.RSS;
using System.Threading.Tasks;

namespace ByteFlood
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        RssUrlEntry SelectedRssEntry = null;
        bool gripped = false;
        bool ignoreclose = true;
        //bool closing = false;
        Thread thr;
        //bool bound = false;
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
        //public Formatters.SpeedFormatter speedformatter = new Formatters.SpeedFormatter();
        public MainWindow()
        {
            InitializeComponent();
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

                        MultiBindingExpression exp = BindingOperations.GetMultiBindingExpression(this, MainWindow.TitleProperty);
                        exp.UpdateTarget();
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
            ExecuteWindowBehavior(WindowBehavior.Exit);
            if (ignoreclose)
                e.Cancel = true;
            else
            {
                state.Shutdown();
                Environment.Exit(0);
            }
        }

        private void mainlist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;
            SetDataContext((TorrentInfo)e.AddedItems[0]);
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            var data = ((DataObject)e.Data);

            if (data.ContainsText())
            {
                string text = (string)data.GetData(typeof(string));
                if (!string.IsNullOrWhiteSpace(text))
                {
                    if (text.StartsWith("magnet:?"))
                    {
                        state.AddTorrentByMagnet(text);
                    }
                }
            }
            else
            {
                string toppest = (string)data.GetFileDropList()[0];
                state.AddTorrentByPath(toppest);
            }
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
            TorrentPropertiesForm tp = new TorrentPropertiesForm(t.Torrent) { Owner = this, Icon = this.Icon };
            tp.ShowDialog();
        }

        public void ActionOnAllTorrents(object sender, RoutedEventArgs e)
        {
            string tag = ((MenuItem)e.Source).Tag.ToString();
            switch (tag)
            {
                case "pause":
                    foreach (TorrentInfo ti in state.Torrents)
                        ti.Pause();
                    break;
                case "resume":
                    foreach (TorrentInfo ti in state.Torrents)
                        ti.Start();
                    break;
            }

        }

        public void RemoveSelectedTorrents(object sender, RoutedEventArgs e)
        {
            if (mainlist.SelectedIndex == -1)
                return;
            string tag = ((FrameworkElement)e.Source).Tag.ToString();
            TorrentInfo[] arr = new TorrentInfo[mainlist.SelectedItems.Count];
            mainlist.SelectedItems.CopyTo(arr, 0);
            foreach (TorrentInfo ti in arr)
                RemoveTorrent(ti, tag);
        }

        public void RemoveTorrent(TorrentInfo t, string action)
        {
            t.Invisible = true;
            t.UpdateList("Invisible", "ShowOnList");
            ThreadPool.QueueUserWorkItem(delegate
            {
                t.Torrent.Stop();
                while (t.Torrent.State != TorrentState.Stopped) ;
                state.ce.Unregister(t.Torrent);
                uiContext.Send(x =>
                {
                    state.Torrents.Remove(t);
                }, null);
                switch (action)
                {
                    case "torrentonly":
                        DeleteTorrent(t);
                        break;
                    case "dataonly":
                        DeleteData(t);
                        break;
                    case "both":
                        DeleteData(t);
                        DeleteTorrent(t);
                        break;
                    default:
                        break;
                }
            });
        }

        public void DeleteTorrent(TorrentInfo t)
        {
            System.IO.File.Delete(t.Torrent.Torrent.TorrentPath);
        }

        public void DeleteData(TorrentInfo t)
        {
            List<string> directories = new List<string>();
            foreach (TorrentFile file in t.Torrent.Torrent.Files)
            {
                if (System.IO.File.Exists(file.FullPath))
                {
                    directories.Add(new System.IO.FileInfo(file.FullPath).Directory.FullName);
                    System.IO.File.Delete(file.FullPath);
                }
            }
            directories = directories.Distinct().ToList();
            foreach (string str in directories)
            {
                System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(str);
                if (dir.GetFiles().Length == 0)
                    dir.Delete();
            }
        }
        public void ChangePriority(object sender, RoutedEventArgs e)
        {
            string tag = ((MenuItem)e.Source).Tag.ToString();
            Priority p = (Priority)Enum.Parse(typeof(Priority), tag);
            TorrentInfo t;
            if (!GetSelectedTorrent(out t))
                return;
            foreach (FileInfo fi in files_list.SelectedItems)
                t.Torrent.Torrent.Files.FirstOrDefault(ti => ti.FullPath == fi.Name).Priority = p;
            //TorrentInfo t;
            //if (!GetSelectedTorrent(out t))
            //    return;
            //t.Torrent.Torrent.Files[files_list.SelectedIndex].Priority = (Priority)Enum.Parse(typeof(Priority), tag);
        }
        public void OpenSelectedFile(object sender, RoutedEventArgs e)
        {
            FileInfo fi;
            if (files_list.SelectedItem == null)
                return;
            fi = (FileInfo)files_list.SelectedItem;
            string path = fi.Name;
            if (MessageBox.Show(string.Format(@"Opening files downloaded from the Internet may result in harm to your computer or your data. Are you sure that you want to open {0}?", path), "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                Process.Start(path);
        }
        public void OpenSelectedFileLocation(object sender, RoutedEventArgs e)
        {
            FileInfo fi;
            if (files_list.SelectedItem == null)
                return;
            fi = (FileInfo)files_list.SelectedItem;
            string filepath = fi.Name;
            string dir = new System.IO.FileInfo(filepath).Directory.FullName;
            Process.Start("explorer.exe", "\"" + dir + "\"");
        }
        public void OpenSelectedTorrentLocation(object sender, RoutedEventArgs e)
        {
            TorrentInfo t;
            if (!GetSelectedTorrent(out t))
                return;
            Process.Start("explorer.exe", "\"" + t.SavePath + "\"");
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

        #region Commands

        private void Commands_AddTorrent(object sender, ExecutedRoutedEventArgs e)
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

        private void Commands_AddMagnet(object sender, ExecutedRoutedEventArgs e)
        {
            var dialog = new UI.AddMagnetTextInputDialog() { Owner = this, Icon = this.Icon };

            if (dialog.ShowDialog().Value == true)
            {
                this.state.AddTorrentByMagnet(dialog.Input);
            }
        }

        private void Commands_AddRssFeed(object sender, ExecutedRoutedEventArgs e)
        {
            var query = new UI.AddRSSFeed() { Icon = this.Icon, Owner = this };
            if (query.ShowDialog() == true)
            {
                if (string.IsNullOrWhiteSpace(query.Url))
                {
                    MessageBox.Show(this, "Url cannot be empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var rss_entry = new RssUrlEntry()
                {
                    Url = query.Url,
                    Alias = query.CustomAlias,
                    AutoDownload = query.AutoDownload == true,
                    FilterExpression = query.FilterExpression,
                    FilterAction = query.FilterAction == 0 ? RssUrlEntry.FilterActionEnum.Download : RssUrlEntry.FilterActionEnum.Skip,
                    DefaultSettings = new TorrentSettings()
                };

                Task.Factory.StartNew(new Action(() =>
                {
                    if (rss_entry.Test())
                    {
                        App.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            FeedsManager.Add(rss_entry);
                        }));
                    }
                    else
                    {
                        App.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            MessageBox.Show(this,
                             "This RSS entry seem to be invalid. \n\n If your internet connection is down, try adding it when it's up again.",
                             "Error",
                             MessageBoxButton.OK, MessageBoxImage.Error);
                        }));
                    }

                }));
            }
            //Services.RSS.FeedsManager.Add(new Services.RSS.RssUrlEntry() 
            //{
            //    Url = "http://www.nyaa.se/?page=rss"
            //});
        }

        private void Commands_OpenPreferences(object sender, ExecutedRoutedEventArgs e)
        {
            Preferences pref = new Preferences() { Owner = this, Icon = this.Icon };
            pref.ShowDialog();
            state.SaveSettings();
            UpdateVisibility();
        }

        #endregion

        private void graphtab_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ReDrawGraph();
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            NotifyIcon.Icon = new System.Drawing.Icon("Assets/icon-16.ico");
            this.Icon = new BitmapImage(new Uri("Assets/icon-allsizes.ico", UriKind.Relative));
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
                if (Utility.IsMagnetLink(str))
                    state.AddTorrentByMagnet(str);
                else
                    state.AddTorrentByPath(str);
            }

            this.DataContext = state.ce;
            left_treeview.DataContext = App.Settings;
            info_canvas.DataContext = App.Settings;
            feeds_tree_item.ItemsSource = FeedsManager.EntriesList;
        }
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
        public void UpdateVisibility()
        {
            //App.Settings.NotifyChanged("TreeViewVisibility", "BottomCanvasVisibility");
            left_treeview.DataContext = App.Settings;
            info_canvas.DataContext = App.Settings;
            BindingExpression exp1 = left_treeview.GetBindingExpression(TreeView.VisibilityProperty);
            BindingExpression exp2 = info_canvas.GetBindingExpression(Canvas.VisibilityProperty);
            exp1.UpdateTarget();
            exp1.UpdateSource();
            exp2.UpdateTarget();
            exp2.UpdateSource();
            foreach (Image img in FindVisualChildren<Image>(this))
            {
                try
                {
                    img.GetBindingExpression(Image.VisibilityProperty).UpdateTarget();
                }
                catch
                {
                }
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
                if (p.Y > ActualSize.ActualHeight - 120 || p.Y < 90)
                    return;
                Point listpos = mainlist.TransformToAncestor(this).Transform(new Point(0, 0));
                double totalsize = mainlist.ActualHeight + info_canvas.ActualHeight;
                double mouse_relative_to_list = p.Y - listpos.Y;
                double listsize = mouse_relative_to_list;
                double canvassize = totalsize - mouse_relative_to_list;
                mainlist.Height = listsize;
                info_canvas.Height = canvassize;
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Point listpos = mainlist.TransformToAncestor(this).Transform(new Point(0, 0));
            double newheight = ActualSize.ActualHeight - info_canvas.ActualHeight - listpos.Y;
            if (newheight > 20)
                mainlist.Height = newheight;
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

        private void ShowHide(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == System.Windows.WindowState.Minimized)
                this.WindowState = System.Windows.WindowState.Normal;
            else
                this.WindowState = System.Windows.WindowState.Minimized;
        }

        public void Exit(object sender, RoutedEventArgs e)
        {
            ignoreclose = false;
            this.Close();
        }

        private void ExecuteTrayBehavior(TrayIconBehavior behavior)
        {
            switch (behavior)
            {
                case TrayIconBehavior.ContextMenu:
                    NotifyIcon.ShowContextMenu(Hardcodet.Wpf.TaskbarNotification.Util.GetMousePosition(NotifyIcon));
                    break;
                case TrayIconBehavior.ShowHide:
                    ShowHide(null, null);
                    break;
            }
        }

        private void ExecuteWindowBehavior(WindowBehavior behavior)
        {
            switch (behavior)
            {
                case WindowBehavior.MinimizeToTaskbar:
                    this.WindowState = System.Windows.WindowState.Minimized;
                    this.ShowInTaskbar = true;
                    break;
                case WindowBehavior.MinimizeToTray:
                    this.WindowState = System.Windows.WindowState.Minimized;
                    this.ShowInTaskbar = false;
                    NotifyIcon.ShowBalloonTip("ByteFlood", "ByteFlood has been minimized to the traybar.", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
                    break;
                case WindowBehavior.Exit:
                    ignoreclose = false;
                    break;
            }
        }

        private void NotifyIcon_TrayLeftMouseUp(object sender, RoutedEventArgs e)
        {
            ExecuteTrayBehavior(App.Settings.TrayIconClickBehavior);
        }

        private void NotifyIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            ExecuteTrayBehavior(App.Settings.TrayIconDoubleClickBehavior);
        }

        private void NotifyIcon_TrayRightMouseUp(object sender, RoutedEventArgs e)
        {
            ExecuteTrayBehavior(App.Settings.TrayIconRightClickBehavior);
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == System.Windows.WindowState.Minimized)
                ExecuteWindowBehavior(App.Settings.MinimizeBehavior);
        }

        private void OperationOnSelectedTorrents(object sender, RoutedEventArgs e)
        {
            TorrentInfo[] arr = new TorrentInfo[mainlist.SelectedItems.Count];
            mainlist.SelectedItems.CopyTo(arr, 0);
            if (arr.Length == 0)
                return;
            string tag = ((FrameworkElement)e.Source).Tag.ToString();
            Action<TorrentInfo> f = new Action<TorrentInfo>(t => { });
            switch (tag)
            {
                case "Start":
                    f = new Action<TorrentInfo>(t => t.Start());
                    break;
                case "Pause":
                    f = new Action<TorrentInfo>(t => t.Pause());
                    break;
                case "Stop":
                    f = new Action<TorrentInfo>(t => t.Stop());
                    break;
            }
            foreach (TorrentInfo ti in arr)
            {
                f(ti);
            }
        }

        private void OperationOnRssItem(object sender, RoutedEventArgs e)
        {
            RssUrlEntry entry = null;

            MenuItem source = (MenuItem)e.Source;

            entry = (RssUrlEntry)source.DataContext;

            switch (source.Tag.ToString())
            {
                case "Refresh":
                    FeedsManager.ForceUpdate(entry);
                    break;
                case "Remove":
                    FeedsManager.Remove(entry);
                    break;
                case "Edit":
                    var query = new UI.AddRSSFeed() { Owner = this, Icon = this.Icon, Title = "Edit rss feed" };
                    query.AllowUrlChange = false;
                    query.Url = entry.Url;
                    query.CustomAlias = entry.Alias;
                    query.FilterExpression = entry.FilterExpression;
                    query.FilterAction = entry.FilterAction == RssUrlEntry.FilterActionEnum.Download ? 0 : 1;
                    query.AutoDownload = entry.AutoDownload;
                    
                    if (query.ShowDialog() == true)
                    {
                        entry.FilterAction = query.FilterAction == 0 ? RssUrlEntry.FilterActionEnum.Download : RssUrlEntry.FilterActionEnum.Skip;
                        entry.Alias = query.CustomAlias;
                        entry.FilterExpression = query.FilterExpression;
                        entry.AutoDownload = query.AutoDownload == true;
                        entry.NotifyUpdate();
                        FeedsManager.Save();
                    }
                    break;
            }
        }

    }
}
