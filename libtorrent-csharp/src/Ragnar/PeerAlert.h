#pragma once
#pragma once

#include "Alert.h"

namespace libtorrent
{
    struct peer_alert;
}

namespace Ragnar
{
    public ref class PeerAlert abstract : Alert
    {
    private:
        System::Net::EndPoint^ _endPoint;

    internal:
        PeerAlert(libtorrent::peer_alert* alert);

    public:
        property System::Net::EndPoint^ EndPoint { System::Net::EndPoint^ get(); }


        // todo: peer_id
    };
}
