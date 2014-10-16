using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoTorrent.Client.Connections
{

    /// <summary>
    /// This class handle HTTP seeds 
    /// as defined in http://www.bittorrent.org/beps/bep_0017.html
    /// </summary>
    class HTTPSeedConnection : IConnection
    {
        public byte[] AddressBytes
        {
            get { return new byte[4]; }
        }

        public bool CanReconnect
        {
            get { return false; }
        }

        public bool Connected
        {
            get { return true; }
        }

        public bool IsIncoming
        {
            get { throw new NotImplementedException(); }
        }

        public System.Net.EndPoint EndPoint
        {
            get { throw new NotImplementedException(); }
        }

        public IAsyncResult BeginConnect(AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public void EndConnect(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginReceive(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public int EndReceive(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginSend(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public int EndSend(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        public Uri Uri
        {
            get { throw new NotImplementedException(); }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
