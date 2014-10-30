using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteFlood
{
    public class LibTorrentAlerts : Ragnar.IAlertFactory
    {
        public bool PeekWait(TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public Ragnar.Alert Pop()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Ragnar.Alert> PopAll()
        {
            throw new NotImplementedException();
        }
    }
}
