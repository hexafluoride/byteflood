#include "stdafx.h"
#include "TorrentRemovedAlert.h"

#include <libtorrent\alert_types.hpp>

namespace Ragnar
{
    TorrentRemovedAlert::TorrentRemovedAlert(libtorrent::torrent_removed_alert* alert)
        : TorrentAlert((libtorrent::torrent_alert*) alert)
    {
        this->_infoHash = gcnew System::String(libtorrent::to_hex(alert->info_hash.to_string()).c_str());
    }

    System::String^ TorrentRemovedAlert::InfoHash::get()
    {
        return this->_infoHash;
    }
}