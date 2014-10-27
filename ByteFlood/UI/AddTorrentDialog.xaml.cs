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

using MonoTorrent.Common;
using Microsoft.Win32;
using System.Threading;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

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

        public Torrent Torrent
        {
            get { return (Torrent)GetValue(TorrentProperty); }
            set { SetValue(TorrentProperty, value); }
        }

        public static readonly DependencyProperty TorrentProperty =
            DependencyProperty.Register("Torrent", typeof(Torrent), typeof(AddTorrentDialog), new PropertyMetadata(null));


        public ObservableCollection<FileInfo> FileList
        {
            get { return (ObservableCollection<FileInfo>)GetValue(FileListProperty); }
            set { SetValue(FileListProperty, value); }
        }

        public static readonly DependencyProperty FileListProperty =
            DependencyProperty.Register("FileList", typeof(ObservableCollection<FileInfo>), typeof(AddTorrentDialog), new PropertyMetadata(null));

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

        public List<string> SavedPathList
        {
            get { return App.Settings.PreviousPaths; }
        }

        public AddTorrentDialog(TorrentInfo torrent)
        {
            InitializeComponent();
            this.Closed += (s, e) => { this.WindowClosed = true; };
            this.FileList = new ObservableCollection<FileInfo>();
            this.TorrentInfo = torrent;
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
            
            load_files(this.TorrentInfo.FilesTree);

            UpdateSize();

            this.Activate();
        }

        private void load_files(DirectoryKey dk)
        {
            foreach (var item in dk.Values)
            {
                if (item.GetType() == typeof(FileInfo))
                {
                    this.FileList.Add((FileInfo)item);
                }
                else if (item.GetType() == typeof(DirectoryKey))
                {
                    load_files((DirectoryKey)item);
                }
            }
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
            foreach (FileInfo file in this.FileList)
            {
                file.DownloadFile = true;
            }
        }

        private void Commands_DeselectAll(object sender, ExecutedRoutedEventArgs e)
        {
            foreach (FileInfo file in this.FileList)
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
