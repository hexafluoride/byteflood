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

namespace ftorrent
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
            tm = new TorrentManager(Torrent.Load(path), System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Downloads"), new TorrentSettings());
            this.DataContext = tm;
            foreach (TorrentFile file in tm.Torrent.Files)
            {
                FileInfo fi = new FileInfo();
                fi.Name = file.Path;
                fi.RawSize = file.Length;
                files.Add(fi);
            }
            torrentname = tm.Torrent.Name;
            name.Text = torrentname;
            filelist.ItemsSource = files;
            UpdateTextBox();

            UpdateSize();
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
            if (((CheckBox)e.Source).IsChecked == true) // have to do this, sorry guys
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
    }
}
