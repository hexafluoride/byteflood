#pragma once

#include "TorrentAlert.h"

namespace libtorrent
{
    struct storage_moved_alert;
}

namespace Ragnar
{
    public ref class StorageMovedAlert : TorrentAlert
    {
    private:
        System::String^ _path;

    internal:
        StorageMovedAlert(libtorrent::storage_moved_alert* alert);

    public:
        property System::String^ Path{ System::String^ get(); }
    };
}
