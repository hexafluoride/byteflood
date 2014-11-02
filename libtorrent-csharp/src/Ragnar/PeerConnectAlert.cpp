#include "stdafx.h"
#include "PeerConnectAlert.h"

#include <libtorrent\alert_types.hpp>

namespace Ragnar
{
    PeerConnectAlert::PeerConnectAlert(libtorrent::peer_connect_alert* alert)
        : PeerAlert((libtorrent::peer_alert*) alert)
    {
        this->_socketType = alert->socket_type;
    }

    int PeerConnectAlert::SocketType::get()
    {
        return this->_socketType;
    }
}