#pragma once

#include "TorrentAlert.h"

namespace libtorrent
{
    struct torrent_error_alert;
}

namespace Ragnar
{
    public ref class TorrentErrorAlert : TorrentAlert
    {
        // TODO: should use error code enum

    private:
        int _errorCode;

    internal:
        TorrentErrorAlert(libtorrent::torrent_error_alert* alert);

    public:
        property int ErrorCode { int get(); }
    };
}
