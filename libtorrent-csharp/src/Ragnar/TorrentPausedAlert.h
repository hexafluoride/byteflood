#pragma once

#include "TorrentAlert.h"

namespace libtorrent
{
    struct torrent_paused_alert;
}

namespace Ragnar
{
    public ref class TorrentPausedAlert : TorrentAlert
    {
    internal:
        TorrentPausedAlert(libtorrent::torrent_paused_alert* alert);;
    };
}
