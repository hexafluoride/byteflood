#include "stdafx.h"
#include "StorageMovedAlert.h"

#include <libtorrent\alert_types.hpp>

namespace Ragnar
{
    StorageMovedAlert::StorageMovedAlert(libtorrent::storage_moved_alert* alert)
        : TorrentAlert((libtorrent::torrent_alert*) alert)
    {
        this->_path = gcnew System::String(alert->path.c_str());
    }

    System::String^ StorageMovedAlert::Path::get()
    {
        return this->_path;
    }
}
