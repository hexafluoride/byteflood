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
    public class TorrentProperties
    {
        public int MaxConnections { get; set; }
        public int MaxDownloadSpeed { get; set; }
        public int MaxUploadSpeed { get; set; }
        public int UploadSlots { get; set; }
        public bool UseDHT { get; set; }
        public bool EnablePeerExchange { get; set; }
        public string OnFinish { get; set; }

        public static TorrentProperties DefaultTorrentProperties = new TorrentProperties()
        {
            MaxConnections = 60,
            MaxDownloadSpeed = 0,
            MaxUploadSpeed = 0,
            UploadSlots = 4,
            UseDHT = true,
            EnablePeerExchange = true
        };

        public TorrentProperties()
        {
        }

        public static TorrentProperties FromTorrentSettings(TorrentSettings ts)
        {
            TorrentProperties tp = new TorrentProperties();
            tp.MaxConnections = ts.MaxConnections;
            tp.MaxDownloadSpeed = ts.MaxDownloadSpeed;
            tp.MaxUploadSpeed = ts.MaxUploadSpeed;
            tp.UploadSlots = ts.UploadSlots;
            tp.UseDHT = ts.UseDht;
            tp.EnablePeerExchange = ts.EnablePeerExchange;
            return tp;
        }

        public static void Apply(TorrentManager tm, TorrentProperties tp)
        {
            if (tp == null) { return; }
            tm.Settings.MaxConnections = tp.MaxConnections;
            tm.Settings.MaxDownloadSpeed = tp.MaxDownloadSpeed;
            tm.Settings.MaxUploadSpeed = tp.MaxUploadSpeed;
            tm.Settings.UploadSlots = tp.UploadSlots;
            tm.Settings.UseDht = tp.UseDHT;
            tm.Settings.EnablePeerExchange = tp.EnablePeerExchange;
        }
    }
}
