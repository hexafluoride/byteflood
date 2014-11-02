#include "stdafx.h"
#include "TorrentFinishedAlert.h"

#include <libtorrent\alert_types.hpp>

namespace Ragnar
{
    TorrentFinishedAlert::TorrentFinishedAlert(libtorrent::torrent_finished_alert* alert)
        : TorrentAlert((libtorrent::torrent_alert*) alert)
    {
    }
}