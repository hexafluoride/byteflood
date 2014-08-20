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
        public bool userselected = false;
        public bool start = true;
        public string torrentname = "";
        public float limit = 0f;
        public AddTorrentDialog(string path)
        {
            InitializeComponent();
            tm = new TorrentManager(Torrent.Load(path), App.Settings.DefaultDownloadPath, new TorrentSettings());
            this.DataContext = tm;
            foreach (TorrentFile file in tm.Torrent.Files)
            {
                FileInfo fi = new FileInfo();
                fi.Name = file.Path;
                fi.DownloadFile = true;
                if (App.Settings.EnableFileRegex && Regex.IsMatch(fi.Name, App.Settings.FileRegex))
                {
                    fi.DownloadFile = false;
                    UpdateFile(fi.Name, fi.DownloadFile);
                }
                fi.RawSize = file.Length;
                files.Add(fi);
                fi.SetSelf(fi);
            }
            torrentname = tm.Torrent.Name;
            name.Text = torrentname;
            filelist.ItemsSource = files;
            UpdateTextBox();

            UpdateSize();

            this.Activate();
        }
        public void UpdateSize()
        {
            DirectoryInfo dir = new DirectoryInfo(pathbox.Text);
            DriveInfo drive = new DriveInfo(dir.Root.FullName);
            size.Content = Utility.PrettifyAmount(tm.Torrent.Size) + string.Format(" (Available disk space: {0})", Utility.PrettifyAmount(drive.AvailableFreeSpace));
        }
        ObservableCollection<FileInfo> files = new ObservableCollection<FileInfo>();
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            var fd = new System.Windows.Forms.FolderBrowserDialog();
            fd.ShowNewFolderButton = true;
            fd.ShowDialog();
            tm = new TorrentManager(tm.Torrent, fd.SelectedPath, new TorrentSettings());
            UpdateTextBox();
            UpdateSize();
        }

        private void UpdateTextBox()
        {
            // i have to do this because WPF is retarded
            pathbox.Text = tm.SavePath;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            string path = ((FileInfo)filelist.SelectedItem).Name;
            UpdateFile(path, ((CheckBox)e.Source).IsChecked == true);
        }

        private void UpdateFile(string path, bool download)
        {
            if (download)
                tm.Torrent.Files.First(t => t.Path == path).Priority = Priority.Normal;
            else
                tm.Torrent.Files.First(t => t.Path == path).Priority = Priority.DoNotDownload;
        }

        private void button1_Click_1(object sender, RoutedEventArgs e)
        {
            userselected = true;
            torrentname = name.Text;
            start = (start_torrent.IsChecked == true); // sorry
            if (!float.TryParse(ratiolimit.Text, out limit))
            {
                System.Media.SystemSounds.Beep.Play();
                ratiolimit.Background = Brushes.Salmon;
                return;
            }
            this.Close();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SelectAll(object sender, RoutedEventArgs e)
        {
            foreach (FileInfo file in filelist.Items)
            {
                file.DownloadFile = true;
                file.UpdateList("DownloadFile");
                UpdateFile(file.Name, file.DownloadFile);
            }
        }
        private void DeselectAll(object sender, RoutedEventArgs e)
        {
            foreach (FileInfo file in filelist.Items)
            {
                file.DownloadFile = false;
                file.UpdateList("DownloadFile");
                UpdateFile(file.Name, file.DownloadFile);
            }
        }
    }
}
