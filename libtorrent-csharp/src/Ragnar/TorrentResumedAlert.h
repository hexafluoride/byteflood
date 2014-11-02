#pragma once

#include "TorrentAlert.h"

namespace libtorrent
{
    struct torrent_resumed_alert;
}

namespace Ragnar
{
    public ref class TorrentResumedAlert : TorrentAlert
    {
    internal:
        TorrentResumedAlert(libtorrent::torrent_resumed_alert* alert);
    };
}
