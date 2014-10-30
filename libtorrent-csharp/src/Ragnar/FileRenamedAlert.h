#pragma once

#include "TorrentAlert.h"

namespace libtorrent
{
    struct file_renamed_alert;
}

namespace Ragnar
{
    public ref class FileRenamedAlert : TorrentAlert
    {
    private:
        int _index;
        System::String^ _name;

    internal:
        FileRenamedAlert(libtorrent::file_renamed_alert* alert);

    public:
        property int Index { int get() { return this->_index; } }

        property System::String^ Name { System::String^ get() { return this->_name; } }
    };
}
