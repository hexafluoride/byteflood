using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Jayrock;
using Jayrock.JsonRpc;

namespace ByteFlood
{
    /// <summary>
    /// Handles all communications between RPC and State.
    /// </summary>
    // This, at the moment, is a simple wrapper but can be used to simplify methods for RPC use, and retrieve info about the program state in the future.
    // Also, Visual Studio thinks this is a component for some reason. Maybe JsonRpcService.
    public class StateRpcHandler : JsonRpcService
    {
        public State State;
        public StateRpcHandler(State s)
        {
            State = s;
        }

        [JsonRpcMethod]
        public void AddTorrentByPath(string path)
        {
            State.AddTorrentByPath(path);
        }

        [JsonRpcMethod]
        public void AddTorrentByMagnet(string magnet)
        {
            State.AddTorrentByMagnet(magnet);
        }

        [JsonRpcMethod]
        public int GetTorrentCount()
        {
            return State.Torrents.Count;
        }

        [JsonRpcMethod]
        public object GetTorrents()
        {
            return State.Torrents.ToArray();
        }

        [JsonRpcMethod]
        public void Shutdown()
        {
            Task.Factory.StartNew(new Action(() => 
            {
                State.Shutdown();
            }));
        }

    }
}
