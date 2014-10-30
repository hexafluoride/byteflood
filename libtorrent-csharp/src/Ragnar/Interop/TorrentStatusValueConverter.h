#pragma once

#include "ValueConverter.h"

namespace libtorrent
{
    struct torrent_status;
}

namespace Ragnar
{
    ref class TorrentStatus;

    namespace Interop
    {
        template ref class ValueConverter<libtorrent::torrent_status, Ragnar::TorrentStatus^>;

        public ref class TorrentStatusValueConverter : ValueConverter<libtorrent::torrent_status, Ragnar::TorrentStatus^>
        {
        public:
            virtual libtorrent::torrent_status To(Ragnar::TorrentStatus^ value) override;

            virtual Ragnar::TorrentStatus^ From(const libtorrent::torrent_status &value) override;
        };
    }
}
