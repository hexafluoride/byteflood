#pragma once

#include "PeerAlert.h"

namespace libtorrent
{
    struct peer_ban_alert;
}

namespace Ragnar
{
    public ref class PeerBanAlert : PeerAlert
    {
    internal:
        PeerBanAlert(libtorrent::peer_ban_alert* alert);
    };
}

