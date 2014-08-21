using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ByteFlood.Services.TorrentCache
{
    public interface ITorrentCache
    {
        string Name { get; }

        string Url { get; }

        byte[] Fetch(MonoTorrent.MagnetLink magnet);
    }
}
