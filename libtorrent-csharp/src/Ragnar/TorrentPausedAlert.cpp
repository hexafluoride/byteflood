#include "stdafx.h"
#include "TorrentPausedAlert.h"

#include <libtorrent\alert_types.hpp>

namespace Ragnar
{
    TorrentPausedAlert::TorrentPausedAlert(libtorrent::torrent_paused_alert* alert)
        : TorrentAlert((libtorrent::torrent_alert*) alert)
    {
    }
}
