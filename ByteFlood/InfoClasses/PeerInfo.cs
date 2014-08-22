using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace ByteFlood
{
    public class PeerInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public string IP { get; set; }
        public byte[] AddressBytes { get; set; }
        public string Client { get; set; }
        public string PieceInfo { get; set; }
        public PeerInfo() { }
        public void SetSelf(PeerInfo pi)
        {
            this.IP = pi.IP;
            this.Client = pi.Client;
            this.PieceInfo = pi.PieceInfo;
            UpdateList("IP", "Client", "PieceInfo", "AddressBytes");
        }
        public void UpdateList(params string[] columns)
        {
            if (PropertyChanged == null)
                return;
            foreach (string str in columns)
                PropertyChanged(this, new PropertyChangedEventArgs(str));
        }
    }
}
