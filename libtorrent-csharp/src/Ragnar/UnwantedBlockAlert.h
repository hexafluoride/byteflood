#pragma once

#include "PeerAlert.h"

namespace libtorrent
{
    struct unwanted_block_alert;
}

namespace Ragnar
{
    public ref class UnwantedBlockAlert : PeerAlert
    {
    private:
        int _blockIndex;
        int _pieceIndex;

    internal:
        UnwantedBlockAlert(libtorrent::unwanted_block_alert* alert);

    public:
        property int BlockIndex { int get(); }

        property int PieceIndex { int get(); }
    };
}
