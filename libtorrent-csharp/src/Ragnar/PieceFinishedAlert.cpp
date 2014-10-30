#include "stdafx.h"
#include "PieceFinishedAlert.h"

#include <libtorrent\alert_types.hpp>

namespace Ragnar
{
    PieceFinishedAlert::PieceFinishedAlert(libtorrent::piece_finished_alert* alert)
        : TorrentAlert((libtorrent::torrent_alert*) alert)
    {
        this->_pieceIndex = alert->piece_index;
    }

    int PieceFinishedAlert::PieceIndex::get()
    {
        return this->_pieceIndex;
    }
}
