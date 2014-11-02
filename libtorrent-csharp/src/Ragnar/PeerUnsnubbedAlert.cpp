#include "stdafx.h"
#include "PeerUnsnubbedAlert.h"

#include <libtorrent\alert_types.hpp>

namespace Ragnar
{
    PeerUnsnubbedAlert::PeerUnsnubbedAlert(libtorrent::peer_unsnubbed_alert* alert)
        : PeerAlert((libtorrent::peer_alert*) alert)
    {
    }
}
