#pragma once

namespace libtorrent
{
    struct create_torrent;
}

namespace Ragnar
{
    ref class TorrentInfo;

    public ref class TorrentCreator
    {
    private:
        libtorrent::create_torrent* _creator;

    public:
        TorrentCreator(TorrentInfo^ info);
        ~TorrentCreator();

        cli::array<byte>^ Generate();
    };
}
