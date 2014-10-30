#include "stdafx.h"
#include "FileCompletedAlert.h"

#include <libtorrent\alert_types.hpp>

namespace Ragnar
{
    FileCompletedAlert::FileCompletedAlert(libtorrent::file_completed_alert* alert)
        : TorrentAlert((libtorrent::torrent_alert*) alert)
    {
        this->_index = alert->index;
    }
}
