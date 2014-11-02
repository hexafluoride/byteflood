#include "stdafx.h"
#include "TorrentAlert.h"
#include "TorrentHandle.h"

#include <libtorrent\alert_types.hpp>

namespace Ragnar
{
    TorrentAlert::TorrentAlert(libtorrent::torrent_alert* alert)
        : Alert((libtorrent::alert*) alert)
    {
        this->_handle = gcnew TorrentHandle(alert->handle);
    }

    TorrentHandle^ TorrentAlert::Handle::get()
    {
        return this->_handle;
    }
}
