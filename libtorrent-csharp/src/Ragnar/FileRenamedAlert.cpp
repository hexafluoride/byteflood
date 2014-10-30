#include "stdafx.h"
#include "FileRenamedAlert.h"

#include <libtorrent\alert_types.hpp>

namespace Ragnar
{
    FileRenamedAlert::FileRenamedAlert(libtorrent::file_renamed_alert* alert)
        : TorrentAlert((libtorrent::torrent_alert*) alert)
    {
        this->_index = alert->index;
        this->_name = gcnew String(alert->name.c_str());
    }
}
