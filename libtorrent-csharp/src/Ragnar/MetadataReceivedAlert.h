#pragma once

#include "TorrentAlert.h"

namespace libtorrent
{
    struct metadata_received_alert;
}

namespace Ragnar
{
    public ref class MetadataReceivedAlert : TorrentAlert
    {
    internal:
        MetadataReceivedAlert(libtorrent::metadata_received_alert* alert);
    };
}
