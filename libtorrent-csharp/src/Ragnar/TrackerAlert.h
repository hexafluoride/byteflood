#pragma once

#include "TorrentAlert.h"

namespace libtorrent
{
    struct tracker_alert;
}

namespace Ragnar
{
    public ref class TrackerAlert : TorrentAlert
    {
    private:
        System::String^ _url;

    internal:
        TrackerAlert(libtorrent::tracker_alert* alert);

    public:
        property System::String^ Url { System::String^ get(); }
    };
}
