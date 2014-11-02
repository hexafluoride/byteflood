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
using System.Windows;
using System.Windows.Input;
using System.Text.RegularExpressions;

namespace ByteFlood
{
    /// <summary>
    /// Interaction logic for TorrentPropertiesEditor.xaml
    /// </summary>
    public partial class TorrentPropertiesEditor : Window
    {
        private TorrentInfo ti = null;

        public TorrentProperties TorrentProperties { get; private set; }

        public TorrentPropertiesEditor(TorrentProperties props) 
        {
            InitializeComponent();

            if (props != null) 
            {
                this.Title = string.Format("{0} - ({1})", this.Title, "Default options");
                this.TorrentProperties = props;
            }
            else 
            {
                throw new System.ArgumentNullException("props");
            }
        }

        public TorrentPropertiesEditor(TorrentInfo torrent)
        {
            InitializeComponent();

            if (torrent != null)
            {
                this.ti = torrent;
                this.TorrentProperties = torrent.TorrentSettings;
                this.Title = string.Format("{0} - ({1})", this.Title, this.ti.Name);
            }
            else
            {
                throw new System.ArgumentNullException("torrent");
            }
        }

        public void PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            maxcons.Text = TorrentProperties.MaxConnections.ToString();
            maxdown.Text = (TorrentProperties.MaxDownloadSpeed / 1024).ToString();
            maxup.Text = (TorrentProperties.MaxUploadSpeed / 1024).ToString();
            dht.IsChecked = TorrentProperties.UseDHT;
            peerex.IsChecked = TorrentProperties.EnablePeerExchange;
            uploadslots.Text = TorrentProperties.UploadSlots.ToString();
            this.ratiolimit.Text = TorrentProperties.RatioLimit.ToString();

            if (this.ti == null)
            {
                this.comp.IsEnabled = false;
                this.comp.Text = null;
            }
            else
            {
                this.comp.Text = TorrentProperties.OnFinish;
            }
        }

        static Regex regex = new Regex("[^0-9]+", RegexOptions.Compiled);

        private static bool IsTextAllowed(string text)
        {
            return !regex.IsMatch(text);
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            TorrentProperties.MaxConnections = int.Parse(maxcons.Text);
            TorrentProperties.MaxDownloadSpeed = int.Parse(maxdown.Text) * 1024;
            TorrentProperties.MaxUploadSpeed = int.Parse(maxup.Text) * 1024;
            TorrentProperties.UseDHT = dht.IsChecked == true;
            TorrentProperties.EnablePeerExchange = peerex.IsChecked == true;
            TorrentProperties.UploadSlots = int.Parse(uploadslots.Text);
            TorrentProperties.RatioLimit = float.Parse(ratiolimit.Text);

            if (this.ti != null)
            {
                TorrentProperties.OnFinish = comp.Text;
                this.ti.ApplyTorrentSettings(TorrentProperties);
            }

            this.DialogResult = true;
            this.Close();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
