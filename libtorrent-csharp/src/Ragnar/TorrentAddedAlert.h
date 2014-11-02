#pragma once

#include "TorrentAlert.h"

namespace libtorrent
{
    struct torrent_added_alert;
}

namespace Ragnar
{
    public ref class TorrentAddedAlert : TorrentAlert
    {
    internal:
        TorrentAddedAlert(libtorrent::torrent_added_alert* alert);
    };
}
