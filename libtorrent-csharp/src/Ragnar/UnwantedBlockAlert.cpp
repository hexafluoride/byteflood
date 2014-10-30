#include "stdafx.h"
#include "UnwantedBlockAlert.h"

#include <libtorrent\alert_types.hpp>

namespace Ragnar
{
    UnwantedBlockAlert::UnwantedBlockAlert(libtorrent::unwanted_block_alert* alert)
        : PeerAlert((libtorrent::peer_alert*) alert)
    {
        this->_blockIndex = alert->block_index;
        this->_pieceIndex = alert->piece_index;
    }

    int UnwantedBlockAlert::BlockIndex::get()
    {
        return this->_blockIndex;
    }

    int UnwantedBlockAlert::PieceIndex::get()
    {
        return this->_pieceIndex;
    }
}
