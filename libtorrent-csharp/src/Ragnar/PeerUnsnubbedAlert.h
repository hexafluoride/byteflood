#pragma once

#include "PeerAlert.h"

namespace libtorrent
{
    struct peer_unsnubbed_alert;
}

namespace Ragnar
{
    public ref class PeerUnsnubbedAlert : PeerAlert
    {
    internal:
        PeerUnsnubbedAlert(libtorrent::peer_unsnubbed_alert* alert);
    };
}
