#pragma once

#include "TorrentAlert.h"

namespace libtorrent
{
    struct piece_finished_alert;
}

namespace Ragnar
{
    public ref class PieceFinishedAlert : TorrentAlert
    {
    private:
        int _pieceIndex;

    internal:
        PieceFinishedAlert(libtorrent::piece_finished_alert* alert);

    public:
        property int PieceIndex { int get(); }
    };
}
