#pragma once

#include "TorrentAlert.h"

namespace libtorrent
{
    struct torrent_finished_alert;
}

namespace Ragnar
{
    public ref class TorrentFinishedAlert : TorrentAlert
    {
    internal:
        TorrentFinishedAlert(libtorrent::torrent_finished_alert* alert);
    };
}
