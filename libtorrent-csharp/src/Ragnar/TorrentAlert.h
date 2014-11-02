#pragma once

#include "Alert.h"

namespace libtorrent
{
    struct torrent_alert;
}

namespace Ragnar
{
    ref class TorrentHandle;

    public ref class TorrentAlert abstract : Alert
    {
    private:
        TorrentHandle^ _handle;

    internal:
        TorrentAlert(libtorrent::torrent_alert* alert);

    public:
        property TorrentHandle^ Handle { TorrentHandle^ get(); }
    };
}