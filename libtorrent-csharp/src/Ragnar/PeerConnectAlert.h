#pragma once

#include "PeerAlert.h"

namespace libtorrent
{
    struct peer_connect_alert;
}

namespace Ragnar
{
    public ref class PeerConnectAlert : PeerAlert
    {
    private:
        int _socketType;

    internal:
        PeerConnectAlert(libtorrent::peer_connect_alert* alert);

    public:
        property int SocketType { int get(); }
    };
}
