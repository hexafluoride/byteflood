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
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.IO;

namespace ByteFlood
{
    /// <summary>
    /// Interaction logic for AddTorrent.xaml
    /// </summary>
    public partial class AddTorrentDialog : Window
    {
        public TorrentInfo TorrentInfo { get; private set; }

        public bool AutoStartTorrent = true;
        public bool UserOK = false;
        public bool WindowClosed = false;
        public float RatioLimit { get; set; }

        public string TorrentName
        {
            get { return (string)GetValue(TorrentNameProperty); }
            set { SetValue(TorrentNameProperty, value); }
        }

        public static readonly DependencyProperty TorrentNameProperty =
            DependencyProperty.Register("TorrentName", typeof(string), typeof(AddTorrentDialog), new PropertyMetadata(null));


        public string TorrentSavePath
        {
            get { return (string)GetValue(TorrentSavePathProperty); }
            set { SetValue(TorrentSavePathProperty, value); }
        }

        public static readonly DependencyProperty TorrentSavePathProperty =
            DependencyProperty.Register("TorrentSavePath", typeof(string), typeof(AddTorrentDialog), new PropertyMetadata(new PropertyChangedCallback((Do, des) =>
            {
                (Do as AddTorrentDialog).UpdateSize();
            })));


        public Ragnar.TorrentInfo TorrentFileInfo
        {
            get { return (Ragnar.TorrentInfo)GetValue(TorrentFileInfoProperty); }
            set { SetValue(TorrentFileInfoProperty, value); }
        }

        public static readonly DependencyProperty TorrentFileInfoProperty =
            DependencyProperty.Register("TorrentFileInfo", typeof(Ragnar.TorrentInfo), typeof(AddTorrentDialog), new PropertyMetadata(null));

        public List<string> SavedPathList
        {
            get { return App.Settings.PreviousPaths; }
        }

        public AddTorrentDialog(TorrentInfo torrent)
        {
            InitializeComponent();
            this.Closed += (s, e) => { this.WindowClosed = true; };
            this.TorrentInfo = torrent;
            this.TorrentFileInfo = torrent.Torrent.TorrentFile;
            Load();
        }

        public void UpdateSize()
        {
            try
            {
                DriveInfo drive = new DriveInfo(System.IO.Path.GetPathRoot(this.TorrentSavePath));
                size.Content = Utility.PrettifyAmount(this.TorrentInfo.Size) + string.Format(" (Available disk space: {0})", Utility.PrettifyAmount(drive.AvailableFreeSpace));
            }
            catch { }
        }

        public void Load()
        {
            loading.Visibility = Visibility.Hidden;

            this.RatioLimit = 0f;

            this.TorrentName = this.TorrentInfo.Torrent.TorrentFile.Name;

            this.TorrentSavePath = App.Settings.DefaultDownloadPath;

            this.fileList.ItemsSource = this.TorrentInfo.FileInfoList;

            UpdateSize();

            this.Activate();
        }

        #region Commands

        private void Commands_Browse(object sender, ExecutedRoutedEventArgs e)
        {
            string path = Utility.PromptFolderSelection("Choose torrent save directory", null, this);
            if (path != null)
                this.TorrentSavePath = path;
        }

        private void Commands_OK(object sender, ExecutedRoutedEventArgs e)
        {
            this.AutoStartTorrent = (start_torrent.IsChecked == true); // sorry
            this.UserOK = true;
            if (!App.Settings.PreviousPaths.Contains(TorrentSavePath))
                App.Settings.PreviousPaths.Insert(0, TorrentSavePath);
            this.Close();
        }

        private void Commands_Cancel(object sender, RoutedEventArgs e)
        {
            this.UserOK = false;
            this.Close();
        }

        private void Commands_SelectAll(object sender, ExecutedRoutedEventArgs e)
        {
            foreach (FileInfo file in this.TorrentInfo.FileInfoList)
            {
                file.DownloadFile = true;
            }
        }

        private void Commands_DeselectAll(object sender, ExecutedRoutedEventArgs e)
        {
            foreach (FileInfo file in this.TorrentInfo.FileInfoList)
            {
                file.DownloadFile = false;
            }
        }

        #endregion

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox s = sender as CheckBox;
            FileInfo fi = s.Tag as FileInfo;
            fi.DownloadFile = s.IsChecked == true;
        }
    }
}
