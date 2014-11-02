#include "stdafx.h"
#include "StatsAlert.h"

#include <libtorrent\alert_types.hpp>

namespace Ragnar
{
    StatsAlert::StatsAlert(libtorrent::stats_alert* alert)
        : TorrentAlert((libtorrent::torrent_alert*) alert)
    {
        this->_uploadPayload = alert->transferred[libtorrent::stats_alert::stats_channel::upload_payload];
        this->_uploadProtocol = alert->transferred[libtorrent::stats_alert::stats_channel::upload_protocol];
        this->_downloadPayload = alert->transferred[libtorrent::stats_alert::stats_channel::download_payload];
        this->_downloadProtocol = alert->transferred[libtorrent::stats_alert::stats_channel::download_protocol];
        this->_uploadIpProtocol = alert->transferred[libtorrent::stats_alert::stats_channel::upload_ip_protocol];
        this->_uploadDhtProtocol = alert->transferred[libtorrent::stats_alert::stats_channel::upload_dht_protocol];
        this->_uploadTrackerProtocol = alert->transferred[libtorrent::stats_alert::stats_channel::upload_tracker_protocol];
        this->_downloadIpProtocol = alert->transferred[libtorrent::stats_alert::stats_channel::download_ip_protocol];
        this->_downloadDhtProtocol = alert->transferred[libtorrent::stats_alert::stats_channel::download_dht_protocol];
        this->_downloadTrackerProtocol = alert->transferred[libtorrent::stats_alert::stats_channel::download_tracker_protocol];
        this->_interval = alert->interval;
    }
}
