#pragma once

#include "TorrentAlert.h"

namespace libtorrent
{
    struct torrent_removed_alert;
}

namespace Ragnar
{
    public ref class TorrentRemovedAlert : TorrentAlert
    {
    private:
        System::String^ _infoHash;

    internal:
        TorrentRemovedAlert(libtorrent::torrent_removed_alert* alert);

    public:
        property System::String^ InfoHash { System::String^ get(); }
    };
}
