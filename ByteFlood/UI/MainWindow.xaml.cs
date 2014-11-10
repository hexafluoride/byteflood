/* 
    ByteFlood - A BitTorrent client.
    Copyright (C) 2014 ***REMOVED***

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
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using MonoTorrent.Client;
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
    public partial class MainWindow : Window, IDisposable
    {
        bool ignoreclose = true;
        public SynchronizationContext uiContext = SynchronizationContext.Current;
        public Func<TorrentInfo, bool> itemselector;
        public Func<TorrentInfo, bool> ShowAll = new Func<TorrentInfo, bool>((t) => { return true; });
        public Func<TorrentInfo, bool> Downloading = new Func<TorrentInfo, bool>((t) => { return t.Torrent.QueryStatus().State == Ragnar.TorrentState.Downloading; });
        public Func<TorrentInfo, bool> Seeding = new Func<TorrentInfo, bool>((t) => { return t.Torrent.QueryStatus().State == Ragnar.TorrentState.Seeding; });
        public Func<TorrentInfo, bool> Active = new Func<TorrentInfo, bool>((t) => { return !t.Torrent.IsPaused; });
        public Func<TorrentInfo, bool> Inactive = new Func<TorrentInfo, bool>((t) => { return t.Torrent.IsPaused || !string.IsNullOrEmpty(t.Torrent.QueryStatus().Error); });
        public Func<TorrentInfo, bool> Finished = new Func<TorrentInfo, bool>((t) => { return t.Torrent.IsFinished; });
        GraphDrawer graph;
        public State state;
        int ticks = 0;

        /// <summary>
        /// Color used to paint the toolbar and info tabs buttons
        /// </summary>
        public Color ElementsColor
        {
            get { return (Color)GetValue(ElementsColorProperty); }
            set { SetValue(ElementsColorProperty, value); }
        }

        public static readonly DependencyProperty ElementsColorProperty =
            DependencyProperty.Register("ElementsColor", typeof(Color), typeof(MainWindow), new PropertyMetadata(Color.FromArgb(255, 38, 139, 210)));

        public MainWindow()
        {
            InitializeComponent();
            UpdateGridLength();
            UpdateAppStyle();
            UpdateMiscUISettings();
        }

        bool disposed = false;
        public void Dispose()
        {
            if (!disposed)
            {
                this.NotifyIcon.Dispose();
                disposed = true;
            }
        }

        public void ReDrawGraph()
        {
            if (mainlist.SelectedIndex == -1)
            {
                graph.Clear();
                return;
            }
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
            ImportTorrents it = new ImportTorrents() { Icon = this.Icon, Owner = this };
            it.Load(true);
            if (it.list.Count > 0)
            {
                it.ShowDialog(); // calling show dialog will call window_loaded and therefore the real Load() statement

                foreach (TorrentInfo ti in it.selected)
                {
                    state.Torrents.Add(ti);
                    ti.Start();
                }
                return true;
            }
            return false;
        }

        public void Update(CancellationToken cancellationToken)
        {
            while (true)
            {
	            if (cancellationToken.IsCancellationRequested)
		            break;

                var status = this.state.LibtorrentSession.QueryStatus();

                try
                {
                    uiContext.Send(x =>
                    {
                        if (mainlist.SelectedIndex == -1)
                            ResetDataContext();
                        if (info_canvas.SelectedIndex == 1)
                        {
                            ReDrawGraph();
                        }

                        string dsp = Utility.PrettifySpeed(status.DownloadRate);
                        string usp = Utility.PrettifySpeed(status.UploadRate);

                        this.DownloadStatus.Text = string.Format("D: {0} T: {1}",
                            dsp, Utility.PrettifyAmount(status.TotalDownload));

                        this.UploadStatus.Text = string.Format("U: {0} T: {1}",
                           usp, Utility.PrettifyAmount(status.TotalUpload));

                        this.DHTStatus.Text = string.Format("DHT Peers: {0}", status.DhtNodes);

                        this.Title = string.Format("ByteFlood [D: {0}, U: {1}]", dsp, usp);

                        status.Dispose();

                        this.ElementsColor = Utility.WindowsAero.ColorizationColor.ToWPFColor();

                    }, null);

                    // TODO: Update theses values only when they are really changed.
                    state.NotifyChanged("DownloadingTorrentCount", "SeedingTorrentCount",
                        "InactiveTorrentCount", "ActiveTorrentCount", "FinishedTorrentCount");

                    if (ticks >= 60) //1 min
                    {
                        state.SaveState();
                        ticks = 0;
                    }
                    else
                    {
                        ticks++;
                    }
                }
                catch { }
                Thread.Sleep(1000);
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

        private void GenerateDownloadContextMenu(object sender, MouseButtonEventArgs e)
        {
            DownloadStatus.ContextMenu = Utility.GenerateContextMenu(true, state);
        }

        private void GenerateUploadContextMenu(object sender, MouseButtonEventArgs e)
        {
            UploadStatus.ContextMenu = Utility.GenerateContextMenu(false, state);
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

        private async void Commands_AddRssFeed(object sender, ExecutedRoutedEventArgs e)
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
                    CustomUpdateInterval = new TimeSpan(0, 0, query.CustomIntervalSeconds),
                    DefaultSettings = App.Settings.DefaultTorrentProperties
                };

	            if (await rss_entry.TestAsync())
		            FeedsManager.Add(rss_entry);
	            else
		            MessageBox.Show(
						this,
			            "This RSS entry seem to be invalid. \n\n If your internet connection is down, try adding it when it's up again.",
			            "Error",
			            MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                int priority = Convert.ToInt32(e.Parameter);

                foreach (Aga.Controls.Tree.TreeNode item in files_tree.SelectedItems)
                {
                    if (item.Tag is FileInfo)
                    {
                        FileInfo fi = item.Tag as FileInfo;
                        fi.ChangePriority(priority);
                    }
                    else if (item.Tag is DirectoryKey)
                    {
                        DirectoryKey dk = item.Tag as DirectoryKey;
                        ApplyPriority_DirectoryTree(dk, priority);
                    }
                }
            }
        }

        private void ApplyPriority_DirectoryTree(DirectoryKey dk, int p)
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
                    System.IO.FileInfo fifo = new System.IO.FileInfo(fi.File.Path);

                    if (fifo.Exists)
                    {
                        string[] dangerous_file_types = { ".exe", ".scr", ".pif", ".js", ".com", ".bat", ".cmd", ".vbs", ".hta" };

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
                else
                {
                    DirectoryKey dk = item.Tag as DirectoryKey;
                    if (dk != null)
                    {
                        string partial_path = this.GetDirectoryKeyRelativePath(item);
                        string full_path = System.IO.Path.Combine(dk.OwnerTorrent.SavePath, partial_path);
                        System.IO.Directory.CreateDirectory(full_path);
                        Process.Start("explorer.exe", string.Format("\"{0}\"", full_path));
                    }
                }

            }
        }

        private string GetDirectoryKeyRelativePath(Aga.Controls.Tree.TreeNode item_node)
        {
            List<string> dirs = new List<string>();
            _recursive_resolve(item_node, dirs);
            dirs.Reverse();
            string res = System.IO.Path.Combine(dirs.ToArray());
            return res;
        }

        private void _recursive_resolve(Aga.Controls.Tree.TreeNode node, List<string> dirs)
        {
            DirectoryKey dk = node.Tag as DirectoryKey;
            if (dk != null)
            {
                dirs.Add(dk.Name);
                _recursive_resolve(node.Parent, dirs);
            }
        }

        public void TorrentCommands_OpenFileLocation(object sender, RoutedEventArgs e)
        {
            Aga.Controls.Tree.TreeNode item = files_tree.SelectedItem as Aga.Controls.Tree.TreeNode;

            if (item != null)
            {
                FileInfo fi = item.Tag as FileInfo;
                if (fi != null)
                {
                    System.IO.FileInfo fifo = new System.IO.FileInfo(fi.File.Path);
                    System.IO.Directory.CreateDirectory(fifo.Directory.FullName);
                    Process.Start("explorer.exe", string.Format("\"{0}\"", fifo.Directory.FullName));
                    return;
                }

                //we must have hit a directory then
                //if it's a directory residing inside the torrent folder (root folder), we simply
                //open the torrent save dir
                DirectoryKey dk = item.Tag as DirectoryKey;
                if (dk != null)
                {
                    if (item.Parent != null && item.Parent.Tag != null)
                    {
                        //it's a subdirectory
                        string partial_path = GetDirectoryKeyRelativePath(item.Parent);
                        string full_path = System.IO.Path.Combine(dk.OwnerTorrent.SavePath, partial_path);
                        System.IO.Directory.CreateDirectory(full_path);
                        Process.Start("explorer.exe", string.Format("\"{0}\"", full_path));
                    }
                    else
                    {
                        //it's a root dir
                        Process.Start("explorer.exe", string.Format("\"{0}\"", dk.OwnerTorrent.SavePath));
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
            //peers_list.ItemsSource = ti.Peers;
            files_tree.Model = ti.FilesTree;
            //piece_bar.AttachTorrent(ti);
            trackers_list.ItemsSource = ti.Trackers;
            overview_canvas.DataContext = ti;
            //if (ti.Torrent.Torrent.GetRightHttpSeeds.Count > 0)
            //{
            //    webseeds_tab.Visibility = Visibility.Visible;
            //    webseeds_list.ItemsSource = ti.Torrent.Torrent.GetRightHttpSeeds;
            //}
        }

        private void ResetDataContext()
        {
            //peers_list.ItemsSource = null;
            files_tree.Model = null;
            //piece_bar.DetachTorrent();
            trackers_list.ItemsSource = null;
            overview_canvas.DataContext = null;
            webseeds_list.ItemsSource = null;
            webseeds_tab.Visibility = Visibility.Collapsed;
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

	        var cts = new CancellationTokenSource();
	        
            state = new State
            {
	            uiContext = uiContext,
				MainTaskCancellationTokenSource = cts
            };

	        Task.Run(() => Update(cts.Token), cts.Token);

            mainlist.ItemsSource = state.Torrents;
            mainlist.DataContext = App.Settings;
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

            left_treeview.DataContext = App.Settings;

            feeds_tree_item.ItemsSource = FeedsManager.EntriesList;

            if (!App.Settings.ImportedTorrents)
                ImportTorrents();
            Utility.ReloadTheme(App.Settings.Theme);

            DHTStatus.DataContext = state;

            Services.AutoUpdater.NewUpdate += AutoUpdater_NewUpdate;
            Services.AutoUpdater.StartMonitoring();
        }

        bool notify_later_clicked = false;
        void AutoUpdater_NewUpdate(Services.NewUpdateInfo info)
        {
            if (notify_later_clicked) { return; }
            Dispatcher.Invoke(() =>
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
            });
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
            UpdateGridLength();
            UpdateMiscUISettings();

            this.left_treeview.Visibility = App.Settings.TreeViewVisibility;
            this.info_canvas.Visibility = App.Settings.BottomCanvasVisibility;
            this.splitter.Visibility = this.info_canvas.Visibility;

            this.StatusBar.Visibility = App.Settings.StatusBarVisibility;

            foreach (Image img in FindVisualChildren<Image>(this))
            {
                try
                {
                    img.GetBindingExpression(Image.VisibilityProperty).UpdateTarget();
                }
                catch { }
            }
        }

        public void UpdateAppStyle(int _override = -1)
        {
            int style = App.Settings.ApplicationStyle;
            if (_override > -1)
            {
                style = _override;
            }

            if (style == 0)
            {
                left_treeview.Width = 170d;
                left_treeview.Margin = new Thickness(5);

                toolbar.Margin = new Thickness(5);

                mainlist.Margin = new Thickness(5);
                mainlist.ClearValue(ListView.BorderThicknessProperty);

                splitter.ClearValue(GridSplitter.BackgroundProperty);
                splitter.Height = 5d;
                splitter.Margin = new Thickness(5, 0, 0, 5);
                splitter.ClearValue(Panel.ZIndexProperty);

                info_canvas.Margin = new Thickness(0, 5, 5, 5);
            }
            else
            {
                left_treeview.Width = 180d;
                left_treeview.ClearValue(TreeView.MarginProperty);

                toolbar.ClearValue(TreeView.MarginProperty);

                mainlist.ClearValue(TreeView.MarginProperty);
                mainlist.BorderThickness = new Thickness(0, 1, 1, 0);

                splitter.Background = Brushes.Black;
                splitter.Height = 1d;
                splitter.ClearValue(TreeView.MarginProperty);
                splitter.SetValue(Panel.ZIndexProperty, 5);

                info_canvas.Margin = new Thickness(-1, -1, 0, 0);
            }
        }

        private void UpdateGridLength()
        {
            GridLength auto = new GridLength(1, GridUnitType.Star);
            GridLength zero = new GridLength(0);
            this.left_tree_colum.Width = App.Settings.TreeViewVisible ? new GridLength(180d) : zero;
            this.info_tabs_row.Height = App.Settings.BottomCanvasVisible ? auto : zero;
            this.statusbar_gridrow.Height = App.Settings.StatusBarVisible ? auto : zero;
            this.torrent_list_row.Height = App.Settings.BottomCanvasVisible ? new GridLength(250d) : auto;
        }

        private void UpdateMiscUISettings()
        {
            if (App.Settings.DisplayStripsOnTorrentList)
                mainlist.SetValue(ListView.AlternationCountProperty, 2);
            else
                mainlist.ClearValue(ListView.AlternationCountProperty);
        }

        private void CopyMagnetLink(object sender, RoutedEventArgs e)
        {
            TorrentInfo ti = mainlist.SelectedItem as TorrentInfo;
            Clipboard.SetText(ti.GetMagnetLink());
        }

        private void Torrent_RetrieveMovieInfo(object sender, RoutedEventArgs e)
        {
            TorrentInfo ti = mainlist.SelectedItem as TorrentInfo;
            if (ti.PickedMovieData != null && ti.PickedMovieData.Value != null)
            {
                var a = MessageBox.Show("The selected torrent already has movie infomation. Do you which to change it?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (a != MessageBoxResult.Yes)
                {
                    return;
                }
            }
            UI.MovieInfoChooser m = new UI.MovieInfoChooser()
            {
                Title = string.Format("Pick info for torrent: {0}", ti.Name),
                Owner = this,
                Icon = this.Icon,
                SearchQuery = ti.Name,
                Torrent = ti
            };
            m.Show();
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

        #region NotifyIcon

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

        #endregion

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

        private void PeersCommands_AddPeer(object sender, RoutedEventArgs e)
        {
            TorrentInfo ti = mainlist.SelectedItem as TorrentInfo;
            if (ti != null)
            {
                if (ti.Torrent.TorrentFile.Private)
                {
                    MessageBox.Show("You cannot add external peers to a private torrent", "Add peer");
                }
                else
                {
                    MessageBox.Show("Not implemented yet");
                    /*
                    var p = new UI.AddNewPeerDialog() { Owner = this, Icon = this.Icon };

                    if (p.ShowDialog() == true)
                    {

                        ti.Torrent.ManualAddPeer(p.IP.Split(':').First(), Convert.ToInt32(p.IP.Split(':').Last()));
                    }*/
                }
            }
        }

        #region Torrent List Commands

        private void TorrentListCommands_ActionOnSelectedTorrents(object sender, ExecutedRoutedEventArgs e)
        {
            if (mainlist.SelectedItems.Count > 0)
            {
                TorrentInfo[] arr = new TorrentInfo[mainlist.SelectedItems.Count];
                mainlist.SelectedItems.CopyTo(arr, 0);
                Action<TorrentInfo> f = new Action<TorrentInfo>(t => { });

                string action = Convert.ToString(e.Parameter);
                switch (action)
                {
                    case "OpenDownloadDirectory":
                        f = new Action<TorrentInfo>(t =>
                        {
                            System.IO.Directory.CreateDirectory(t.SavePath);
                            Process.Start("explorer.exe", "\"" + t.SavePath + "\"");
                        });
                        break;
                    case "EditProperties":
                        f = new Action<TorrentInfo>(t =>
                        {
                            var editor = new TorrentPropertiesEditor(t) { Owner = this, Icon = this.Icon };
                            editor.ShowDialog();
                        });
                        break;
                    case "Start":
                        f = new Action<TorrentInfo>(t => t.Start());
                        break;
                    case "Pause":
                        f = new Action<TorrentInfo>(t => t.Pause());
                        break;
                    case "Stop":
                        f = new Action<TorrentInfo>(t => t.Stop());
                        break;
                    case "ForcePause":
                        f = new Action<TorrentInfo>(t => t.ForcePause());
                        break;
                    case "ForceStart":
                        f = new Action<TorrentInfo>(t => t.ForceStart());
                        break;
                    case "Recheck":
                        f = new Action<TorrentInfo>(t => t.Recheck());
                        break;

                    case "remove_torrent_unlist":
                    case "remove_torrent_torrentonly":
                    case "remove_torrent_dataonly":
                    case "remove_torrent_both":
                        f = new Action<TorrentInfo>(t => { RemoveTorrent(t, action); });
                        break;
                    default:
                        break;
                }
                foreach (TorrentInfo ti in arr)
                {
                    f(ti);
                }
            }
        }

        private void RemoveTorrent(TorrentInfo t, string action)
        {
            t.Invisible = true;
            t.UpdateList("Invisible", "ShowOnList");
            ThreadPool.QueueUserWorkItem(delegate
            {
                t.Torrent.AutoManaged = false;
                t.Torrent.Pause();

                this.state.LibtorrentSession.RemoveTorrent(t.Torrent);
                this.state.DeleteTorrentStateData(t.InfoHash);

                uiContext.Send(x =>
                {
                    state.Torrents.Remove(t);
                }, null);

                switch (action)
                {
                    case "remove_torrent_torrentonly":
                        DeleteTorrent(t);
                        break;
                    case "remove_torrent_dataonly":
                        DeleteData(t);
                        break;
                    case "remove_torrent_both":
                        DeleteData(t);
                        DeleteTorrent(t);
                        break;
                    default:
                        break;
                }
            });
        }

        private void DeleteTorrent(TorrentInfo t)
        {
            System.IO.File.Delete(t.OriginalTorrentFilePath);
        }

        private void DeleteData(TorrentInfo t)
        {
            return;
            /*List<string> directories = new List<string>();
           
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
            }*/
        }

        #endregion

    }
}
