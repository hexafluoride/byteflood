#include "stdafx.h"
#include "TrackerAlert.h"

#include <libtorrent\alert_types.hpp>

namespace Ragnar
{
    TrackerAlert::TrackerAlert(libtorrent::tracker_alert* alert)
        : TorrentAlert((libtorrent::torrent_alert*) alert)
    {
        this->_url = gcnew System::String(alert->url.c_str());
    }

    System::String^ TrackerAlert::Url::get()
    {
        return this->_url;
    }
}
