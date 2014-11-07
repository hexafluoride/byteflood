using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.ComponentModel;

namespace ByteFlood
{
    public class PieceInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool f = false;
        public bool Finished 
        {
            get { return f; }
            set 
            {
                if (value != f) 
                {
                    f = value;
					OnPropertyChanged();
                }
            }
        }
       
        public int ID { get; set; }
        public string Tooltip { get; set; }
        public PieceInfo() { }
        public void SetSelf(PieceInfo pi)
        {
            this.Finished = pi.Finished;
            this.ID = pi.ID;
            UpdateList("Finished", "ID", "Tooltip");
        }
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
