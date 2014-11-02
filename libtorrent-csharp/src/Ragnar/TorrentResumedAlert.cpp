#include "stdafx.h"
#include "TorrentResumedAlert.h"

#include <libtorrent\alert_types.hpp>

namespace Ragnar
{
    TorrentResumedAlert::TorrentResumedAlert(libtorrent::torrent_resumed_alert* alert)
        : TorrentAlert((libtorrent::torrent_alert*) alert)
    {
    }
}