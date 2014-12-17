using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.ComponentModel;

namespace ByteFlood
{
    public class PeerInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public string IP { get; set; }
        public byte[] AddressBytes { get; set; }
        public string Encryption { get; set; }

        private string client = null;
        public string Client
        {
            get { return this.client; }
            set 
            {
                if (value != client) 
                {
                    client = value;

					OnPropertyChanged();
                }
            }
        }
       
        private string pi_info = null;
        public string PieceInfo 
        {
            get { return this.pi_info; }
            set 
            {
                if (value != this.pi_info) 
                {
                    this.pi_info = value;

					OnPropertyChanged();
                }
            }
        }

        public PeerInfo() { }

        public void UpdateList(params string[] columns)
        {
            foreach (string str in columns)
                OnPropertyChanged(str);
        }

		public void OnPropertyChanged([CallerMemberName]string name = null)
		{
			var handler = PropertyChanged;
			if (handler != null)
				handler(this, new PropertyChangedEventArgs(name));
		}
    }
}
