#include "stdafx.h"
#include "TorrentAddedAlert.h"

#include <libtorrent\alert_types.hpp>

namespace Ragnar
{
    TorrentAddedAlert::TorrentAddedAlert(libtorrent::torrent_added_alert* alert)
        : TorrentAlert((libtorrent::torrent_alert*) alert)
    {
    }
}