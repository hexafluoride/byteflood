#include "stdafx.h"
#include "TorrentErrorAlert.h"

#include <libtorrent\alert_types.hpp>

namespace Ragnar
{
    TorrentErrorAlert::TorrentErrorAlert(libtorrent::torrent_error_alert* alert)
        : TorrentAlert((libtorrent::torrent_alert*) alert)
    {
        this->_errorCode = alert->error.value();
    }

    int TorrentErrorAlert::ErrorCode::get()
    {
        return this->_errorCode;
    }
}
