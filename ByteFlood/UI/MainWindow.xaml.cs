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
using IO = System.IO;

namespace ByteFlood
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool gripped = false;
        bool ignoreclose = true;
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
        int ticks = 0;

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

        /// <summary>
        /// Imports torrents from BitTorrent/uTorrent.
        /// </summary>
        /// <returns>true if successful.</returns>
        public bool ImportTorrents()
        {
            if (ByteFlood.ImportTorrents.ResumeExist())
            {
                ImportTorrents it = new ImportTorrents() { Icon = this.Icon };
                it.ShowDialog();
                foreach (TorrentInfo ti in it.selected)
                {
                    state.Torrents.Add(ti);
                    ti.Start();
                }
                return true;
            }
            return false;
        }

        public void Update()
        {
            string[] torrentstates = new string[] 
            { 
                "Downloading",
                "Seeding",
                "Inactive",
                "Active",
               "Finished"
            };
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
                        List<MultiBindingExpression> multi_update = new List<MultiBindingExpression>();
                        List<BindingExpression> single_update = new List<BindingExpression>();
                        multi_update.Add(BindingOperations.GetMultiBindingExpression(this, MainWindow.TitleProperty));
                        multi_update.Add(BindingOperations.GetMultiBindingExpression(DownloadStatus, TextBlock.TextProperty));
                        multi_update.Add(BindingOperations.GetMultiBindingExpression(UploadStatus, TextBlock.TextProperty));
                        single_update.Add(BindingOperations.GetBindingExpression(DHTStatus, TextBlock.TextProperty));
                        multi_update.ForEach(t => t.UpdateTarget());
                        single_update.ForEach(t => t.UpdateTarget());
                    }, null);

                    foreach (string str in torrentstates)
                    {
                        state.NotifyChanged(str + "Torrents", str + "TorrentCount");
                    }
                    state.NotifyChanged("TorrentCount");

                    if (ticks >= 120) //1 min
                    {
                        // find DHT peers
                        if (state.DHTPeers < 500)
                        {
                            foreach (TorrentInfo ti in state.Torrents)
                                if (state.DHTPeers < 500 // we don't know if there are a lot of torrents, so let's check every time
                                    && ti.Torrent.State == TorrentState.Downloading || ti.Torrent.State == TorrentState.Seeding)
                                    state.ce.DhtEngine.GetPeers(ti.Torrent.InfoHash);
                        }
                        state.SaveState();
                        ticks = 0;
                    }
                    else
                    {
                        ticks++;
                    }
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
            ExecuteWindowBehavior(App.Settings.ExitBehavior);
            if (ignoreclose)
                e.Cancel = true;
            else
            {
                NotifyIcon.Visibility = System.Windows.Visibility.Hidden;
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
                    if (Utility.IsMagnetLink(text))
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
            TorrentPropertiesForm tp = new TorrentPropertiesForm(t) { Owner = this, Icon = this.Icon };
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
            ofd.InitialDirectory = App.Settings.OpenTorrentDialogLastPath;
            ofd.CheckFileExists = true;
            ofd.Multiselect = true;
            if (ofd.ShowDialog() == true)
            {
                App.Settings.OpenTorrentDialogLastPath = System.IO.Path.GetDirectoryName(ofd.FileName);
                foreach (string str in ofd.FileNames)
                {
                    state.AddTorrentByPath(str);
                }
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
                    Filters = query.Filters,
                    DownloadDirectory = string.IsNullOrWhiteSpace(query.DownloadPath) ? App.Settings.DefaultDownloadPath : query.DownloadPath,
                    IsCustomtUpdateInterval = query.UpdateIntervalType == 1,
                    CustomUpdateInterval = new TimeSpan(0,0, query.CustomIntervalSeconds),
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

        private void Commands_SearchOnlineTorrents(object sender, ExecutedRoutedEventArgs e)
        {
            UI.SearchOnlineTorrents a = new UI.SearchOnlineTorrents() { Owner = this, Icon = this.Icon };
            a.Show();
        }

        #endregion

        #region Torrent Commands

        private void TorrentCommands_ChangeFilePriority(object sender, ExecutedRoutedEventArgs e)
        {
            if (files_tree.SelectedItems.Count > 0)
            {
                Priority p = (Priority)Enum.Parse(typeof(Priority), e.Parameter.ToString());

                foreach (Aga.Controls.Tree.TreeNode item in files_tree.SelectedItems)
                {
                    if (item.Tag is FileInfo)
                    {
                        FileInfo fi = item.Tag as FileInfo;
                        fi.ChangePriority(p);
                    }
                    else if (item.Tag is DirectoryKey)
                    {
                        DirectoryKey dk = item.Tag as DirectoryKey;
                        ApplyPriority_DirectoryTree(dk, p);
                    }
                }
            }
        }

        private void ApplyPriority_DirectoryTree(DirectoryKey dk, Priority p)
        {
            foreach (object ob in dk.Values)
            {
                if (ob is FileInfo)
                {
                    (ob as FileInfo).ChangePriority(p);
                }
                else if (ob is DirectoryKey)
                {
                    this.ApplyPriority_DirectoryTree(ob as DirectoryKey, p);
                }
            }
        }

        public void TorrentCommands_OpenFile(object sender, ExecutedRoutedEventArgs e)
        {
            Aga.Controls.Tree.TreeNode item = files_tree.SelectedItem as Aga.Controls.Tree.TreeNode;

            if (item != null)
            {
                FileInfo fi = item.Tag as FileInfo;
                if (fi != null)
                {
                    System.IO.FileInfo fifo = new System.IO.FileInfo(fi.File.FullPath);

                    if (fifo.Exists)
                    {
                        string[] dangerous_file_types = { ".exe", ".scr", ".pif", ".com", ".bat", ".cmd", ".vbs", ".hta" };

                        if (dangerous_file_types.Contains(fifo.Extension.ToLower()))
                        {
                            if (MessageBox.Show(string.Format(@"Opening files downloaded from the Internet may result in harm to your computer or your data."
                                + " Are you sure that you want to open {0}?", fi.File.Path),
                                "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                            {
                                Process.Start(fifo.FullName);
                            }
                        }
                        else
                        {
                            Process.Start(fifo.FullName);
                        }
                    }
                    else
                    {
                        MessageBox.Show("File doesn't exist", "Error");
                    }

                }
            }
        }

        public void TorrentCommands_OpenFileLocation(object sender, RoutedEventArgs e)
        {
            // TODO: Add support to open DirectoryKeys
            Aga.Controls.Tree.TreeNode item = files_tree.SelectedItem as Aga.Controls.Tree.TreeNode;

            if (item != null)
            {
                FileInfo fi = item.Tag as FileInfo;
                if (fi != null)
                {
                    System.IO.FileInfo fifo = new System.IO.FileInfo(fi.File.FullPath);

                    if (fifo.Directory.Exists)
                    {
                        Process.Start("explorer.exe", "\"" + fifo.Directory.FullName + "\"");
                    }
                    else
                    {
                        Process.Start("explorer.exe", "\"" + fi.Owner.SavePath + "\"");
                    }

                }
            }
        }

        #endregion

        private void graphtab_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ReDrawGraph();
        }

        private void SetDataContext(TorrentInfo ti)
        {
            peers_list.ItemsSource = ti.Peers;
            files_tree.Model = ti.FilesTree;
            pieces_list.ItemsSource = ti.Pieces;
            trackers_list.ItemsSource = ti.Trackers;
            overview_canvas.DataContext = ti;
        }

        private void ResetDataContext()
        {
            peers_list.ItemsSource = null;
            files_tree.Model = null;
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
            if (!App.Settings.ImportedTorrents)
                ImportTorrents();
            Utility.ReloadTheme(App.Settings.Theme);
            DownloadStatus.DataContext = state.ce;
            UploadStatus.DataContext = state.ce;
            DHTStatus.DataContext = state;

            Services.AutoUpdater.NewUpdate += AutoUpdater_NewUpdate;
            Services.AutoUpdater.StartMonitoring();
        }

        bool notify_later_clicked = false;
        void AutoUpdater_NewUpdate(Services.NewUpdateInfo info)
        {
            if (notify_later_clicked) { return; }
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                UI.NewUpdateWindow nuw = new UI.NewUpdateWindow()
                {
                    DataContext = info,
                    Icon = this.Icon,
                    Owner = this
                };

                bool? res = nuw.ShowDialog();

                if (res == true)
                {
                    string byteflood_location = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    System.IO.FileInfo fi = new IO.FileInfo(byteflood_location);
                    string updater = System.IO.Path.Combine(fi.DirectoryName, "ByteFloodUpdater.exe");
                    if (System.IO.File.Exists(updater))
                    {
                        ProcessStartInfo psi = new ProcessStartInfo(updater);
                        psi.Arguments = string.Format("\"{0}\" \"{1}\"", info.DownloadUrl, fi.DirectoryName);

                        Process.Start(psi);

                        state.Shutdown();
                        Environment.Exit(0);
                    }
                    else
                    {
                        MessageBox.Show("Updater executable does not exist!", "Cannot update");
                    }
                }

                notify_later_clicked = res == false;
            }));
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

        private void CopyMagnetLink(object sender, RoutedEventArgs e)
        {
            TorrentInfo ti = mainlist.SelectedItem as TorrentInfo;
            Clipboard.SetText(ti.GetMagnetLink());
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
                    if (App.Settings.NotifyOnTray)
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
                    query.LoadFilters(entry.Filters.ToArray());
                    query.AutoDownload = entry.AutoDownload;
                    query.DownloadPath = entry.DownloadDirectory;
                    query.UpdateIntervalType = entry.IsCustomtUpdateInterval ? 1 : 0;
                    query.ManualUpdateIntervalSeconds = entry.CustomUpdateInterval.TotalSeconds.ToString();

                    if (query.ShowDialog() == true)
                    {
                        entry.Alias = query.CustomAlias;
                        entry.AutoDownload = query.AutoDownload == true;
                        entry.Filters = query.Filters;
                        entry.DownloadDirectory = string.IsNullOrWhiteSpace(query.DownloadPath) ? App.Settings.DefaultDownloadPath : query.DownloadPath;
                        entry.CustomUpdateInterval = new TimeSpan(0, 0, query.CustomIntervalSeconds);
                        entry.IsCustomtUpdateInterval = query.UpdateIntervalType == 1;
                        entry.NotifyUpdate();
                        FeedsManager.Save();
                    }
                    break;
                case "View":
                    UI.FeedViewer fv = new UI.FeedViewer() { Owner = this, Icon = this.Icon, DataContext = entry };
                    fv.Show();
                    break;
            }
        }



    }
}
