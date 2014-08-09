using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace ByteFlood
{
    public class PieceInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public bool Finished { get; set; }
        public int ID { get; set; }
        public PieceInfo() { }
        public void SetSelf(PieceInfo pi)
        {
            this.Finished = pi.Finished;
            this.ID = pi.ID;
            UpdateList("Finished", "ID");
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
