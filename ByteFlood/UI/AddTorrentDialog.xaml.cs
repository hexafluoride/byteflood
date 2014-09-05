﻿/* 
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
using MonoTorrent.Dht;
using MonoTorrent.Dht.Listeners;
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
        public TorrentManager tm;

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
            set { SetValue(TorrentSavePathProperty, value); UpdateSize(); }
        }

        public static readonly DependencyProperty TorrentSavePathProperty =
            DependencyProperty.Register("TorrentSavePath", typeof(string), typeof(AddTorrentDialog), new PropertyMetadata(null));
        

        public AddTorrentDialog(string path)
        {
            InitializeComponent();
            this.Closed += (s, e) => { this.WindowClosed = true; };
            this.FileList = new ObservableCollection<FileInfo>();
            if (!string.IsNullOrWhiteSpace(path))
                Load(path);
            else
                loading.Visibility = Visibility.Visible;
        }

        public void UpdateSize()
        {
            DirectoryInfo dir = new DirectoryInfo(this.TorrentSavePath);
            DriveInfo drive = new DriveInfo(dir.Root.FullName);
            size.Content = Utility.PrettifyAmount(tm.Torrent.Size) + string.Format(" (Available disk space: {0})", Utility.PrettifyAmount(drive.AvailableFreeSpace));
        }

        public void Load(string path)
        {
            loading.Visibility = Visibility.Collapsed;
            
            this.tm = new TorrentManager(Torrent.Load(path), App.Settings.DefaultDownloadPath, new TorrentSettings());
            
            this.Torrent = tm.Torrent;
            
            this.RatioLimit = 0f;

            foreach (TorrentFile file in tm.Torrent.Files)
            {
                FileInfo fi = new FileInfo(null, file);
                fi.DownloadFile = !(App.Settings.EnableFileRegex && Regex.IsMatch(fi.Name, App.Settings.FileRegex));
                this.FileList.Add(fi);
            }

            this.TorrentName = tm.Torrent.Name;

            this.TorrentSavePath = tm.SavePath;

            this.Activate();
        }

        #region Commands

        private void Commands_Browse(object sender, ExecutedRoutedEventArgs e)
        {
            using (var fd = new System.Windows.Forms.FolderBrowserDialog())
            {
                fd.ShowNewFolderButton = true;
                if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    tm = new TorrentManager(tm.Torrent, fd.SelectedPath, new TorrentSettings());
                    this.TorrentSavePath = tm.SavePath;
                }
            }
        }

        private void Commands_OK(object sender, ExecutedRoutedEventArgs e)
        {
            this.AutoStartTorrent = (start_torrent.IsChecked == true); // sorry
            this.UserOK = true;
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
