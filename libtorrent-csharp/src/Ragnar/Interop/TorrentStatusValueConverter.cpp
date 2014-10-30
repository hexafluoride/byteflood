#include "stdafx.h"
#include "../TorrentStatus.h"
#include "TorrentStatusValueConverter.h"

#include <libtorrent\torrent_handle.hpp>

namespace Ragnar
{
    namespace Interop
    {
        libtorrent::torrent_status TorrentStatusValueConverter::To(Ragnar::TorrentStatus^ value)
        {
            return *value->get_ptr();
        }

        Ragnar::TorrentStatus^ TorrentStatusValueConverter::From(const libtorrent::torrent_status &value)
        {
            return gcnew Ragnar::TorrentStatus(value);
        }
    }
}
