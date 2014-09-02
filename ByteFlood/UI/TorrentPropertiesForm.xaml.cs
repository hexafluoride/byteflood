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
using System.Windows.Shapes;
using MonoTorrent;
using MonoTorrent.Client;
using System.Text.RegularExpressions;

namespace ByteFlood
{
    /// <summary>
    /// Interaction logic for TorrentPropertiesForm.xaml
    /// </summary>
    public partial class TorrentPropertiesForm : Window
    {
        public TorrentInfo ti;
        public TorrentProperties tp;
        public bool fake = false;
        public bool success = false;

        public TorrentPropertiesForm(TorrentProperties trp)
        {
            InitializeComponent();
            fake = true;
            tp = trp;
        }

        public TorrentPropertiesForm(TorrentInfo t)
        {
            InitializeComponent();
            ti = t;
        }
        public void PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if(!fake)
                tp = TorrentProperties.FromTorrentSettings(ti.Torrent.Settings);
            maxcons.Text = tp.MaxConnections.ToString();
            maxdown.Text = (tp.MaxDownloadSpeed / 1024).ToString();
            maxup.Text = (tp.MaxUploadSpeed / 1024).ToString();
            dht.IsChecked = tp.UseDHT;
            peerex.IsChecked = tp.EnablePeerExchange;
            uploadslots.Text = tp.UploadSlots.ToString();
        }
        private static bool IsTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9]+");
            return !regex.IsMatch(text);
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            tp.MaxConnections = int.Parse(maxcons.Text);
            tp.MaxDownloadSpeed = int.Parse(maxdown.Text) * 1024;
            tp.MaxUploadSpeed = int.Parse(maxup.Text) * 1024;
            tp.UseDHT = (bool)dht.IsChecked;
            tp.EnablePeerExchange = (bool)peerex.IsChecked;
            tp.UploadSlots = int.Parse(uploadslots.Text);
            if (!fake)
            {
                TorrentProperties.Apply(ti.Torrent, tp);
                ti.CompletionCommand = comp.Text;
            }
            success = true;
            this.Close();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
