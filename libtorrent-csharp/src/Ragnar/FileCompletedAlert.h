#pragma once

#include "TorrentAlert.h"

namespace libtorrent
{
    struct file_completed_alert;
}

namespace Ragnar
{
    public ref class FileCompletedAlert : TorrentAlert
    {
    private:
        int _index;

    internal:
        FileCompletedAlert(libtorrent::file_completed_alert* alert);

    public:
        property int Index { int get() { return this->_index; } }
    };
}
