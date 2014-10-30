#include "stdafx.h"
#include "PeerBanAlert.h"

#include <libtorrent\alert_types.hpp>

namespace Ragnar
{
    PeerBanAlert::PeerBanAlert(libtorrent::peer_ban_alert* alert)
        : PeerAlert((libtorrent::peer_alert*) alert)
    {
    }
}